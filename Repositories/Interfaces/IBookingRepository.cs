using Tebing.Common.Enums;
using Tebing.Models;

namespace Tebing.Repositories.Interfaces;

public interface IBookingRepository : IRepository<Booking>
{
    Task<Booking?> GetByIdWithDetailsAsync(int id);
    Task<List<Booking>> GetByUserIdAsync(int userId);
    Task<List<Booking>> GetFilteredAsync(int? mountainId, BookingStatus? status, DateOnly? climbDate);
    Task<Dictionary<BookingStatus, int>> GetStatusCountsAsync(int? mountainId, DateOnly? climbDate);

    // Total orang yang udah "makan" kuota di jalur & tanggal tsb (booking Rejected/Cancelled gak dihitung).
    Task<int> GetBookedPeopleCountAsync(int hikingRouteId, DateOnly climbDate);
}
