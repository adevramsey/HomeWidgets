using HomeWidgets.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeWidgets.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for the Widget entity.
/// </summary>
public class WidgetConfiguration : IEntityTypeConfiguration<Widget>
{
    public void Configure(EntityTypeBuilder<Widget> builder)
    {
        builder.ToTable("widgets");

        builder.HasKey(w => w.Id);

        builder.Property(w => w.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(w => w.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(w => w.ComponentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(w => w.ComponentType)
            .IsUnique();

        builder.Property(w => w.Icon)
            .HasMaxLength(50);

        builder.Property(w => w.DefaultConfig)
            .HasColumnType("jsonb"); // PostgreSQL JSON type

        builder.Property(w => w.CreatedAt)
            .IsRequired();
    }
}

