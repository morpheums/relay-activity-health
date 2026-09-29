using Relay.Api.Tests.Fixtures;

namespace Relay.Api.Tests.ActivityHealth;

[Collection(SeededApiTestGroup.Name)]
public sealed class DefaultWeekGoldenTests(SeededApiFixture fixture) : SeededApiTest(fixture)
{
    private const string LatestCompleteWeek = "2026-07-20";
    private const string January26 = "2026-01-26";
    private const string February2 = "2026-02-02";

    [Fact]
    public async Task GetActivityHealthAccount14DefaultRequestReportsLatestCompleteWeekAllTypesExactDataAsOfAndConstants()
    {
        var report = await GetReportAsync("/api/accounts/14/activity-health");

        report.WeekStart.ShouldBe("2026-07-20");
        report.WeekEnd.ShouldBe("2026-07-26");
        report.EventType.ShouldBe("all");
        report.LatestCompleteWeek.ShouldBe("2026-07-20");
        report.EarliestWeek.ShouldBe("2026-01-26");
        report.DataAsOf.GetString().ShouldBe("2026-07-27T22:20:34Z");
        report.Root.GetProperty("baselineWeeks").GetInt32().ShouldBe(8);
        report.Root.GetProperty("minimumEligibleWeeks").GetInt32().ShouldBe(4);
    }

    [Fact]
    public async Task GetActivityHealthAccount14DefaultWeekIsNormalWithSiteBBelowRankedFirstThenCAD()
    {
        var report = await GetReportAsync("/api/accounts/14/activity-health");

        report.Summary.ShouldHaveRange(count: 26, low: 18, high: 38, WireStatus.Normal);
        report.Summary.ShouldHaveMedian(27);
        report.LocationNames.ShouldBe(["Site B", "Site C", "Site A", "Site D"]);
        report.Locations[0].ShouldHaveRange(count: 2, low: 3, high: 12, WireStatus.Below);
        report.Locations[0].ShouldHaveDeviation(-2.16);
        report.Locations.Skip(1).ShouldAllBe(location => location.Status == WireStatus.Normal);
    }

    [Fact]
    public async Task GetActivityHealthAccount12DefaultWeekFlagsNothingWithSiteFRankedFirst()
    {
        var report = await GetReportAsync("/api/accounts/12/activity-health");

        report.Summary.ShouldHaveRange(count: 54, low: 40, high: 74, WireStatus.Normal);
        report.Summary.ShouldHaveMedian(56);
        var siteF = report.Locations[0];
        siteF.Location.ShouldBe("Site F");
        siteF.ShouldHaveRange(count: 11, low: 2, high: 11, WireStatus.Normal);
        siteF.ShouldHaveDeviation(1.90);
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
