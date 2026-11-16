using LibraryApi.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryApi.Data.Configurations;

public class LoanConfiguration: IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Loans_DueAfterBorrowed",
                "\"DueAt\" > \"CreatedAt\"");
            t.HasCheckConstraint("CK_Loans_ReturnedAfterBorrowed",
                "\"ReturnedAt\" IS NULL OR \"ReturnedAt\" >= \"CreatedAt\"");
        });

        // Only one open (not returned) loan per member per book
        builder.HasIndex(l => new { l.MemberId, l.BookId })
            .IsUnique()
            .HasFilter("\"ReturnedAt\" IS NULL");

        builder.HasOne(l => l.Book)
            .WithMany(b => b.Loans)
            .HasForeignKey(l => l.BookId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Member)
            .WithMany(m => m.Loans)
            .HasForeignKey(l => l.MemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
