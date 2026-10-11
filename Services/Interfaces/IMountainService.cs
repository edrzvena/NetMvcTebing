using Tebing.Common.Enums;
using Tebing.Common.Feeds;
using Tebing.Common.Regions;
using Tebing.DTOs.Mountain;

namespace Tebing.Services.Interfaces;

// Error = pesan buat ditampilin di form (misal foto gak valid). NotFound = gunungnya gak ada.
public record MountainResult(bool Success, string? Error = null, int? MountainId = null, bool NotFound = false);

public interface IMountainService
{
    // Filter pulau dijalanin setelah query (pulau diturunin dari teks provinsi, gak ada di database).
    Task<List<MountainListDto>> GetAllAsync(string? search = null, Island? island = null,
        DifficultyLevel? difficulty = null);

    // Jumlah gunung per pulau dari semua gunung (tanpa filter), buat chip pulau.
    Task<List<IslandCount>> GetIslandSummaryAsync();

    // Banner + 3 rail + chip di Home (aturannya di MountainFeed).
    Task<HomeFeed> GetHomeFeedAsync();

    Task<List<MountainListDto>> GetSameIslandAsync(int mountainId, Island? island, int count);

    // Angka ringkas buat footer (jumlah gunung, wilayah, jalur aktif).
    Task<SiteStats> GetSiteStatsAsync();

    // Foto acak buat panel kiri halaman akun. Null = belum ada gunung Open yang punya foto.
    Task<MountainListDto?> GetRandomPhotoAsync();

    Task<List<string>> GetProvincesAsync();
    Task<MountainDetailDto?> GetByIdAsync(int id);
    Task<MountainResult> CreateAsync(MountainCreateDto dto);
    Task<MountainResult> UpdateAsync(MountainUpdateDto dto);
    Task<bool> CloseAsync(int id);

    Task<RouteDto?> GetRouteByIdAsync(int routeId);
    Task<RouteDto?> AddRouteAsync(RouteCreateDto dto);
    Task<bool> UpdateRouteAsync(RouteCreateDto dto);
    Task<bool> DeactivateRouteAsync(int routeId);
}
