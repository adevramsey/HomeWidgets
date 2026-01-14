using System.Text.Json.Serialization;

namespace HomeWidgets.Core.Entities;

/// <summary>
/// Base class for all domain entities.
/// Provides a common identifier and equality comparison.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier for the entity.
    /// Using Guid for globally unique, non-sequential IDs (better for distributed systems).
    /// </summary>
    [JsonInclude]
    public Guid Id { get; protected set; } = Guid.NewGuid();

    /// <summary>
    /// Timestamp when the entity was created.
    /// </summary>
    [JsonInclude]
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the entity was last modified.
    /// </summary>
    [JsonInclude]
    public DateTime? UpdatedAt { get; protected set; }

    /// <summary>
    /// Marks the entity as modified.
    /// </summary>
    public void MarkAsUpdated() => UpdatedAt = DateTime.UtcNow;

    // Entity equality is based on Id, not reference
    public override bool Equals(object? obj)
    {
        if (obj is not BaseEntity other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        // If either has a default Id, they're not equal
        if (Id == Guid.Empty || other.Id == Guid.Empty)
            return false;

        return Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(BaseEntity? a, BaseEntity? b)
    {
        if (a is null && b is null)
            return true;

        if (a is null || b is null)
            return false;

        return a.Equals(b);
    }

    public static bool operator !=(BaseEntity? a, BaseEntity? b) => !(a == b);
}

