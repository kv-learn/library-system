using System;
using LibraryApi.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryApi.Data.Configurations;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        // Filtered unique index: only ACTIVE rows must be unique.
        // A member can have many old Cancelled/Fulfilled reservations for the same book.
        builder.HasIndex(r => new { r.MemberId, r.BookId })
            .IsUnique()
            .HasFilter("\"Status\" = 'Active'");

        builder.HasOne(r => r.Book)
            .WithMany(b => b.Reservations)
            .HasForeignKey(r => r.BookId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Member)
            .WithMany(m => m.Reservations)
            .HasForeignKey(r => r.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }

}
