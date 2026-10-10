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

    // Null = belum ada review.
    public double? AverageRating { get; set; }
    public int ReviewCount { get; set; }
}
