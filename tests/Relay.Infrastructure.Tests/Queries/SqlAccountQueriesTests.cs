using Relay.Core.Queries;
using Relay.Infrastructure.Queries;
using Relay.Infrastructure.Tests.Fixtures;
using static Relay.Infrastructure.Tests.Fixtures.EventRow;

namespace Relay.Infrastructure.Tests.Queries;

[Collection(SqlServerTestGroup.Name)]
public sealed class SqlAccountQueriesTests(SqlServerFixture fixture) : SqlServerTest(fixture)
{
    private static readonly AccountRow MetroCollision = new(6, "Metro Collision Centers", "America/New_York");
    private static readonly AccountRow BeaconHomeSecurity = new(14, "Beacon Home Security", "America/New_York");
    private static readonly AccountRow AccountWithoutEvents = new(20, "Quiet Account", "America/Phoenix");

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        await Database.InsertAccountsAsync([MetroCollision, BeaconHomeSecurity, AccountWithoutEvents], CancellationToken);
        await Database.InsertEventsAsync(
            [
                EventAt("2026-03-04T15:30:00Z") with { AccountId = MetroCollision.Id },
                EventAt("2026-03-04T15:30:00Z") with { AccountId = BeaconHomeSecurity.Id },
            ],
            CancellationToken);
    }

    [Fact]
    public async Task ListSeveralAccountsReturnsEveryAccountIncludingOneWithoutEvents()
    {
        var accounts = await Queries.ListAsync(CancellationToken);

        accounts.ShouldBe(
            [
                new AccountListItem(6, "Metro Collision Centers", "America/New_York"),
                new AccountListItem(14, "Beacon Home Security", "America/New_York"),
                new AccountListItem(20, "Quiet Account", "America/Phoenix"),
            ],
            ignoreOrder: true);
    }

    [Fact]
    public async Task FindAccountWithoutEventsReturnsItsNameAndTimezone()
    {
        var account = await Queries.FindAsync(20, CancellationToken);

        account.ShouldBe(new AccountListItem(20, "Quiet Account", "America/Phoenix"));
    }

    [Fact]
    public async Task FindUnknownAccountReturnsNull()
    {
        var account = await Queries.FindAsync(99, CancellationToken);

        account.ShouldBeNull();
    }

    private SqlAccountQueries Queries => new(QueryDbContext);
}
