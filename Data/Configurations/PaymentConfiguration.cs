using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tebing.Models;

namespace Tebing.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        // 1 booking = maksimal 1 payment (unique index BookingId dibikin otomatis dari relasi one-to-one).
        // Kalau ditolak admin, user upload ulang ke record yang sama, bukan bikin baru.
        builder.HasOne(p => p.Booking).WithOne(b => b.Payment).HasForeignKey<Payment>(p => p.BookingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
