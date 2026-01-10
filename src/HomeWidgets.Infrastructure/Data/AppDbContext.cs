using HomeWidgets.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace HomeWidgets.Infrastructure.Data;

/// <summary>
/// Entity Framework Core database context for HomeWidgets.
/// This is the main entry point for database operations.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Users table.
    /// </summary>
    public DbSet<User> Users => Set<User>();

    /// <summary>
    /// Widgets table (widget templates/definitions).
    /// </summary>
    public DbSet<Widget> Widgets => Set<Widget>();

    /// <summary>
    /// UserWidgets table (join table for user-widget relationships).
    /// </summary>
    public DbSet<UserWidget> UserWidgets => Set<UserWidget>();

    /// <summary>
    /// Configure entity mappings and relationships.
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}

