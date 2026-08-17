using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Mappings;

public class ProductOptionConfiguration
    : IEntityTypeConfiguration<ProductOption>
{
    public void Configure(
        EntityTypeBuilder<ProductOption> builder)
    {
        builder.ToTable("ProductOptions");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.HasOne(x => x.Product)
            .WithMany(x => x.Options)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(x => x.Values)
            .WithOne(x => x.ProductOption)
            .HasForeignKey(x => x.ProductOptionId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(x => new
            {
                x.ProductId,
                x.Name
            })
            .IsUnique();
    }
}