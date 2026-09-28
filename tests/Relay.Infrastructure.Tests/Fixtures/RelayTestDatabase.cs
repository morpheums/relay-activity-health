using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Relay.Infrastructure.Persistence;

namespace Relay.Infrastructure.Tests.Fixtures;

public sealed class RelayTestDatabase(SqlServerFixture fixture)
{
    private const string DeleteAllRowsSql = "DELETE FROM activity_events; DELETE FROM accounts;";

    private const string InsertAccountSql = """
        INSERT INTO accounts (id, name, industry, timezone, created_at)
        VALUES (@id, @name, @industry, @timezone, @createdAt)
        """;

    private const string InsertEventSql = """
        INSERT INTO activity_events (id, account_id, location, event_type, occurred_at, duration_seconds, outcome)
        VALUES (@id, @accountId, @location, @eventType, @occurredAt, @durationSeconds, @outcome)
        """;

    private int _nextEventId = 1;

    public RelayDbContext CreateDbContext() => fixture.CreateDbContext();

    public async Task DeleteAllRowsAsync(CancellationToken cancellationToken)
    {
        await using var dbContext = fixture.CreateDbContext();
        await dbContext.Database.ExecuteSqlRawAsync(DeleteAllRowsSql, cancellationToken);
    }

    public async Task InsertAccountsAsync(IEnumerable<AccountRow> accounts, CancellationToken cancellationToken)
    {
        await using var dbContext = fixture.CreateDbContext();
        foreach (var account in accounts)
        {
            await dbContext.Database.ExecuteSqlRawAsync(
                InsertAccountSql,
                [
                    Parameter("@id", SqlDbType.Int, account.Id),
                    Parameter("@name", SqlDbType.VarChar, account.Name),
                    Parameter("@industry", SqlDbType.VarChar, account.Industry),
                    Parameter("@timezone", SqlDbType.VarChar, account.Timezone),
                    Parameter("@createdAt", SqlDbType.DateTime2, account.CreatedAtUtc),
                ],
                cancellationToken);
        }
    }

    public async Task InsertEventsAsync(IEnumerable<EventRow> events, CancellationToken cancellationToken)
    {
        await using var dbContext = fixture.CreateDbContext();
        foreach (var activityEvent in events)
        {
            await dbContext.Database.ExecuteSqlRawAsync(
                InsertEventSql,
                [
                    Parameter("@id", SqlDbType.Int, _nextEventId++),
                    Parameter("@accountId", SqlDbType.Int, activityEvent.AccountId),
                    Parameter("@location", SqlDbType.VarChar, activityEvent.Location),
                    Parameter("@eventType", SqlDbType.VarChar, activityEvent.EventType),
                    Parameter("@occurredAt", SqlDbType.DateTime2, activityEvent.OccurredAtUtc),
                    Parameter("@durationSeconds", SqlDbType.Int, activityEvent.DurationSeconds),
                    Parameter("@outcome", SqlDbType.VarChar, activityEvent.Outcome),
                ],
                cancellationToken);
        }
    }

    private static SqlParameter Parameter(string name, SqlDbType type, object? value) =>
        new(name, type) { Value = value ?? DBNull.Value };
}
