using Tebing.Common.Enums;
using Tebing.Common.Regions;

namespace Tebing.DTOs.Mountain;

public class MountainListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public int HeightMeters { get; set; }
    public DifficultyLevel Difficulty { get; set; }
    public MountainStatus Status { get; set; }
    // Path dasar foto (lihat MountainImageProcessor.Url). Null = belum ada foto.
    public string? ImagePath { get; set; }

    // Kredit foto (syarat lisensi CC) — ditampilin di panel foto halaman akun.
    public string? ImageCredit { get; set; }

    // Null = belum ada review.
    public double? AverageRating { get; set; }
    public int ReviewCount { get; set; }

    // Diturunin dari Province (IslandRegion). Null = provinsi gak dikenal.
    public Island? Island { get; set; }

    // Kalimat pertama Description, buat banner Home. Null = gak ada deskripsi.
    public string? Tagline { get; set; }

    // Cuma jalur aktif yang dihitung. MinPricePerPerson null = belum ada jalur aktif.
    public int RouteCount { get; set; }
    public decimal? MinPricePerPerson { get; set; }
}
