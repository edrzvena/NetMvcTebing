using Tebing.Common;
using Tebing.Common.Enums;
using Tebing.Constants;
using Tebing.DTOs.Payment;
using Tebing.Models;
using Tebing.Repositories.Interfaces;
using Tebing.Services.Interfaces;

namespace Tebing.Services.Implementations;

public class PaymentService(
    IPaymentRepository paymentRepository,
    IBookingRepository bookingRepository,
    IWebHostEnvironment environment) : IPaymentService
{
    // Ekstensi yang diizinkan + signature (magic bytes) awal file-nya, biar file .exe yang di-rename
    // jadi .jpg tetap ketolak.
    private static readonly Dictionary<string, (string ContentType, byte[][] Signatures)> AllowedFileTypes = new()
    {
        [".jpg"] = ("image/jpeg", [[0xFF, 0xD8, 0xFF]]),
        [".jpeg"] = ("image/jpeg", [[0xFF, 0xD8, 0xFF]]),
        [".png"] = ("image/png", [[0x89, 0x50, 0x4E, 0x47]]),
        [".pdf"] = ("application/pdf", [[0x25, 0x50, 0x44, 0x46]])
    };

    // Di luar wwwroot, jadi file gak bisa diakses langsung lewat URL statis.
    private string UploadFolder => Path.Combine(environment.ContentRootPath, "App_Data", "uploads", "payments");

    public async Task<PaymentResult> UploadAsync(int userId, PaymentUploadDto dto)
    {
        var booking = await bookingRepository.GetByIdWithDetailsAsync(dto.BookingId);
        if (booking is null || booking.UserId != userId)
            return new PaymentResult(false, "Booking tidak ditemukan.");

        if (booking.Status != BookingStatus.Pending)
            return new PaymentResult(false, "Bukti transfer cuma bisa diupload untuk booking yang masih Pending.");

        if (booking.TotalPrice <= 0)
            return new PaymentResult(false, "Booking ini gratis, gak perlu upload bukti transfer.");

        if (booking.Payment?.Status == PaymentStatus.Verified)
            return new PaymentResult(false, "Pembayaran booking ini sudah diverifikasi.");

        var file = dto.ProofFile;
        if (file is null || file.Length == 0)
            return new PaymentResult(false, "Pilih file bukti transfer.");

        if (file.Length > FileUploadSettings.PaymentProofMaxBytes)
            return new PaymentResult(false, "Ukuran file maksimal 2 MB.");

        // Nama asli dari user cuma diambil ekstensinya buat dicek, gak pernah dipakai buat nyimpen
        // file (rawan path traversal) — nama file di disk selalu GUID.
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedFileTypes.TryGetValue(extension, out var fileType) ||
            !await HasValidSignatureAsync(file, fileType.Signatures))
            return new PaymentResult(false, "File harus berupa JPG, PNG, atau PDF.");

        Directory.CreateDirectory(UploadFolder);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(UploadFolder, fileName);
        await using (var stream = new FileStream(fullPath, FileMode.CreateNew))
        {
            await file.CopyToAsync(stream);
        }

        var oldFileName = booking.Payment?.PaymentProofPath;
        try
        {
            if (booking.Payment is null)
            {
                await paymentRepository.AddAsync(new Payment
                {
                    BookingId = booking.Id,
                    Amount = booking.TotalPrice,
                    PaymentProofPath = fileName
                });
            }
            else
            {
                // Upload ulang (sebelumnya ditolak admin atau salah file): record lama dipakai lagi.
                var payment = booking.Payment;
                payment.Amount = booking.TotalPrice;
                payment.PaymentProofPath = fileName;
                payment.Status = PaymentStatus.Pending;
                payment.RejectionReason = null;
                payment.UploadedAt = DateTime.UtcNow;
                payment.VerifiedAt = null;
                // Gak perlu Update(): entity-nya udah di-track. Update() malah nandain seluruh graph
                // (Booking, User, Mountain, ...) jadi Modified dan bikin UPDATE yang gak perlu.
            }

            await paymentRepository.SaveChangesAsync();
        }
        catch
        {
            File.Delete(fullPath);
            throw;
        }

        if (oldFileName is not null)
            File.Delete(Path.Combine(UploadFolder, oldFileName));

        return new PaymentResult(true);
    }

    public async Task<PaymentResult> VerifyAsync(PaymentVerifyDto dto)
    {
        if (dto.Status is not (PaymentStatus.Verified or PaymentStatus.Rejected))
            return new PaymentResult(false, "Status verifikasi tidak valid.");

        var payment = await paymentRepository.GetByBookingIdAsync(dto.BookingId);
        if (payment is null)
            return new PaymentResult(false, "Booking ini belum punya bukti pembayaran.");

        if (payment.Status != PaymentStatus.Pending)
            return new PaymentResult(false, "Pembayaran ini sudah diverifikasi sebelumnya.");

        var booking = payment.Booking;
        if (dto.Status == PaymentStatus.Verified)
        {
            if (!BookingStatusFlow.CanTransition(booking.Status, BookingStatus.Approved))
                return new PaymentResult(false, $"Booking berstatus {booking.Status}, tidak bisa di-approve.");

            payment.Status = PaymentStatus.Verified;
            payment.VerifiedAt = DateTime.UtcNow;
            booking.Status = BookingStatus.Approved;
        }
        else
        {
            // Booking tetap Pending, user bisa upload ulang bukti yang benar.
            payment.Status = PaymentStatus.Rejected;
            payment.RejectionReason = dto.RejectionReason;
        }

        // Payment & Booking udah di-track DbContext yang sama, jadi cukup 1x SaveChanges = 1 transaksi.
        await paymentRepository.SaveChangesAsync();

        return new PaymentResult(true);
    }

    public async Task<PaymentProofFile?> GetProofAsync(int bookingId, int userId, bool isAdmin)
    {
        var payment = await paymentRepository.GetByBookingIdAsync(bookingId);
        if (payment is null || (payment.Booking.UserId != userId && !isAdmin))
            return null;

        var fullPath = Path.Combine(UploadFolder, payment.PaymentProofPath);
        if (!File.Exists(fullPath))
            return null;

        var extension = Path.GetExtension(fullPath).ToLowerInvariant();
        var contentType = AllowedFileTypes.TryGetValue(extension, out var fileType)
            ? fileType.ContentType
            : "application/octet-stream";

        return new PaymentProofFile(fullPath, contentType);
    }

    private static async Task<bool> HasValidSignatureAsync(IFormFile file, byte[][] signatures)
    {
        var maxLength = signatures.Max(s => s.Length);
        var header = new byte[maxLength];

        await using var stream = file.OpenReadStream();
        var read = await stream.ReadAtLeastAsync(header, maxLength, throwOnEndOfStream: false);

        return signatures.Any(signature =>
            read >= signature.Length && header.AsSpan(0, signature.Length).SequenceEqual(signature));
    }
}
