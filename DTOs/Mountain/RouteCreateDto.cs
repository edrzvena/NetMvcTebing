using System.ComponentModel.DataAnnotations;

namespace Tebing.DTOs.Mountain;

public class RouteCreateDto
{
    public int Id { get; set; }
    public int MountainId { get; set; }

    [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;

    [Range(0.1, 1000)] public decimal DistanceKm { get; set; }

    [MaxLength(100)] public string? EstimatedDuration { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Kuota harus lebih dari 0, atau kosongkan kalau tanpa batas.")]
    public int? DailyQuota { get; set; }
}
