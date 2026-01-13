using HomeWidgets.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HomeWidgets.Infrastructure.Data;

public class DbSeeder
{
    private readonly AppDbContext _context; // add widgets to database
    private readonly ILogger<DbSeeder> _logger; // to log what's happening
    
    // Dependency Injection (Constructor)
    // What this does - receives dependencies from DI and stores them in fields.
    public DbSeeder(AppDbContext context, ILogger<DbSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    // Public SeedAsync method
    // What this does:
    // 1. Ensures migrations are applied.
    // 2. Checks if widgets table is empty.
    // 3. If empty, seeds widgets.
    // 4. If not empty, skips seeding.
    // 5. Logs everything.
    // 6. Handles errors.
    public async Task SeedAsync()
    {
        try
        {
            // Step 1 - apply any pending migrations
            await _context.Database.MigrateAsync();
            
            // Step 2 - check if widgets already exist
            if (!await _context.Widgets.AnyAsync())
            {
                _logger.LogInformation("No widgets found. Seeding initial widget library...");
                await SeedWidgetsAsync();
                _logger.LogInformation("Widget library seeded successfully.");
            }
            else
            {
                _logger.LogInformation("Widget library already exists. Skipping seed.");
            }
        } catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task SeedWidgetsAsync()
    {
        var widgets = new[]
        {
            // Widget 1: Digital Clock
            CreateWidget(
                name: "Digital Clock",
                description: "Displays current time in digital format with customizable time zones.",
                componentType: "ClockWidget",
                icon: "Schedule",
                displayOrder: 1),

            // Widget 2: Weather
            CreateWidget(
                name: "Weather",
                description: "Shows current weather conditions and forecast for your location",
                componentType: "WeatherWidget",
                icon: "Cloud",
                displayOrder: 2),

            // Widget 3: Quick Notes
            CreateWidget(
                name: "Quick Notes",
                description: "Simple notepad for quick thoughts and reminders",
                componentType: "NotesWidget",
                icon: "Note",
                displayOrder: 3),

            // Widget 4: Calendar
            CreateWidget(
                name: "Calendar",
                description: "Displays upcoming events and appointments from your calendar",
                componentType: "CalendarWidget",
                icon: "Event",
                displayOrder: 4),

            //Widget 5: Task List
            CreateWidget(
                name: "Task List",
                description: "Manage your to-do list and track completed tasks",
                componentType: "TaskListWidget",
                icon: "CheckCircle",
                displayOrder: 5)
        };

        await _context.Widgets.AddRangeAsync(widgets);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Seeded {Count} widget templates.", widgets.Length);
    }

    private Widget CreateWidget(string name, string description, string componentType,
        string icon, int displayOrder)
    {
        var widget = new Widget(name, description, componentType, icon);
        widget.SetDisplayOrder(displayOrder);
        return widget;
    }
    
}