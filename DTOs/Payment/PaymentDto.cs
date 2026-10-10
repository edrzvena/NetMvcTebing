using Tebing.Common.Enums;

namespace Tebing.DTOs.Payment;

public class PaymentDto
{
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime UploadedAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
}
