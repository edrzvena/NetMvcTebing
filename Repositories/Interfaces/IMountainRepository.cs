using Tebing.Common.Enums;
using Tebing.Models;

namespace Tebing.Repositories.Interfaces;

public interface IMountainRepository : IRepository<Mountain>
{
    // Routes ikut di-Include (buat hitung jumlah jalur & harga termurah di kartu gunung).
    // search dicocokkan ke nama atau provinsi.
    Task<List<Mountain>> GetFilteredAsync(string? search, string? province, DifficultyLevel? difficulty);
    Task<Mountain?> GetByIdWithRoutesAsync(int id);
    Task<List<string>> GetProvincesAsync();
}
