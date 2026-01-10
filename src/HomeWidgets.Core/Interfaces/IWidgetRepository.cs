using HomeWidgets.Core.Entities;

namespace HomeWidgets.Core.Interfaces;

/// <summary>
/// Repository interface for Widget entity operations.
/// Extends generic repository with widget-specific queries.
/// </summary>
public interface IWidgetRepository : IRepository<Widget>
{
    /// <summary>
    /// Gets all active widgets available for users to add.
    /// </summary>
    Task<IReadOnlyList<Widget>> GetActiveWidgetsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a widget by its component type name.
    /// </summary>
    Task<Widget?> GetByComponentTypeAsync(string componentType, CancellationToken cancellationToken = default);
}

