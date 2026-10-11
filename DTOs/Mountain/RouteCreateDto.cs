using System.ComponentModel.DataAnnotations;

namespace Tebing.DTOs.Mountain;

public class RouteCreateDto
{
    public int Id { get; set; }
    public int MountainId { get; set; }

    [Required, MaxLength(100), Display(Name = "Nama Jalur")] public string Name { get; set; } = string.Empty;

    [Range(0.1, 1000), Display(Name = "Jarak")] public decimal DistanceKm { get; set; }

    [MaxLength(100), Display(Name = "Estimasi Waktu")] public string? EstimatedDuration { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Kuota harus lebih dari 0, atau kosongkan kalau tanpa batas.")]
    [Display(Name = "Kuota Harian")]
    public int? DailyQuota { get; set; }

    [Range(0, 100_000_000), Display(Name = "Harga per Orang")] public decimal PricePerPerson { get; set; }
}
