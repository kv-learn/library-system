using LibraryApi.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryApi.Data.Configurations;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        // ISBN-13, stored as digits only (no hyphens)
        builder.Property(b => b.Isbn)
            .IsRequired()
            .HasMaxLength(13);

        builder.HasIndex(b => b.Isbn)
            .IsUnique();

        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(b => b.Description)
            .HasMaxLength(2000);
    }
}
