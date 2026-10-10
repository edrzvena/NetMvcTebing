using Microsoft.EntityFrameworkCore;
using Tebing.Data;
using Tebing.Models;
using Tebing.Repositories.Interfaces;

namespace Tebing.Repositories.Implementations;

public class PaymentRepository(AppDbContext context) : Repository<Payment>(context), IPaymentRepository
{
    public async Task<Payment?> GetByBookingIdAsync(int bookingId) =>
        await DbSet.Include(p => p.Booking).FirstOrDefaultAsync(p => p.BookingId == bookingId);
}
