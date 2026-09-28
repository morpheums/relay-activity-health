using Relay.Core.Calendar;
using Relay.Core.Queries;

namespace Relay.Core.Normality;

public sealed class WeeklyGridBuilder : IWeeklyGridBuilder
{
    public IReadOnlyList<LocationSeries> BuildLocationSeries(
        WeekWindow selectedWeek,
        IReadOnlyList<WeekWindow> baselineWindows,
        IReadOnlyList<SiteFirstActivity> siteFirstActivities,
        IReadOnlyList<WeeklySiteCount> weeklySiteCounts) =>
        throw new NotImplementedException();

    public WeeklySeries BuildAccountSeries(
        WeekWindow selectedWeek,
        IReadOnlyList<WeekWindow> baselineWindows,
        IReadOnlyList<SiteFirstActivity> siteFirstActivities,
        IReadOnlyList<WeeklySiteCount> weeklySiteCounts) =>
        throw new NotImplementedException();
}
