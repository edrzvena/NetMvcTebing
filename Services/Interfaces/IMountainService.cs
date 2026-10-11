using Tebing.Common.Enums;
using Tebing.DTOs.Mountain;

namespace Tebing.Services.Interfaces;

// Error = pesan buat ditampilin di form (misal foto gak valid). NotFound = gunungnya gak ada.
public record MountainResult(bool Success, string? Error = null, int? MountainId = null, bool NotFound = false);

public interface IMountainService
{
    Task<List<MountainListDto>> GetAllAsync(string? search = null, string? province = null,
        DifficultyLevel? difficulty = null);

    // Buat halaman Home: gunung Open dengan rating tertinggi, sisanya diisi gunung Open lain
    // kalau yang punya rating kurang dari `count`.
    Task<List<MountainListDto>> GetTopRatedAsync(int count);

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
