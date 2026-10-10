using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tebing.Models;

public class HikingRoute
{
    [Key] public int Id { get; set; }

    public int MountainId { get; set; }
    public Mountain Mountain { get; set; } = null!;

    [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;

    [Column(TypeName = "decimal(6,2)")] public decimal DistanceKm { get; set; }

    [MaxLength(100)] public string? EstimatedDuration { get; set; }

    // null = tanpa batas kuota
    public int? DailyQuota { get; set; }

    // Harga tiket/SIMAKSI per orang, dipakai buat hitung Booking.TotalPrice di server.
    [Column(TypeName = "decimal(12,2)")] public decimal PricePerPerson { get; set; }

    public bool IsActive { get; set; } = true;
}
