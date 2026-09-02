using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Mappings;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
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
            .Property(p => p.AddedAt)
            .IsRequired();

        builder
            .Property(p => p.IsDeleted)
            .IsRequired();

        builder
            .Property(p => p.DeletedAt);

        builder
            .Property(p => p.RowVersion)
            .IsRowVersion()
            .IsConcurrencyToken();

        builder
            .Property(p => p.UpdatedAt)
            .IsRequired();

        builder
            .HasOne(p => p.Category)
            .WithMany(p => p.Products)
            .HasForeignKey(f => f.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(p => p.Brand)
            .WithMany(b => b.Products)
            .HasForeignKey(f => f.BrandId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.Options)
            .WithOne(x => x.Product)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Variants)
            .WithOne(x => x.Product)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.CategoryId);

        builder.HasIndex(x => x.BrandId);

        builder.HasIndex(x => new
        {
            x.IsDeleted,
            x.AddedAt
        });

        builder.HasMany(p => p.Images)
            .WithOne(p => p.Product)
            .HasForeignKey(p => p.ProductId);
    }
}