using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Relay.Api.Tests.Fixtures;

namespace Relay.Api.Tests.ActivityHealth;

[Collection(SeededApiTestGroup.Name)]
public sealed class EmptyDatabaseTests(SeededApiFixture fixture) : SeededApiTest(fixture)
{
    private const string EmptyDatabaseName = "relay_api_empty_database";

    private const string InsertBeaconHomeSecuritySql = """
        INSERT INTO accounts (id, name, industry, timezone, created_at)
        VALUES (14, 'Beacon Home Security', 'Home Services', 'America/New_York', '2025-06-28 12:00:00')
        """;

    private static readonly DateTimeOffset MondayNoonInNewYork = new(2026, 9, 28, 12, 0, 0, TimeSpan.FromHours(-4));

    [Fact]
    public async Task GetActivityHealthEmptyDatabaseReturnsEmptyStateWithNullDataAsOf()
    {
        await using var emptyDatabase = await Fixture.CreateScratchDatabaseAsync(
            EmptyDatabaseName,
            SeededApiFixture.InitialCreateMigration,
            CancellationToken);
        await using (var dbContext = emptyDatabase.CreateDbContext())
        {
            await dbContext.Database.ExecuteSqlRawAsync(InsertBeaconHomeSecuritySql, CancellationToken);
        }

        await using var factory = new RelayApiFactory(
            emptyDatabase.ConnectionString,
            Environments.Production,
            services => services.AddSingleton<TimeProvider>(new FixedTimeProvider(MondayNoonInNewYork)));
        using var client = factory.CreateClient();

        var report = (await ApiResponse.GetAsync(client, "/api/accounts/14/activity-health", CancellationToken)).ShouldBeHealthReport();

        report.DataAsOf.ValueKind.ShouldBe(JsonValueKind.Null);
        report.LatestCompleteWeek.ShouldBe("2026-09-21");
        report.EarliestWeek.ShouldBe("2026-09-21");
        report.WeekStart.ShouldBe("2026-09-21");
        report.WeekEnd.ShouldBe("2026-09-27");
        report.Summary.ShouldBeInsufficient(count: 0, weeksUsed: 0);
        report.Locations.ShouldBeEmpty();
    }
}
