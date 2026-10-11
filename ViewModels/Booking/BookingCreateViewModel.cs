using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Tebing.DTOs.Booking;

namespace Tebing.ViewModels.Booking;

public class BookingCreateViewModel
{
    public BookingCreateDto Booking { get; set; } = new();

    // Data tambahan khusus tampilan — gak ikut dikirim ke Service.
    [ValidateNever] public string MountainName { get; set; } = string.Empty;

    // Jalur aktif buat dropdown. Harga ikut dikirim ke view (data-price) biar ringkasan total bisa
    // di-update live di browser — total final tetap dihitung ulang di server.
    [ValidateNever] public List<RouteOption> Routes { get; set; } = [];
}

public record RouteOption(int Id, string Name, decimal PricePerPerson, int? DailyQuota);
