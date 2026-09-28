namespace Relay.Core.Queries;

public sealed record WeeklySiteCount(string Location, DateOnly WeekStart, int Count);
