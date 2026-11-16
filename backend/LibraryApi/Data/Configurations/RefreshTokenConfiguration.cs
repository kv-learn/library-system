using LibraryApi.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryApi.Data.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.Property(rt => rt.TokenHash)
            .IsRequired()
            .HasMaxLength(64);
        
        builder.HasIndex(rt => rt.TokenHash)
            .IsUnique();

        builder.HasOne(rt => rt.Member)
            .WithMany(m => m.RefreshTokens)
            .HasForeignKey(rt => rt.MemberId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
