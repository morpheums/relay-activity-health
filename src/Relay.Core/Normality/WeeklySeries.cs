namespace Relay.Core.Normality;

public sealed record WeeklySeries(int SelectedWeekCount, IReadOnlyList<int> EligibleWeekCounts);
