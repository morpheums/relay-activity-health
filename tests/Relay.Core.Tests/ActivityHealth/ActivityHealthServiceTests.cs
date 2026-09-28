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
    private const int SecurityAccountId = 14;
    private const int AutoGroupAccountId = 1;
    private const int SpaAccountId = 20;

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    private static ActivityHealthService CreateService(
        FakeAccountQueries accountQueries,
        FakeActivityQueries activityQueries,
        NormalityOptions? normalityOptions = null,
        string clockUtc = MondayClockUtc)
    {
        var options = normalityOptions ?? new NormalityOptions();
        return new ActivityHealthService(
            accountQueries,
            activityQueries,
            new WeekCalendar(),
            new WeeklyGridBuilder(),
            new BaselineEvaluator(options),
            new LocationRanker(),
            options,
            new FixedTimeProvider(new DateTimeOffset(TestTime.Utc(clockUtc))));
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

    private static async Task<ActivityHealthReport> GetStorageReportAsync(DateOnly? week, NormalityOptions? normalityOptions = null)
    {
        var service = CreateService(StorageAccount(), StorageActivity(), normalityOptions);
        var result = await service.GetAsync(StorageAccountId, week, ActivityType.All, CancellationToken);
        return result.ShouldBeOfType<ActivityHealthResult.Found>().Report;
    }

    private static FakeAccountQueries AutoGroupAccount() =>
        new FakeAccountQueries().WithAccount(AutoGroupAccountId, "Summit Auto Group", "America/Chicago");

    private static FakeActivityQueries AutoGroupActivity() =>
        new FakeActivityQueries()
            .WithDataAnchor(SeedDataAnchor)
            .WithSite(AutoGroupAccountId, "Site A", "2026-01-06T15:00:00Z");

    private static WeekWindow ChicagoWindow(string weekStart, string startUtc, string endUtc) =>
        new(TestTime.Day(weekStart), TestTime.Utc(startUtc), TestTime.Utc(endUtc));

    [Fact]
    public async Task GetAsyncUnknownAccountReturnsAccountNotFound()
    {
        var service = CreateService(StorageAccount(), StorageActivity());

        var result = await service.GetAsync(999, null, ActivityType.All, CancellationToken);

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
    public async Task GetAsyncExplicitWeekReturnsThatMondayToSundayAndKeepsLatestCompleteWeek()
    {
        var report = await GetStorageReportAsync(TestTime.Day("2026-07-13"));

        report.Week.ShouldBe(new WeekRange(TestTime.Day("2026-07-13"), TestTime.Day("2026-07-19")));
        report.LatestCompleteWeek.ShouldBe(TestTime.Day("2026-07-20"));
    }

    [Fact]
    public async Task GetAsyncFoundReportCarriesAccountEventTypeDataAnchorAndOptions()
    {
        var report = await GetStorageReportAsync(null);

        report.Account.ShouldBe(new AccountListItem(StorageAccountId, "Capital City Storage", "UTC"));
        report.EventType.ShouldBe(ActivityType.All);
        report.DataAsOf.ShouldBe(TestTime.Utc(SeedDataAnchor));
        report.BaselineWeeks.ShouldBe(8);
        report.MinimumEligibleWeeks.ShouldBe(4);
    }

    [Fact]
    public async Task GetAsyncEarliestWeekIsLocalWeekOfAccountFirstEvent()
    {
        var report = await GetStorageReportAsync(null);

        report.EarliestWeek.ShouldBe(TestTime.Day("2026-05-25"));
    }

    [Fact]
    public async Task GetAsyncAccountSummaryEligibilityUsesAccountFirstEventGivingSevenWeeksMedian23Range15To33()
    {
        var report = await GetStorageReportAsync(null);

        report.Summary.Count.ShouldBe(18);
        report.Summary.Baseline.WeeksUsed.ShouldBe(7);
        report.Summary.Baseline.Median.ShouldBe(23.0);
        report.Summary.Baseline.Low.ShouldBe(15);
        report.Summary.Baseline.High.ShouldBe(33);
        report.Summary.Status.ShouldBe(HealthStatus.Normal);
        report.Summary.Deviation.ShouldNotBeNull().ShouldBe(-1.0963257031657339, Tolerance);
    }

    [Fact]
    public async Task GetAsyncLocationsAreEvaluatedAgainstTheirOwnHistoryAndRanked()
    {
        var report = await GetStorageReportAsync(null);

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
    public async Task GetAsyncSiteFirstSeenAfterSelectedWeekIsNotListed()
    {
        var report = await GetStorageReportAsync(TestTime.Day("2026-06-22"));

        report.Locations.Select(location => location.Location).ShouldBe(["Site A"]);
    }

    [Fact]
    public async Task GetAsyncEarliestWeekItselfIsAcceptedWithNoEligibleWeeks()
    {
        var report = await GetStorageReportAsync(TestTime.Day("2026-05-25"));

        report.Week.Start.ShouldBe(TestTime.Day("2026-05-25"));
        report.Summary.Count.ShouldBe(5);
        report.Summary.Status.ShouldBe(HealthStatus.InsufficientData);
        report.Summary.Baseline.WeeksUsed.ShouldBe(0);
        report.Locations.Select(location => location.Location).ShouldBe(["Site A"]);
    }

    [Theory]
    [InlineData("2026-07-21", InvalidWeekReason.NotAWeekStart)]
    [InlineData("2026-07-26", InvalidWeekReason.NotAWeekStart)]
    [InlineData("2026-07-27", InvalidWeekReason.AfterLatestCompleteWeek)]
    [InlineData("2026-08-03", InvalidWeekReason.AfterLatestCompleteWeek)]
    [InlineData("2026-05-18", InvalidWeekReason.BeforeEarliestWeek)]
    [InlineData("2026-01-05", InvalidWeekReason.BeforeEarliestWeek)]
    public async Task GetAsyncRejectedWeekReturnsInvalidWeekWithReason(string requestedWeek, InvalidWeekReason expectedReason)
    {
        var service = CreateService(StorageAccount(), StorageActivity());

        var result = await service.GetAsync(StorageAccountId, TestTime.Day(requestedWeek), ActivityType.All, CancellationToken);

        result.ShouldBeOfType<ActivityHealthResult.InvalidWeek>().Reason.ShouldBe(expectedReason);
    }

    [Fact]
    public async Task GetAsyncWeekEqualToLatestCompleteWeekIsFound()
    {
        var report = await GetStorageReportAsync(TestTime.Day("2026-07-20"));

        report.Week.ShouldBe(new WeekRange(TestTime.Day("2026-07-20"), TestTime.Day("2026-07-26")));
    }

    [Theory]
    [InlineData("2026-07-28")]
    [InlineData("2026-05-19")]
    public async Task GetAsyncNonMondayWeekOutsideValidRangeReturnsNotAWeekStart(string requestedWeek)
    {
        var service = CreateService(StorageAccount(), StorageActivity());

        var result = await service.GetAsync(StorageAccountId, TestTime.Day(requestedWeek), ActivityType.All, CancellationToken);

        result.ShouldBeOfType<ActivityHealthResult.InvalidWeek>().Reason.ShouldBe(InvalidWeekReason.NotAWeekStart);
    }

    [Theory]
    [InlineData("2026-07-21")]
    [InlineData("2026-08-03")]
    [InlineData("2026-01-05")]
    [InlineData("2026-07-28")]
    public async Task GetAsyncUnknownAccountWithInvalidWeekReturnsAccountNotFound(string requestedWeek)
    {
        var service = CreateService(StorageAccount(), StorageActivity());

        var result = await service.GetAsync(999, TestTime.Day(requestedWeek), ActivityType.All, CancellationToken);

        result.ShouldBeOfType<ActivityHealthResult.AccountNotFound>();
    }

    [Fact]
    public async Task GetAsyncAccountFirstSeenInIncompleteAnchorWeekHasEarliestWeekEqualToLatestCompleteWeekAndEmptyState()
    {
        var accountQueries = new FakeAccountQueries().WithAccount(StorageAccountId, "Capital City Storage", "UTC");
        var activityQueries = new FakeActivityQueries()
            .WithDataAnchor(SeedDataAnchor)
            .WithSite(StorageAccountId, "Site A", "2026-07-27T10:00:00Z")
            .WithWeeklyCounts(StorageAccountId, "Site A", "2026-07-27", ActivityType.All, 3);
        var service = CreateService(accountQueries, activityQueries);

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
    public async Task GetAsyncEarliestWeekUsesAccountTimeZoneNotUtc()
    {
        var accountQueries = new FakeAccountQueries().WithAccount(SecurityAccountId, "Beacon Home Security", "America/New_York");
        var activityQueries = new FakeActivityQueries()
            .WithDataAnchor(SeedDataAnchor)
            .WithSite(SecurityAccountId, "Site B", "2026-02-02T03:00:00Z");
        var service = CreateService(accountQueries, activityQueries);

        var result = await service.GetAsync(SecurityAccountId, TestTime.Day("2026-01-26"), ActivityType.All, CancellationToken);

        var report = result.ShouldBeOfType<ActivityHealthResult.Found>().Report;
        report.EarliestWeek.ShouldBe(TestTime.Day("2026-01-26"));
        report.Locations.Select(location => location.Location).ShouldBe(["Site B"]);
    }

    [Fact]
    public async Task GetAsyncAccountWithNoEventsReturnsEmptyStateWithEarliestWeekEqualToLatestCompleteWeek()
    {
        var accountQueries = new FakeAccountQueries().WithAccount(SpaAccountId, "Quiet Harbor Spa", "America/Los_Angeles");
        var activityQueries = new FakeActivityQueries()
            .WithDataAnchor(SeedDataAnchor)
            .WithSite(StorageAccountId, "Site A", "2026-05-26T09:00:00Z");
        var service = CreateService(accountQueries, activityQueries);

        var result = await service.GetAsync(SpaAccountId, null, ActivityType.All, CancellationToken);

        var report = result.ShouldBeOfType<ActivityHealthResult.Found>().Report;
        report.Week.ShouldBe(new WeekRange(TestTime.Day("2026-07-20"), TestTime.Day("2026-07-26")));
        report.LatestCompleteWeek.ShouldBe(TestTime.Day("2026-07-20"));
        report.EarliestWeek.ShouldBe(TestTime.Day("2026-07-20"));
        report.DataAsOf.ShouldBe(TestTime.Utc(SeedDataAnchor));
        report.Summary.Count.ShouldBe(0);
        report.Summary.Status.ShouldBe(HealthStatus.InsufficientData);
        report.Summary.Baseline.ShouldBe(new Baseline(0, null, null, null));
        report.Summary.Deviation.ShouldBeNull();
        report.Locations.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAsyncAccountWithNoEventsRejectsWeekBeforeLatestCompleteWeekAsBeforeEarliestWeek()
    {
        var accountQueries = new FakeAccountQueries().WithAccount(SpaAccountId, "Quiet Harbor Spa", "America/Los_Angeles");
        var activityQueries = new FakeActivityQueries().WithDataAnchor(SeedDataAnchor);
        var service = CreateService(accountQueries, activityQueries);

        var result = await service.GetAsync(SpaAccountId, TestTime.Day("2026-03-02"), ActivityType.All, CancellationToken);

        result.ShouldBeOfType<ActivityHealthResult.InvalidWeek>().Reason.ShouldBe(InvalidWeekReason.BeforeEarliestWeek);
    }

    [Fact]
    public async Task GetAsyncEmptyDatabaseReturnsNullDataAsOfAndLatestCompleteWeekFromClock()
    {
        var accountQueries = new FakeAccountQueries().WithAccount(SecurityAccountId, "Beacon Home Security", "America/New_York");
        var service = CreateService(accountQueries, new FakeActivityQueries(), clockUtc: MondayClockUtc);

        var result = await service.GetAsync(SecurityAccountId, null, ActivityType.All, CancellationToken);

        var report = result.ShouldBeOfType<ActivityHealthResult.Found>().Report;
        report.DataAsOf.ShouldBeNull();
        report.LatestCompleteWeek.ShouldBe(TestTime.Day("2026-09-21"));
        report.EarliestWeek.ShouldBe(TestTime.Day("2026-09-21"));
        report.Week.ShouldBe(new WeekRange(TestTime.Day("2026-09-21"), TestTime.Day("2026-09-27")));
        report.Summary.Count.ShouldBe(0);
        report.Summary.Status.ShouldBe(HealthStatus.InsufficientData);
        report.Summary.Baseline.WeeksUsed.ShouldBe(0);
        report.Locations.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetAsyncRequestsEightBaselineWindowsOldestFirstIncludingDstWeek()
    {
        var activityQueries = AutoGroupActivity();
        var service = CreateService(AutoGroupAccount(), activityQueries);
        var selectedWeekStart = TestTime.Day("2026-03-16");

        await service.GetAsync(AutoGroupAccountId, selectedWeekStart, ActivityType.All, CancellationToken);

        var baselineWindows = activityQueries.RequestedWindows.Where(window => window.WeekStart != selectedWeekStart).ToList();
        baselineWindows.ShouldBe(
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
    public async Task GetAsyncRequestsSelectedWeekWindowInAccountTimeZone()
    {
        var activityQueries = AutoGroupActivity();
        var service = CreateService(AutoGroupAccount(), activityQueries);
        var selectedWeekStart = TestTime.Day("2026-03-16");

        await service.GetAsync(AutoGroupAccountId, selectedWeekStart, ActivityType.All, CancellationToken);

        activityQueries.RequestedWindows
            .Where(window => window.WeekStart == selectedWeekStart)
            .Distinct()
            .ShouldBe([ChicagoWindow("2026-03-16", "2026-03-16T05:00:00Z", "2026-03-23T05:00:00Z")]);
    }

    [Fact]
    public async Task GetAsyncBaselineWindowCountComesFromOptions()
    {
        var activityQueries = AutoGroupActivity();
        var service = CreateService(AutoGroupAccount(), activityQueries, new NormalityOptions { BaselineWeeks = 3 });
        var selectedWeekStart = TestTime.Day("2026-03-16");

        await service.GetAsync(AutoGroupAccountId, selectedWeekStart, ActivityType.All, CancellationToken);

        activityQueries.RequestedWindows
            .Where(window => window.WeekStart != selectedWeekStart)
            .Select(window => window.WeekStart)
            .ShouldBe([TestTime.Day("2026-02-23"), TestTime.Day("2026-03-02"), TestTime.Day("2026-03-09")]);
    }

    [Fact]
    public async Task GetAsyncPassesAccountIdAndEventTypeToWeeklyCountQuery()
    {
        var activityQueries = AutoGroupActivity();
        var service = CreateService(AutoGroupAccount(), activityQueries);

        await service.GetAsync(AutoGroupAccountId, TestTime.Day("2026-03-16"), ActivityType.LeadCreated, CancellationToken);

        activityQueries.WeeklyCountRequests.ShouldNotBeEmpty();
        activityQueries.WeeklyCountRequests.ShouldAllBe(request =>
            request.AccountId == AutoGroupAccountId && request.EventType == ActivityType.LeadCreated);
    }

    [Fact]
    public async Task GetAsyncReportOptionsFieldsAndBaselineLengthFollowNormalityOptions()
    {
        var report = await GetStorageReportAsync(null, new NormalityOptions { BaselineWeeks = 6, MinimumEligibleWeeks = 3 });

        report.BaselineWeeks.ShouldBe(6);
        report.MinimumEligibleWeeks.ShouldBe(3);
        report.Summary.Baseline.WeeksUsed.ShouldBe(6);
        report.Locations.Single(location => location.Location == "Site B").Status.ShouldBe(HealthStatus.InsufficientData);
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
