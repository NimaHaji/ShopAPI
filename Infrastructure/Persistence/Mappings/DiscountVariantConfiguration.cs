using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Mappings;

public class DiscountVariantConfiguration:IEntityTypeConfiguration<DiscountVariant>
{
    public void Configure(EntityTypeBuilder<DiscountVariant> builder)
    {
        builder.ToTable("DiscountVariants");
        
        builder.HasKey(x => new
        {
            x.DiscountId,
            x.ProductVariantId
        });
        
        builder.HasOne(x => x.Discount)
            .WithMany(x => x.DiscountVariants)
            .HasForeignKey(x => x.DiscountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ProductVariant)
            .WithMany(x => x.DiscountVariants)
            .HasForeignKey(x => x.ProductVariantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.ProductVariantId);
    }
}