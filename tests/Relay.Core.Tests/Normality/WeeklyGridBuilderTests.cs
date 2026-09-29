using Relay.Core.Calendar;
using Relay.Core.Normality;
using Relay.Core.Queries;
using Relay.Core.Tests.TestDoubles;

namespace Relay.Core.Tests.Normality;

public sealed class WeeklyGridBuilderTests
{
    private const string SelectedWeekStart = "2026-03-16";

    private static readonly WeekWindow SelectedWeek = TestTime.UtcMidnightWeek(SelectedWeekStart);
    private static readonly IReadOnlyList<WeekWindow> BaselineWindows = TestTime.UtcMidnightBaselineBefore(SelectedWeekStart, 8);

    private readonly WeeklyGridBuilder _weeklyGridBuilder = new();

    private static SiteFirstActivity SiteFirstSeen(string location, string firstActivityUtc) =>
        new(location, TestTime.Utc(firstActivityUtc));

    private static WeeklySiteCount CountFor(string location, string weekStart, int count) =>
        new(location, TestTime.Day(weekStart), count);

    private static WeeklySeries SeriesFor(IReadOnlyList<LocationSeries> locationSeries, string location) =>
        locationSeries.Single(series => series.Location == location).Series;

    [Fact]
    public void BuildLocationSeriesSiteWithNoEventsInSelectedWeekAppearsWithZeroCount()
    {
        var siteFirstActivities = new[] { SiteFirstSeen("Site A", "2026-01-05T10:00:00Z") };
        var weeklySiteCounts = BaselineWindows
            .Select(window => new WeeklySiteCount("Site A", window.WeekStart, 2))
            .ToList();

        var locationSeries = _weeklyGridBuilder.BuildLocationSeries(SelectedWeek, BaselineWindows, siteFirstActivities, weeklySiteCounts);

        var siteSeries = SeriesFor(locationSeries, "Site A");
        siteSeries.SelectedWeekCount.ShouldBe(0);
        siteSeries.EligibleWeekCounts.ShouldBe([2, 2, 2, 2, 2, 2, 2, 2]);
    }

    [Fact]
    public void BuildLocationSeriesSiteSilentForWholeBaselineIsZeroFilledNotDropped()
    {
        var siteFirstActivities = new[] { SiteFirstSeen("Site A", "2026-01-05T10:00:00Z") };
        var weeklySiteCounts = new[] { CountFor("Site A", SelectedWeekStart, 4) };

        var locationSeries = _weeklyGridBuilder.BuildLocationSeries(SelectedWeek, BaselineWindows, siteFirstActivities, weeklySiteCounts);

        var siteSeries = SeriesFor(locationSeries, "Site A");
        siteSeries.SelectedWeekCount.ShouldBe(4);
        siteSeries.EligibleWeekCounts.ShouldBe([0, 0, 0, 0, 0, 0, 0, 0]);
    }

    [Theory]
    [InlineData("2026-02-10T09:00:00Z", new[] { 2, 3, 4, 5 })]
    [InlineData("2026-02-09T00:00:00Z", new[] { 2, 3, 4, 5 })]
    [InlineData("2026-02-08T23:59:59Z", new[] { 1, 2, 3, 4, 5 })]
    [InlineData("2026-01-18T23:00:00Z", new[] { 9, 0, 7, 1, 2, 3, 4, 5 })]
    public void BuildLocationSeriesOnlyWeeksStartingAfterTheFirstActivityWeekAreEligible(
        string firstActivityUtc,
        int[] expectedEligibleWeekCounts)
    {
        var siteFirstActivities = new[] { SiteFirstSeen("Site A", firstActivityUtc) };
        var weeklySiteCounts = new[]
        {
            CountFor("Site A", "2026-01-19", 9),
            CountFor("Site A", "2026-02-02", 7),
            CountFor("Site A", "2026-02-09", 1),
            CountFor("Site A", "2026-02-16", 2),
            CountFor("Site A", "2026-02-23", 3),
            CountFor("Site A", "2026-03-02", 4),
            CountFor("Site A", "2026-03-09", 5),
        };

        var locationSeries = _weeklyGridBuilder.BuildLocationSeries(SelectedWeek, BaselineWindows, siteFirstActivities, weeklySiteCounts);

        SeriesFor(locationSeries, "Site A").EligibleWeekCounts.ShouldBe(expectedEligibleWeekCounts, ignoreOrder: true);
    }

    [Theory]
    [InlineData("2026-03-22T23:59:59Z", new[] { "Site A", "Site B" })]
    [InlineData("2026-03-23T00:00:00Z", new[] { "Site A" })]
    public void BuildLocationSeriesListsOnlySitesFirstSeenBeforeSelectedWeekEnd(string siteBFirstActivityUtc, string[] expectedLocations)
    {
        var siteFirstActivities = new[]
        {
            SiteFirstSeen("Site A", "2026-01-05T10:00:00Z"),
            SiteFirstSeen("Site B", siteBFirstActivityUtc),
        };

        var locationSeries = _weeklyGridBuilder.BuildLocationSeries(SelectedWeek, BaselineWindows, siteFirstActivities, []);

        locationSeries.Select(series => series.Location).ShouldBe(expectedLocations, ignoreOrder: true);
    }

    [Fact]
    public void BuildLocationSeriesEachSiteUsesItsOwnFirstActivityForEligibility()
    {
        var siteFirstActivities = new[]
        {
            SiteFirstSeen("Site A", "2026-01-05T10:00:00Z"),
            SiteFirstSeen("Site B", "2026-02-24T10:00:00Z"),
        };
        var weeklySiteCounts = new[]
        {
            CountFor("Site A", "2026-02-23", 6),
            CountFor("Site B", "2026-02-23", 1),
            CountFor("Site B", "2026-03-02", 3),
            CountFor("Site B", "2026-03-09", 4),
            CountFor("Site B", SelectedWeekStart, 5),
        };

        var locationSeries = _weeklyGridBuilder.BuildLocationSeries(SelectedWeek, BaselineWindows, siteFirstActivities, weeklySiteCounts);

        SeriesFor(locationSeries, "Site A").EligibleWeekCounts.ShouldBe([0, 0, 0, 0, 0, 6, 0, 0], ignoreOrder: true);
        SeriesFor(locationSeries, "Site B").EligibleWeekCounts.ShouldBe([3, 4], ignoreOrder: true);
        SeriesFor(locationSeries, "Site B").SelectedWeekCount.ShouldBe(5);
    }

    [Fact]
    public void BuildLocationSeriesDstWindowsDecideEligibilityByUtcWindowStartNotUtcMidnight()
    {
        var chicagoSelectedWeek = new WeekWindow(TestTime.Day("2026-03-16"), TestTime.Utc("2026-03-16T05:00:00Z"), TestTime.Utc("2026-03-23T05:00:00Z"));
        var chicagoBaselineWindows = new[]
        {
            new WeekWindow(TestTime.Day("2026-03-02"), TestTime.Utc("2026-03-02T06:00:00Z"), TestTime.Utc("2026-03-09T05:00:00Z")),
            new WeekWindow(TestTime.Day("2026-03-09"), TestTime.Utc("2026-03-09T05:00:00Z"), TestTime.Utc("2026-03-16T05:00:00Z")),
        };
        var siteFirstActivities = new[] { SiteFirstSeen("Site A", "2026-03-09T04:30:00Z") };
        var weeklySiteCounts = new[]
        {
            CountFor("Site A", "2026-03-02", 1),
            CountFor("Site A", "2026-03-09", 6),
        };

        var locationSeries = _weeklyGridBuilder.BuildLocationSeries(chicagoSelectedWeek, chicagoBaselineWindows, siteFirstActivities, weeklySiteCounts);

        SeriesFor(locationSeries, "Site A").EligibleWeekCounts.ShouldBe([6]);
    }

    [Fact]
    public void BuildAccountSeriesSumsSitesPerWeekAndZeroFillsSilentWeeks()
    {
        var siteFirstActivities = new[]
        {
            SiteFirstSeen("Site A", "2026-01-05T10:00:00Z"),
            SiteFirstSeen("Site B", "2026-01-06T10:00:00Z"),
        };
        var weeklySiteCounts = new[]
        {
            CountFor("Site A", "2026-01-19", 2),
            CountFor("Site B", "2026-01-19", 3),
            CountFor("Site A", "2026-02-02", 4),
            CountFor("Site B", "2026-03-09", 1),
            CountFor("Site A", SelectedWeekStart, 7),
            CountFor("Site B", SelectedWeekStart, 8),
        };

        var accountSeries = _weeklyGridBuilder.BuildAccountSeries(SelectedWeek, BaselineWindows, siteFirstActivities, weeklySiteCounts);

        accountSeries.SelectedWeekCount.ShouldBe(15);
        accountSeries.EligibleWeekCounts.ShouldBe([5, 0, 4, 0, 0, 0, 0, 1], ignoreOrder: true);
    }

    [Fact]
    public void BuildAccountSeriesUsesEarliestSiteFirstActivityAndDropsIneligibleWeeksInsteadOfZeroFilling()
    {
        var siteFirstActivities = new[]
        {
            SiteFirstSeen("Site A", "2026-02-24T10:00:00Z"),
            SiteFirstSeen("Site B", "2026-02-10T10:00:00Z"),
        };
        var weeklySiteCounts = new[]
        {
            CountFor("Site B", "2026-02-09", 9),
            CountFor("Site B", "2026-02-16", 2),
            CountFor("Site B", "2026-02-23", 3),
            CountFor("Site B", "2026-03-02", 4),
            CountFor("Site B", "2026-03-09", 5),
        };

        var accountSeries = _weeklyGridBuilder.BuildAccountSeries(SelectedWeek, BaselineWindows, siteFirstActivities, weeklySiteCounts);

        accountSeries.EligibleWeekCounts.ShouldBe([2, 3, 4, 5], ignoreOrder: true);
    }

    [Fact]
    public void BuildAccountSeriesIncludesCountsFromWeeksThatAreIneligibleForTheSiteItself()
    {
        var siteFirstActivities = new[]
        {
            SiteFirstSeen("Site A", "2026-01-05T10:00:00Z"),
            SiteFirstSeen("Site B", "2026-02-24T10:00:00Z"),
        };
        var weeklySiteCounts = new[]
        {
            CountFor("Site A", "2026-02-23", 6),
            CountFor("Site B", "2026-02-23", 1),
        };

        var accountSeries = _weeklyGridBuilder.BuildAccountSeries(SelectedWeek, BaselineWindows, siteFirstActivities, weeklySiteCounts);

        accountSeries.EligibleWeekCounts.ShouldBe([0, 0, 0, 0, 0, 7, 0, 0], ignoreOrder: true);
    }
}
