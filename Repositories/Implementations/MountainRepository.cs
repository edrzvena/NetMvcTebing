using Microsoft.EntityFrameworkCore;
using Tebing.Common.Enums;
using Tebing.Data;
using Tebing.Models;
using Tebing.Repositories.Interfaces;

namespace Tebing.Repositories.Implementations;

public class MountainRepository(AppDbContext context) : Repository<Mountain>(context), IMountainRepository
{
    public async Task<List<Mountain>> GetFilteredAsync(string? search, string? province, DifficultyLevel? difficulty)
    {
        var query = DbSet.Include(m => m.Routes).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();
            query = query.Where(m => m.Name.Contains(keyword) || m.Province.Contains(keyword));
        }

        if (!string.IsNullOrWhiteSpace(province))
            query = query.Where(m => m.Province == province);

        if (difficulty is not null)
            query = query.Where(m => m.Difficulty == difficulty);

        return await query.OrderBy(m => m.Name).ToListAsync();
    }

    public async Task<Mountain?> GetByIdWithRoutesAsync(int id) =>
        await DbSet.Include(m => m.Routes).FirstOrDefaultAsync(m => m.Id == id);

    public async Task<List<string>> GetProvincesAsync() =>
        await DbSet.Select(m => m.Province).Distinct().OrderBy(p => p).ToListAsync();
}
