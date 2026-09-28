using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Relay.Api.Tests.Fixtures;
using Relay.Core.Calendar;
using Relay.Core.Queries;

namespace Relay.Api.Tests.Seed;

[Collection(SeededApiTestGroup.Name)]
public sealed class SeedLoadTests(SeededApiFixture fixture) : SeededApiTest(fixture)
{
    private const int WeeksFromJanuary26ThroughJuly27 = 27;

    private static readonly DateOnly FirstSeedWeek = new(2026, 1, 26);

    [Fact]
    public async Task SeedMigrationLoadsTwentyAccounts()
    {
        await using var dbContext = Fixture.CreateSeededDbContext();

        (await dbContext.Accounts.CountAsync(CancellationToken)).ShouldBe(20);
    }

    [Fact]
    public async Task SeedMigrationLoadsEveryRawEventIncludingDuplicates()
    {
        await using var dbContext = Fixture.CreateSeededDbContext();

        (await dbContext.ActivityEvents.CountAsync(CancellationToken)).ShouldBe(12626);
    }

    [Fact]
    public async Task SeedMigrationKeepsBothRawRowsOfTheAccount1SiteCDuplicate()
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

        rawRows.ShouldBe(5);
    }

    [Fact]
    public async Task CountWeeklyBySiteAccount1SiteCWeekOfJuly6CountsTheExactDuplicateOnce()
    {
        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var weekCalendar = scope.ServiceProvider.GetRequiredService<IWeekCalendar>();
        var activityQueries = scope.ServiceProvider.GetRequiredService<IActivityQueries>();
        var weekOfJuly6 = weekCalendar.Window(new DateOnly(2026, 7, 6), "America/Chicago");

        var weeklyCounts = await activityQueries.CountWeeklyBySiteAsync(1, [weekOfJuly6], ActivityType.All, CancellationToken);

        weeklyCounts.Single(weeklyCount => weeklyCount.Location == "Site C").Count.ShouldBe(4);
    }

    [Theory]
    [InlineData(1, 1221)]
    [InlineData(2, 729)]
    [InlineData(3, 477)]
    [InlineData(4, 796)]
    [InlineData(5, 884)]
    [InlineData(6, 2637)]
    [InlineData(7, 437)]
    [InlineData(8, 260)]
    [InlineData(9, 546)]
    [InlineData(10, 342)]
    [InlineData(11, 354)]
    [InlineData(12, 1303)]
    [InlineData(13, 205)]
    [InlineData(14, 638)]
    [InlineData(15, 499)]
    [InlineData(16, 167)]
    [InlineData(17, 323)]
    [InlineData(18, 586)]
    [InlineData(19, 210)]
    [InlineData(20, 0)]
    public async Task CountWeeklyBySiteEveryWeekOfTheSeedSumsToTheDeduplicatedAccountTotal(int accountId, int expectedDeduplicatedEvents)
    {
        var deduplicatedEvents = await CountDeduplicatedEventsAcrossTheSeedAsync(accountId);

        deduplicatedEvents.ShouldBe(expectedDeduplicatedEvents);
    }

    [Fact]
    public async Task CountWeeklyBySiteEveryAccountAndWeekSumsTo12614()
    {
        var deduplicatedEvents = 0;
        for (var accountId = 1; accountId <= 20; accountId++)
        {
            deduplicatedEvents += await CountDeduplicatedEventsAcrossTheSeedAsync(accountId);
        }

        deduplicatedEvents.ShouldBe(12614);
    }

    [Fact]
    public async Task ListSiteFirstActivitiesAccount14SitesStartInTheGoldenWeeks()
    {
        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var weekCalendar = scope.ServiceProvider.GetRequiredService<IWeekCalendar>();
        var activityQueries = scope.ServiceProvider.GetRequiredService<IActivityQueries>();

        var siteFirstActivities = await activityQueries.ListSiteFirstActivitiesAsync(14, CancellationToken);

        siteFirstActivities
            .Select(site => (site.Location, FirstWeek: weekCalendar.WeekContaining(site.FirstActivityUtc, "America/New_York")))
            .ShouldBe(
                [
                    ("Site A", new DateOnly(2026, 2, 2)),
                    ("Site B", new DateOnly(2026, 1, 26)),
                    ("Site C", new DateOnly(2026, 2, 2)),
                    ("Site D", new DateOnly(2026, 1, 26)),
                ],
                ignoreOrder: true);
    }

    private async Task<int> CountDeduplicatedEventsAcrossTheSeedAsync(int accountId)
    {
        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var weekCalendar = scope.ServiceProvider.GetRequiredService<IWeekCalendar>();
        var accountQueries = scope.ServiceProvider.GetRequiredService<IAccountQueries>();
        var activityQueries = scope.ServiceProvider.GetRequiredService<IActivityQueries>();
        var account = (await accountQueries.FindAsync(accountId, CancellationToken)).ShouldNotBeNull();
        var seedWeekWindows = Enumerable.Range(0, WeeksFromJanuary26ThroughJuly27)
            .Select(weekIndex => weekCalendar.Window(FirstSeedWeek.AddDays(7 * weekIndex), account.Timezone))
            .ToList();

        var weeklyCounts = await activityQueries.CountWeeklyBySiteAsync(accountId, seedWeekWindows, ActivityType.All, CancellationToken);

        return weeklyCounts.Sum(weeklyCount => weeklyCount.Count);
    }
}
