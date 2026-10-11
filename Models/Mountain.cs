using System.ComponentModel.DataAnnotations;
using Tebing.Common.Enums;

namespace Tebing.Models;

public class Mountain
{
    [Key] public int Id { get; set; }

    [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string Province { get; set; } = string.Empty;

    public int HeightMeters { get; set; }
    public DifficultyLevel Difficulty { get; set; }
    public MountainStatus Status { get; set; } = MountainStatus.Open;

    public string? Description { get; set; }
    // Path dasar foto tanpa akhiran ukuran, misal "/images/mountains/gunung-gede" → file aslinya
    // "-640.webp" & "-1200.webp" (lihat MountainImageProcessor). Null = belum ada foto.
    public string? ImagePath { get; set; }

    // Kredit & link sumber foto (wajib buat foto berlisensi CC, misal dari Wikimedia Commons).
    [MaxLength(300)] public string? ImageCredit { get; set; }
    [MaxLength(500)] public string? ImageSourceUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<HikingRoute> Routes { get; set; } = new List<HikingRoute>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
