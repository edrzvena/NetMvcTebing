using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tebing.Common.Enums;

namespace Tebing.Models;

public class Booking
{
    [Key] public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int MountainId { get; set; }
    public Mountain Mountain { get; set; } = null!;

    public int HikingRouteId { get; set; }
    public HikingRoute HikingRoute { get; set; } = null!;

    public DateOnly ClimbDate { get; set; }
    public int NumberOfPeople { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    // Snapshot harga saat booking dibuat — kalau admin ubah harga jalur nanti, booking lama gak ikut berubah.
    [Column(TypeName = "decimal(14,2)")] public decimal TotalPrice { get; set; }

    [MaxLength(500)] public string? RejectionReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<BookingParticipant> Participants { get; set; } = new List<BookingParticipant>();
    public Payment? Payment { get; set; }
    public Review? Review { get; set; }
}
