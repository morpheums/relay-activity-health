using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Relay.Api.Tests.Fixtures;

namespace Relay.Api.Tests.Startup;

[Collection(SeededApiTestGroup.Name)]
public sealed class MigrateOnStartTests(SeededApiFixture fixture) : SeededApiTest(fixture)
{
    [Fact]
    public async Task StartInDevelopmentAgainstAMissingDatabaseAppliesEveryMigrationWithTheSeed()
    {
        await using var freshDatabase = await Fixture.CreateScratchDatabaseAsync("relay_api_migrate_on_start", targetMigration: null, CancellationToken);
        await using var factory = new RelayApiFactory(freshDatabase.ConnectionString, Environments.Development);

        using var client = factory.CreateClient();

        await using var dbContext = freshDatabase.CreateDbContext();
        (await dbContext.Accounts.CountAsync(CancellationToken)).ShouldBe(20);
        (await dbContext.ActivityEvents.CountAsync(CancellationToken)).ShouldBe(12626);
        (await dbContext.Database.GetPendingMigrationsAsync(CancellationToken)).ShouldBeEmpty();
    }
}
