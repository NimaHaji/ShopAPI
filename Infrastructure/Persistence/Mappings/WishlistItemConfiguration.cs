using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Mappings;

public class WishlistItemConfiguration:IEntityTypeConfiguration<WishlistItem>
{
    public void Configure(EntityTypeBuilder<WishlistItem> builder)
    {
        builder.ToTable("WishlistItems");

        builder.HasKey(w => w.Id);

        builder
            .Property(w => w.AddedAt)
            .IsRequired();

        builder
            .HasOne(w => w.Wishlist)
            .WithMany(w => w.WishlistItems)
            .HasForeignKey(w => w.WishlistId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(w => w.Product)
            .WithMany()
            .HasForeignKey(w => w.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasIndex(x => new { x.WishlistId, x.ProductId })
            .IsUnique();
    }
}