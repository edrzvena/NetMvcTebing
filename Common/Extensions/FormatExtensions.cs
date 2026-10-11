using System.Globalization;

namespace Tebing.Common.Extensions;

// Format angka & tanggal buat tampilan: "Rp 35.000", "12 Okt 2026", "12 Okt 2026, 14.30".
// Culture id-ID dipasang eksplisit di sini, bukan di-set global lewat RequestLocalization — kalau
// global, model binding form ikut id-ID (desimal pakai koma) dan bentrok sama validasi client
// jQuery yang cuma ngerti titik, jadi input kayak DistanceKm bisa kebaca salah.
public static class FormatExtensions
{
    private static readonly CultureInfo Indonesia = CultureInfo.GetCultureInfo("id-ID");

    public static string ToRupiah(this decimal amount) => "Rp " + amount.ToString("N0", Indonesia);

    public static string ToNumber(this int value) => value.ToString("N0", Indonesia);

    // Jarak km: "10,5" / "8".
    public static string ToDecimalNumber(this decimal value) => value.ToString("0.##", Indonesia);

    public static string ToRating(this double rating) => rating.ToString("0.0", Indonesia);

    public static string ToDisplayDate(this DateOnly date) => date.ToString("d MMM yyyy", Indonesia);

    // DateTime di database disimpan UTC, jadi dikonversi ke waktu lokal server dulu.
    public static string ToDisplayDate(this DateTime utc) => utc.ToLocalTime().ToString("d MMM yyyy", Indonesia);

    public static string ToDisplayDateTime(this DateTime utc) =>
        utc.ToLocalTime().ToString("d MMM yyyy, HH.mm", Indonesia);
}
