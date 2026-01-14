using HomeWidgets.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HomeWidgets.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core configuration for the UserWidget join entity.
/// </summary>
public class UserWidgetConfiguration : IEntityTypeConfiguration<UserWidget>
{
    public void Configure(EntityTypeBuilder<UserWidget> builder)
    {
        builder.ToTable("user_widgets");

        builder.HasKey(uw => uw.Id);

        // Foreign keys are configured, but relationships are set up in UserConfiguration
        builder.Property(uw => uw.UserId)
            .IsRequired();

        builder.Property(uw => uw.WidgetId)
            .IsRequired();

        // Composite unique index: a user can only have each widget once
        builder.HasIndex(uw => new { uw.UserId, uw.WidgetId })
            .IsUnique();

        // Order for dashboard positioning
        builder.Property(uw => uw.Order)
            .IsRequired();

        // Whether this widget is active on the user's dashboard
        builder.Property(uw => uw.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // User-specific config overrides
        builder.Property(uw => uw.Config)
            .HasColumnType("jsonb");

        builder.Property(uw => uw.CreatedAt)
            .IsRequired();

        // Relationship to Widget
        builder.HasOne(uw => uw.Widget)
            .WithMany()
            .HasForeignKey(uw => uw.WidgetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

