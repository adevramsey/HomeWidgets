using HomeWidgets.Core.Entities;
using HomeWidgets.Core.Interfaces;
using HomeWidgets.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HomeWidgets.Infrastructure.Repositories;

/// <summary>
/// Widget repository implementation with widget-specific queries.
/// </summary>
public class WidgetRepository : Repository<Widget>, IWidgetRepository
{
    public WidgetRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Widget>> GetActiveWidgetsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(w => w.IsActive)
            .OrderBy(w => w.DisplayOrder)
            .ThenBy(w => w.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Widget?> GetByComponentTypeAsync(string componentType, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(w => w.ComponentType == componentType, cancellationToken);
    }
}

