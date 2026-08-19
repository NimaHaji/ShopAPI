using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Mappings;

public class InventoryItemMapping
    : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(
        EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("InventoryItems", "dbo");

        builder.HasKey(i => i.InventoryId);

        builder.Property(i => i.InventoryId)
            .HasColumnName("InventoryId")
            .IsRequired();

        builder.Property(i => i.ProductVariantId)
            .HasColumnName("ProductVariantId")
            .IsRequired();

        builder.Property(i => i.StockQuantity)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(i => i.ReservedQuantity)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(i => i.LastUpdated)
            .IsRequired();

        builder.Property(i => i.RowVersion)
            .IsRequired()
            .IsRowVersion()
            .IsConcurrencyToken();

        builder.HasIndex(i => i.ProductVariantId)
            .IsUnique();

        builder.HasIndex(i => i.LastUpdated);

        builder.HasOne(i => i.ProductVariant)
            .WithOne(v=>v.InventoryItem)
            .HasForeignKey<InventoryItem>(
                i => i.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(i => i.Transactions)
            .WithOne(t => t.InventoryItem)
            .HasForeignKey(t => t.InventoryItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}