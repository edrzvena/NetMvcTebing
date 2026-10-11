using Tebing.Common.Enums;

namespace Tebing.Common.Extensions;

// Satu-satunya tempat mapping enum → label Bahasa Indonesia. View & pesan TempData jangan nampilin
// nama enum mentah (Pending, Hard, ...), selalu lewat ToLabel().
public static class EnumDisplayExtensions
{
    public static string ToLabel(this BookingStatus status) => status switch
    {
        BookingStatus.Pending => "Menunggu",
        BookingStatus.Approved => "Disetujui",
        BookingStatus.Completed => "Selesai",
        BookingStatus.Rejected => "Ditolak",
        BookingStatus.Cancelled => "Dibatalkan",
        _ => status.ToString()
    };

    public static string ToLabel(this PaymentStatus status) => status switch
    {
        PaymentStatus.Pending => "Menunggu verifikasi",
        PaymentStatus.Verified => "Terverifikasi",
        PaymentStatus.Rejected => "Ditolak",
        _ => status.ToString()
    };

    public static string ToLabel(this DifficultyLevel level) => level switch
    {
        DifficultyLevel.Easy => "Mudah",
        DifficultyLevel.Medium => "Sedang",
        DifficultyLevel.Hard => "Sulit",
        _ => level.ToString()
    };

    public static string ToLabel(this MountainStatus status) => status switch
    {
        MountainStatus.Open => "Buka",
        MountainStatus.Closed => "Ditutup",
        _ => status.ToString()
    };
}
