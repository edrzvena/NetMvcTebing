using Tebing.DTOs.Payment;

namespace Tebing.Services.Interfaces;

public record PaymentResult(bool Success, string? Error = null);

public record PaymentProofFile(string FullPath, string ContentType);

public interface IPaymentService
{
    Task<PaymentResult> UploadAsync(int userId, PaymentUploadDto dto);
    Task<PaymentResult> VerifyAsync(PaymentVerifyDto dto);

    // Null kalau payment gak ada atau yang minta bukan pemilik booking/admin.
    Task<PaymentProofFile?> GetProofAsync(int bookingId, int userId, bool isAdmin);
}
