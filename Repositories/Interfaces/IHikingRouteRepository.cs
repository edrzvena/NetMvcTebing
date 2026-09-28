using Tebing.Models;

namespace Tebing.Repositories.Interfaces;

public interface IHikingRouteRepository : IRepository<HikingRoute>
{
    Task<List<HikingRoute>> GetActiveRoutesByMountainIdAsync(int mountainId);
}
