using Relay.Api.Tests.Fixtures;

namespace Relay.Api.Tests.ActivityHealth;

[Collection(SeededApiTestGroup.Name)]
public sealed class Account14GoldenTests(SeededApiFixture fixture) : SeededApiTest(fixture)
{
    private const string EarliestWeekPath = "/api/accounts/14/activity-health?week=2026-01-26";
    private const string NoEligibleWeeksPath = "/api/accounts/14/activity-health?week=2026-02-02";
    private const string MixedHistoryPath = "/api/accounts/14/activity-health?week=2026-03-02";
    private const string AppointmentsPath = "/api/accounts/14/activity-health?type=appointment_set";

    [Fact]
    public async Task GetActivityHealthEarliestWeekListsOnlySitesAlreadyActive()
    {
        var report = await GetReportAsync(EarliestWeekPath);

        report.LocationNames.ShouldBe(["Site B", "Site D"]);
    }

    [Fact]
    public async Task GetActivityHealthEarliestWeekEverySeriesIsInsufficientWithNoEligibleWeeks()
    {
        var report = await GetReportAsync(EarliestWeekPath);

        report.Summary.ShouldBeInsufficient(count: 2, weeksUsed: 0);
        report.Location("Site B").ShouldBeInsufficient(count: 1, weeksUsed: 0);
        report.Location("Site D").ShouldBeInsufficient(count: 1, weeksUsed: 0);
    }

    [Fact]
    public async Task GetActivityHealthFebruary2SummaryIsInsufficientWithNoEligibleWeeks()
    {
        var report = await GetReportAsync(NoEligibleWeeksPath);

        report.Summary.ShouldBeInsufficient(count: 27, weeksUsed: 0);
    }

    [Fact]
    public async Task GetActivityHealthFebruary2AllFourSitesInsufficientOrderedByName()
    {
        var report = await GetReportAsync(NoEligibleWeeksPath);

        report.LocationNames.ShouldBe(["Site A", "Site B", "Site C", "Site D"]);
        report.Locations.ShouldAllBe(location =>
            location.Status == WireStatus.InsufficientData && location.WeeksUsed == 0 && location.Deviation == null);
    }

    [Fact]
    public async Task GetActivityHealthMixedHistorySummaryIsAbove()
    {
        var report = await GetReportAsync(MixedHistoryPath);

        report.Summary.ShouldHaveRange(count: 40, low: 16, high: 36, WireStatus.Above);
        report.Summary.ShouldHaveMedian(25);
        report.Summary.WeeksUsed.ShouldBe(4);
    }

    [Fact]
    public async Task GetActivityHealthMixedHistoryRanksFlaggedThenNormalThenInsufficientByName()
    {
        var report = await GetReportAsync(MixedHistoryPath);

        report.LocationNames.ShouldBe(["Site D", "Site B", "Site A", "Site C"]);
    }

    [Fact]
    public async Task GetActivityHealthMixedHistorySitesWithFullHistoryHaveGoldenRanges()
    {
        var report = await GetReportAsync(MixedHistoryPath);

        report.Location("Site D").ShouldHaveRange(count: 16, low: 2, high: 11, WireStatus.Above);
        report.Location("Site B").ShouldHaveRange(count: 9, low: 2, high: 10, WireStatus.Normal);
    }

    [Fact]
    public async Task GetActivityHealthMixedHistorySitesFirstActiveInFebruaryHaveThreeWeeks()
    {
        var report = await GetReportAsync(MixedHistoryPath);

        foreach (var siteName in new[] { "Site A", "Site C" })
        {
            var site = report.Location(siteName);
            site.Status.ShouldBe(WireStatus.InsufficientData, siteName);
            site.WeeksUsed.ShouldBe(3, siteName);
            site.Low.ShouldBeNull(siteName);
            site.Deviation.ShouldBeNull(siteName);
        }
    }

    [Fact]
    public async Task GetActivityHealthAppointmentsSummaryIsNormal()
    {
        var report = await GetReportAsync(AppointmentsPath);

        report.EventType.ShouldBe("appointment_set");
        report.Summary.ShouldHaveRange(count: 2, low: 1, high: 8, WireStatus.Normal);
    }

    [Fact]
    public async Task GetActivityHealthAppointmentsSiteAWithZeroIsNormalAndRankedFirst()
    {
        var report = await GetReportAsync(AppointmentsPath);

        var siteA = report.Locations[0];
        siteA.Location.ShouldBe("Site A");
        siteA.ShouldHaveRange(count: 0, low: 0, high: 4, WireStatus.Normal);
        siteA.ShouldHaveDeviation(-1.12);
    }

    [Fact]
    public async Task GetActivityHealthAppointmentsSiteBWithZeroAndMedianZeroCannotBeBelow()
    {
        var report = await GetReportAsync(AppointmentsPath);

        var siteB = report.Location("Site B");
        siteB.ShouldHaveRange(count: 0, low: 0, high: 2, WireStatus.Normal);
        siteB.ShouldHaveMedian(0);
    }

    [Fact]
    public async Task GetActivityHealthAppointmentsZeroDeviationTiesAreOrderedByName()
    {
        var report = await GetReportAsync(AppointmentsPath);

        report.LocationNames.ShouldBe(["Site A", "Site B", "Site C", "Site D"]);
        foreach (var tiedSite in report.Locations.Skip(1))
        {
            tiedSite.ShouldHaveDeviation(0);
        }
    }

    [Fact]
    public async Task GetActivityHealthAppointmentsListsAllFourSites()
    {
        var report = await GetReportAsync(AppointmentsPath);

        report.Locations.Count.ShouldBe(4);
    }

    [Theory]
    [InlineData("call_received", 16, 9, 24)]
    [InlineData("lead_created", 8, 2, 12)]
    public async Task GetActivityHealthPerTypeTotalsAreNormal(string type, int expectedCount, int expectedLow, int expectedHigh)
    {
        var report = await GetReportAsync($"/api/accounts/14/activity-health?week=2026-07-20&type={type}");

        report.Summary.ShouldHaveRange(expectedCount, expectedLow, expectedHigh, WireStatus.Normal);
    }
}
