using System.ComponentModel.DataAnnotations;

namespace Tebing.Models;

public class BookingParticipant
{
    [Key] public int Id { get; set; }

    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;

    [Required, MaxLength(100)] public string FullName { get; set; } = string.Empty;
    [Required, MaxLength(16)] public string NIK { get; set; } = string.Empty;
    [Required, MaxLength(20)] public string PhoneNumber { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string EmergencyContactName { get; set; } = string.Empty;
    [Required, MaxLength(20)] public string EmergencyContactPhone { get; set; } = string.Empty;
}
