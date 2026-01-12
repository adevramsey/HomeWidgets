namespace HomeWidgets.Core.Entities;

/// <summary>
/// Join entity representing a widget on a user's dashboard.
/// Stores user-specific configuration and position.
/// </summary>
public class UserWidget : BaseEntity
{
    // Private parameterless constructor for EF Core
    private UserWidget() { }

    /// <summary>
    /// Creates a new user-widget association.
    /// </summary>
    internal UserWidget(User user, Widget widget, int order, bool isActive)
    {
        UserId = user.Id;
        User = user;
        WidgetId = widget.Id;
        Widget = widget;
        Order = order;
        IsActive = isActive;
    }

    /// <summary>
    /// Foreign key to the user.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Navigation property to the user.
    /// </summary>
    public User User { get; private set; } = null!;

    /// <summary>
    /// Foreign key to the widget template.
    /// </summary>
    public Guid WidgetId { get; private set; }

    /// <summary>
    /// Navigation property to the widget template.
    /// </summary>
    public Widget Widget { get; private set; } = null!;

    /// <summary>
    /// Position/order of this widget on the user's dashboard.
    /// Lower numbers appear first.
    /// </summary>
    public int Order { get; private set; }
    
    public bool IsActive { get; private set; }

    /// <summary>
    /// User-specific configuration JSON (overrides widget's default config).
    /// </summary>
    public string? Config { get; private set; }

    // ============ Domain Methods ============

    /// <summary>
    /// Updates the widget's position on the dashboard.
    /// </summary>
    internal void SetOrder(int order)
    {
        if (order < 0)
            throw new ArgumentException("Order cannot be negative.", nameof(order));

        Order = order;
        MarkAsUpdated();
    }

    /// <summary>
    /// Updates the user-specific configuration.
    /// </summary>
    public void SetConfig(string? configJson)
    {
        Config = configJson;
        MarkAsUpdated();
    }
}

