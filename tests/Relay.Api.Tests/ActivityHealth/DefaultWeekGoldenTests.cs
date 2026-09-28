using Relay.Api.Tests.Fixtures;

namespace Relay.Api.Tests.ActivityHealth;

[Collection(SeededApiTestGroup.Name)]
public sealed class DefaultWeekGoldenTests(SeededApiFixture fixture) : SeededApiTest(fixture)
{
    private const string LatestCompleteWeek = "2026-07-20";
    private const string January26 = "2026-01-26";
    private const string February2 = "2026-02-02";

    [Fact]
    public async Task GetActivityHealthAccount14DefaultWeekReportsTheLatestCompleteWeek()
    {
        var report = await GetReportAsync("/api/accounts/14/activity-health");

        report.WeekStart.ShouldBe("2026-07-20");
        report.WeekEnd.ShouldBe("2026-07-26");
        report.EventType.ShouldBe("all");
        report.LatestCompleteWeek.ShouldBe("2026-07-20");
        report.EarliestWeek.ShouldBe("2026-01-26");
    }

    [Fact]
    public async Task GetActivityHealthAccount14DefaultWeekSummaryIsNormal()
    {
        var report = await GetReportAsync("/api/accounts/14/activity-health");

        var summary = report.Summary;
        summary.ShouldHaveRange(count: 26, low: 18, high: 38, WireStatus.Normal);
        summary.ShouldHaveMedian(27);
    }

    [Fact]
    public async Task GetActivityHealthAccount14DefaultWeekSiteBIsBelowAndRankedFirst()
    {
        var report = await GetReportAsync("/api/accounts/14/activity-health");

        var siteB = report.Locations[0];
        siteB.Location.ShouldBe("Site B");
        siteB.ShouldHaveRange(count: 2, low: 3, high: 12, WireStatus.Below);
        siteB.ShouldHaveDeviation(-2.16);
    }

    [Fact]
    public async Task GetActivityHealthAccount14DefaultWeekRanksSitesBCAD()
    {
        var report = await GetReportAsync("/api/accounts/14/activity-health");

        report.LocationNames.ShouldBe(["Site B", "Site C", "Site A", "Site D"]);
    }

    [Fact]
    public async Task GetActivityHealthAccount14DefaultWeekOtherSitesAreNormal()
    {
        var report = await GetReportAsync("/api/accounts/14/activity-health");

        report.Locations.Skip(1).ShouldAllBe(location => location.Status == WireStatus.Normal);
    }

    [Fact]
    public async Task GetActivityHealthAccount12DefaultWeekSummaryIsNormal()
    {
        var report = await GetReportAsync("/api/accounts/12/activity-health");

        report.Summary.ShouldHaveRange(count: 54, low: 40, high: 74, WireStatus.Normal);
        report.Summary.ShouldHaveMedian(56);
    }

    [Fact]
    public async Task GetActivityHealthAccount12DefaultWeekSiteFIsNormalAndRankedFirst()
    {
        var report = await GetReportAsync("/api/accounts/12/activity-health");

        var siteF = report.Locations[0];
        siteF.Location.ShouldBe("Site F");
        siteF.ShouldHaveRange(count: 11, low: 2, high: 11, WireStatus.Normal);
        siteF.ShouldHaveDeviation(1.90);
    }

    [Fact]
    public async Task GetActivityHealthAccount12DefaultWeekFlagsNoLocation()
    {
        var report = await GetReportAsync("/api/accounts/12/activity-health");

        report.Locations.ShouldNotBeEmpty();
        report.Locations.ShouldAllBe(location => !WireStatus.Flagged.Contains(location.Status));
    }

    [Theory]
    [InlineData(1, January26)]
    [InlineData(2, February2)]
    [InlineData(3, February2)]
    [InlineData(4, January26)]
    [InlineData(5, January26)]
    [InlineData(6, January26)]
    [InlineData(7, January26)]
    [InlineData(8, February2)]
    [InlineData(9, February2)]
    [InlineData(10, February2)]
    [InlineData(11, February2)]
    [InlineData(12, January26)]
    [InlineData(13, February2)]
    [InlineData(14, January26)]
    [InlineData(15, February2)]
    [InlineData(16, February2)]
    [InlineData(17, February2)]
    [InlineData(18, January26)]
    [InlineData(19, February2)]
    [InlineData(20, LatestCompleteWeek)]
    public async Task GetActivityHealthEveryAccountDefaultWeekIsJuly20WithGoldenEarliestWeek(int accountId, string expectedEarliestWeek)
    {
        var report = await GetReportAsync($"/api/accounts/{accountId}/activity-health");

        report.WeekStart.ShouldBe(LatestCompleteWeek);
        report.LatestCompleteWeek.ShouldBe(LatestCompleteWeek);
        report.EarliestWeek.ShouldBe(expectedEarliestWeek);
    }
}
