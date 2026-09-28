using Relay.Api.Tests.Fixtures;

namespace Relay.Api.Tests.ActivityHealth;

[Collection(SeededApiTestGroup.Name)]
public sealed class Account8GoldenTests(SeededApiFixture fixture) : SeededApiTest(fixture)
{
    [Fact]
    public async Task GetActivityHealthSingleSiteDefaultWeekListsOnlySiteA()
    {
        var report = await GetReportAsync("/api/accounts/8/activity-health");

        report.LocationNames.ShouldBe(["Site A"]);
        report.EarliestWeek.ShouldBe("2026-02-02");
    }

    [Fact]
    public async Task GetActivityHealthSingleSiteDefaultWeekSummaryIsNormal()
    {
        var report = await GetReportAsync("/api/accounts/8/activity-health");

        report.Summary.ShouldHaveRange(count: 7, low: 5, high: 17, WireStatus.Normal);
        report.Summary.ShouldHaveMedian(10);
    }

    [Fact]
    public async Task GetActivityHealthSingleSiteDefaultWeekSiteMatchesSummary()
    {
        var report = await GetReportAsync("/api/accounts/8/activity-health");

        var siteA = report.Location("Site A");
        siteA.ShouldHaveRange(count: 7, low: 5, high: 17, WireStatus.Normal);
        siteA.ShouldHaveMedian(10);
    }

    [Fact]
    public async Task GetActivityHealthFloorCaseWeekMarch9IsNormalWithRange6To18()
    {
        var report = await GetReportAsync("/api/accounts/8/activity-health?week=2026-03-09");

        report.Summary.ShouldHaveRange(count: 11, low: 6, high: 18, WireStatus.Normal);
        report.Summary.ShouldHaveMedian(11);
        report.Summary.WeeksUsed.ShouldBe(4);
    }

    [Fact]
    public async Task GetActivityHealthFloorCaseCountEqualToMedianHasZeroDeviation()
    {
        var report = await GetReportAsync("/api/accounts/8/activity-health?week=2026-03-09");

        report.Summary.ShouldHaveDeviation(0);
    }

    [Fact]
    public async Task GetActivityHealthThreeEligibleWeeksIsInsufficientWithCountShown()
    {
        var report = await GetReportAsync("/api/accounts/8/activity-health?week=2026-03-02");

        report.Summary.ShouldBeInsufficient(count: 8, weeksUsed: 3);
    }
}
