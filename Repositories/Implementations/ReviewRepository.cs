using Microsoft.EntityFrameworkCore;
using Tebing.Data;
using Tebing.Models;
using Tebing.Repositories.Interfaces;

namespace Tebing.Repositories.Implementations;

public class ReviewRepository(AppDbContext context) : Repository<Review>(context), IReviewRepository
{
    public async Task<List<Review>> GetByMountainIdAsync(int mountainId) =>
        await DbSet
            .Include(r => r.User)
            .Where(r => r.MountainId == mountainId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

    public async Task<bool> ExistsForBookingAsync(int bookingId) =>
        await DbSet.AnyAsync(r => r.BookingId == bookingId);

    public async Task<Dictionary<int, RatingSummary>> GetRatingSummariesAsync(IEnumerable<int> mountainIds)
    {
        var ids = mountainIds.ToList();
        return await DbSet
            .Where(r => ids.Contains(r.MountainId))
            .GroupBy(r => r.MountainId)
            .Select(g => new { MountainId = g.Key, Average = g.Average(r => (double)r.Rating), Count = g.Count() })
            .ToDictionaryAsync(x => x.MountainId, x => new RatingSummary(x.Average, x.Count));
    }
}
