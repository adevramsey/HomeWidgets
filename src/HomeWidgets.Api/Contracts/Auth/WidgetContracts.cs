namespace HomeWidgets.Api.Contracts.Auth;

public record AddWidgetRequest
{
    public required Guid WidgetId { get; init; }
    public int? Position { get; init; }
}

public record EditWidgetRequest
{
    public required Guid WidgetId { get; init; }
}

public record ReorderWidgetsRequest
{
    public required List<Guid> WidgetIdsInOrder { get; init; }
}