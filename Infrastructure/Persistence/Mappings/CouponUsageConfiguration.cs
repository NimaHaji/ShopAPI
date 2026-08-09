using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Mappings;

public class CouponUsageConfiguration : IEntityTypeConfiguration<CouponUsage>
{
    public void Configure(EntityTypeBuilder<CouponUsage> builder)
    {
        builder.ToTable("CouponUsages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.DiscountAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.UsedAt)
            .IsRequired();

        builder.HasOne(x => x.Coupon)
            .WithMany(x => x.CouponUsages)
            .HasForeignKey(x => x.CouponId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Order)
            .WithOne()
            .HasForeignKey<CouponUsage>(x => x.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.OrderId)
            .IsUnique();

        builder.HasIndex(x => new
        {
            x.CouponId,
            x.UserId
        });
    }
}