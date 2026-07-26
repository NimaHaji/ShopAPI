using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Mappings;

public class OrderItemMapping : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Price)
            .IsRequired();

        builder.Property(x => x.Quantity)
            .IsRequired();

        builder
            .Property(x => x.OrderId)
            .IsRequired();

        builder.Property(x => x.ProductTitle)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.ProductId)
            .IsRequired();
    }
}