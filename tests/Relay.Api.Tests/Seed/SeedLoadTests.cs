using Microsoft.Extensions.DependencyInjection;
using Relay.Api.Tests.Fixtures;
using Relay.Core.Calendar;
using Relay.Core.Queries;

namespace Relay.Api.Tests.Seed;

[Collection(SeededApiTestGroup.Name)]
public sealed class SeedLoadTests(SeededApiFixture fixture) : SeededApiTest(fixture)
{
    private const int WeeksFromJanuary26ThroughJuly27 = 27;
    private const int SeedAccountCount = 20;

    private static readonly DateOnly FirstSeedWeek = new(2026, 1, 26);

    [Fact]
    public async Task CountWeeklyBySiteEveryAccountAndSeedWeekSumsTo12614DeduplicatedEvents()
    {
        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var weekCalendar = scope.ServiceProvider.GetRequiredService<IWeekCalendar>();
        var accountQueries = scope.ServiceProvider.GetRequiredService<IAccountQueries>();
        var activityQueries = scope.ServiceProvider.GetRequiredService<IActivityQueries>();
        var deduplicatedEvents = 0;

        for (var accountId = 1; accountId <= SeedAccountCount; accountId++)
        {
            var account = (await accountQueries.FindAsync(accountId, CancellationToken)).ShouldNotBeNull();
            var seedWeekWindows = Enumerable.Range(0, WeeksFromJanuary26ThroughJuly27)
                .Select(weekIndex => weekCalendar.Window(FirstSeedWeek.AddDays(7 * weekIndex), account.Timezone))
                .ToList();
            var weeklyCounts = await activityQueries.CountWeeklyBySiteAsync(accountId, seedWeekWindows, ActivityType.All, CancellationToken);
            deduplicatedEvents += weeklyCounts.Sum(weeklyCount => weeklyCount.Count);
        }

        deduplicatedEvents.ShouldBe(12614);
    }
}
