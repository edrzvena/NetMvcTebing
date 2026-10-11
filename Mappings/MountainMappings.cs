using Tebing.DTOs.Mountain;
using Tebing.Models;

namespace Tebing.Mappings;

public static class MountainMappings
{
    public static MountainListDto ToListDto(this Mountain mountain) => new()
    {
        Id = mountain.Id,
        Name = mountain.Name,
        Province = mountain.Province,
        HeightMeters = mountain.HeightMeters,
        Difficulty = mountain.Difficulty,
        Status = mountain.Status,
        ImagePath = mountain.ImagePath,
        // Butuh Mountain.Routes di-Include.
        RouteCount = mountain.Routes.Count(r => r.IsActive),
        MinPricePerPerson = mountain.Routes.Where(r => r.IsActive).Select(r => (decimal?)r.PricePerPerson).Min()
    };

    public static MountainDetailDto ToDetailDto(this Mountain mountain) => new()
    {
        Id = mountain.Id,
        Name = mountain.Name,
        Province = mountain.Province,
        HeightMeters = mountain.HeightMeters,
        Difficulty = mountain.Difficulty,
        Status = mountain.Status,
        Description = mountain.Description,
        ImagePath = mountain.ImagePath,
        ImageCredit = mountain.ImageCredit,
        ImageSourceUrl = mountain.ImageSourceUrl,
        Routes = mountain.Routes.Select(r => r.ToDto()).ToList()
    };

    public static RouteDto ToDto(this HikingRoute route) => new()
    {
        Id = route.Id,
        MountainId = route.MountainId,
        Name = route.Name,
        DistanceKm = route.DistanceKm,
        EstimatedDuration = route.EstimatedDuration,
        DailyQuota = route.DailyQuota,
        PricePerPerson = route.PricePerPerson,
        IsActive = route.IsActive
    };
}
