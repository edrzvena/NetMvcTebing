using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Tebing.Common.Enums;
using Tebing.Common.Extensions;
using Tebing.DTOs.Booking;
using Tebing.Services.Interfaces;
using Tebing.ViewModels.Booking;

namespace Tebing.Controllers;

[Authorize]
public class BookingController(IBookingService bookingService, IMountainService mountainService) : Controller
{
    private int CurrentUserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> Create(int mountainId)
    {
        var model = new BookingCreateViewModel
        {
            Booking = new BookingCreateDto { MountainId = mountainId, Participants = [new ParticipantDto()] }
        };

        if (!await PopulateCreateViewModelAsync(model))
            return NotFound();

        if (model.Routes.Count == 0)
        {
            TempData["ErrorMessage"] = "Gunung ini sedang tidak menerima booking.";
            return RedirectToAction("Detail", "Mountain", new { id = mountainId });
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookingCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCreateViewModelAsync(model);
            return View(model);
        }

        var result = await bookingService.CreateAsync(CurrentUserId, model.Booking);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Booking gagal dibuat.");
            await PopulateCreateViewModelAsync(model);
            return View(model);
        }

        TempData["SuccessMessage"] = "Booking dibuat. Ikuti langkah berikutnya di bawah biar booking kamu bisa diproses.";
        return RedirectToAction(nameof(Detail), new { id = result.BookingId });
    }

    [HttpGet]
    public async Task<IActionResult> MyBookings()
    {
        var bookings = await bookingService.GetByUserAsync(CurrentUserId);
        return View(bookings);
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        var booking = await bookingService.GetByIdAsync(id);

        // NotFound (bukan Forbid) buat booking orang lain, biar gak bocor info kalau ID-nya ada.
        if (booking is null || (booking.UserId != CurrentUserId && !User.IsInRole("Admin")))
            return NotFound();

        return View(booking);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var result = await bookingService.CancelByUserAsync(id, CurrentUserId);
        if (result.Success)
            TempData["SuccessMessage"] = "Booking dibatalkan.";
        else
            TempData["ErrorMessage"] = result.Error;

        return RedirectToAction(nameof(Detail), new { id });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Manage(int? mountainId, BookingStatus? status, DateOnly? climbDate)
    {
        ViewData["MountainId"] = mountainId;
        ViewData["Status"] = status;
        ViewData["ClimbDate"] = climbDate;
        ViewData["MountainOptions"] = (await mountainService.GetAllAsync())
            .Select(m => new SelectListItem(m.Name, m.Id.ToString(), m.Id == mountainId))
            .ToList();

        // Hitungan per status ngikut filter gunung & tanggal, tapi gak ngikut filter status (buat angka di tab).
        ViewData["StatusCounts"] = await bookingService.GetStatusCountsAsync(mountainId, climbDate);

        var bookings = await bookingService.GetAllForAdminAsync(mountainId, status, climbDate);
        return View(bookings);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(BookingStatusUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Data perubahan status tidak valid.";
            return RedirectToAction(nameof(Detail), new { id = dto.BookingId });
        }

        var result = await bookingService.UpdateStatusAsync(dto);
        if (result.Success)
            TempData["SuccessMessage"] = $"Status booking diubah jadi {dto.Status.ToLabel()}.";
        else
            TempData["ErrorMessage"] = result.Error;

        return RedirectToAction(nameof(Detail), new { id = dto.BookingId });
    }

    // Isi data tampilan (nama gunung & dropdown jalur aktif). False kalau gunungnya gak ada.
    private async Task<bool> PopulateCreateViewModelAsync(BookingCreateViewModel model)
    {
        var mountain = await mountainService.GetByIdAsync(model.Booking.MountainId);
        if (mountain is null)
            return false;

        model.MountainName = mountain.Name;
        model.Routes = mountain.Status == MountainStatus.Open
            ? mountain.Routes
                .Where(r => r.IsActive)
                .Select(r => new RouteOption(r.Id, r.Name, r.PricePerPerson, r.DailyQuota))
                .ToList()
            : [];

        if (model.Booking.Participants.Count == 0)
            model.Booking.Participants.Add(new ParticipantDto());

        return true;
    }
}
