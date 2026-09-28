namespace Relay.Core.Calendar;

public sealed record WeekWindow(DateOnly WeekStart, DateTime StartUtc, DateTime EndUtc);
