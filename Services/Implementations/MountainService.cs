using Tebing.Common.Enums;
using Tebing.DTOs.Mountain;
using Tebing.Mappings;
using Tebing.Models;
using Tebing.Repositories.Interfaces;
using Tebing.Services.Interfaces;

namespace Tebing.Services.Implementations;

public class MountainService(IMountainRepository mountainRepository, IHikingRouteRepository routeRepository)
    : IMountainService
{
    public async Task<List<MountainListDto>> GetAllAsync(string? province = null, DifficultyLevel? difficulty = null)
    {
        var mountains = await mountainRepository.GetFilteredAsync(province, difficulty);
        return mountains.Select(m => m.ToListDto()).ToList();
    }

    public async Task<MountainDetailDto?> GetByIdAsync(int id)
    {
        var mountain = await mountainRepository.GetByIdWithRoutesAsync(id);
        return mountain?.ToDetailDto();
    }

    public async Task<int> CreateAsync(MountainCreateDto dto)
    {
        var mountain = new Mountain
        {
            Name = dto.Name,
            Province = dto.Province,
            HeightMeters = dto.HeightMeters,
            Difficulty = dto.Difficulty,
            Status = dto.Status,
            Description = dto.Description,
            ImageUrl = dto.ImageUrl
        };

        await mountainRepository.AddAsync(mountain);
        await mountainRepository.SaveChangesAsync();

        return mountain.Id;
    }

    public async Task<bool> UpdateAsync(MountainUpdateDto dto)
    {
        var mountain = await mountainRepository.GetByIdAsync(dto.Id);
        if (mountain is null)
            return false;

        mountain.Name = dto.Name;
        mountain.Province = dto.Province;
        mountain.HeightMeters = dto.HeightMeters;
        mountain.Difficulty = dto.Difficulty;
        mountain.Status = dto.Status;
        mountain.Description = dto.Description;
        mountain.ImageUrl = dto.ImageUrl;

        mountainRepository.Update(mountain);
        await mountainRepository.SaveChangesAsync();

        return true;
    }

    // Ditutup (Status = Closed), bukan dihapus dari database — jaga integritas relasi ke
    // HikingRoute/Booking/Review yang bakal nempel ke Mountain ini di tahap-tahap berikutnya.
    public async Task<bool> CloseAsync(int id)
    {
        var mountain = await mountainRepository.GetByIdAsync(id);
        if (mountain is null)
            return false;

        mountain.Status = MountainStatus.Closed;
        mountainRepository.Update(mountain);
        await mountainRepository.SaveChangesAsync();

        return true;
    }

    public async Task<RouteDto?> GetRouteByIdAsync(int routeId)
    {
        var route = await routeRepository.GetByIdAsync(routeId);
        return route?.ToDto();
    }

    public async Task<RouteDto?> AddRouteAsync(RouteCreateDto dto)
    {
        var mountain = await mountainRepository.GetByIdAsync(dto.MountainId);
        if (mountain is null)
            return null;

        var route = new HikingRoute
        {
            MountainId = dto.MountainId,
            Name = dto.Name,
            DistanceKm = dto.DistanceKm,
            EstimatedDuration = dto.EstimatedDuration,
            DailyQuota = dto.DailyQuota,
            PricePerPerson = dto.PricePerPerson
        };

        await routeRepository.AddAsync(route);
        await routeRepository.SaveChangesAsync();

        return route.ToDto();
    }

    public async Task<bool> UpdateRouteAsync(RouteCreateDto dto)
    {
        var route = await routeRepository.GetByIdAsync(dto.Id);
        if (route is null)
            return false;

        route.Name = dto.Name;
        route.DistanceKm = dto.DistanceKm;
        route.EstimatedDuration = dto.EstimatedDuration;
        route.DailyQuota = dto.DailyQuota;
        route.PricePerPerson = dto.PricePerPerson;

        routeRepository.Update(route);
        await routeRepository.SaveChangesAsync();

        return true;
    }

    // Soft-delete via IsActive, bukan hard delete — jalur yang udah pernah dibooking harus
    // tetap ada di database biar histori booking lama gak kehilangan referensi.
    public async Task<bool> DeactivateRouteAsync(int routeId)
    {
        var route = await routeRepository.GetByIdAsync(routeId);
        if (route is null)
            return false;

        route.IsActive = false;
        routeRepository.Update(route);
        await routeRepository.SaveChangesAsync();

        return true;
    }
}
