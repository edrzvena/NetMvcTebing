using Microsoft.AspNetCore.Mvc;
using Tebing.DTOs.Auth;
using Tebing.Services.Interfaces;

namespace Tebing.Controllers;

public class AccountController(IAuthService authService) : Controller
{
    [HttpGet]
    public IActionResult Register() => View(new RegisterDto());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var result = await authService.RegisterAsync(dto);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Registrasi gagal.");
            return View(dto);
        }

        TempData["SuccessMessage"] = "Registrasi berhasil, silakan login.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto dto, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid)
            return View(dto);

        var result = await authService.LoginAsync(dto);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Login gagal.");
            return View(dto);
        }

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await authService.LogoutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult ForgotPassword() => View(new ForgotPasswordDto());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var token = await authService.GenerateResetTokenAsync(dto.Email);

        // Belum ada pengiriman email (lihat Tahap 7 di IMPLEMENTATION_PLAN.md), jadi buat
        // kebutuhan development/testing, link reset ditampilkan langsung di halaman konfirmasi.
        ViewBag.ResetToken = token;
        return View("ForgotPasswordConfirmation");
    }

    [HttpGet]
    public IActionResult ResetPassword(string token) => View(new ResetPasswordDto { Token = token });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
    {
        if (!ModelState.IsValid)
            return View(dto);

        var result = await authService.ResetPasswordAsync(dto);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Error ?? "Reset password gagal.");
            return View(dto);
        }

        TempData["SuccessMessage"] = "Password berhasil diubah, silakan login.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();
}
