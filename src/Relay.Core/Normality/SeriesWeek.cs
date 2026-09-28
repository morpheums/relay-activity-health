namespace Relay.Core.Normality;

public sealed record SeriesWeek(DateOnly WeekStart, int Count, bool IsEligible);
