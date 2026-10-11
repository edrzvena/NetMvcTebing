using Microsoft.AspNetCore.Html;
using Tebing.Common.Enums;

namespace Tebing.Common.Extensions;

// Badge status yang sama di semua halaman (docs/UI_UX.md §4.5). Warna = arti status:
// warning = nunggu, success = beres, primary = selesai, danger = ditolak, secondary = netral/berhenti.
public static class StatusBadgeExtensions
{
    public static IHtmlContent ToBadge(this BookingStatus status) => Badge(status.ToLabel(), status switch
    {
        BookingStatus.Pending => "warning",
        BookingStatus.Approved => "success",
        BookingStatus.Completed => "primary",
        BookingStatus.Rejected => "danger",
        _ => "secondary"
    });

    // Null = belum upload bukti transfer. Booking gratis gak butuh pembayaran sama sekali.
    public static IHtmlContent ToPaymentBadge(this PaymentStatus? status, decimal totalPrice)
    {
        if (totalPrice <= 0)
            return Badge("Gratis", "secondary");

        return status switch
        {
            null => Badge("Belum bayar", "secondary"),
            PaymentStatus.Pending => Badge(PaymentStatus.Pending.ToLabel(), "warning"),
            PaymentStatus.Verified => Badge(PaymentStatus.Verified.ToLabel(), "success"),
            _ => Badge(PaymentStatus.Rejected.ToLabel(), "danger")
        };
    }

    // Open = kondisi normal, gak perlu badge.
    public static IHtmlContent ToBadge(this MountainStatus status) =>
        status == MountainStatus.Closed ? Badge(status.ToLabel(), "danger") : HtmlString.Empty;

    // Sengaja netral (bukan hijau/kuning/merah) biar gak bentrok sama arti warna status.
    public static IHtmlContent ToBadge(this DifficultyLevel level) =>
        new HtmlString($"<span class=\"badge border fw-medium bg-light text-body\">{level.ToLabel()}</span>");

    public static IHtmlContent InactiveBadge() => Badge("Nonaktif", "secondary");

    // Label selalu berasal dari ToLabel()/string konstan di kelas ini, bukan input user — aman tanpa encode.
    private static HtmlString Badge(string label, string color) =>
        new($"<span class=\"badge border fw-medium bg-{color}-subtle text-{color}-emphasis border-{color}-subtle\">{label}</span>");
}
