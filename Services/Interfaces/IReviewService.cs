using Tebing.DTOs.Review;

namespace Tebing.Services.Interfaces;

public record ReviewResult(bool Success, string? Error = null);

public interface IReviewService
{
    Task<ReviewResult> CreateAsync(int userId, ReviewCreateDto dto);
}
