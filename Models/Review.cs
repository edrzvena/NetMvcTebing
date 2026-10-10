using System.ComponentModel.DataAnnotations;

namespace Tebing.Models;

public class Review
{
    [Key] public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int MountainId { get; set; }
    public Mountain Mountain { get; set; } = null!;

    // Bukti pernah booking & Completed. Unique — 1 booking cuma boleh 1 review.
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    [Range(1, 5)] public int Rating { get; set; }
    [MaxLength(1000)] public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
