using Tebing.Common.Enums;
using Tebing.DTOs.Booking;
using Tebing.Mappings;
using Tebing.Models;
using Tebing.Repositories.Interfaces;
using Tebing.Services.Interfaces;

namespace Tebing.Services.Implementations;

public class BookingService(
    IBookingRepository bookingRepository,
    IMountainRepository mountainRepository,
    IHikingRouteRepository routeRepository) : IBookingService
{
    // State machine status booking — perubahan status cuma boleh lewat jalur yang ada di sini.
    // Rejected/Completed/Cancelled = status akhir, gak bisa diubah lagi.
    private static readonly Dictionary<BookingStatus, BookingStatus[]> AllowedTransitions = new()
    {
        [BookingStatus.Pending] = [BookingStatus.Approved, BookingStatus.Rejected, BookingStatus.Cancelled],
        [BookingStatus.Approved] = [BookingStatus.Completed, BookingStatus.Cancelled],
        [BookingStatus.Rejected] = [],
        [BookingStatus.Completed] = [],
        [BookingStatus.Cancelled] = []
    };

    public async Task<BookingResult> CreateAsync(int userId, BookingCreateDto dto)
    {
        var route = await routeRepository.GetByIdAsync(dto.HikingRouteId);
        if (route is null || !route.IsActive || route.MountainId != dto.MountainId)
            return new BookingResult(false, "Jalur pendakian tidak ditemukan atau sudah tidak aktif.");

        var mountain = await mountainRepository.GetByIdAsync(route.MountainId);
        if (mountain is null || mountain.Status != MountainStatus.Open)
            return new BookingResult(false, "Gunung ini sedang ditutup untuk pendakian.");

        if (dto.ClimbDate is not { } climbDate || climbDate < DateOnly.FromDateTime(DateTime.Today))
            return new BookingResult(false, "Tanggal naik tidak boleh sebelum hari ini.");

        var numberOfPeople = dto.Participants.Count;
        if (numberOfPeople == 0)
            return new BookingResult(false, "Minimal 1 peserta.");

        if (dto.Participants.Select(p => p.NIK).Distinct().Count() != numberOfPeople)
            return new BookingResult(false, "Ada NIK peserta yang dobel dalam satu rombongan.");

        if (route.DailyQuota is { } quota)
        {
            var booked = await bookingRepository.GetBookedPeopleCountAsync(route.Id, climbDate);
            var remaining = Math.Max(quota - booked, 0);
            if (numberOfPeople > remaining)
                return new BookingResult(false,
                    $"Kuota jalur {route.Name} tanggal {climbDate:dd MMM yyyy} tinggal {remaining} orang.");
        }

        var booking = new Booking
        {
            UserId = userId,
            MountainId = route.MountainId,
            HikingRouteId = route.Id,
            ClimbDate = climbDate,
            NumberOfPeople = numberOfPeople,
            // Dihitung di server dari harga jalur, bukan dari input client.
            TotalPrice = route.PricePerPerson * numberOfPeople,
            Participants = dto.Participants.Select(p => new BookingParticipant
            {
                FullName = p.FullName,
                NIK = p.NIK,
                PhoneNumber = p.PhoneNumber,
                EmergencyContactName = p.EmergencyContactName,
                EmergencyContactPhone = p.EmergencyContactPhone
            }).ToList()
        };

        await bookingRepository.AddAsync(booking);
        await bookingRepository.SaveChangesAsync();

        return new BookingResult(true, BookingId: booking.Id);
    }

    public async Task<List<BookingListDto>> GetByUserAsync(int userId)
    {
        var bookings = await bookingRepository.GetByUserIdAsync(userId);
        return bookings.Select(b => b.ToListDto()).ToList();
    }

    public async Task<BookingDetailDto?> GetByIdAsync(int id)
    {
        var booking = await bookingRepository.GetByIdWithDetailsAsync(id);
        if (booking is null)
            return null;

        var dto = booking.ToDetailDto();
        dto.AllowedNextStatuses = AllowedTransitions[booking.Status].ToList();
        return dto;
    }

    public async Task<List<BookingListDto>> GetAllForAdminAsync(int? mountainId = null, BookingStatus? status = null,
        DateOnly? climbDate = null)
    {
        // Belum ada relasi "gunung milik admin X" di MVP, jadi admin lihat semua booking
        // (bisa difilter per gunung).
        var bookings = await bookingRepository.GetFilteredAsync(mountainId, status, climbDate);
        return bookings.Select(b => b.ToListDto()).ToList();
    }

    public async Task<BookingResult> UpdateStatusAsync(BookingStatusUpdateDto dto)
    {
        var booking = await bookingRepository.GetByIdAsync(dto.BookingId);
        if (booking is null)
            return new BookingResult(false, "Booking tidak ditemukan.");

        if (!AllowedTransitions[booking.Status].Contains(dto.Status))
            return new BookingResult(false, $"Status booking tidak bisa diubah dari {booking.Status} ke {dto.Status}.");

        if (dto.Status == BookingStatus.Completed && booking.ClimbDate > DateOnly.FromDateTime(DateTime.Today))
            return new BookingResult(false, "Booking baru bisa diselesaikan mulai tanggal pendakiannya.");

        booking.Status = dto.Status;
        booking.RejectionReason = dto.Status == BookingStatus.Rejected ? dto.RejectionReason : null;

        bookingRepository.Update(booking);
        await bookingRepository.SaveChangesAsync();

        return new BookingResult(true, BookingId: booking.Id);
    }

    // Pendaki cuma boleh batalin booking miliknya sendiri yang masih Pending —
    // kalau udah Approved, pembatalan lewat admin.
    public async Task<BookingResult> CancelByUserAsync(int bookingId, int userId)
    {
        var booking = await bookingRepository.GetByIdAsync(bookingId);
        if (booking is null || booking.UserId != userId)
            return new BookingResult(false, "Booking tidak ditemukan.");

        if (booking.Status != BookingStatus.Pending)
            return new BookingResult(false, "Cuma booking berstatus Pending yang bisa dibatalkan.");

        booking.Status = BookingStatus.Cancelled;
        bookingRepository.Update(booking);
        await bookingRepository.SaveChangesAsync();

        return new BookingResult(true, BookingId: booking.Id);
    }
}
