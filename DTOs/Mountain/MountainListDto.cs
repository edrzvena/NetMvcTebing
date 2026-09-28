using Tebing.Common.Enums;

namespace Tebing.DTOs.Mountain;

public class MountainListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public int HeightMeters { get; set; }
    public DifficultyLevel Difficulty { get; set; }
    public MountainStatus Status { get; set; }
    public string? ImageUrl { get; set; }

    // Null dulu sampai Review ada (Tahap 6), belum ada data buat dihitung.
    public double? AverageRating { get; set; }
}
