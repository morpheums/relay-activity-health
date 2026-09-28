using Relay.Infrastructure.Queries;
using Relay.Infrastructure.Tests.Fixtures;
using static Relay.Infrastructure.Tests.Fixtures.EventRow;

namespace Relay.Infrastructure.Tests.Queries;

[Collection(SqlServerTestGroup.Name)]
public sealed class SqlActivityQueriesFindDataAnchorTests(SqlServerFixture fixture) : SqlServerTest(fixture)
{
    private const int OtherAccountId = 2;

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        await Database.InsertAccountsAsync(
            [
                new AccountRow(DefaultAccountId, "Target Account", "America/Chicago"),
                new AccountRow(OtherAccountId, "Other Account", "America/New_York"),
            ],
            CancellationToken);
    }

    [Fact]
    public async Task FindDataAnchorEventsInSeveralAccountsReturnsLatestInstantOverall()
    {
        await Database.InsertEventsAsync(
            [
                EventAt("2026-02-01T10:57:44Z"),
                EventAt("2026-07-27T22:20:34Z") with { AccountId = OtherAccountId, EventType = AppointmentSet },
                EventAt("2026-07-20T09:00:00Z"),
                EventAt("2026-03-15T12:00:00Z") with { AccountId = OtherAccountId },
            ],
            CancellationToken);

        var dataAnchor = await FindDataAnchorAsync();

        dataAnchor.ShouldBe(Utc.At("2026-07-27T22:20:34Z"));
    }

    [Fact]
    public async Task FindDataAnchorEventsExistReturnsUtcKind()
    {
        await Database.InsertEventsAsync([EventAt("2026-07-27T22:20:34Z")], CancellationToken);

        var dataAnchor = await FindDataAnchorAsync();

        dataAnchor.ShouldNotBeNull().Kind.ShouldBe(DateTimeKind.Utc);
    }

    [Fact]
    public async Task FindDataAnchorNoEventsReturnsNull()
    {
        var dataAnchor = await FindDataAnchorAsync();

        dataAnchor.ShouldBeNull();
    }

    private Task<DateTime?> FindDataAnchorAsync() =>
        new SqlActivityQueries(QueryDbContext).FindDataAnchorAsync(CancellationToken);
}
