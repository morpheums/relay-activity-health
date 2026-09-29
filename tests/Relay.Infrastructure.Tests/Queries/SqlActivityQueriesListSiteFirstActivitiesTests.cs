using Relay.Core.Queries;
using Relay.Infrastructure.Queries;
using Relay.Infrastructure.Tests.Fixtures;
using static Relay.Infrastructure.Tests.Fixtures.EventRow;

namespace Relay.Infrastructure.Tests.Queries;

[Collection(SqlServerTestGroup.Name)]
public sealed class SqlActivityQueriesListSiteFirstActivitiesTests(SqlServerFixture fixture) : SqlServerTest(fixture)
{
    private const int OtherAccountId = 2;

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        await Database.InsertAccountsAsync(
            [
                new AccountRow(DefaultAccountId, "Target Account", "America/Chicago"),
                new AccountRow(OtherAccountId, "Other Account", "America/Chicago"),
            ],
            CancellationToken);
    }

    [Fact]
    public async Task ListSiteFirstActivitiesReturnsEarliestInstantPerLocationOfAnyEventType()
    {
        await Database.InsertEventsAsync(
            [
                EventAt("2026-03-05T14:00:00Z"),
                EventAt("2026-03-02T06:00:00Z") with { EventType = LeadCreated, DurationSeconds = null, Outcome = null },
                EventAt("2026-04-01T09:30:00Z"),
                EventAt("2026-02-10T12:30:15Z") with { Location = "Site B", EventType = AppointmentSet, DurationSeconds = null },
                EventAt("2026-06-10T12:30:15Z") with { Location = "Site B" },
            ],
            CancellationToken);

        var siteFirstActivities = await ListAsync();

        siteFirstActivities.ShouldBe(
            [
                new SiteFirstActivity("Site A", Utc.At("2026-03-02T06:00:00Z")),
                new SiteFirstActivity("Site B", Utc.At("2026-02-10T12:30:15Z")),
            ],
            ignoreOrder: true);
    }

    [Fact]
    public async Task ListSiteFirstActivitiesOtherAccountsRowsAreIgnored()
    {
        await Database.InsertEventsAsync(
            [
                EventAt("2026-03-10T12:00:00Z"),
                EventAt("2026-02-01T12:00:00Z") with { AccountId = OtherAccountId },
                EventAt("2026-02-01T12:00:00Z") with { AccountId = OtherAccountId, Location = "Site Q" },
            ],
            CancellationToken);

        var siteFirstActivities = await ListAsync();

        siteFirstActivities.ShouldBe([new SiteFirstActivity("Site A", Utc.At("2026-03-10T12:00:00Z"))]);
    }

    private Task<IReadOnlyList<SiteFirstActivity>> ListAsync() =>
        new SqlActivityQueries(QueryDbContext).ListSiteFirstActivitiesAsync(DefaultAccountId, CancellationToken);
}
