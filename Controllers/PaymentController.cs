using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tebing.Common.Enums;
using Tebing.Constants;
using Tebing.DTOs.Payment;
using Tebing.Services.Interfaces;

namespace Tebing.Controllers;

// Gak punya halaman sendiri — upload & verifikasi dilakukan dari halaman Booking/Detail.
[Authorize]
public class PaymentController(IPaymentService paymentService) : Controller
{
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(FileUploadSettings.UploadRequestMaxBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = FileUploadSettings.UploadRequestMaxBytes)]
    public async Task<IActionResult> Upload(PaymentUploadDto dto)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Pilih file bukti transfer (JPG, PNG, atau PDF, maksimal 2 MB).";
            return RedirectToBooking(dto.BookingId);
        }

        var result = await paymentService.UploadAsync(CurrentUserId, dto);
        if (result.Success)
            TempData["SuccessMessage"] = "Bukti transfer berhasil diupload, menunggu verifikasi admin.";
        else
            TempData["ErrorMessage"] = result.Error;

        return RedirectToBooking(dto.BookingId);
    }

    [HttpGet]
    public async Task<IActionResult> Proof(int bookingId)
    {
        var proof = await paymentService.GetProofAsync(bookingId, CurrentUserId, User.IsInRole("Admin"));
        if (proof is null)
            return NotFound();

        return PhysicalFile(proof.FullPath, proof.ContentType);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Verify(PaymentVerifyDto dto)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Data verifikasi tidak valid.";
            return RedirectToBooking(dto.BookingId);
        }

        var result = await paymentService.VerifyAsync(dto);
        if (result.Success)
            TempData["SuccessMessage"] = dto.Status == PaymentStatus.Verified
                ? "Pembayaran diverifikasi, booking otomatis Approved."
                : "Pembayaran ditolak, pendaki bisa upload ulang bukti transfer.";
        else
            TempData["ErrorMessage"] = result.Error;

        return RedirectToBooking(dto.BookingId);
    }

    private IActionResult RedirectToBooking(int bookingId) =>
        RedirectToAction("Detail", "Booking", new { id = bookingId });
}
