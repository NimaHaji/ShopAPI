using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Mappings;

public class WishlistConfiguration:IEntityTypeConfiguration<Wishlist>
{
    public void Configure(EntityTypeBuilder<Wishlist> builder)
    {
        builder.ToTable("Wishlist");
        
        builder.HasKey(w => w.Id);

        builder
            .Property(w => w.CreatedAt)
            .IsRequired();
        
        builder
            .Property(w => w.UpdatedAt)
            .IsRequired();
        
        builder
            .HasOne(w=>w.User)
            .WithOne(w=>w.Wishlist)
            .HasForeignKey<Wishlist>(w=>w.UserId);
        
        builder
            .HasIndex(w => w.UserId)
            .IsUnique();
    }
}