using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Mappings;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FullName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Role)
            .IsRequired();

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.PhoneNumber)
            .IsRequired()
            .HasMaxLength(11);

        builder.Property(x => x.Password)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.PasswordResetCodeHash)
            .HasMaxLength(500);

        builder.Property(x => x.PasswordResetCodeExpireAt);

        builder.Property(x => x.PasswordResetAttemptsCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.HasCheckConstraint(
            "CK_Role_Valid_Values",
            "[Role] IN (0,1,2)"
        );
    }
}