using HomeWidgets.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeWidgets.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for the User entity.
/// Defines table name, column constraints, and indexes.
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Table name (PostgreSQL convention: lowercase with underscores)
        builder.ToTable("users");

        // Primary key
        builder.HasKey(u => u.Id);

        // Email: required, unique, max length
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        // Password hash: required
        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(512);

        // Display name: required, max length
        builder.Property(u => u.DisplayName)
            .IsRequired()
            .HasMaxLength(100);

        // Timestamps
        builder.Property(u => u.CreatedAt)
            .IsRequired();

        // Relationship: User has many UserWidgets
        builder.HasMany(u => u.UserWidgets)
            .WithOne(uw => uw.User)
            .HasForeignKey(uw => uw.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

