using Microsoft.EntityFrameworkCore;
using Tebing.Common.Enums;
using Tebing.Data;
using Tebing.Models;
using Tebing.Repositories.Interfaces;

namespace Tebing.Repositories.Implementations;

public class BookingRepository(AppDbContext context) : Repository<Booking>(context), IBookingRepository
{
    public async Task<Booking?> GetByIdWithDetailsAsync(int id) =>
        await DbSet
            .Include(b => b.User)
            .Include(b => b.Mountain)
            .Include(b => b.HikingRoute)
            .Include(b => b.Participants)
            .Include(b => b.Payment)
            .FirstOrDefaultAsync(b => b.Id == id);

    public async Task<List<Booking>> GetByUserIdAsync(int userId) =>
        await DbSet
            .Include(b => b.Mountain)
            .Include(b => b.HikingRoute)
            .Include(b => b.Payment)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

    public async Task<List<Booking>> GetFilteredAsync(int? mountainId, BookingStatus? status, DateOnly? climbDate)
    {
        var query = DbSet
            .Include(b => b.User)
            .Include(b => b.Mountain)
            .Include(b => b.HikingRoute)
            .Include(b => b.Payment)
            .AsQueryable();

        if (mountainId is not null)
            query = query.Where(b => b.MountainId == mountainId);

        if (status is not null)
            query = query.Where(b => b.Status == status);

        if (climbDate is not null)
            query = query.Where(b => b.ClimbDate == climbDate);

        return await query.OrderBy(b => b.ClimbDate).ThenBy(b => b.CreatedAt).ToListAsync();
    }

    public async Task<int> GetBookedPeopleCountAsync(int hikingRouteId, DateOnly climbDate) =>
        await DbSet
            .Where(b => b.HikingRouteId == hikingRouteId && b.ClimbDate == climbDate &&
                        b.Status != BookingStatus.Rejected && b.Status != BookingStatus.Cancelled)
            .SumAsync(b => b.NumberOfPeople);
}
