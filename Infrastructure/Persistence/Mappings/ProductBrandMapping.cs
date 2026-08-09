using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Mappings;

public class ProductBrandMapping : IEntityTypeConfiguration<ProductBrand>
{
    public void Configure(EntityTypeBuilder<ProductBrand> builder)
    {
        builder.ToTable("ProductBrands");

        builder.HasKey(pc => pc.Id);

        builder
            .Property(p => p.Title)
            .HasMaxLength(200)
            .IsRequired();
        
        builder
            .Property(b=>b.IsDeleted)
            .IsRequired();
        
        builder
            .Property(b=>b.CreatedAt)
            .IsRequired();
        
        builder
            .Property(b=>b.DeletedAt);

        builder
            .Property(b => b.UpdatedAt)
            .IsRequired();
    }
}