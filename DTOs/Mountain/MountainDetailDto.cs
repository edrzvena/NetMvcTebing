using Tebing.Common.Enums;
using Tebing.Common.Regions;
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

    // Diturunin dari Province (IslandRegion). Null = provinsi gak dikenal.
    public Island? Island { get; set; }
    // Path dasar foto (lihat MountainImageProcessor.Url). Null = belum ada foto.
    public string? ImagePath { get; set; }
    public string? ImageCredit { get; set; }
    public string? ImageSourceUrl { get; set; }

    // Null = belum ada review.
    public double? AverageRating { get; set; }

    public List<RouteDto> Routes { get; set; } = [];
    public List<ReviewDto> Reviews { get; set; } = [];
}
