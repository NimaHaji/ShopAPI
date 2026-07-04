using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Mappings;

public class RefreshTokenConfiguration:IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Token)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasIndex(x => x.Token)
            .IsUnique();

        builder.HasOne(x => x.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
       builder
           .Property(rf => rf.TenantId)
           .IsRequired();
    }
}