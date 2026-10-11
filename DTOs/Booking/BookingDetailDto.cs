using Tebing.Common.Enums;
using Tebing.DTOs.Payment;
using Tebing.DTOs.Review;

namespace Tebing.DTOs.Booking;

public class BookingDetailDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;

    public int MountainId { get; set; }
    public string MountainName { get; set; } = string.Empty;

    // Path dasar foto gunung buat thumbnail (lihat MountainImageProcessor.Url). Null = gunung tanpa foto.
    public string? MountainImagePath { get; set; }
    public string RouteName { get; set; } = string.Empty;

    public DateOnly ClimbDate { get; set; }
    public int NumberOfPeople { get; set; }
    public decimal TotalPrice { get; set; }
    public BookingStatus Status { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<ParticipantDto> Participants { get; set; } = [];

    // Status yang boleh diset admin secara manual dari status sekarang (buat nampilin tombol aksi admin).
    // Approved gak termasuk kalau booking berbayar — itu lewat verifikasi pembayaran.
    public List<BookingStatus> AllowedNextStatuses { get; set; } = [];

    // Null = belum upload bukti transfer.
    public PaymentDto? Payment { get; set; }

    // Null = belum kasih review (cuma bisa setelah Completed).
    public ReviewDto? Review { get; set; }
}
