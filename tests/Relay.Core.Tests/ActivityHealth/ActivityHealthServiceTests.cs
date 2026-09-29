using Relay.Core.ActivityHealth;
using Relay.Core.Calendar;
using Relay.Core.Normality;
using Relay.Core.Queries;
using Relay.Core.Tests.TestDoubles;

namespace Relay.Core.Tests.ActivityHealth;

public sealed class ActivityHealthServiceTests
{
    private const double Tolerance = 1e-9;
    private const string SeedDataAnchor = "2026-07-27T22:20:34Z";
    private const string MondayClockUtc = "2026-09-28T16:00:00Z";

    private const int StorageAccountId = 18;
    private const int CollisionAccountId = 6;
    private const int AutoGroupAccountId = 1;

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    private static ActivityHealthService CreateService(FakeAccountQueries accountQueries, FakeActivityQueries activityQueries)
    {
        var normalityOptions = new NormalityOptions();
        return new ActivityHealthService(
            accountQueries,
            activityQueries,
            new WeekCalendar(),
            new WeeklyGridBuilder(),
            new BaselineEvaluator(normalityOptions),
            new LocationRanker(),
            normalityOptions,
            new FixedTimeProvider(new DateTimeOffset(TestTime.Utc(MondayClockUtc))));
    }

    private static FakeAccountQueries StorageAccount() =>
        new FakeAccountQueries().WithAccount(StorageAccountId, "Capital City Storage", "UTC");

    private static FakeActivityQueries StorageActivity() =>
        new FakeActivityQueries()
            .WithDataAnchor(SeedDataAnchor)
            .WithSite(StorageAccountId, "Site A", "2026-05-26T09:00:00Z")
            .WithSite(StorageAccountId, "Site B", "2026-06-30T10:00:00Z")
            .WithWeeklyCounts(StorageAccountId, "Site A", "2026-05-25", ActivityType.All, 5, 26, 24, 29, 23, 15, 16, 21, 14)
            .WithWeeklyCounts(StorageAccountId, "Site B", "2026-06-29", ActivityType.All, 2, 3, 1, 4);

    private static async Task<ActivityHealthReport> GetStorageReportAsync(DateOnly? week)
    {
        var service = CreateService(StorageAccount(), StorageActivity());
        var result = await service.GetAsync(StorageAccountId, week, ActivityType.All, CancellationToken);
        return result.ShouldBeOfType<ActivityHealthResult.Found>().Report;
    }

    private static WeekWindow ChicagoWindow(string weekStart, string startUtc, string endUtc) =>
        new(TestTime.Day(weekStart), TestTime.Utc(startUtc), TestTime.Utc(endUtc));

    [Theory]
    [InlineData(null)]
    [InlineData("2026-07-21")]
    [InlineData("2026-08-03")]
    public async Task GetAsyncUnknownAccountReturnsAccountNotFoundBeforeAnyWeekRule(string? requestedWeek)
    {
        var service = CreateService(StorageAccount(), StorageActivity());
        DateOnly? week = requestedWeek is null ? null : TestTime.Day(requestedWeek);

        var result = await service.GetAsync(999, week, ActivityType.All, CancellationToken);

        result.ShouldBeOfType<ActivityHealthResult.AccountNotFound>();
    }

    [Fact]
    public async Task GetAsyncNoWeekUsesLatestCompleteWeekFromDataAnchorNotClock()
    {
        var report = await GetStorageReportAsync(null);

        report.Week.ShouldBe(new WeekRange(TestTime.Day("2026-07-20"), TestTime.Day("2026-07-26")));
        report.LatestCompleteWeek.ShouldBe(TestTime.Day("2026-07-20"));
    }

    [Fact]
    public async Task GetAsyncEvaluatesSummaryAndEachLocationAgainstItsOwnHistoryAndRanksThem()
    {
        var report = await GetStorageReportAsync(null);

        report.Summary.Count.ShouldBe(18);
        report.Summary.Baseline.ShouldBe(new Baseline(7, 23.0, 15, 33));
        report.Summary.Status.ShouldBe(HealthStatus.Normal);
        report.Summary.Deviation.ShouldNotBeNull().ShouldBe(-1.0963257031657339, Tolerance);

        report.Locations.Select(location => location.Location).ShouldBe(["Site A", "Site B"]);

        var siteA = report.Locations[0];
        siteA.Count.ShouldBe(14);
        siteA.Baseline.ShouldBe(new Baseline(7, 23.0, 15, 33));
        siteA.Status.ShouldBe(HealthStatus.Below);
        siteA.Deviation.ShouldNotBeNull().ShouldBe(-2.086664358855307, Tolerance);

        var siteB = report.Locations[1];
        siteB.Count.ShouldBe(4);
        siteB.Baseline.ShouldBe(new Baseline(2, null, null, null));
        siteB.Status.ShouldBe(HealthStatus.InsufficientData);
        siteB.Deviation.ShouldBeNull();
    }

    [Fact]
    public async Task GetAsyncEarliestWeekItselfIsAcceptedWithNoEligibleWeeks()
    {
        var report = await GetStorageReportAsync(TestTime.Day("2026-05-25"));

        report.Week.Start.ShouldBe(TestTime.Day("2026-05-25"));
        report.EarliestWeek.ShouldBe(TestTime.Day("2026-05-25"));
        report.Summary.Count.ShouldBe(5);
        report.Summary.Status.ShouldBe(HealthStatus.InsufficientData);
        report.Summary.Baseline.WeeksUsed.ShouldBe(0);
        report.Locations.Select(location => location.Location).ShouldBe(["Site A"]);
    }

    [Theory]
    [InlineData("2026-07-21", InvalidWeekReason.NotAWeekStart)]
    [InlineData("2026-07-28", InvalidWeekReason.NotAWeekStart)]
    [InlineData("2026-05-19", InvalidWeekReason.NotAWeekStart)]
    [InlineData("2026-07-27", InvalidWeekReason.AfterLatestCompleteWeek)]
    [InlineData("2026-05-18", InvalidWeekReason.BeforeEarliestWeek)]
    public async Task GetAsyncRejectedWeekReturnsInvalidWeekWithNotAWeekStartTakingPrecedence(string requestedWeek, InvalidWeekReason expectedReason)
    {
        var service = CreateService(StorageAccount(), StorageActivity());

        var result = await service.GetAsync(StorageAccountId, TestTime.Day(requestedWeek), ActivityType.All, CancellationToken);

        result.ShouldBeOfType<ActivityHealthResult.InvalidWeek>().Reason.ShouldBe(expectedReason);
    }

    [Fact]
    public async Task GetAsyncAccountFirstSeenInIncompleteAnchorWeekHasEarliestWeekEqualToLatestCompleteWeekAndEmptyState()
    {
        var activityQueries = new FakeActivityQueries()
            .WithDataAnchor(SeedDataAnchor)
            .WithSite(StorageAccountId, "Site A", "2026-07-27T10:00:00Z")
            .WithWeeklyCounts(StorageAccountId, "Site A", "2026-07-27", ActivityType.All, 3);
        var service = CreateService(StorageAccount(), activityQueries);

        var result = await service.GetAsync(StorageAccountId, null, ActivityType.All, CancellationToken);

        var report = result.ShouldBeOfType<ActivityHealthResult.Found>().Report;
        report.LatestCompleteWeek.ShouldBe(TestTime.Day("2026-07-20"));
        report.EarliestWeek.ShouldBe(TestTime.Day("2026-07-20"));
        report.Week.Start.ShouldBe(TestTime.Day("2026-07-20"));
        report.Summary.Count.ShouldBe(0);
        report.Summary.Status.ShouldBe(HealthStatus.InsufficientData);
        report.Summary.Baseline.WeeksUsed.ShouldBe(0);
        report.Locations.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAsyncRequestsSelectedWeekAndEightPrecedingWindowsOldestFirstInAccountTimeZoneAcrossDst()
    {
        var activityQueries = new FakeActivityQueries()
            .WithDataAnchor(SeedDataAnchor)
            .WithSite(AutoGroupAccountId, "Site A", "2026-01-06T15:00:00Z");
        var service = CreateService(
            new FakeAccountQueries().WithAccount(AutoGroupAccountId, "Summit Auto Group", "America/Chicago"),
            activityQueries);
        var selectedWeekStart = TestTime.Day("2026-03-16");

        await service.GetAsync(AutoGroupAccountId, selectedWeekStart, ActivityType.All, CancellationToken);

        activityQueries.RequestedWindows
            .Where(window => window.WeekStart == selectedWeekStart)
            .Distinct()
            .ShouldBe([ChicagoWindow("2026-03-16", "2026-03-16T05:00:00Z", "2026-03-23T05:00:00Z")]);
        activityQueries.RequestedWindows.Where(window => window.WeekStart != selectedWeekStart).ShouldBe(
        [
            ChicagoWindow("2026-01-19", "2026-01-19T06:00:00Z", "2026-01-26T06:00:00Z"),
            ChicagoWindow("2026-01-26", "2026-01-26T06:00:00Z", "2026-02-02T06:00:00Z"),
            ChicagoWindow("2026-02-02", "2026-02-02T06:00:00Z", "2026-02-09T06:00:00Z"),
            ChicagoWindow("2026-02-09", "2026-02-09T06:00:00Z", "2026-02-16T06:00:00Z"),
            ChicagoWindow("2026-02-16", "2026-02-16T06:00:00Z", "2026-02-23T06:00:00Z"),
            ChicagoWindow("2026-02-23", "2026-02-23T06:00:00Z", "2026-03-02T06:00:00Z"),
            ChicagoWindow("2026-03-02", "2026-03-02T06:00:00Z", "2026-03-09T05:00:00Z"),
            ChicagoWindow("2026-03-09", "2026-03-09T05:00:00Z", "2026-03-16T05:00:00Z"),
        ]);
    }

    [Fact]
    public async Task GetAsyncTypeFilterKeepsSiteListAndEligibilityFromAnyTypeAndChangesCountsOnly()
    {
        var accountQueries = new FakeAccountQueries().WithAccount(CollisionAccountId, "Metro Collision Centers", "America/New_York");
        var activityQueries = new FakeActivityQueries()
            .WithDataAnchor(SeedDataAnchor)
            .WithSite(CollisionAccountId, "Site A", "2026-01-06T15:00:00Z")
            .WithSite(CollisionAccountId, "Site B", "2026-01-07T15:00:00Z")
            .WithWeeklyCounts(CollisionAccountId, "Site A", "2026-05-25", ActivityType.All, 10, 10, 10, 10, 10, 10, 10, 10, 10)
            .WithWeeklyCounts(CollisionAccountId, "Site B", "2026-05-25", ActivityType.All, 6, 6, 6, 6, 6, 6, 6, 6, 6)
            .WithWeeklyCounts(CollisionAccountId, "Site A", "2026-05-25", ActivityType.CallReceived, 3, 3, 3, 3, 3, 3, 3, 3, 3);
        var service = CreateService(accountQueries, activityQueries);

        var result = await service.GetAsync(CollisionAccountId, null, ActivityType.CallReceived, CancellationToken);

        var report = result.ShouldBeOfType<ActivityHealthResult.Found>().Report;
        report.EventType.ShouldBe(ActivityType.CallReceived);
        report.Summary.Count.ShouldBe(3);
        report.Summary.Baseline.ShouldBe(new Baseline(8, 3.0, 1, 7));
        report.Locations.Select(location => location.Location).ShouldBe(["Site A", "Site B"]);
        report.Locations[0].Count.ShouldBe(3);
        report.Locations[0].Baseline.ShouldBe(new Baseline(8, 3.0, 1, 7));
        report.Locations[0].Status.ShouldBe(HealthStatus.Normal);
        report.Locations[1].Count.ShouldBe(0);
        report.Locations[1].Baseline.ShouldBe(new Baseline(8, 0.0, 0, 2));
        report.Locations[1].Status.ShouldBe(HealthStatus.Normal);
    }

    [Fact]
    public async Task GetAsyncWeeklyCountAndSiteRowOrderDoesNotChangeTheReport()
    {
        FakeActivityQueries TiedSitesActivity() =>
            new FakeActivityQueries()
                .WithDataAnchor(SeedDataAnchor)
                .WithSite(StorageAccountId, "Site G", "2026-07-01T10:00:00Z")
                .WithSite(StorageAccountId, "Site F", "2026-06-30T10:00:00Z")
                .WithSite(StorageAccountId, "Site B", "2026-05-12T09:00:00Z")
                .WithSite(StorageAccountId, "Site A", "2026-05-12T09:00:00Z")
                .WithSite(StorageAccountId, "Site D", "2026-05-12T09:00:00Z")
                .WithSite(StorageAccountId, "Site C", "2026-05-12T09:00:00Z")
                .WithWeeklyCounts(StorageAccountId, "Site G", "2026-06-29", ActivityType.All, 2, 2, 2, 2)
                .WithWeeklyCounts(StorageAccountId, "Site F", "2026-06-29", ActivityType.All, 2, 2, 2, 2)
                .WithWeeklyCounts(StorageAccountId, "Site B", "2026-05-25", ActivityType.All, 10, 10, 10, 10, 10, 10, 10, 10, 10)
                .WithWeeklyCounts(StorageAccountId, "Site A", "2026-05-25", ActivityType.All, 10, 10, 10, 10, 10, 10, 10, 10, 10)
                .WithWeeklyCounts(StorageAccountId, "Site D", "2026-05-25", ActivityType.All, 3, 3, 3, 3, 3, 3, 3, 3)
                .WithWeeklyCounts(StorageAccountId, "Site C", "2026-05-25", ActivityType.All, 3, 3, 3, 3, 3, 3, 3, 3);
        var naturalOrderService = CreateService(StorageAccount(), TiedSitesActivity());
        var reversedOrderService = CreateService(StorageAccount(), TiedSitesActivity().WithRowsReversed());

        var naturalOrderResult = await naturalOrderService.GetAsync(StorageAccountId, null, ActivityType.All, CancellationToken);
        var reversedOrderResult = await reversedOrderService.GetAsync(StorageAccountId, null, ActivityType.All, CancellationToken);

        var naturalOrderReport = naturalOrderResult.ShouldBeOfType<ActivityHealthResult.Found>().Report;
        var reversedOrderReport = reversedOrderResult.ShouldBeOfType<ActivityHealthResult.Found>().Report;
        naturalOrderReport.Locations.Select(location => location.Location)
            .ShouldBe(["Site C", "Site D", "Site A", "Site B", "Site F", "Site G"]);
        reversedOrderReport.Locations.ShouldBe(naturalOrderReport.Locations);
        (reversedOrderReport with { Locations = naturalOrderReport.Locations }).ShouldBe(naturalOrderReport);
    }
}
