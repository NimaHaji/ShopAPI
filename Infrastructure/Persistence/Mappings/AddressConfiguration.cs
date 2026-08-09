using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Mappings;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.ToTable("Addresses");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.AddressTitle)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.ReceiverName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.PhoneNumber)
            .IsRequired()
            .HasMaxLength(11);

        builder.Property(x => x.Province)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.City)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.AddressLine)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.PostalCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.IsDefault)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasOne(x => x.User)
            .WithMany(x => x.Addresses)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.UserId);
    }
}