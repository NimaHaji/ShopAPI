using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Mappings;

public class DiscountConfiguration:IEntityTypeConfiguration<Discount>
{
    public void Configure(EntityTypeBuilder<Discount> builder)
    {
        builder.ToTable("Discounts");
        
        builder
            .HasKey(d => d.Id);
        
        builder
            .Property(d => d.Title)
            .HasMaxLength(100)
            .IsRequired();
        
        builder
            .Property(d=>d.DiscountType)
            .IsRequired();
        
        builder
            .Property(d=>d.Value)
            .IsRequired();

        builder
            .Property(d => d.MaxDiscountAmount);
        
        builder
            .Property(d=>d.StartsAt)
            .IsRequired();
        
        builder
            .Property(d=>d.EndsAt)
            .IsRequired();
        
        builder
            .Property(d=>d.IsActive)
            .IsRequired();
        
        builder
            .Property(d=>d.CreatedAt)
            .IsRequired();
        
        builder
            .Property(d=>d.UpdatedAt)
            .IsRequired();

        builder
            .Property(d => d.DeletedAt);
        
        builder
            .Property(d=>d.IsDeleted)
            .IsRequired();
        
        builder.HasIndex(x => new
        {
            x.IsActive,
            x.IsDeleted,
            x.StartsAt,
            x.EndsAt
        });
    }
}