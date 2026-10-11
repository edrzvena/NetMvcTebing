using Microsoft.EntityFrameworkCore;
using Tebing.Common.Enums;
using Tebing.Models;

namespace Tebing.Data.Seed;

// Data awal buat development/testing: gunung & jalur contoh se-Indonesia. Akun gak di-seed — pakai
// akun yang udah ada. Cuma dipanggil di environment Development (lihat Program.cs) dan aman dijalanin
// berkali-kali — gunung dicek per nama, yang belum ada aja yang ditambah. Jadi kalau list di bawah
// ditambah, gunung barunya ikut masuk pas app start berikutnya.
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
        var existingNames = (await context.Mountains.Select(m => m.Name).ToListAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var newMountains = SampleMountains().Where(m => !existingNames.Contains(m.Name)).ToList();
        if (newMountains.Count == 0)
            return;

        context.Mountains.AddRange(newMountains);
        logger.LogInformation("Seed: {Count} gunung contoh beserta jalurnya ditambahkan.", newMountains.Count);
    }

    // Ketinggian sesuai data umum. Kuota, harga, jarak, durasi, & status cuma perkiraan buat testing —
    // bukan tarif/aturan resmi pengelola. Beberapa gunung api aktif sengaja Closed buat ngetes status itu.
    private static IEnumerable<Mountain> SampleMountains() =>
    [
        // ---------- Jawa Barat ----------
        Mountain("Gunung Gede", "Jawa Barat", 2958, DifficultyLevel.Medium,
            "Bagian dari Taman Nasional Gede Pangrango. Terkenal dengan alun-alun Suryakencana dan padang edelweis-nya.",
            Route("Via Cibodas", 10.5m, "2 hari 1 malam", 300, 35_000),
            Route("Via Gunung Putri", 8.5m, "2 hari 1 malam", 200, 35_000),
            Route("Via Selabintana", 12m, "2 hari 1 malam", 100, 35_000)),
        Mountain("Gunung Pangrango", "Jawa Barat", 3019, DifficultyLevel.Medium,
            "Kembaran Gunung Gede dengan Lembah Mandalawangi yang legendaris di dekat puncaknya.",
            Route("Via Cibodas", 11.5m, "2 hari 1 malam", 150, 35_000)),
        Mountain("Gunung Ciremai", "Jawa Barat", 3078, DifficultyLevel.Hard,
            "Gunung tertinggi di Jawa Barat. Jalurnya panjang dan menanjak terus tanpa banyak bonus.",
            Route("Via Apuy", 8m, "2 hari 1 malam", 200, 50_000),
            Route("Via Palutungan", 11m, "2 hari 1 malam", 200, 50_000),
            Route("Via Linggarjati", 10m, "2 hari 1 malam", 100, 50_000)),
        Mountain("Gunung Papandayan", "Jawa Barat", 2665, DifficultyLevel.Easy,
            "Kawah aktif, hutan mati, dan Tegal Alun yang penuh edelweis. Ramah buat pemula dan keluarga.",
            Route("Via Camp David", 6m, "1 hari / 2 hari 1 malam", 800, 35_000)),
        Mountain("Gunung Cikuray", "Jawa Barat", 2821, DifficultyLevel.Hard,
            "Tanjakan tanpa ampun dengan puncak berbentuk kerucut sempurna dan lautan awan di pagi hari.",
            Route("Via Pemancar", 6m, "2 hari 1 malam", 150, 30_000)),
        Mountain("Gunung Guntur", "Jawa Barat", 2249, DifficultyLevel.Medium,
            "Jalur berpasir dan berbatu yang terbuka, cocok buat latihan sebelum naik gunung yang lebih tinggi.",
            Route("Via Citiis", 5m, "2 hari 1 malam", 200, 25_000)),
        Mountain("Gunung Salak", "Jawa Barat", 2211, DifficultyLevel.Hard,
            "Hutan hujan lebat, lembap, dan jalur yang mudah membingungkan. Wajib bareng pemandu atau yang sudah hafal jalur.",
            Route("Via Cimelati", 7m, "2 hari 1 malam", 100, 30_000),
            Route("Via Cidahu", 8m, "2 hari 1 malam", 100, 30_000)),
        Mountain("Gunung Burangrang", "Jawa Barat", 2050, DifficultyLevel.Easy,
            "Sisa kaldera Gunung Sunda purba, puncaknya menghadap langsung ke Situ Lembang.",
            Route("Via Legok Haji", 4m, "4-5 jam", null, 15_000)),
        Mountain("Gunung Manglayang", "Jawa Barat", 1818, DifficultyLevel.Easy,
            "Gunung dekat Bandung yang pas buat tektok setengah hari.",
            Route("Via Batu Kuda", 3m, "3-4 jam", null, 15_000)),

        // ---------- Jawa Tengah ----------
        Mountain("Gunung Prau", "Jawa Tengah", 2565, DifficultyLevel.Easy,
            "Cocok buat pendaki pemula. Golden sunrise dengan latar Sindoro-Sumbing jadi andalannya.",
            Route("Via Patak Banteng", 4m, "3-4 jam", 500, 25_000),
            Route("Via Dieng", 5m, "4-5 jam", null, 25_000)),
        Mountain("Gunung Merbabu", "Jawa Tengah", 3145, DifficultyLevel.Medium,
            "Sabana luas dan pemandangan langsung ke Gunung Merapi.",
            Route("Via Selo", 9m, "2 hari 1 malam", 250, 50_000),
            Route("Via Suwanting", 8m, "2 hari 1 malam", 150, 50_000),
            Route("Via Wekas", 7m, "2 hari 1 malam", 150, 50_000)),
        Mountain("Gunung Sindoro", "Jawa Tengah", 3136, DifficultyLevel.Medium,
            "Kerucut sempurna yang berhadapan dengan Sumbing. Jalurnya terbuka dan berdebu di musim kemarau.",
            Route("Via Kledung", 7m, "2 hari 1 malam", 300, 30_000),
            Route("Via Alang-Alang Sewu", 8m, "2 hari 1 malam", 100, 30_000)),
        Mountain("Gunung Sumbing", "Jawa Tengah", 3371, DifficultyLevel.Hard,
            "Gunung tertinggi ketiga di Jawa. Puncaknya berupa kawah dengan dinding batu yang megah.",
            Route("Via Garung", 8m, "2 hari 1 malam", 250, 30_000),
            Route("Via Butuh", 7m, "2 hari 1 malam", 200, 30_000)),
        Mountain("Gunung Slamet", "Jawa Tengah", 3428, DifficultyLevel.Hard,
            "Gunung tertinggi di Jawa Tengah dan tertinggi kedua di Pulau Jawa. Jalur panjang dengan banyak pos.",
            Route("Via Bambangan", 11m, "2 hari 1 malam", 200, 30_000),
            Route("Via Guci", 12m, "2 hari 1 malam", 100, 30_000)),
        Mountain("Gunung Lawu", "Jawa Tengah", 3265, DifficultyLevel.Medium,
            "Di perbatasan Jawa Tengah-Jawa Timur. Kental nuansa sejarah, ada warung legendaris di dekat puncak.",
            Route("Via Cemoro Sewu", 6.5m, "2 hari 1 malam", 400, 25_000),
            Route("Via Cemoro Kandang", 9m, "2 hari 1 malam", 300, 25_000),
            Route("Via Candi Cetho", 11m, "2 hari 1 malam", 150, 25_000)),
        Mountain("Gunung Andong", "Jawa Tengah", 1726, DifficultyLevel.Easy,
            "Gunung mungil dengan punggungan cantik. Favorit buat pendakian pertama.",
            Route("Via Sawit", 2m, "1-2 jam", 1000, 15_000)),
        Mountain("Gunung Ungaran", "Jawa Tengah", 2050, DifficultyLevel.Easy,
            "Dekat Semarang, lewat kebun teh dan Candi Gedong Songo di kakinya.",
            Route("Via Mawar", 5m, "4-5 jam", null, 15_000)),
        Mountain("Gunung Merapi", "Jawa Tengah", 2930, DifficultyLevel.Hard,
            "Salah satu gunung api paling aktif di dunia. Status contoh: ditutup untuk pendakian.",
            MountainStatus.Closed,
            Route("Via Selo", 5m, "1 hari", 100, 25_000)),

        // ---------- Jawa Timur ----------
        Mountain("Gunung Semeru", "Jawa Timur", 3676, DifficultyLevel.Hard,
            "Puncak tertinggi di Pulau Jawa (Mahameru). Lewat Ranu Kumbolo dan Kalimati.",
            Route("Via Ranu Pani", 18m, "3 hari 2 malam", 200, 100_000)),
        Mountain("Gunung Arjuno", "Jawa Timur", 3339, DifficultyLevel.Hard,
            "Sepasang dengan Welirang, jalurnya panjang dan sarat situs petilasan.",
            Route("Via Tretes", 15m, "2 hari 1 malam", 150, 30_000),
            Route("Via Lawang", 14m, "2 hari 1 malam", 100, 30_000),
            Route("Via Purwosari", 13m, "3 hari 2 malam", 100, 30_000)),
        Mountain("Gunung Welirang", "Jawa Timur", 3156, DifficultyLevel.Medium,
            "Gunung belerang yang masih ditambang tradisional. Jalurnya bareng Arjuno sampai Pondokan.",
            Route("Via Tretes", 13m, "2 hari 1 malam", 150, 30_000)),
        Mountain("Gunung Penanggungan", "Jawa Timur", 1653, DifficultyLevel.Medium,
            "Pendek tapi curam, dengan puluhan candi kuno di lerengnya.",
            Route("Via Tamiajeng", 4.5m, "3-4 jam", null, 15_000),
            Route("Via Jolotundo", 5m, "4-5 jam", null, 15_000)),
        Mountain("Gunung Raung", "Jawa Timur", 3344, DifficultyLevel.Hard,
            "Kaldera raksasa dan jalur punggungan tajam ke Puncak Sejati. Butuh teknik & alat panjat.",
            Route("Via Kalibaru", 15m, "3 hari 2 malam", 50, 75_000),
            Route("Via Sumberwringin", 12m, "2 hari 1 malam", 50, 75_000)),
        Mountain("Gunung Argopuro", "Jawa Timur", 3088, DifficultyLevel.Hard,
            "Jalur terpanjang di Jawa, lewat Danau Taman Hidup dan sabana Cikasur.",
            Route("Via Baderan", 40m, "4 hari 3 malam", 100, 40_000),
            Route("Via Bremi", 36m, "4 hari 3 malam", 100, 40_000)),
        Mountain("Gunung Butak", "Jawa Timur", 2868, DifficultyLevel.Medium,
            "Sabana luas di puncak, berhadapan dengan Gunung Kawi.",
            Route("Via Sirah Kencong", 10m, "2 hari 1 malam", null, 20_000),
            Route("Via Panderman", 12m, "2 hari 1 malam", null, 20_000)),
        Mountain("Gunung Bromo", "Jawa Timur", 2329, DifficultyLevel.Easy,
            "Kawah aktif di tengah lautan pasir Tengger. Lebih ke wisata, tapi tetap seru buat pemula.",
            Route("Via Cemoro Lawang", 3m, "1-2 jam", null, 30_000)),

        // ---------- Bali & Nusa Tenggara ----------
        Mountain("Gunung Agung", "Bali", 3142, DifficultyLevel.Hard,
            "Gunung tertinggi dan tersuci di Bali. Pendakian dimulai tengah malam dari pura.",
            Route("Via Pura Besakih", 7m, "1 hari", 100, 50_000),
            Route("Via Pasar Agung", 4m, "1 hari", 100, 50_000)),
        Mountain("Gunung Batur", "Bali", 1717, DifficultyLevel.Easy,
            "Sunrise trekking paling populer di Bali, dengan Danau Batur di bawahnya.",
            Route("Via Toya Bungkah", 4m, "2-3 jam", null, 50_000)),
        Mountain("Gunung Batukaru", "Bali", 2276, DifficultyLevel.Medium,
            "Tertinggi kedua di Bali, hutannya masih lebat dan sepi pendaki.",
            Route("Via Pura Luhur Batukaru", 6m, "1 hari", null, 40_000)),
        Mountain("Gunung Rinjani", "Nusa Tenggara Barat", 3726, DifficultyLevel.Hard,
            "Gunung berapi tertinggi kedua di Indonesia, dengan Danau Segara Anak di kalderanya.",
            Route("Via Sembalun", 20m, "3 hari 2 malam", 150, 150_000),
            Route("Via Senaru", 17m, "3 hari 2 malam", 100, 150_000)),
        Mountain("Gunung Tambora", "Nusa Tenggara Barat", 2850, DifficultyLevel.Hard,
            "Letusan 1815-nya mengubah iklim dunia. Kaldera di puncaknya selebar 6 km.",
            Route("Via Pancasila", 17m, "3 hari 2 malam", null, 50_000),
            Route("Via Doro Ncanga", 15m, "2 hari 1 malam", null, 50_000)),
        Mountain("Gunung Mutis", "Nusa Tenggara Timur", 2427, DifficultyLevel.Medium,
            "Hutan ampupu dan padang savana di atap Pulau Timor.",
            Route("Via Fatumnasi", 8m, "1 hari", null, 25_000)),

        // ---------- Sumatra ----------
        Mountain("Gunung Kerinci", "Jambi", 3805, DifficultyLevel.Hard,
            "Gunung berapi tertinggi di Indonesia, dikelilingi kebun teh Kayu Aro.",
            Route("Via Kersik Tuo", 11m, "2 hari 1 malam", 150, 100_000)),
        Mountain("Gunung Marapi", "Sumatera Barat", 2891, DifficultyLevel.Medium,
            "Gunung api aktif di dekat Bukittinggi. Status contoh: ditutup untuk pendakian.",
            MountainStatus.Closed,
            Route("Via Koto Baru", 7m, "2 hari 1 malam", 100, 30_000)),
        Mountain("Gunung Singgalang", "Sumatera Barat", 2877, DifficultyLevel.Medium,
            "Hutan lumut yang lebat dan Telaga Dewi di puncaknya.",
            Route("Via Pandai Sikek", 7m, "2 hari 1 malam", null, 25_000)),
        Mountain("Gunung Talang", "Sumatera Barat", 2597, DifficultyLevel.Medium,
            "Dari puncaknya kelihatan Danau Diatas dan Danau Dibawah.",
            Route("Via Bukit Sileh", 6m, "2 hari 1 malam", null, 25_000)),
        Mountain("Gunung Sibayak", "Sumatera Utara", 2212, DifficultyLevel.Easy,
            "Dekat Berastagi, jalurnya pendek dengan kawah dan pemandian air panas di kaki gunung.",
            Route("Via Raja Berneh", 6m, "3-4 jam", null, 20_000)),
        Mountain("Gunung Sinabung", "Sumatera Utara", 2460, DifficultyLevel.Hard,
            "Gunung api aktif di Tanah Karo. Status contoh: ditutup untuk pendakian.",
            MountainStatus.Closed,
            Route("Via Danau Lau Kawar", 6m, "1 hari", null, 20_000)),
        Mountain("Gunung Dempo", "Sumatera Selatan", 3173, DifficultyLevel.Hard,
            "Kawah berwarna toska di puncak dan kebun teh Pagar Alam di kakinya.",
            Route("Via Kampung 4", 7m, "2 hari 1 malam", null, 30_000),
            Route("Via Rimau", 8m, "2 hari 1 malam", null, 30_000)),
        Mountain("Gunung Kaba", "Bengkulu", 1952, DifficultyLevel.Easy,
            "Jalur aspal sebagian, ujungnya dua kawah besar yang berdampingan.",
            Route("Via Sumber Urip", 8m, "3-4 jam", null, 15_000)),
        Mountain("Gunung Leuser", "Aceh", 3404, DifficultyLevel.Hard,
            "Jantung Taman Nasional Gunung Leuser. Ekspedisi panjang lewat hutan primer.",
            Route("Via Kedah", 45m, "10 hari 9 malam", 30, 250_000)),

        // ---------- Sulawesi, Maluku, Papua ----------
        Mountain("Gunung Latimojong", "Sulawesi Selatan", 3478, DifficultyLevel.Hard,
            "Atap Sulawesi (Puncak Rante Mario), salah satu Seven Summits Indonesia.",
            Route("Via Karangan", 14m, "3 hari 2 malam", null, 50_000)),
        Mountain("Gunung Bawakaraeng", "Sulawesi Selatan", 2845, DifficultyLevel.Medium,
            "Gunung favorit pendaki Makassar, jalurnya melewati 10 pos.",
            Route("Via Lembanna", 10m, "2 hari 1 malam", null, 20_000)),
        Mountain("Gunung Klabat", "Sulawesi Utara", 1995, DifficultyLevel.Medium,
            "Gunung tertinggi di Sulawesi Utara, sunrise-nya menghadap Laut Maluku.",
            Route("Via Airmadidi", 6m, "1 hari", null, 20_000)),
        Mountain("Gunung Binaiya", "Maluku", 3027, DifficultyLevel.Hard,
            "Atap Maluku di Taman Nasional Manusela, Pulau Seram. Akses dan jalurnya masih sangat liar.",
            Route("Via Piliana", 30m, "6 hari 5 malam", 30, 150_000)),
        Mountain("Puncak Jaya (Carstensz)", "Papua Tengah", 4884, DifficultyLevel.Hard,
            "Puncak tertinggi di Indonesia dan Oseania. Pendakian tebing es & batu, wajib operator berizin.",
            Route("Via Sugapa", 60m, "14 hari", 20, 2_500_000))
    ];

    private static Mountain Mountain(string name, string province, int heightMeters, DifficultyLevel difficulty,
        string description, params HikingRoute[] routes) =>
        Mountain(name, province, heightMeters, difficulty, description, MountainStatus.Open, routes);

    // Foto diambil dari SeedMountainImages. Gunung seed yang udah ada di DB sebelum foto ditambahkan
    // diisi lewat migration AddMountainImages (sekali jalan), bukan di sini — biar foto yang sengaja
    // dihapus admin gak balik lagi tiap app start.
    private static Mountain Mountain(string name, string province, int heightMeters, DifficultyLevel difficulty,
        string description, MountainStatus status, params HikingRoute[] routes)
    {
        var image = SeedMountainImages.ByMountainName.GetValueOrDefault(name);
        return new Mountain
        {
            Name = name,
            Province = province,
            HeightMeters = heightMeters,
            Difficulty = difficulty,
            Status = status,
            Description = description,
            ImagePath = image?.ImagePath,
            ImageCredit = image?.Credit,
            ImageSourceUrl = image?.SourceUrl,
            Routes = routes.ToList()
        };
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
