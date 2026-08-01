using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Mappings;

public class ProductMapping : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder
            .Property(p => p.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder
            .Property(p => p.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder
            .Property(p => p.Price)
            .IsRequired();

        builder
            .Property(p => p.DiscountPercentage);

        builder
            .Property(p=>p.AddedAt)
            .IsRequired();
        
        builder
            .Property(p=>p.IsDeleted)
            .IsRequired();

        builder
            .Property(p => p.DeletedAt);
        
        builder
            .Property(p=>p.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();
        
        builder
            .Property(p=>p.UpdatedAt)
            .IsRequired();
        
        builder
            .Property(p=>p.Sku)
            .IsRequired();
        
        builder
            .HasOne(p => p.Category)
            .WithMany(p => p.Products)
            .HasForeignKey(f => f.CategoryId);

        builder
            .HasOne(p => p.Brand)
            .WithMany(b => b.Products)
            .HasForeignKey(f => f.BrandId);

        builder.HasOne(x => x.InventoryItem)
            .WithOne(x => x.Product)
            .HasForeignKey<InventoryItem>(x => x.ProductId);
        
        builder.HasMany(p => p.Images)
            .WithOne(p => p.Product)
            .HasForeignKey(p => p.ProductId);
        
        builder
            .HasIndex(p => p.Sku)
            .IsUnique();
    }
}