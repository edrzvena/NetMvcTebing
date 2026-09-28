using System.ComponentModel.DataAnnotations;
using Tebing.Common.Enums;

namespace Tebing.DTOs.Mountain;

public class MountainUpdateDto
{
    public int Id { get; set; }

    [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string Province { get; set; } = string.Empty;

    [Range(1, 10000)] public int HeightMeters { get; set; }
    public DifficultyLevel Difficulty { get; set; }
    public MountainStatus Status { get; set; }

    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
}
