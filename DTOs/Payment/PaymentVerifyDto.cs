using System.ComponentModel.DataAnnotations;
using Tebing.Common.Enums;

namespace Tebing.DTOs.Payment;

public class PaymentVerifyDto
{
    public int BookingId { get; set; }

    // Cuma Verified atau Rejected yang valid, dicek di PaymentService.
    public PaymentStatus Status { get; set; }

    [MaxLength(500)] public string? RejectionReason { get; set; }
}
