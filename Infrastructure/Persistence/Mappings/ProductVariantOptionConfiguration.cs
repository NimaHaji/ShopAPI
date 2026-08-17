using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Mappings;

public class ProductVariantOptionConfiguration
    : IEntityTypeConfiguration<ProductVariantOption>
{
    public void Configure(
        EntityTypeBuilder<ProductVariantOption> builder)
    {
        builder.ToTable("ProductVariantOptions");

        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.ProductVariant)
            .WithMany(x => x.Options)
            .HasForeignKey(x => x.ProductVariantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ProductOption)
            .WithMany()
            .HasForeignKey(x => x.ProductOptionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ProductOptionValue)
            .WithMany()
            .HasForeignKey(x => x.ProductOptionValueId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasIndex(x => new
            {
                x.ProductVariantId,
                x.ProductOptionId
            })
            .IsUnique();

        builder.HasIndex(x => new
            {
                x.ProductVariantId,
                x.ProductOptionValueId
            })
            .IsUnique();
    }
}