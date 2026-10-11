using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Tebing.Common.Feeds;
using Tebing.Services.Interfaces;

namespace Tebing.ViewComponents;

// Strip angka di footer ("46 gunung · 5 wilayah · 62 jalur"). Footer muncul di tiap halaman, jadi hasilnya
// di-cache 10 menit — angka segini gak perlu real-time, dan gak perlu query semua gunung tiap request.
public class FooterStatsViewComponent(IMountainService mountainService, IMemoryCache cache) : ViewComponent
{
    private const string CacheKey = "footer-stats";

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var stats = await cache.GetOrCreateAsync(CacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
            return mountainService.GetSiteStatsAsync();
        });
        return View(stats!);
    }
}
