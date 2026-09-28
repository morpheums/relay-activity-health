using Relay.Core.Calendar;
using Relay.Core.Queries;

namespace Relay.Core.Normality;

public sealed class WeeklyGridBuilder : IWeeklyGridBuilder
{
    public IReadOnlyList<LocationSeries> BuildLocationSeries(
        WeekWindow selectedWeek,
        IReadOnlyList<WeekWindow> baselineWindows,
        IReadOnlyList<SiteFirstActivity> siteFirstActivities,
        IReadOnlyList<WeeklySiteCount> weeklySiteCounts)
    {
        var countsBySiteAndWeek = weeklySiteCounts
            .GroupBy(weeklySiteCount => (weeklySiteCount.Location, weeklySiteCount.WeekStart))
            .ToDictionary(group => group.Key, group => group.Sum(weeklySiteCount => weeklySiteCount.Count));

        int CountFor(string location, DateOnly weekStart) =>
            countsBySiteAndWeek.GetValueOrDefault((location, weekStart));

        return siteFirstActivities
            .Where(site => site.FirstActivityUtc < selectedWeek.EndUtc)
            .Select(site => new LocationSeries(
                site.Location,
                new WeeklySeries(
                    CountFor(site.Location, selectedWeek.WeekStart),
                    EligibleWindows(baselineWindows, site.FirstActivityUtc)
                        .Select(window => CountFor(site.Location, window.WeekStart))
                        .ToList())))
            .ToList();
    }

    public WeeklySeries BuildAccountSeries(
        WeekWindow selectedWeek,
        IReadOnlyList<WeekWindow> baselineWindows,
        IReadOnlyList<SiteFirstActivity> siteFirstActivities,
        IReadOnlyList<WeeklySiteCount> weeklySiteCounts)
    {
        var accountCountsByWeek = weeklySiteCounts
            .GroupBy(weeklySiteCount => weeklySiteCount.WeekStart)
            .ToDictionary(group => group.Key, group => group.Sum(weeklySiteCount => weeklySiteCount.Count));

        var selectedWeekCount = accountCountsByWeek.GetValueOrDefault(selectedWeek.WeekStart);
        if (siteFirstActivities.Count == 0)
        {
            return new WeeklySeries(selectedWeekCount, []);
        }

        var accountFirstActivityUtc = siteFirstActivities.Min(site => site.FirstActivityUtc);
        var eligibleWeekCounts = EligibleWindows(baselineWindows, accountFirstActivityUtc)
            .Select(window => accountCountsByWeek.GetValueOrDefault(window.WeekStart))
            .ToList();
        return new WeeklySeries(selectedWeekCount, eligibleWeekCounts);
    }

    private static IEnumerable<WeekWindow> EligibleWindows(IReadOnlyList<WeekWindow> baselineWindows, DateTime firstActivityUtc) =>
        baselineWindows.Where(window => window.StartUtc > firstActivityUtc);
}
