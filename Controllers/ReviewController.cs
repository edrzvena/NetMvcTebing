using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tebing.DTOs.Review;
using Tebing.Services.Interfaces;

namespace Tebing.Controllers;

// Gak punya halaman sendiri — form review ada di Booking/Detail, daftar review di Mountain/Detail.
[Authorize]
public class ReviewController(IReviewService reviewService) : Controller
{
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ReviewCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Pilih rating 1–5, komentar maksimal 1000 karakter.";
            return RedirectToAction("Detail", "Booking", new { id = dto.BookingId });
        }

        var result = await reviewService.CreateAsync(CurrentUserId, dto);
        if (result.Success)
            TempData["SuccessMessage"] = "Terima kasih, review kamu sudah tersimpan.";
        else
            TempData["ErrorMessage"] = result.Error;

        return RedirectToAction("Detail", "Booking", new { id = dto.BookingId });
    }
}
