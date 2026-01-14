namespace HomeWidgets.Core.Entities;

/// <summary>
/// Represents a user in the system.
/// Contains authentication info and navigation to user's widgets.
/// </summary>
public class User : BaseEntity
{
    // Private parameterless constructor for EF Core
    private User() { }

    /// <summary>
    /// Creates a new user with required fields.
    /// </summary>
    public User(string email, string passwordHash, string? displayName = null)
    {
        SetEmail(email);
        PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        DisplayName = displayName ?? email.Split('@')[0]; // Default to email prefix
    }

    /// <summary>
    /// User's email address. Used for login.
    /// </summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>
    /// Hashed password. Never store plain text!
    /// </summary>
    public string PasswordHash { get; private set; } = string.Empty;

    /// <summary>
    /// Display name shown in the UI.
    /// </summary>
    public string DisplayName { get; private set; } = string.Empty;

    /// <summary>
    /// Whether the user's email has been verified.
    /// </summary>
    public bool IsEmailVerified { get; private set; }

    /// <summary>
    /// Navigation property to user's widgets (many-to-many through UserWidget).
    /// </summary>
    private readonly List<UserWidget> _userWidgets = [];
    public IReadOnlyCollection<UserWidget> UserWidgets => _userWidgets.AsReadOnly();

    // ============ Domain Methods ============

    /// <summary>
    /// Updates the user's email with validation.
    /// </summary>
    public void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));

        if (!email.Contains('@') || !email.Contains('.'))
            throw new ArgumentException("Invalid email format.", nameof(email));

        Email = email.ToLowerInvariant().Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Updates the password hash.
    /// </summary>
    public void SetPasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty.", nameof(passwordHash));

        PasswordHash = passwordHash;
        MarkAsUpdated();
    }

    /// <summary>
    /// Updates the display name.
    /// </summary>
    public void SetDisplayName(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("Display name cannot be empty.", nameof(displayName));

        DisplayName = displayName.Trim();
        MarkAsUpdated();
    }

    /// <summary>
    /// Marks the email as verified.
    /// </summary>
    public void VerifyEmail()
    {
        IsEmailVerified = true;
        MarkAsUpdated();
    }

    /// <summary>
    /// Adds a widget to the user's dashboard.
    /// </summary>
    public UserWidget AddWidget(Widget widget, int? position = null)
    {
        ArgumentNullException.ThrowIfNull(widget);

        // Check if widget already added
        if (_userWidgets.Any(uw => uw.WidgetId == widget.Id))
            throw new InvalidOperationException($"Widget '{widget.Name}' is already on the dashboard.");

        // Calculate position: either specified or append to end
        var order = position ?? (_userWidgets.Count > 0 ? _userWidgets.Max(uw => uw.Order) + 1 : 0);
        bool isActive = true;

        var userWidget = new UserWidget(this, widget, order, isActive);
        _userWidgets.Add(userWidget);
        MarkAsUpdated();

        return userWidget;
    }

    /// <summary>
    /// Removes a widget from the user's dashboard.
    /// </summary>
    public void RemoveWidget(Guid widgetId)
    {
        var userWidget = _userWidgets.FirstOrDefault(uw => uw.WidgetId == widgetId);
        if (userWidget is null)
            throw new InvalidOperationException("Widget not found on dashboard.");

        _userWidgets.Remove(userWidget);
        MarkAsUpdated();
    }

    /// <summary>
    /// Reorders widgets on the dashboard.
    /// </summary>
    public void ReorderWidgets(IEnumerable<Guid> widgetIdsInOrder)
    {
        var orderedIds = widgetIdsInOrder.ToList();
        var order = 0;

        foreach (var widgetId in orderedIds)
        {
            var userWidget = _userWidgets.FirstOrDefault(uw => uw.WidgetId == widgetId);
            if (userWidget is not null)
            {
                userWidget.SetOrder(order++);
            }
        }

        MarkAsUpdated();
    }

    /// <summary>
    /// Activates a widget on the user's dashboard.
    /// </summary>
    /// <param name="widget"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public void ActivateWidget(Widget widget)
    {
        var userWidget = _userWidgets.FirstOrDefault(uw => uw.WidgetId == widget.Id);
        if (userWidget is null)
            throw new InvalidOperationException("Widget not found on dashboard.");

        userWidget.SetActive(true);
        MarkAsUpdated();
    }

    /// <summary>
    /// Deactivates a widget on the user's dashboard.
    /// </summary>
    /// <param name="widget"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public void DeactivateWidget(Widget widget)
    {
        var userWidget = _userWidgets.FirstOrDefault(uw => uw.WidgetId == widget.Id);
        if (userWidget is null)
            throw new InvalidOperationException("Widget not found on dashboard.");
        
        userWidget.SetActive(false);
        MarkAsUpdated();
    }
}

