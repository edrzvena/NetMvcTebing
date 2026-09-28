using Microsoft.EntityFrameworkCore;
using Tebing.Data;
using Tebing.Models;
using Tebing.Repositories.Interfaces;

namespace Tebing.Repositories.Implementations;

public class HikingRouteRepository(AppDbContext context) : Repository<HikingRoute>(context), IHikingRouteRepository
{
    public async Task<List<HikingRoute>> GetActiveRoutesByMountainIdAsync(int mountainId) =>
        await DbSet.Where(r => r.MountainId == mountainId && r.IsActive).ToListAsync();
}
