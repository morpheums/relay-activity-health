using Relay.Core.Calendar;
using Relay.Core.Queries;

namespace Relay.Core.Normality;

public interface IWeeklyGridBuilder
{
    IReadOnlyList<LocationSeries> BuildLocationSeries(
        WeekWindow selectedWeek,
        IReadOnlyList<WeekWindow> baselineWindows,
        IReadOnlyList<SiteFirstActivity> siteFirstActivities,
        IReadOnlyList<WeeklySiteCount> weeklySiteCounts);

    WeeklySeries BuildAccountSeries(
        WeekWindow selectedWeek,
        IReadOnlyList<WeekWindow> baselineWindows,
        IReadOnlyList<SiteFirstActivity> siteFirstActivities,
        IReadOnlyList<WeeklySiteCount> weeklySiteCounts);
}
