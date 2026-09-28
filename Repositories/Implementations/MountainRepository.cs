using Microsoft.EntityFrameworkCore;
using Tebing.Common.Enums;
using Tebing.Data;
using Tebing.Models;
using Tebing.Repositories.Interfaces;

namespace Tebing.Repositories.Implementations;

public class MountainRepository(AppDbContext context) : Repository<Mountain>(context), IMountainRepository
{
    public async Task<List<Mountain>> GetFilteredAsync(string? province, DifficultyLevel? difficulty)
    {
        var query = DbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(province))
            query = query.Where(m => m.Province == province);

        if (difficulty is not null)
            query = query.Where(m => m.Difficulty == difficulty);

        return await query.OrderBy(m => m.Name).ToListAsync();
    }

    public async Task<Mountain?> GetByIdWithRoutesAsync(int id) =>
        await DbSet.Include(m => m.Routes).FirstOrDefaultAsync(m => m.Id == id);
}
