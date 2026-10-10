using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tebing.Models;

namespace Tebing.Data.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        // Restrict (bukan cascade): SQL Server nolak multiple cascade path
        // (Mountain -> HikingRoute -> Booking dan Mountain -> Booking), dan histori booking
        // memang gak boleh ikut kehapus kalau user/gunung/jalur dihapus.
        builder.HasOne(b => b.User).WithMany().HasForeignKey(b => b.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(b => b.Mountain).WithMany().HasForeignKey(b => b.MountainId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(b => b.HikingRoute).WithMany().HasForeignKey(b => b.HikingRouteId).OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(b => b.Participants).WithOne(p => p.Booking).HasForeignKey(p => p.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        // Dipakai query cek kuota (jalur + tanggal).
        builder.HasIndex(b => new { b.HikingRouteId, b.ClimbDate });
    }
}
