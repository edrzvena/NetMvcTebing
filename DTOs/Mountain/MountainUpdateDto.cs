using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
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
    // Foto baru (opsional). Diproses server jadi WebP 16:9 ukuran 640 & 1200 — file aslinya gak disimpan.
    [Display(Name = "Foto")] public IFormFile? Image { get; set; }

    [MaxLength(300), Display(Name = "Kredit foto")] public string? ImageCredit { get; set; }

    [MaxLength(500), Url(ErrorMessage = "Link sumber harus URL lengkap, misal https://..."), Display(Name = "Link sumber foto")]
    public string? ImageSourceUrl { get; set; }

    // Cuma buat preview di form; yang dipakai buat simpan/hapus file tetap data dari database.
    [ValidateNever] public string? CurrentImagePath { get; set; }

    [Display(Name = "Hapus foto")] public bool RemoveImage { get; set; }
}
