using Tebing.Models;

namespace Tebing.Repositories.Interfaces;

public interface IPaymentRepository : IRepository<Payment>
{
    // Include Booking, karena verifikasi payment sekalian ngubah status booking-nya.
    Task<Payment?> GetByBookingIdAsync(int bookingId);
}
