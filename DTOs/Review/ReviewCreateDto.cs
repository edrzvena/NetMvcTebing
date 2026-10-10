using System.ComponentModel.DataAnnotations;

namespace Tebing.DTOs.Review;

public class ReviewCreateDto
{
    public int BookingId { get; set; }

    [Range(1, 5, ErrorMessage = "Rating harus 1 sampai 5.")]
    public int Rating { get; set; }

    [MaxLength(1000)] public string? Comment { get; set; }
}
