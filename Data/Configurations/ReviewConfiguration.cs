using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tebing.Models;

namespace Tebing.Data.Configurations;

public class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        // Restrict ke User & Mountain: sama kayak Booking, hindari multiple cascade path
        // (Mountain -> Booking -> Review dan Mountain -> Review).
        builder.HasOne(r => r.User).WithMany().HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(r => r.Mountain).WithMany(m => m.Reviews).HasForeignKey(r => r.MountainId)
            .OnDelete(DeleteBehavior.Restrict);

        // One-to-one -> unique index BookingId otomatis.
        builder.HasOne(r => r.Booking).WithOne(b => b.Review).HasForeignKey<Review>(r => r.BookingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable(t => t.HasCheckConstraint("CK_Reviews_Rating", "[Rating] BETWEEN 1 AND 5"));
    }
}
