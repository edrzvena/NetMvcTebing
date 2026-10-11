using Tebing.Common.Enums;
using Tebing.Common.Feeds;
using Tebing.Common.Regions;
using Tebing.DTOs.Mountain;
using Tebing.Mappings;
using Tebing.Models;
using Tebing.Repositories.Interfaces;
using Tebing.Services.Interfaces;

namespace Tebing.Services.Implementations;

public class MountainService(
    IMountainRepository mountainRepository,
    IHikingRouteRepository routeRepository,
    IReviewRepository reviewRepository,
    IMountainImageService imageService) : IMountainService
{
    public async Task<List<MountainListDto>> GetAllAsync(string? search = null, Island? island = null,
        DifficultyLevel? difficulty = null)
    {
        var mountains = await mountainRepository.GetFilteredAsync(search, difficulty);
        var ratings = await reviewRepository.GetRatingSummariesAsync(mountains.Select(m => m.Id));

        return mountains.Select(m =>
        {
            var dto = m.ToListDto();
            if (ratings.TryGetValue(m.Id, out var rating))
            {
                dto.AverageRating = rating.AverageRating;
                dto.ReviewCount = rating.ReviewCount;
            }

            return dto;
        })
            .Where(dto => island is null || dto.Island == island)
            .ToList();
    }

    public async Task<List<IslandCount>> GetIslandSummaryAsync() => MountainFeed.IslandSummary(await GetAllAsync());

    public async Task<HomeFeed> GetHomeFeedAsync() => MountainFeed.BuildHome(await GetAllAsync());

    public async Task<List<MountainListDto>> GetSameIslandAsync(int mountainId, Island? island, int count) =>
        MountainFeed.SameIsland(await GetAllAsync(island: island), mountainId, island, count);

    public async Task<SiteStats> GetSiteStatsAsync() => MountainFeed.SiteStats(await GetAllAsync());

    public async Task<MountainListDto?> GetRandomPhotoAsync() =>
        MountainFeed.PickRandomPhoto(await GetAllAsync(), Random.Shared);

    public Task<List<string>> GetProvincesAsync() => mountainRepository.GetProvincesAsync();

    public async Task<MountainDetailDto?> GetByIdAsync(int id)
    {
        var mountain = await mountainRepository.GetByIdWithRoutesAsync(id);
        if (mountain is null)
            return null;

        var dto = mountain.ToDetailDto();
        var reviews = await reviewRepository.GetByMountainIdAsync(id);
        dto.Reviews = reviews.Select(r => r.ToDto()).ToList();
        dto.AverageRating = reviews.Count > 0 ? reviews.Average(r => r.Rating) : null;
        return dto;
    }

    public async Task<MountainResult> CreateAsync(MountainCreateDto dto)
    {
        string? imagePath = null;
        if (dto.Image is not null)
        {
            var saved = await imageService.SaveAsync(dto.Image);
            if (!saved.Success)
                return new MountainResult(false, saved.Error);
            imagePath = saved.ImagePath;
        }

        var mountain = new Mountain
        {
            Name = dto.Name,
            Province = dto.Province,
            HeightMeters = dto.HeightMeters,
            Difficulty = dto.Difficulty,
            Status = dto.Status,
            Description = dto.Description,
            ImagePath = imagePath,
            ImageCredit = imagePath is null ? null : dto.ImageCredit,
            ImageSourceUrl = imagePath is null ? null : dto.ImageSourceUrl
        };

        await mountainRepository.AddAsync(mountain);
        await mountainRepository.SaveChangesAsync();

        return new MountainResult(true, MountainId: mountain.Id);
    }

    public async Task<MountainResult> UpdateAsync(MountainUpdateDto dto)
    {
        var mountain = await mountainRepository.GetByIdAsync(dto.Id);
        if (mountain is null)
            return new MountainResult(false, NotFound: true);

        // File foto lama baru dihapus setelah data baru tersimpan, biar gak ada gunung yang nunjuk ke file hilang.
        var oldImagePath = mountain.ImagePath;
        if (dto.Image is not null)
        {
            var saved = await imageService.SaveAsync(dto.Image);
            if (!saved.Success)
                return new MountainResult(false, saved.Error);
            mountain.ImagePath = saved.ImagePath;
        }
        else if (dto.RemoveImage)
        {
            mountain.ImagePath = null;
        }

        mountain.ImageCredit = mountain.ImagePath is null ? null : dto.ImageCredit;
        mountain.ImageSourceUrl = mountain.ImagePath is null ? null : dto.ImageSourceUrl;

        mountain.Name = dto.Name;
        mountain.Province = dto.Province;
        mountain.HeightMeters = dto.HeightMeters;
        mountain.Difficulty = dto.Difficulty;
        mountain.Status = dto.Status;
        mountain.Description = dto.Description;

        mountainRepository.Update(mountain);
        await mountainRepository.SaveChangesAsync();

        if (oldImagePath != mountain.ImagePath)
            imageService.Delete(oldImagePath);

        return new MountainResult(true, MountainId: mountain.Id);
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
