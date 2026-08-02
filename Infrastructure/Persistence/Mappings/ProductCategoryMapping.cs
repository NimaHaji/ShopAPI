using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Mappings;

public class ProductCategoryMapping : IEntityTypeConfiguration<ProductCategory>
{
    public void Configure(EntityTypeBuilder<ProductCategory> builder)
    {
        builder.ToTable("ProductCategories");

        builder.HasKey(pc => pc.Id);

        builder
            .Property(p => p.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder
            .Property(p => p.IsDeleted)
            .IsRequired();

        builder
            .Property(pc => pc.CreatedAt)
            .IsRequired();
        
        builder
            .Property(pc=>pc.DeletedAt);
        
        builder
            .Property(pc => pc.UpdatedAt)
            .IsRequired();
    }
}