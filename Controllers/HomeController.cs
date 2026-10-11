using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Tebing.Models;
using Tebing.Services.Interfaces;

namespace Tebing.Controllers;

public class HomeController(IMountainService mountainService) : Controller
{
    public async Task<IActionResult> Index()
    {
        return View(await mountainService.GetHomeFeedAsync());
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    // Tujuan UseStatusCodePagesWithReExecute di Program.cs, biar 404 dkk gak jadi halaman kosong browser.
    public IActionResult HttpStatus(int code)
    {
        Response.StatusCode = code;
        return View(code);
    }
}
