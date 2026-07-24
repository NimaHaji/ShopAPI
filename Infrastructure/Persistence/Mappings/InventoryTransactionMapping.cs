using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Mappings;

public class InventoryTransactionMapping : IEntityTypeConfiguration<InventoryTransaction>
{
    public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
    {
        builder.ToTable("InventoryTransactions");
        
        builder.HasKey(t => t.InventoryTransactionId);
        
        builder.Property(t => t.InventoryTransactionId)
            .IsRequired();

        builder.Property(t => t.InventoryItemId)
            .IsRequired();

        builder.Property(t => t.Type)
            .IsRequired();


        builder.Property(t => t.Quantity)
            .IsRequired();

        builder.Property(t => t.Reference)
            .HasMaxLength(200);

        builder.Property(t => t.Description)
            .HasMaxLength(500);

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.HasIndex(t => t.InventoryItemId);

        builder.HasIndex(t => t.CreatedAt);

        builder.HasIndex(t => new { t.InventoryItemId, t.CreatedAt });

        builder.HasIndex(t => t.Reference);
        
        builder.HasOne(t => t.InventoryItem)
            .WithMany(i => i.Transactions)
            .HasForeignKey(t => t.InventoryItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}