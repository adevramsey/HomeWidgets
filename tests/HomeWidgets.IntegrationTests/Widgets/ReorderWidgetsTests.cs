using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using HomeWidgets.Api.Contracts.Auth;
using HomeWidgets.Core.Entities;
using HomeWidgets.Infrastructure.Data;
using HomeWidgets.IntegrationTests.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace  HomeWidgets.IntegrationTests.Widgets;

public class ReorderWidgetsTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private string _authToken = string.Empty;
    private Guid _testUserId;
    private Guid _testWidgetIdClock;
    private Guid _testWidgetIdWeather;
    private Guid _testWidgetIdNotes;
    
    public ReorderWidgetsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        // Create a test user and get auth token
        var registerRequest = new RegisterRequest 
        { 
            Email = $"widgettest-{Guid.NewGuid()}@example.com", 
            Password = "P@ssword123", 
            DisplayName = "Widget Test User" 
        };
        
        var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        
        var authResponse = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>();
        _authToken = authResponse!.Token;
        _testUserId = authResponse.UserId;
        
        // Seed a test widget in the database with unique ComponentType
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var uniqueComponentType1 = $"Test{Guid.NewGuid().ToString()
            .Replace("-", "").Substring(0, 8)}Widget";
        var uniqueComponentType2 = $"Test{Guid.NewGuid().ToString()
            .Replace("-", "").Substring(0, 8)}Widget";
        var uniqueComponentType3 = $"Test{Guid.NewGuid().ToString()
            .Replace("-", "").Substring(0, 8)}Widget";
        var widgetClock = new Widget("Test Clock", 
            "A test clock widget", uniqueComponentType1, "Schedule");
        var widgetWeather = new Widget("Test Weather",
            "A test weather widget", uniqueComponentType2, "Weather");
        var widgetNotes = new Widget("Test Notes",
            "A test notes widget", uniqueComponentType3, "Notes");
        dbContext.Widgets.Add(widgetClock);
        dbContext.Widgets.Add(widgetWeather);
        dbContext.Widgets.Add(widgetNotes);
        await dbContext.SaveChangesAsync();
        _testWidgetIdClock = widgetClock.Id;
        _testWidgetIdWeather = widgetWeather.Id;
        _testWidgetIdNotes = widgetNotes.Id;
    }
    
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task ReorderWidgets_WithValidRequest_UpdatesWidgetOrder()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
        // add clock widget
        await _client.PostAsJsonAsync("/api/widgets/me", 
            new AddWidgetRequest { WidgetId = _testWidgetIdClock });
        // add weather widget
        await _client.PostAsJsonAsync("/api/widgets/me", 
            new AddWidgetRequest { WidgetId = _testWidgetIdWeather });
        
        // Act
        var reorderRequest = new ReorderWidgetsRequest
        {
            WidgetIdsInOrder = new List<Guid>
            {
                _testWidgetIdWeather,
                _testWidgetIdClock
            }
        };
        var response = await _client.PutAsJsonAsync("/api/widgets/me/reorder", reorderRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        // GET widgets to verify new order
        var getResponse = await _client.GetAsync("/api/widgets/me");
        var widgets = await getResponse.Content.ReadFromJsonAsync<List<Widget>>();

        Assert.NotNull(widgets);
        Assert.Equal(2, widgets.Count);
        
        // Verify clock is now second
        Assert.Equal("Test Clock", widgets[1].Name);

    }
    
    [Fact]
    public async Task ReorderWidgets_WithoutAuthentication_Returns401Unauthorized()
    {
        // Arrange - Don't set Authorization header
        var reorderRequest = new ReorderWidgetsRequest
        {
            WidgetIdsInOrder = new List<Guid>
            {
                _testWidgetIdClock,
                _testWidgetIdWeather
            }
        };
        
        // Act
        var response = await _client.PutAsJsonAsync("/api/widgets/me/reorder", reorderRequest);
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    
    [Fact]
    public async Task ReorderWidgets_WithPartialList_ReordersOnlyProvidedWidgets()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
        
        // Add 3 widgets: Clock (0), Weather (1), Notes (2)
        await _client.PostAsJsonAsync("/api/widgets/me", 
            new AddWidgetRequest { WidgetId = _testWidgetIdClock });
        await _client.PostAsJsonAsync("/api/widgets/me", 
            new AddWidgetRequest { WidgetId = _testWidgetIdWeather });
        await _client.PostAsJsonAsync("/api/widgets/me", 
            new AddWidgetRequest { WidgetId = _testWidgetIdNotes });
        
        // Act - Reorder only Weather and Clock (leave Notes out)
        var reorderRequest = new ReorderWidgetsRequest
        {
            WidgetIdsInOrder = new List<Guid>
            {
                _testWidgetIdWeather,  // Should become position 0
                _testWidgetIdClock     // Should become position 1
                // Notes not included - should stay at position 2
            }
        };
        var response = await _client.PutAsJsonAsync("/api/widgets/me/reorder", reorderRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        // Verify new order
        var getResponse = await _client.GetAsync("/api/widgets/me");
        var widgets = await getResponse.Content.ReadFromJsonAsync<List<Widget>>();

        Assert.NotNull(widgets);
        Assert.Equal(3, widgets.Count);
        
        // Verify reordered widgets
        Assert.Equal("Test Weather", widgets[0].Name);  // Position 0
        Assert.Equal("Test Clock", widgets[1].Name);     // Position 1
        Assert.Equal("Test Notes", widgets[2].Name);     // Position 2 (unchanged)
    }
    
    [Fact]
    public async Task ReorderWidgets_WithNonExistentWidgetIds_IgnoresThem()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
        
        // Add 2 widgets
        await _client.PostAsJsonAsync("/api/widgets/me", 
            new AddWidgetRequest { WidgetId = _testWidgetIdClock });
        await _client.PostAsJsonAsync("/api/widgets/me", 
            new AddWidgetRequest { WidgetId = _testWidgetIdWeather });
        
        // Act - Include a random GUID that doesn't exist
        var nonExistentWidgetId = Guid.NewGuid();
        var reorderRequest = new ReorderWidgetsRequest
        {
            WidgetIdsInOrder = new List<Guid>
            {
                _testWidgetIdClock,
                nonExistentWidgetId,      // This doesn't exist - should be ignored
                _testWidgetIdWeather
            }
        };
        var response = await _client.PutAsJsonAsync("/api/widgets/me/reorder", reorderRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        // Verify only the 2 existing widgets are reordered
        var getResponse = await _client.GetAsync("/api/widgets/me");
        var widgets = await getResponse.Content.ReadFromJsonAsync<List<Widget>>();

        Assert.NotNull(widgets);
        Assert.Equal(2, widgets.Count);  // Still only 2 widgets
        
        // Verify order (non-existent GUID was ignored)
        Assert.Equal("Test Clock", widgets[0].Name);
        Assert.Equal("Test Weather", widgets[1].Name);
    }
    
    [Fact]
    public async Task ReorderWidgets_WithEmptyList_ReturnsNoContent()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
        
        // Add 2 widgets
        await _client.PostAsJsonAsync("/api/widgets/me", 
            new AddWidgetRequest { WidgetId = _testWidgetIdClock });
        await _client.PostAsJsonAsync("/api/widgets/me", 
            new AddWidgetRequest { WidgetId = _testWidgetIdWeather });
        
        // Act - Send empty list
        var reorderRequest = new ReorderWidgetsRequest
        {
            WidgetIdsInOrder = new List<Guid>()  // Empty list
        };
        var response = await _client.PutAsJsonAsync("/api/widgets/me/reorder", reorderRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        // Verify widgets unchanged (no-op)
        var getResponse = await _client.GetAsync("/api/widgets/me");
        var widgets = await getResponse.Content.ReadFromJsonAsync<List<Widget>>();

        Assert.NotNull(widgets);
        Assert.Equal(2, widgets.Count);
        
        // Widgets should still be in original order
        Assert.Equal("Test Clock", widgets[0].Name);
        Assert.Equal("Test Weather", widgets[1].Name);
    }
    
    [Fact]
    public async Task ReorderWidgets_WithDuplicateIds_HandlesProperly()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);
        
        // Add 2 widgets
        await _client.PostAsJsonAsync("/api/widgets/me", 
            new AddWidgetRequest { WidgetId = _testWidgetIdClock });
        await _client.PostAsJsonAsync("/api/widgets/me", 
            new AddWidgetRequest { WidgetId = _testWidgetIdWeather });
        
        // Act - Send list with duplicate IDs
        var reorderRequest = new ReorderWidgetsRequest
        {
            WidgetIdsInOrder = new List<Guid>
            {
                _testWidgetIdClock,
                _testWidgetIdClock,   // Duplicate!
                _testWidgetIdWeather
            }
        };
        var response = await _client.PutAsJsonAsync("/api/widgets/me/reorder", reorderRequest);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        
        // Verify behavior - Clock gets last position it was assigned (1), Weather gets 2
        var getResponse = await _client.GetAsync("/api/widgets/me");
        var widgets = await getResponse.Content.ReadFromJsonAsync<List<Widget>>();

        Assert.NotNull(widgets);
        Assert.Equal(2, widgets.Count);
        
        // Current domain logic: Clock gets order=0, then order=1 (last assigned)
        // Weather gets order=2
        // Result after OrderBy: Clock(1), Weather(2)
        Assert.Equal("Test Clock", widgets[0].Name);
        Assert.Equal("Test Weather", widgets[1].Name);
    }
}