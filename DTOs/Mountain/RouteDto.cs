namespace Tebing.DTOs.Mountain;

public class RouteDto
{
    public int Id { get; set; }
    public int MountainId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal DistanceKm { get; set; }
    public string? EstimatedDuration { get; set; }
    public int? DailyQuota { get; set; }
    public bool IsActive { get; set; }
}
