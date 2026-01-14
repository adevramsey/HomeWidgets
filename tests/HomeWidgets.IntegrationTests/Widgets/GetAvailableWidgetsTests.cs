using System.Net;
using System.Net.Http.Json;
using HomeWidgets.Api.Contracts.Auth;
using HomeWidgets.Core.Entities;
using HomeWidgets.Infrastructure.Data;
using HomeWidgets.IntegrationTests.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HomeWidgets.IntegrationTests.Widgets;

public class GetAvailableWidgetsTests : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
{
    // Fields
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private string _authToken = string.Empty;
    
    // InitializeAsync: Create test user, get auth token
    public GetAvailableWidgetsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
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

    }
    
    // Dispose Async
    public Task DisposeAsync() => Task.CompletedTask;

    // GetAvailableWidgets_ReturnsSeededWidgets_Successfully?
    [Fact]
    public async Task GetAvailableWidgets_ReturnsSeededWidgets_Successfully()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authToken);
        
        // Act
        var response = await _client.GetAsync("/api/widgets");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var widgets = await response.Content.ReadFromJsonAsync<List<Widget>>();
        Assert.NotNull(widgets);
        Assert.Equal(5, widgets.Count);

        Assert.Equal("Digital Clock", widgets[0].Name);
        Assert.Equal("Weather", widgets[1].Name);
        Assert.Equal("Quick Notes", widgets[2].Name);
        Assert.Equal("Calendar", widgets[3].Name);
        Assert.Equal("Task List", widgets[4].Name);
    }

    // GetAvailableWidgets_OnlyReturnsActiveWidgets_Successfully
    [Fact]
    public async Task GetAvailableWidgets_OnlyReturnsActiveWidgets_Successfully()
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _authToken);
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        // Find the clock widget and deactivate it
        var clockWidget = await dbContext.Widgets
            .FirstOrDefaultAsync(w => w.Name == "Digital Clock");
        if (clockWidget is null)
            return;
        clockWidget.Deactivate();
        await dbContext.SaveChangesAsync();
        
        // Act
        var response = await _client.GetAsync("/api/widgets");
        
        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var widgets = await response.Content.ReadFromJsonAsync<List<Widget>>();
        Assert.NotNull(widgets);
        Assert.Equal(4, widgets.Count);
        //Verify clock is NOT in the list
        Assert.DoesNotContain(widgets, w => w.Name == "Digital Clock");
        // Verify the other 4 are present
        Assert.Contains(widgets, w => w.Name == "Weather");
        Assert.Contains(widgets, w => w.Name == "Quick Notes");
        Assert.Contains(widgets, w => w.Name == "Calendar");
        Assert.Contains(widgets, w => w.Name == "Task List");
    }

    // GetAvailableWidgets_WithoutAuthentication_Returns401Unauthorized
    [Fact]
    public async Task GetAvailableWidgets_WithoutAuthentication_Returns401Unauthorized()
    {
        // Arrange
        
        // Act
        var response = await _client.GetAsync("/api/widgets");
        
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}