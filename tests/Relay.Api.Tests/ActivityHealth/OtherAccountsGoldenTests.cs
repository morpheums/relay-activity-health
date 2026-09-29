using Microsoft.EntityFrameworkCore;
using Relay.Api.Tests.Fixtures;

namespace Relay.Api.Tests.ActivityHealth;

[Collection(SeededApiTestGroup.Name)]
public sealed class OtherAccountsGoldenTests(SeededApiFixture fixture) : SeededApiTest(fixture)
{
    [Fact]
    public async Task GetActivityHealthAccount1SiteCCountsFiveRawRowsWithOneExactDuplicateAsFour()
    {
        var chicagoWeekOfJuly6StartUtc = new DateTime(2026, 7, 6, 5, 0, 0, DateTimeKind.Utc);
        var chicagoWeekOfJuly6EndUtc = new DateTime(2026, 7, 13, 5, 0, 0, DateTimeKind.Utc);
        await using var dbContext = Fixture.CreateSeededDbContext();
        var rawRows = await dbContext.ActivityEvents.CountAsync(
            activityEvent => activityEvent.AccountId == 1
                && activityEvent.Location == "Site C"
                && activityEvent.OccurredAt >= chicagoWeekOfJuly6StartUtc
                && activityEvent.OccurredAt < chicagoWeekOfJuly6EndUtc,
            CancellationToken);

        var report = await GetReportAsync("/api/accounts/1/activity-health?week=2026-07-06");

        rawRows.ShouldBe(5);
        report.Location("Site C").Count.ShouldBe(4);
    }

    [Fact]
    public async Task GetActivityHealthAccount18SevenEligibleWeeksIsNormalWithSiteCBelowAndRankedFirst()
    {
        var report = await GetReportAsync("/api/accounts/18/activity-health?week=2026-03-23");

        report.Summary.ShouldHaveRange(count: 18, low: 15, high: 33, WireStatus.Normal);
        report.Summary.ShouldHaveMedian(23);
        report.Summary.WeeksUsed.ShouldBe(7);
        var siteC = report.Locations[0];
        siteC.Location.ShouldBe("Site C");
        siteC.ShouldHaveRange(count: 0, low: 1, high: 9, WireStatus.Below);
        siteC.ShouldHaveDeviation(-2.90);
    }
}
