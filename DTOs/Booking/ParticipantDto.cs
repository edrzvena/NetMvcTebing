using System.ComponentModel.DataAnnotations;

namespace Tebing.DTOs.Booking;

public class ParticipantDto
{
    [Required, MaxLength(100), Display(Name = "Nama Lengkap")]
    public string FullName { get; set; } = string.Empty;

    [Required, RegularExpression(@"^\d{16}$", ErrorMessage = "NIK harus 16 digit angka."), Display(Name = "NIK")]
    public string NIK { get; set; } = string.Empty;

    [Required, Phone, MaxLength(20), Display(Name = "No. HP")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required, MaxLength(100), Display(Name = "Nama Kontak Darurat")]
    public string EmergencyContactName { get; set; } = string.Empty;

    [Required, Phone, MaxLength(20), Display(Name = "No. HP Kontak Darurat")]
    public string EmergencyContactPhone { get; set; } = string.Empty;
}
