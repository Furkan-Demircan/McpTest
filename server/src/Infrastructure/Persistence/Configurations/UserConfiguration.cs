using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.TcNo)
            .IsRequired()
            .HasMaxLength(11);

        builder.HasIndex(u => u.TcNo)
            .IsUnique();

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.MotherName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.FatherName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.BirthDate)
            .IsRequired();

        builder.Property(u => u.CreatedAt)
            .IsRequired();
    }
}
