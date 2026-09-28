namespace Relay.Core.Normality;

public sealed record WeeklySeries(int SelectedWeekCount, IReadOnlyList<SeriesWeek> PrecedingWeeks);
