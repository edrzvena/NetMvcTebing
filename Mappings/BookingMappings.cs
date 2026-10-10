using Tebing.DTOs.Booking;
using Tebing.DTOs.Payment;
using Tebing.DTOs.Review;
using Tebing.Models;

namespace Tebing.Mappings;

public static class BookingMappings
{
    public static BookingListDto ToListDto(this Booking booking) => new()
    {
        Id = booking.Id,
        MountainName = booking.Mountain.Name,
        RouteName = booking.HikingRoute.Name,
        ClimbDate = booking.ClimbDate,
        NumberOfPeople = booking.NumberOfPeople,
        TotalPrice = booking.TotalPrice,
        Status = booking.Status,
        CreatedAt = booking.CreatedAt,
        PaymentStatus = booking.Payment?.Status,
        // User cuma di-Include di query admin, di "riwayat booking saya" sengaja dibiarkan null.
        UserFullName = booking.User?.FullName
    };

    public static BookingDetailDto ToDetailDto(this Booking booking) => new()
    {
        Id = booking.Id,
        UserId = booking.UserId,
        UserFullName = booking.User.FullName,
        UserEmail = booking.User.Email,
        MountainId = booking.MountainId,
        MountainName = booking.Mountain.Name,
        RouteName = booking.HikingRoute.Name,
        ClimbDate = booking.ClimbDate,
        NumberOfPeople = booking.NumberOfPeople,
        TotalPrice = booking.TotalPrice,
        Status = booking.Status,
        RejectionReason = booking.RejectionReason,
        CreatedAt = booking.CreatedAt,
        Participants = booking.Participants.Select(p => p.ToDto()).ToList(),
        Payment = booking.Payment?.ToDto(),
        // Review booking pasti punya user yang sama dengan booking-nya, jadi nama diambil dari booking.User.
        Review = booking.Review is { } review
            ? new ReviewDto
            {
                UserFullName = booking.User.FullName,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt
            }
            : null
    };

    public static PaymentDto ToDto(this Payment payment) => new()
    {
        Amount = payment.Amount,
        Status = payment.Status,
        RejectionReason = payment.RejectionReason,
        UploadedAt = payment.UploadedAt,
        VerifiedAt = payment.VerifiedAt
    };

    public static ParticipantDto ToDto(this BookingParticipant participant) => new()
    {
        FullName = participant.FullName,
        NIK = participant.NIK,
        PhoneNumber = participant.PhoneNumber,
        EmergencyContactName = participant.EmergencyContactName,
        EmergencyContactPhone = participant.EmergencyContactPhone
    };
}
