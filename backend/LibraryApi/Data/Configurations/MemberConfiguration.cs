using LibraryApi.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryApi.Data.Configurations;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        // NOCASE on the column: comparisons and the unique index ignore ASCII case,
        // so "Bob@x.com" and "bob@x.com" can't both exist
       builder.Property(m => m.Email)
            .IsRequired()
            .HasMaxLength(255)
            .UseCollation("NOCASE");

        builder.HasIndex(m => m.Email)
            .IsUnique();

        builder.Property(m => m.FullName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(m => m.PasswordHash)
            .IsRequired();

        builder.Property(m => m.Role)
         .HasConversion<string>()
         .IsRequired()
         .HasMaxLength(20);
    }
}
