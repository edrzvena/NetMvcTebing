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
    public string? ImageUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<HikingRoute> Routes { get; set; } = new List<HikingRoute>();
}
