using Tebing.Common.Enums;
using Tebing.DTOs.Booking;

namespace Tebing.Services.Interfaces;

public record BookingResult(bool Success, string? Error = null, int? BookingId = null);

public interface IBookingService
{
    Task<BookingResult> CreateAsync(int userId, BookingCreateDto dto);
    Task<List<BookingListDto>> GetByUserAsync(int userId);
    Task<BookingDetailDto?> GetByIdAsync(int id);
    Task<List<BookingListDto>> GetAllForAdminAsync(int? mountainId = null, BookingStatus? status = null,
        DateOnly? climbDate = null);

    // Jumlah booking per status (buat tab di Kelola Booking). Status yang gak ada booking-nya gak masuk dictionary.
    Task<Dictionary<BookingStatus, int>> GetStatusCountsAsync(int? mountainId = null, DateOnly? climbDate = null);

    Task<BookingResult> UpdateStatusAsync(BookingStatusUpdateDto dto);
    Task<BookingResult> CancelByUserAsync(int bookingId, int userId);
}
