using Tebing.Common.Enums;
using Tebing.DTOs.Mountain;

namespace Tebing.Services.Interfaces;

public interface IMountainService
{
    Task<List<MountainListDto>> GetAllAsync(string? province = null, DifficultyLevel? difficulty = null);
    Task<MountainDetailDto?> GetByIdAsync(int id);
    Task<int> CreateAsync(MountainCreateDto dto);
    Task<bool> UpdateAsync(MountainUpdateDto dto);
    Task<bool> CloseAsync(int id);

    Task<RouteDto?> GetRouteByIdAsync(int routeId);
    Task<RouteDto?> AddRouteAsync(RouteCreateDto dto);
    Task<bool> UpdateRouteAsync(RouteCreateDto dto);
    Task<bool> DeactivateRouteAsync(int routeId);
}
