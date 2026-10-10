using Microsoft.EntityFrameworkCore;
using Tebing.Models;

namespace Tebing.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Mountain> Mountains => Set<Mountain>();
    public DbSet<HikingRoute> HikingRoutes => Set<HikingRoute>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingParticipant> BookingParticipants => Set<BookingParticipant>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
