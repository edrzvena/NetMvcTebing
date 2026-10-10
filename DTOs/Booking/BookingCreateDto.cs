using System.ComponentModel.DataAnnotations;

namespace Tebing.DTOs.Booking;

public class BookingCreateDto
{
    public int MountainId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Pilih jalur pendakian.")]
    [Display(Name = "Jalur Pendakian")]
    public int HikingRouteId { get; set; }

    [Required, Display(Name = "Tanggal Naik")]
    public DateOnly? ClimbDate { get; set; }

    // Jumlah item = jumlah orang (NumberOfPeople dihitung dari sini, gak diinput terpisah).
    [MinLength(1, ErrorMessage = "Minimal 1 peserta.")]
    public List<ParticipantDto> Participants { get; set; } = [];
}
