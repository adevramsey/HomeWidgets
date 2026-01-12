namespace HomeWidgets.Api.Contracts.Auth;

public record AddWidgetRequest
{
    public required Guid WidgetId { get; init; }
    public int? Position { get; init; }
}