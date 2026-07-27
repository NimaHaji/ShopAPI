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
            .IsRequired();

        builder
            .Property(p => p.Description)
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
            .HasOne(p => p.Category)
            .WithMany(p => p.Products)
            .HasForeignKey(f => f.CategoryId);

        builder
            .HasOne(p => p.Brand)
            .WithMany(b => b.Products)
            .HasForeignKey(f => f.BrandId);

    }
}