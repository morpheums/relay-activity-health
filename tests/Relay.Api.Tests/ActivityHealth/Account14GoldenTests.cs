using Relay.Api.Tests.Fixtures;

namespace Relay.Api.Tests.ActivityHealth;

[Collection(SeededApiTestGroup.Name)]
public sealed class Account14GoldenTests(SeededApiFixture fixture) : SeededApiTest(fixture)
{
    [Fact]
    public async Task GetActivityHealthEarliestWeekListsOnlySitesAlreadyActiveAndEverySeriesIsInsufficient()
    {
        var report = await GetReportAsync("/api/accounts/14/activity-health?week=2026-01-26");

        report.LocationNames.ShouldBe(["Site B", "Site D"]);
        report.Summary.ShouldBeInsufficient(count: 2, weeksUsed: 0);
        report.Location("Site B").ShouldBeInsufficient(count: 1, weeksUsed: 0);
        report.Location("Site D").ShouldBeInsufficient(count: 1, weeksUsed: 0);
    }

    [Fact]
    public async Task GetActivityHealthFebruary2ListsAllFourSitesInsufficientWithNoEligibleWeeksOrderedByName()
    {
        var report = await GetReportAsync("/api/accounts/14/activity-health?week=2026-02-02");

        report.Summary.ShouldBeInsufficient(count: 27, weeksUsed: 0);
        report.LocationNames.ShouldBe(["Site A", "Site B", "Site C", "Site D"]);
        report.Locations.ShouldAllBe(location =>
            location.Status == WireStatus.InsufficientData && location.WeeksUsed == 0 && location.Deviation == null);
    }

    [Fact]
    public async Task GetActivityHealthMixedHistoryIsAboveAndRanksFlaggedThenNormalThenInsufficientByName()
    {
        var report = await GetReportAsync("/api/accounts/14/activity-health?week=2026-03-02");

        report.Summary.ShouldHaveRange(count: 40, low: 16, high: 36, WireStatus.Above);
        report.Summary.ShouldHaveMedian(25);
        report.Summary.WeeksUsed.ShouldBe(4);
        report.LocationNames.ShouldBe(["Site D", "Site B", "Site A", "Site C"]);
        report.Location("Site D").ShouldHaveRange(count: 16, low: 2, high: 11, WireStatus.Above);
        report.Location("Site B").ShouldHaveRange(count: 9, low: 2, high: 10, WireStatus.Normal);
        report.Location("Site A").WeeksUsed.ShouldBe(3);
        report.Location("Site A").Status.ShouldBe(WireStatus.InsufficientData);
        report.Location("Site C").WeeksUsed.ShouldBe(3);
        report.Location("Site C").Status.ShouldBe(WireStatus.InsufficientData);
    }

    [Fact]
    public async Task GetActivityHealthAppointmentsRankSiteAFirstThenZeroDeviationTiesByName()
    {
        var report = await GetReportAsync("/api/accounts/14/activity-health?type=appointment_set");

        report.EventType.ShouldBe("appointment_set");
        report.Summary.ShouldHaveRange(count: 2, low: 1, high: 8, WireStatus.Normal);
        report.LocationNames.ShouldBe(["Site A", "Site B", "Site C", "Site D"]);
        report.Location("Site A").ShouldHaveRange(count: 0, low: 0, high: 4, WireStatus.Normal);
        report.Location("Site A").ShouldHaveDeviation(-1.12);
        report.Location("Site B").ShouldHaveRange(count: 0, low: 0, high: 2, WireStatus.Normal);
        report.Location("Site B").ShouldHaveMedian(0);
        foreach (var tiedSite in report.Locations.Skip(1))
        {
            tiedSite.ShouldHaveDeviation(0);
        }
    }

    [Theory]
    [InlineData("call_received", 16, 9, 24)]
    [InlineData("lead_created", 8, 2, 12)]
    public async Task GetActivityHealthPerTypeTotalsAreNormalAndEchoTheSnakeCaseType(string type, int expectedCount, int expectedLow, int expectedHigh)
    {
        var report = await GetReportAsync($"/api/accounts/14/activity-health?week=2026-07-20&type={type}");

        report.EventType.ShouldBe(type);
        report.Summary.ShouldHaveRange(expectedCount, expectedLow, expectedHigh, WireStatus.Normal);
    }
}
