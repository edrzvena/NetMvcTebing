using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tebing.Common.Enums;
using Tebing.DTOs.Mountain;
using Tebing.Services.Interfaces;

namespace Tebing.Controllers;

public class MountainController(IMountainService mountainService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(string? province, DifficultyLevel? difficulty)
    {
        ViewData["Province"] = province;
        ViewData["Difficulty"] = difficulty;

        var mountains = await mountainService.GetAllAsync(province, difficulty);
        return View(mountains);
    }

    [HttpGet]
    public async Task<IActionResult> Detail(int id)
    {
        var mountain = await mountainService.GetByIdAsync(id);
        if (mountain is null)
            return NotFound();

        return View(mountain);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult Create() => View(new MountainCreateDto());

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MountainCreateDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var id = await mountainService.CreateAsync(dto);
        TempData["SuccessMessage"] = "Data gunung berhasil ditambahkan.";
        return RedirectToAction(nameof(Detail), new { id });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var mountain = await mountainService.GetByIdAsync(id);
        if (mountain is null)
            return NotFound();

        return View(new MountainUpdateDto
        {
            Id = mountain.Id,
            Name = mountain.Name,
            Province = mountain.Province,
            HeightMeters = mountain.HeightMeters,
            Difficulty = mountain.Difficulty,
            Status = mountain.Status,
            Description = mountain.Description,
            ImageUrl = mountain.ImageUrl
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(MountainUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var success = await mountainService.UpdateAsync(dto);
        if (!success)
            return NotFound();

        TempData["SuccessMessage"] = "Data gunung berhasil diperbarui.";
        return RedirectToAction(nameof(Detail), new { id = dto.Id });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(int id)
    {
        var success = await mountainService.CloseAsync(id);
        if (!success)
            return NotFound();

        TempData["SuccessMessage"] = "Gunung ditutup dari daftar aktif.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult AddRoute(int mountainId) => View(new RouteCreateDto { MountainId = mountainId });

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddRoute(RouteCreateDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var route = await mountainService.AddRouteAsync(dto);
        if (route is null)
            return NotFound();

        TempData["SuccessMessage"] = "Jalur pendakian berhasil ditambahkan.";
        return RedirectToAction(nameof(Detail), new { id = dto.MountainId });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> EditRoute(int id)
    {
        var route = await mountainService.GetRouteByIdAsync(id);
        if (route is null)
            return NotFound();

        return View(new RouteCreateDto
        {
            Id = route.Id,
            MountainId = route.MountainId,
            Name = route.Name,
            DistanceKm = route.DistanceKm,
            EstimatedDuration = route.EstimatedDuration,
            DailyQuota = route.DailyQuota,
            PricePerPerson = route.PricePerPerson
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditRoute(RouteCreateDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var success = await mountainService.UpdateRouteAsync(dto);
        if (!success)
            return NotFound();

        TempData["SuccessMessage"] = "Jalur pendakian berhasil diperbarui.";
        return RedirectToAction(nameof(Detail), new { id = dto.MountainId });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeactivateRoute(int id, int mountainId)
    {
        var success = await mountainService.DeactivateRouteAsync(id);
        if (!success)
            return NotFound();

        TempData["SuccessMessage"] = "Jalur pendakian dinonaktifkan.";
        return RedirectToAction(nameof(Detail), new { id = mountainId });
    }
}
