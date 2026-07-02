using Domain.Entites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Mappings;

public class OrderMapping:IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        
        builder.HasKey(o => o.Id);
        
        builder.Property(o=>o.UserId).IsRequired();
        
        builder.Property(o=>o.TotalPrice).IsRequired();
        
        builder.Property(o=>o.OrderStatus).IsRequired();
        
        builder.Property(o=>o.CreateAt).IsRequired();

        builder
            .HasMany(o => o.Items)
            .WithOne(o => o.Order)
            .HasForeignKey(o => o.OrderId);
    }
}