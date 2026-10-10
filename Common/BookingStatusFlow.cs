using Tebing.Common.Enums;

namespace Tebing.Common;

// State machine status booking — dipakai BookingService & PaymentService, jadi perubahan status
// dari mana pun tetap lewat aturan yang sama. Rejected/Completed/Cancelled = status akhir.
public static class BookingStatusFlow
{
    private static readonly Dictionary<BookingStatus, BookingStatus[]> AllowedTransitions = new()
    {
        [BookingStatus.Pending] = [BookingStatus.Approved, BookingStatus.Rejected, BookingStatus.Cancelled],
        [BookingStatus.Approved] = [BookingStatus.Completed, BookingStatus.Cancelled],
        [BookingStatus.Rejected] = [],
        [BookingStatus.Completed] = [],
        [BookingStatus.Cancelled] = []
    };

    public static IReadOnlyList<BookingStatus> NextStatuses(BookingStatus from) => AllowedTransitions[from];

    public static bool CanTransition(BookingStatus from, BookingStatus to) => AllowedTransitions[from].Contains(to);
}
