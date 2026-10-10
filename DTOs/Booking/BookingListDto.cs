using Tebing.Common.Enums;

namespace Tebing.DTOs.Booking;

public class BookingListDto
{
    public int Id { get; set; }
    public string MountainName { get; set; } = string.Empty;
    public string RouteName { get; set; } = string.Empty;
    public DateOnly ClimbDate { get; set; }
    public int NumberOfPeople { get; set; }
    public decimal TotalPrice { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    // Cuma diisi di daftar booking versi admin (biar tau siapa yang booking).
    public string? UserFullName { get; set; }
}
