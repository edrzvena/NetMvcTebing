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

    // Null = belum upload bukti transfer.
    public PaymentStatus? PaymentStatus { get; set; }

    // Cuma diisi di daftar booking versi admin (biar tau siapa yang booking).
    public string? UserFullName { get; set; }

    // Masih nunggu pembayaran dari pendaki: booking berbayar yang Pending dan belum upload / buktinya ditolak.
    public bool NeedsPayment => Status == BookingStatus.Pending && TotalPrice > 0 &&
                                PaymentStatus is null or Common.Enums.PaymentStatus.Rejected;
}
