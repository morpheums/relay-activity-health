using Relay.Api.Tests.Fixtures;

namespace Relay.Api.Tests.ActivityHealth;

[Collection(SeededApiTestGroup.Name)]
public sealed class Account6GoldenTests(SeededApiFixture fixture) : SeededApiTest(fixture)
{
    private const string SpikeWeekPath = "/api/accounts/6/activity-health?week=2026-06-01";
    private const string WeekAfterSpikePath = "/api/accounts/6/activity-health?week=2026-06-08";
    private const string SiteGoingSilentPath = "/api/accounts/6/activity-health?week=2026-06-29";
    private const string SpikeInBaselinePath = "/api/accounts/6/activity-health?week=2026-07-20";
    private const string SpikeInBaselineCallsPath = "/api/accounts/6/activity-health?week=2026-07-20&type=call_received";
    private const int SiteCount = 15;

    [Fact]
    public async Task GetActivityHealthSpikeWeekSummaryIsAbove()
    {
        var report = await GetReportAsync(SpikeWeekPath);

        report.Summary.ShouldHaveRange(count: 880, low: 39, high: 101, WireStatus.Above);
        report.Summary.ShouldHaveMedian(66);
        report.Summary.ShouldHaveDeviation(22.37);
    }

    [Fact]
    public async Task GetActivityHealthSpikeWeekFlagsAllFifteenSitesAbove()
    {
        var report = await GetReportAsync(SpikeWeekPath);

        report.Locations.Count.ShouldBe(SiteCount);
        report.Locations.ShouldAllBe(location => location.Status == WireStatus.Above);
    }

    [Fact]
    public async Task GetActivityHealthSpikeWeekSiteCIsRankedFirst()
    {
        var report = await GetReportAsync(SpikeWeekPath);

        var siteC = report.Locations[0];
        siteC.Location.ShouldBe("Site C");
        siteC.ShouldHaveRange(count: 67, low: 1, high: 7, WireStatus.Above);
        siteC.ShouldHaveMedian(3);
        siteC.ShouldHaveDeviation(12.74);
    }

    [Fact]
    public async Task GetActivityHealthWeekAfterSpikeSummaryIsNormal()
    {
        var report = await GetReportAsync(WeekAfterSpikePath);

        report.Summary.ShouldHaveRange(count: 102, low: 37, high: 104, WireStatus.Normal);
        report.Summary.ShouldHaveMedian(66);
    }

    [Fact]
    public async Task GetActivityHealthWeekAfterSpikeRanksSitesCAndJAboveFirst()
    {
        var report = await GetReportAsync(WeekAfterSpikePath);

        report.Locations[0].Location.ShouldBe("Site C");
        report.Locations[0].ShouldHaveRange(count: 11, low: 1, high: 8, WireStatus.Above);
        report.Locations[1].Location.ShouldBe("Site J");
        report.Locations[1].ShouldHaveRange(count: 11, low: 2, high: 10, WireStatus.Above);
    }

    [Fact]
    public async Task GetActivityHealthWeekAfterSpikeOtherThirteenSitesAreNormal()
    {
        var report = await GetReportAsync(WeekAfterSpikePath);

        report.Locations.Count.ShouldBe(SiteCount);
        report.Locations.Skip(2).ShouldAllBe(location => location.Status == WireStatus.Normal);
    }

    [Fact]
    public async Task GetActivityHealthSiteGoingSilentSummaryIsNormal()
    {
        var report = await GetReportAsync(SiteGoingSilentPath);

        report.Summary.ShouldHaveRange(count: 69, low: 41, high: 111, WireStatus.Normal);
    }

    [Fact]
    public async Task GetActivityHealthSiteGoingSilentSiteGWithZeroIsBelowAndRankedFirst()
    {
        var report = await GetReportAsync(SiteGoingSilentPath);

        var siteG = report.Locations[0];
        siteG.Location.ShouldBe("Site G");
        siteG.ShouldHaveRange(count: 0, low: 2, high: 9, WireStatus.Below);
        siteG.ShouldHaveDeviation(-3.19);
    }

    [Fact]
    public async Task GetActivityHealthSpikeInBaselineSummaryIsNormalWithUnroundedMedian()
    {
        var report = await GetReportAsync(SpikeInBaselinePath);

        report.Summary.ShouldHaveRange(count: 87, low: 30, high: 134, WireStatus.Normal);
        report.Summary.ShouldHaveMedian(72.5);
        report.Summary.ShouldHaveDeviation(0.53);
        report.Summary.WeeksUsed.ShouldBe(8);
    }

    [Fact]
    public async Task GetActivityHealthSpikeInBaselineAllFifteenSitesAreNormal()
    {
        var report = await GetReportAsync(SpikeInBaselinePath);

        report.Locations.Count.ShouldBe(SiteCount);
        report.Locations.ShouldAllBe(location => location.Status == WireStatus.Normal);
    }

    [Fact]
    public async Task GetActivityHealthSpikeInBaselineSiteMIsRankedFirst()
    {
        var report = await GetReportAsync(SpikeInBaselinePath);

        var siteM = report.Locations[0];
        siteM.Location.ShouldBe("Site M");
        siteM.ShouldHaveRange(count: 7, low: 1, high: 9, WireStatus.Normal);
        siteM.ShouldHaveMedian(3.5);
        siteM.ShouldHaveDeviation(1.30);
        siteM.WeeksUsed.ShouldBe(8);
    }

    [Fact]
    public async Task GetActivityHealthCallsFilterSummaryIsNormal()
    {
        var report = await GetReportAsync(SpikeInBaselineCallsPath);

        report.EventType.ShouldBe("call_received");
        report.Summary.ShouldHaveRange(count: 51, low: 17, high: 79, WireStatus.Normal);
        report.Summary.ShouldHaveMedian(42);
        report.Summary.ShouldHaveDeviation(0.54);
    }

    [Fact]
    public async Task GetActivityHealthCallsFilterKeepsAllFifteenSites()
    {
        var report = await GetReportAsync(SpikeInBaselineCallsPath);

        report.Locations.Count.ShouldBe(SiteCount);
    }
}
