using Relay.Core.Calendar;
using Relay.Core.Normality;
using Relay.Core.Queries;
using static Relay.Core.Calendar.WeekLength;

namespace Relay.Core.ActivityHealth;

public sealed class ActivityHealthService(
    IAccountQueries accountQueries,
    IActivityQueries activityQueries,
    IWeekCalendar weekCalendar,
    IWeeklyGridBuilder weeklyGridBuilder,
    IBaselineEvaluator baselineEvaluator,
    ILocationRanker locationRanker,
    NormalityOptions normalityOptions,
    TimeProvider timeProvider) : IActivityHealthService
{
    public async Task<ActivityHealthResult> GetAsync(
        int accountId,
        DateOnly? week,
        ActivityType eventType,
        CancellationToken cancellationToken)
    {
        var account = await accountQueries.FindAsync(accountId, cancellationToken);
        if (account is null)
        {
            return new ActivityHealthResult.AccountNotFound();
        }

        if (week is { DayOfWeek: not DayOfWeek.Monday })
        {
            return new ActivityHealthResult.InvalidWeek(InvalidWeekReason.NotAWeekStart);
        }

        var dataAnchorUtc = await activityQueries.FindDataAnchorAsync(cancellationToken);
        var latestCompleteWeek = weekCalendar.LatestCompleteWeek(dataAnchorUtc ?? timeProvider.GetUtcNow().UtcDateTime, account.Timezone);
        var siteFirstActivities = await activityQueries.ListSiteFirstActivitiesAsync(accountId, cancellationToken);
        var earliestWeek = EarliestWeekFor(siteFirstActivities, latestCompleteWeek, account.Timezone);

        var selectedWeekStart = week ?? latestCompleteWeek;
        if (selectedWeekStart > latestCompleteWeek)
        {
            return new ActivityHealthResult.InvalidWeek(InvalidWeekReason.AfterLatestCompleteWeek);
        }

        if (selectedWeekStart < earliestWeek)
        {
            return new ActivityHealthResult.InvalidWeek(InvalidWeekReason.BeforeEarliestWeek);
        }

        var selectedWeek = weekCalendar.Window(selectedWeekStart, account.Timezone);
        var baselineWindows = BaselineWindowsBefore(selectedWeekStart, account.Timezone);
        var weeklySiteCounts = await activityQueries.CountWeeklyBySiteAsync(
            accountId,
            [.. baselineWindows, selectedWeek],
            eventType,
            cancellationToken);

        var accountSeries = weeklyGridBuilder.BuildAccountSeries(selectedWeek, baselineWindows, siteFirstActivities, weeklySiteCounts);
        var locationSeries = weeklyGridBuilder.BuildLocationSeries(selectedWeek, baselineWindows, siteFirstActivities, weeklySiteCounts);

        return new ActivityHealthResult.Found(new ActivityHealthReport(
            account,
            eventType,
            new WeekRange(selectedWeekStart, selectedWeekStart.AddDays(DaysPerWeek - 1)),
            dataAnchorUtc,
            latestCompleteWeek,
            earliestWeek,
            normalityOptions.BaselineWeeks,
            normalityOptions.MinimumEligibleWeeks,
            EvaluateSeries(accountSeries),
            locationRanker.Rank(locationSeries.Select(EvaluateLocation).ToList())));
    }

    private DateOnly EarliestWeekFor(IReadOnlyList<SiteFirstActivity> siteFirstActivities, DateOnly latestCompleteWeek, string timeZoneId)
    {
        if (siteFirstActivities.AccountFirstActivityUtc() is not { } accountFirstActivityUtc)
        {
            return latestCompleteWeek;
        }

        var firstActivityWeek = weekCalendar.WeekContaining(accountFirstActivityUtc, timeZoneId);
        return firstActivityWeek < latestCompleteWeek ? firstActivityWeek : latestCompleteWeek;
    }

    private List<WeekWindow> BaselineWindowsBefore(DateOnly selectedWeekStart, string timeZoneId) =>
        Enumerable.Range(1, normalityOptions.BaselineWeeks)
            .Reverse()
            .Select(weeksBack => weekCalendar.Window(selectedWeekStart.AddDays(-DaysPerWeek * weeksBack), timeZoneId))
            .ToList();

    private SeriesHealth EvaluateSeries(WeeklySeries series) =>
        baselineEvaluator.Evaluate(series.EligibleWeekCounts, series.SelectedWeekCount);

    private LocationHealth EvaluateLocation(LocationSeries locationSeries)
    {
        var seriesHealth = EvaluateSeries(locationSeries.Series);
        return new LocationHealth(
            locationSeries.Location,
            seriesHealth.Count,
            seriesHealth.Baseline,
            seriesHealth.Status,
            seriesHealth.Deviation);
    }
}
