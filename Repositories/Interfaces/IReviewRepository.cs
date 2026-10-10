using Tebing.Models;

namespace Tebing.Repositories.Interfaces;

public record RatingSummary(double AverageRating, int ReviewCount);

public interface IReviewRepository : IRepository<Review>
{
    Task<List<Review>> GetByMountainIdAsync(int mountainId);
    Task<bool> ExistsForBookingAsync(int bookingId);

    // Rata-rata & jumlah review per gunung dalam 1 query (buat halaman daftar gunung, hindari N+1).
    // Gunung yang belum punya review gak ada di dictionary.
    Task<Dictionary<int, RatingSummary>> GetRatingSummariesAsync(IEnumerable<int> mountainIds);
}
