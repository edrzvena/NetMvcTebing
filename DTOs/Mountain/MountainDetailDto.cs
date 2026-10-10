using Tebing.Common.Enums;
using Tebing.DTOs.Review;

namespace Tebing.DTOs.Mountain;

public class MountainDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public int HeightMeters { get; set; }
    public DifficultyLevel Difficulty { get; set; }
    public MountainStatus Status { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }

    // Null = belum ada review.
    public double? AverageRating { get; set; }

    public List<RouteDto> Routes { get; set; } = [];
    public List<ReviewDto> Reviews { get; set; } = [];
}
