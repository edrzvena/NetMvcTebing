namespace Tebing.DTOs.Review;

public class ReviewDto
{
    public string UserFullName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}
