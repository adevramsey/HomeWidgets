using HomeWidgets.Core.Interfaces;
using HomeWidgets.Infrastructure.Data;
using HomeWidgets.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HomeWidgets.Infrastructure;

/// <summary>
/// Extension methods for registering Infrastructure services with DI container.
/// Keeps Program.cs clean by grouping related registrations.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all Infrastructure services (DbContext, Repositories).
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="connectionString">PostgreSQL connection string.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        // Register DbContext with PostgreSQL
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Register repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IWidgetRepository, WidgetRepository>();

        return services;
    }
}

