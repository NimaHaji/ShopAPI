using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Mappings;

public class CartMapping : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        builder.ToTable("Carts");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.UserId).IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();

        builder
            .Property(x => x.UpdatedAt)
            .IsRequired();
        
        builder
            .HasMany(x => x.CartItems)
            .WithOne(cart => cart.Cart)
            .HasForeignKey(fk => fk.CartId);

        builder
            .HasOne(x => x.User)
            .WithOne(x => x.Cart)
            .HasForeignKey<Cart>(fk => fk.UserId);
    }
}