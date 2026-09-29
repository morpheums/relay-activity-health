using Relay.Api.Tests.Fixtures;

namespace Relay.Api.Tests.ActivityHealth;

[Collection(SeededApiTestGroup.Name)]
public sealed class Account6GoldenTests(SeededApiFixture fixture) : SeededApiTest(fixture)
{
    private const int SiteCount = 15;

    [Fact]
    public async Task GetActivityHealthSpikeWeekIsAboveWithAllFifteenSitesAboveAndSiteCFirst()
    {
        var report = await GetReportAsync("/api/accounts/6/activity-health?week=2026-06-01");

        report.Summary.ShouldHaveRange(count: 880, low: 39, high: 101, WireStatus.Above);
        report.Summary.ShouldHaveMedian(66);
        report.Summary.ShouldHaveDeviation(22.37);
        report.Locations.Count.ShouldBe(SiteCount);
        report.Locations.ShouldAllBe(location => location.Status == WireStatus.Above);
        var siteC = report.Locations[0];
        siteC.Location.ShouldBe("Site C");
        siteC.ShouldHaveRange(count: 67, low: 1, high: 7, WireStatus.Above);
        siteC.ShouldHaveMedian(3);
        siteC.ShouldHaveDeviation(12.74);
    }

    [Fact]
    public async Task GetActivityHealthWeekAfterSpikeIsNormalWithSitesCAndJAboveFirstAndTheRestNormal()
    {
        var report = await GetReportAsync("/api/accounts/6/activity-health?week=2026-06-08");

        report.Summary.ShouldHaveRange(count: 102, low: 37, high: 104, WireStatus.Normal);
        report.Summary.ShouldHaveMedian(66);
        report.Locations.Count.ShouldBe(SiteCount);
        report.Locations[0].Location.ShouldBe("Site C");
        report.Locations[0].ShouldHaveRange(count: 11, low: 1, high: 8, WireStatus.Above);
        report.Locations[1].Location.ShouldBe("Site J");
        report.Locations[1].ShouldHaveRange(count: 11, low: 2, high: 10, WireStatus.Above);
        report.Locations.Skip(2).ShouldAllBe(location => location.Status == WireStatus.Normal);
    }

    [Fact]
    public async Task GetActivityHealthSiteGoingSilentIsBelowAndRankedFirstWhileTotalIsNormal()
    {
        var report = await GetReportAsync("/api/accounts/6/activity-health?week=2026-06-29");

        report.Summary.ShouldHaveRange(count: 69, low: 41, high: 111, WireStatus.Normal);
        var siteG = report.Locations[0];
        siteG.Location.ShouldBe("Site G");
        siteG.ShouldHaveRange(count: 0, low: 2, high: 9, WireStatus.Below);
        siteG.ShouldHaveDeviation(-3.19);
    }

    [Fact]
    public async Task GetActivityHealthSpikeInBaselineIsNormalWithUnroundedMedianAndSiteMFirst()
    {
        var report = await GetReportAsync("/api/accounts/6/activity-health?week=2026-07-20");

        report.Summary.ShouldHaveRange(count: 87, low: 30, high: 134, WireStatus.Normal);
        report.Summary.ShouldHaveMedian(72.5);
        report.Summary.ShouldHaveDeviation(0.53);
        report.Summary.WeeksUsed.ShouldBe(8);
        report.Locations.Count.ShouldBe(SiteCount);
        report.Locations.ShouldAllBe(location => location.Status == WireStatus.Normal);
        var siteM = report.Locations[0];
        siteM.Location.ShouldBe("Site M");
        siteM.ShouldHaveRange(count: 7, low: 1, high: 9, WireStatus.Normal);
        siteM.ShouldHaveMedian(3.5);
        siteM.ShouldHaveDeviation(1.30);
    }

    [Fact]
    public async Task GetActivityHealthCallsFilterIsNormalAndKeepsAllFifteenSites()
    {
        var report = await GetReportAsync("/api/accounts/6/activity-health?week=2026-07-20&type=call_received");

        report.EventType.ShouldBe("call_received");
        report.Summary.ShouldHaveRange(count: 51, low: 17, high: 79, WireStatus.Normal);
        report.Summary.ShouldHaveMedian(42);
        report.Summary.ShouldHaveDeviation(0.54);
        report.Locations.Count.ShouldBe(SiteCount);
    }
}
