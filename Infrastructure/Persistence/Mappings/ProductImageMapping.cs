using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Mappings;

public class ProductImageMapping:IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("ProductImages");
        
        builder.HasKey(pi => pi.Id);

        builder
            .Property(pi => pi.ImageLink)
            .IsRequired();
        
        builder
            .Property(pi => pi.ProductId)
            .IsRequired();
        
        builder
            .Property(pi=>pi.IsPrimary)
            .IsRequired();
        
        builder
            .Property(pi=>pi.SortOrder)
            .IsRequired();
    }
}