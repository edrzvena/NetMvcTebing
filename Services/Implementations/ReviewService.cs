using Tebing.Common.Enums;
using Tebing.DTOs.Review;
using Tebing.Models;
using Tebing.Repositories.Interfaces;
using Tebing.Services.Interfaces;

namespace Tebing.Services.Implementations;

public class ReviewService(IReviewRepository reviewRepository, IBookingRepository bookingRepository)
    : IReviewService
{
    public async Task<ReviewResult> CreateAsync(int userId, ReviewCreateDto dto)
    {
        var booking = await bookingRepository.GetByIdAsync(dto.BookingId);
        if (booking is null || booking.UserId != userId)
            return new ReviewResult(false, "Booking tidak ditemukan.");

        if (booking.Status != BookingStatus.Completed)
            return new ReviewResult(false, "Review cuma bisa dikasih setelah pendakian selesai (Completed).");

        if (await reviewRepository.ExistsForBookingAsync(booking.Id))
            return new ReviewResult(false, "Kamu sudah kasih review untuk booking ini.");

        await reviewRepository.AddAsync(new Review
        {
            UserId = userId,
            // Gunung diambil dari booking, bukan dari input user.
            MountainId = booking.MountainId,
            BookingId = booking.Id,
            Rating = dto.Rating,
            Comment = string.IsNullOrWhiteSpace(dto.Comment) ? null : dto.Comment.Trim()
        });
        await reviewRepository.SaveChangesAsync();

        return new ReviewResult(true);
    }
}
