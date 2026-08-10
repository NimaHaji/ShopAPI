using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Mappings;

public class IdempotencyKeyConfiguration:IEntityTypeConfiguration<IdempotencyKey>
{
    public void Configure(EntityTypeBuilder<IdempotencyKey> builder)
    {
        builder.ToTable("IdempotencyKeys");
        
        builder.HasKey(k => k.Id);

        builder
            .Property(k => k.CreatedAt)
            .IsRequired();
        
        builder
            .Property(k=>k.UserId)
            .IsRequired();
        
        builder
            .Property(k=>k.Key)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();
        
        builder
            .HasOne(k=>k.User)
            .WithMany()
            .HasForeignKey(k=>k.UserId);
        
        builder
            .HasIndex(k => new { k.UserId, k.Key })
            .IsUnique();
    }
}