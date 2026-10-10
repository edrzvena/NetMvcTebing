using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using Tebing.DTOs.Booking;

namespace Tebing.ViewModels.Booking;

public class BookingCreateViewModel
{
    public BookingCreateDto Booking { get; set; } = new();

    // Data tambahan khusus tampilan — gak ikut dikirim ke Service.
    [ValidateNever] public string MountainName { get; set; } = string.Empty;
    [ValidateNever] public List<SelectListItem> RouteOptions { get; set; } = [];
}
