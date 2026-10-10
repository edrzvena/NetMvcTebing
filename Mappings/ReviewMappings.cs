using Tebing.DTOs.Review;
using Tebing.Models;

namespace Tebing.Mappings;

public static class ReviewMappings
{
    // Butuh Review.User di-Include.
    public static ReviewDto ToDto(this Review review) => new()
    {
        UserFullName = review.User.FullName,
        Rating = review.Rating,
        Comment = review.Comment,
        CreatedAt = review.CreatedAt
    };
}
