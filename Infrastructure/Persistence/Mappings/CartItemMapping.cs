using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Mappings;

public class CartItemMapping : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable("CartItems", "dbo");

        builder.HasKey(x => x.Id);
        
        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.CartId)
            .IsRequired();

        builder.Property(x => x.ProductId)
            .IsRequired();

        builder.Property(x => x.Quantity)
            .IsRequired();

        builder.HasIndex(x => x.CartId);

        builder.HasIndex(x => x.ProductId);


        builder.HasIndex(x => new { x.CartId, x.ProductId })
            .IsUnique();

        builder.HasOne(x => x.Cart)
            .WithMany(c => c.CartItems)
            .HasForeignKey(x => x.CartId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Product)
            .WithMany()
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}