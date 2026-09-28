using Tebing.Common.Enums;
using Tebing.Models;

namespace Tebing.Repositories.Interfaces;

public interface IMountainRepository : IRepository<Mountain>
{
    Task<List<Mountain>> GetFilteredAsync(string? province, DifficultyLevel? difficulty);
    Task<Mountain?> GetByIdWithRoutesAsync(int id);
}
