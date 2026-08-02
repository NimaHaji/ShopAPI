using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Mappings;

public class ReviewMapping : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews");

        builder.HasKey(r => r.Id);

        builder
            .Property(r => r.StarsCount)
            .IsRequired();

        builder.Property(r => r.Comment)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(r => r.CreatedAt)
            .IsRequired();

        builder.Property(r => r.UpdatedAt);

        builder.Property(r => r.DeletedAt);

        builder.Property(r => r.IsDeleted)
            .IsRequired();

        builder.Property(r => r.UserId)
            .IsRequired();

        builder.Property(r => r.ProductId)
            .IsRequired();
        
        builder
            .Property(r=>r.ReviewStatus)
            .IsRequired();
        
        builder.HasIndex(r => new { r.ProductId, r.UserId })
            .IsUnique();
    }
}