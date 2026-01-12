namespace HomeWidgets.Core.Entities;

/// <summary>
/// Represents a widget template in the system.
/// This is the "master" widget definition, not a user's instance of it.
/// </summary>
public class Widget : BaseEntity
{
    // Private parameterless constructor for EF Core
    private Widget() { }

    /// <summary>
    /// Creates a new widget definition.
    /// </summary>
    public Widget(string name, string description, string componentType, string? icon = null)
    {
        SetName(name);
        SetDescription(description);
        SetComponentType(componentType);
        Icon = icon ?? "Widgets"; // Default MudBlazor icon
    }

    /// <summary>
    /// Display name of the widget.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Description shown when browsing available widgets.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// The Blazor component type name (e.g., "ClockWidget", "WeatherWidget").
    /// Used to dynamically render the component.
    /// </summary>
    public string ComponentType { get; private set; } = string.Empty;

    /// <summary>
    /// MudBlazor icon name for display in the widget library.
    /// </summary>
    public string Icon { get; private set; } = string.Empty;

    /// <summary>
    /// Default configuration JSON for new instances of this widget.
    /// </summary>
    public string? DefaultConfig { get; private set; }

    /// <summary>
    /// Whether this widget is available for users to add.
    /// </summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>
    /// Sort order in the widget library.
    /// </summary>
    public int DisplayOrder { get; private set; }

    // ============ Domain Methods ============

    public void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Widget name cannot be empty.", nameof(name));

        if (name.Length > 100)
            throw new ArgumentException("Widget name cannot exceed 100 characters.", nameof(name));

        Name = name.Trim();
        MarkAsUpdated();
    }

    public void SetDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Widget description cannot be empty.", nameof(description));

        Description = description.Trim();
        MarkAsUpdated();
    }

    public void SetComponentType(string componentType)
    {
        if (string.IsNullOrWhiteSpace(componentType))
            throw new ArgumentException("Component type cannot be empty.", nameof(componentType));

        // Component type should be PascalCase and end with "Widget"
        if (!componentType.EndsWith("Widget"))
            throw new ArgumentException("Component type must end with 'Widget'.", nameof(componentType));

        ComponentType = componentType.Trim();
        MarkAsUpdated();
    }

    public void SetDefaultConfig(string? configJson)
    {
        DefaultConfig = configJson;
        MarkAsUpdated();
    }

    public void SetDisplayOrder(int order)
    {
        if (order < 0)
            throw new ArgumentException("Display order cannot be negative.", nameof(order));

        DisplayOrder = order;
        MarkAsUpdated();
    }

    public void Activate() 
    {
        IsActive = true;
        MarkAsUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }
}

