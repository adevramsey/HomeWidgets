using System.Text;
using HomeWidgets.Core.Interfaces;
using HomeWidgets.Infrastructure.Authentication;
using HomeWidgets.Infrastructure.Data;
using HomeWidgets.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace HomeWidgets.Infrastructure;

/// <summary>
/// Extension methods for registering Infrastructure services with DI container.
/// Keeps Program.cs clean by grouping related registrations.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all Infrastructure services (DbContext, Repositories, Auth).
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        // Register DbContext with PostgreSQL
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Register repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IWidgetRepository, WidgetRepository>();

        // Register authentication services
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();

        // Configure JWT settings
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException("JwtSettings not found in configuration.");

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        // Configure JWT authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
            };
        });

        services.AddAuthorization();

        return services;
    }
}

