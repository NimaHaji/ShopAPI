using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Mappings;

public class DiscountProductConfiguration:IEntityTypeConfiguration<DiscountProduct>
{
    public void Configure(EntityTypeBuilder<DiscountProduct> builder)
    {
        builder.ToTable("DiscountProducts");
        
        builder.HasKey(x => new
        {
            x.DiscountId,
            x.ProductId
        });
        
        builder
            .HasOne(x => x.Product)
            .WithMany(x => x.DiscountProducts)
            .HasForeignKey(x => x.ProductId);
        
        builder
            .HasOne(x => x.Discount)
            .WithMany(x => x.DiscountProducts)
            .HasForeignKey(x => x.DiscountId);
        
        builder.HasIndex(x => new
            {
                x.DiscountId,
                x.ProductId
            })
            .IsUnique();
    }
}