using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tebing.Common.Enums;

namespace Tebing.Models;

public class Payment
{
    [Key] public int Id { get; set; }

    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    // Diambil dari Booking.TotalPrice pas upload, bukan dari input user.
    [Column(TypeName = "decimal(14,2)")] public decimal Amount { get; set; }

    // Nama file hasil generate (GUID + ekstensi) di folder upload privat, bukan URL publik —
    // bukti transfer cuma bisa dibuka lewat PaymentController.Proof (cek pemilik/admin).
    [Required, MaxLength(100)] public string PaymentProofPath { get; set; } = string.Empty;

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    [MaxLength(500)] public string? RejectionReason { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public DateTime? VerifiedAt { get; set; }
}
