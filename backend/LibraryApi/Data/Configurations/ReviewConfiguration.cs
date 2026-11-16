using LibraryApi.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryApi.Data.Configurations;

public class ReviewConfiguration:IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable(t => t.HasCheckConstraint("CK_Reviews_Rating",
            "\"Rating\" BETWEEN 1 AND 5"));

        builder.Property(r => r.Comment)
            .HasMaxLength(1000);

        // One review per member per book. No filter: this rule always applies.
        builder.HasIndex(r => new { r.MemberId, r.BookId })
            .IsUnique();

        builder.HasOne(r => r.Book)
            .WithMany(b => b.Reviews)
            .HasForeignKey(r => r.BookId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Member)
            .WithMany(m => m.Reviews)
            .HasForeignKey(r => r.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
