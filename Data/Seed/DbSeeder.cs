using Microsoft.EntityFrameworkCore;
using Tebing.Common.Enums;
using Tebing.Models;

namespace Tebing.Data.Seed;

// Data awal buat development/testing: beberapa gunung & jalur contoh. Akun gak di-seed — pakai akun
// yang udah ada. Cuma dipanggil di environment Development (lihat Program.cs) dan aman dijalanin
// berkali-kali — gunung cuma dibikin kalau tabel Mountains masih kosong.
public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(DbSeeder));

        // Seed sengaja gak jalanin migration sendiri — kalau skema DB belum update, lewati aja daripada crash.
        var pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();
        if (pendingMigrations.Count > 0)
        {
            logger.LogWarning(
                "Seed data dilewati: masih ada {Count} migration pending ({Migrations}). Jalankan `dotnet ef database update` dulu.",
                pendingMigrations.Count, string.Join(", ", pendingMigrations));
            return;
        }

        await SeedMountainsAsync(context, logger);

        await context.SaveChangesAsync();
    }

    private static async Task SeedMountainsAsync(AppDbContext context, ILogger logger)
    {
        if (await context.Mountains.AnyAsync())
            return;

        // Data contoh — ketinggian sesuai data umum, tapi kuota & harga cuma perkiraan buat testing,
        // bukan tarif resmi.
        context.Mountains.AddRange(
            new Mountain
            {
                Name = "Gunung Gede",
                Province = "Jawa Barat",
                HeightMeters = 2958,
                Difficulty = DifficultyLevel.Medium,
                Description = "Bagian dari Taman Nasional Gede Pangrango. Terkenal dengan alun-alun Suryakencana " +
                              "dan padang edelweis-nya.",
                Routes =
                {
                    Route("Via Cibodas", 10.5m, "2 hari 1 malam", 300, 35_000),
                    Route("Via Gunung Putri", 8.5m, "2 hari 1 malam", 200, 35_000),
                    Route("Via Selabintana", 12m, "2 hari 1 malam", 100, 35_000)
                }
            },
            new Mountain
            {
                Name = "Gunung Prau",
                Province = "Jawa Tengah",
                HeightMeters = 2565,
                Difficulty = DifficultyLevel.Easy,
                Description = "Cocok buat pendaki pemula. Golden sunrise dengan latar Sindoro-Sumbing jadi andalannya.",
                Routes =
                {
                    Route("Via Patak Banteng", 4m, "3-4 jam", 500, 25_000),
                    Route("Via Dieng", 5m, "4-5 jam", null, 25_000)
                }
            },
            new Mountain
            {
                Name = "Gunung Merbabu",
                Province = "Jawa Tengah",
                HeightMeters = 3145,
                Difficulty = DifficultyLevel.Medium,
                Description = "Sabana luas dan pemandangan langsung ke Gunung Merapi.",
                Routes =
                {
                    Route("Via Selo", 9m, "2 hari 1 malam", 250, 50_000),
                    Route("Via Suwanting", 8m, "2 hari 1 malam", 150, 50_000)
                }
            },
            new Mountain
            {
                Name = "Gunung Semeru",
                Province = "Jawa Timur",
                HeightMeters = 3676,
                Difficulty = DifficultyLevel.Hard,
                Description = "Puncak tertinggi di Pulau Jawa (Mahameru). Lewat Ranu Kumbolo dan Kalimati.",
                Routes =
                {
                    Route("Via Ranu Pani", 18m, "3 hari 2 malam", 200, 100_000)
                }
            },
            new Mountain
            {
                Name = "Gunung Rinjani",
                Province = "Nusa Tenggara Barat",
                HeightMeters = 3726,
                Difficulty = DifficultyLevel.Hard,
                Description = "Gunung berapi tertinggi kedua di Indonesia, dengan Danau Segara Anak di kalderanya.",
                Routes =
                {
                    Route("Via Sembalun", 20m, "3 hari 2 malam", 150, 150_000),
                    Route("Via Senaru", 17m, "3 hari 2 malam", 100, 150_000)
                }
            },
            // Sengaja ditutup, buat ngetes gunung Closed (tombol booking gak muncul).
            new Mountain
            {
                Name = "Gunung Slamet",
                Province = "Jawa Tengah",
                HeightMeters = 3428,
                Difficulty = DifficultyLevel.Hard,
                Status = MountainStatus.Closed,
                Description = "Sedang ditutup sementara (data contoh untuk status Closed).",
                Routes =
                {
                    Route("Via Bambangan", 11m, "2 hari 1 malam", 200, 30_000)
                }
            });

        logger.LogInformation("Seed: 6 gunung contoh beserta jalurnya dibuat.");
    }

    private static HikingRoute Route(string name, decimal distanceKm, string duration, int? dailyQuota,
        decimal pricePerPerson) => new()
    {
        Name = name,
        DistanceKm = distanceKm,
        EstimatedDuration = duration,
        DailyQuota = dailyQuota,
        PricePerPerson = pricePerPerson
    };
}
