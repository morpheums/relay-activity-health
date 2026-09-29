using Relay.Api.Tests.Fixtures;

namespace Relay.Api.Tests.ActivityHealth;

[Collection(SeededApiTestGroup.Name)]
public sealed class Account8GoldenTests(SeededApiFixture fixture) : SeededApiTest(fixture)
{
    [Fact]
    public async Task GetActivityHealthSingleSiteDefaultWeekSiteAMatchesTheNormalSummary()
    {
        var report = await GetReportAsync("/api/accounts/8/activity-health");

        report.LocationNames.ShouldBe(["Site A"]);
        report.Summary.ShouldHaveRange(count: 7, low: 5, high: 17, WireStatus.Normal);
        report.Summary.ShouldHaveMedian(10);
        report.Location("Site A").ShouldHaveRange(count: 7, low: 5, high: 17, WireStatus.Normal);
        report.Location("Site A").ShouldHaveMedian(10);
    }

    [Fact]
    public async Task GetActivityHealthFloorCaseWeekMarch9IsNormalWithRange6To18()
    {
        var report = await GetReportAsync("/api/accounts/8/activity-health?week=2026-03-09");

        report.Summary.ShouldHaveRange(count: 11, low: 6, high: 18, WireStatus.Normal);
        report.Summary.ShouldHaveMedian(11);
        report.Summary.ShouldHaveDeviation(0);
        report.Summary.WeeksUsed.ShouldBe(4);
    }

    [Fact]
    public async Task GetActivityHealthThreeEligibleWeeksIsInsufficientWithCountShown()
    {
        var report = await GetReportAsync("/api/accounts/8/activity-health?week=2026-03-02");

        report.Summary.ShouldBeInsufficient(count: 8, weeksUsed: 3);
    }
}
