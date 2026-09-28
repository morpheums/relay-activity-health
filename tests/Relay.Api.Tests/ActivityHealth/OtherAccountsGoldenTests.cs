using Relay.Api.Tests.Fixtures;

namespace Relay.Api.Tests.ActivityHealth;

[Collection(SeededApiTestGroup.Name)]
public sealed class OtherAccountsGoldenTests(SeededApiFixture fixture) : SeededApiTest(fixture)
{
    private const string Account18PartialEligibilityPath = "/api/accounts/18/activity-health?week=2026-03-23";

    [Fact]
    public async Task GetActivityHealthAccount1SiteCWithExactDuplicateCountsItOnce()
    {
        var report = await GetReportAsync("/api/accounts/1/activity-health?week=2026-07-06");

        report.Location("Site C").Count.ShouldBe(4);
    }

    [Fact]
    public async Task GetActivityHealthAccount18SevenEligibleWeeksSummaryIsNormal()
    {
        var report = await GetReportAsync(Account18PartialEligibilityPath);

        report.Summary.ShouldHaveRange(count: 18, low: 15, high: 33, WireStatus.Normal);
        report.Summary.ShouldHaveMedian(23);
        report.Summary.WeeksUsed.ShouldBe(7);
    }

    [Fact]
    public async Task GetActivityHealthAccount18SiteCWithZeroIsBelowAndRankedFirst()
    {
        var report = await GetReportAsync(Account18PartialEligibilityPath);

        var siteC = report.Locations[0];
        siteC.Location.ShouldBe("Site C");
        siteC.ShouldHaveRange(count: 0, low: 1, high: 9, WireStatus.Below);
        siteC.ShouldHaveDeviation(-2.90);
    }
}
