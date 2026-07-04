using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Mappings;

public class PaymentMapping : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(p => p.Id);
        
        builder
            .Property(p => p.Amount)
            .IsRequired();
        
        builder
            .Property(p => p.Description)
            .IsRequired();
        
        builder.Property(p=>p.ResNum)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.RefNum)
            .HasMaxLength(100);
        
        builder
            .Property(p => p.TraceNo);
        
        builder
            .Property(p=>p.RRN)
            .HasMaxLength(100);
        
        builder
            .Property(p=>p.PaymentStatus)
            .IsRequired();
        
        builder
            .Property(p => p.SecurePan);
        
        builder
            .Property(p=>p.PaymentGatewayStatus);
        
        builder
            .Property(p => p.CreatedAt)
            .IsRequired();
        
        builder
            .Property(p=>p.PaidAt)
            .IsRequired(false);
        
        builder
            .HasIndex(p=>p.ResNum)
            .IsUnique();

        builder
            .Property(p => p.Gateway);
        
        builder
            .Property(p => p.Authority);
    }
}