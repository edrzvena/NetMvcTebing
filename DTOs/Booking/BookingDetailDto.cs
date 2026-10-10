using Tebing.Common.Enums;

namespace Tebing.DTOs.Booking;

public class BookingDetailDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;

    public int MountainId { get; set; }
    public string MountainName { get; set; } = string.Empty;
    public string RouteName { get; set; } = string.Empty;

    public DateOnly ClimbDate { get; set; }
    public int NumberOfPeople { get; set; }
    public decimal TotalPrice { get; set; }
    public BookingStatus Status { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<ParticipantDto> Participants { get; set; } = [];

    // Status tujuan yang valid dari status sekarang (buat nampilin tombol aksi admin).
    public List<BookingStatus> AllowedNextStatuses { get; set; } = [];

    // Info payment nyusul di Tahap 5.
}
