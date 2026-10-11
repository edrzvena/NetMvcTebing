using System.ComponentModel.DataAnnotations;
using Tebing.Common.Enums;

namespace Tebing.DTOs.Mountain;

public class MountainUpdateDto
{
    public int Id { get; set; }

    [Required, MaxLength(100), Display(Name = "Nama Gunung")] public string Name { get; set; } = string.Empty;
    [Required, MaxLength(100), Display(Name = "Provinsi")] public string Province { get; set; } = string.Empty;

    [Range(1, 10000), Display(Name = "Ketinggian")] public int HeightMeters { get; set; }
    [Display(Name = "Tingkat Kesulitan")] public DifficultyLevel Difficulty { get; set; }
    [Display(Name = "Status")] public MountainStatus Status { get; set; }

    [Display(Name = "Deskripsi")] public string? Description { get; set; }
    [Display(Name = "URL Gambar")] public string? ImageUrl { get; set; }
}
