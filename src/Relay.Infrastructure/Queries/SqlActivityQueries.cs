using System.Data;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Relay.Core.Calendar;
using Relay.Core.Queries;
using Relay.Infrastructure.Persistence;

namespace Relay.Infrastructure.Queries;

public sealed class SqlActivityQueries(RelayDbContext dbContext) : IActivityQueries
{
    private const int EventTypeLength = 40;

    private const string DataAnchorSql = """
        SELECT MAX(events.occurred_at) AS Value
        FROM activity_events AS events
        """;

    private const string SiteFirstActivitiesSql = """
        SELECT events.location AS Location, MIN(events.occurred_at) AS FirstActivityUtc
        FROM activity_events AS events
        WHERE events.account_id = @accountId
        GROUP BY events.location
        """;

    private const string WeeklySiteCountsSql = """
        SELECT deduplicated.location AS Location, deduplicated.week_start AS WeekStart, COUNT(*) AS Count
        FROM (
            SELECT DISTINCT
                windows.week_start,
                events.location,
                events.event_type,
                events.occurred_at,
                COALESCE(events.duration_seconds, 0) AS duration_seconds,
                COALESCE(events.outcome, '') AS outcome
            FROM OPENJSON(@windows) WITH (
                week_start date '$.weekStart',
                window_start datetime2 '$.startUtc',
                window_end datetime2 '$.endUtc'
            ) AS windows
            JOIN activity_events AS events
                ON events.account_id = @accountId
                AND events.occurred_at >= windows.window_start
                AND events.occurred_at < windows.window_end
            WHERE @eventType IS NULL OR events.event_type = @eventType
        ) AS deduplicated
        GROUP BY deduplicated.location, deduplicated.week_start
        """;

    public async Task<DateTime?> FindDataAnchorAsync(CancellationToken cancellationToken)
    {
        var dataAnchors = await dbContext.Database
            .SqlQueryRaw<DateTime?>(DataAnchorSql)
            .ToListAsync(cancellationToken);
        return dataAnchors.Single();
    }

    public async Task<IReadOnlyList<SiteFirstActivity>> ListSiteFirstActivitiesAsync(int accountId, CancellationToken cancellationToken)
    {
        var siteFirstActivities = await dbContext.Database
            .SqlQueryRaw<SiteFirstActivity>(SiteFirstActivitiesSql, AccountIdParameter(accountId))
            .ToListAsync(cancellationToken);
        return [.. siteFirstActivities.Select(site => site with { FirstActivityUtc = AsUtc(site.FirstActivityUtc) })];
    }

    public async Task<IReadOnlyList<WeeklySiteCount>> CountWeeklyBySiteAsync(
        int accountId,
        IReadOnlyList<WeekWindow> weekWindows,
        ActivityType eventType,
        CancellationToken cancellationToken) =>
        await dbContext.Database
            .SqlQueryRaw<WeeklySiteCount>(
                WeeklySiteCountsSql,
                WindowsParameter(weekWindows),
                AccountIdParameter(accountId),
                EventTypeParameter(eventType))
            .ToListAsync(cancellationToken);

    private static SqlParameter AccountIdParameter(int accountId) =>
        new("@accountId", SqlDbType.Int) { Value = accountId };

    private static SqlParameter WindowsParameter(IReadOnlyList<WeekWindow> weekWindows)
    {
        var utcWindows = weekWindows.Select(window => new
        {
            weekStart = window.WeekStart,
            startUtc = ToUniversal(window.StartUtc),
            endUtc = ToUniversal(window.EndUtc),
        });
        return new SqlParameter("@windows", SqlDbType.NVarChar, -1) { Value = JsonSerializer.Serialize(utcWindows) };
    }

    private static SqlParameter EventTypeParameter(ActivityType eventType) =>
        new("@eventType", SqlDbType.VarChar, EventTypeLength)
        {
            Value = eventType == ActivityType.All
                ? DBNull.Value
                : JsonNamingPolicy.SnakeCaseLower.ConvertName(eventType.ToString()),
        };

    private static DateTime ToUniversal(DateTime instant) =>
        instant.Kind == DateTimeKind.Local ? instant.ToUniversalTime() : AsUtc(instant);

    private static DateTime AsUtc(DateTime instant) => DateTime.SpecifyKind(instant, DateTimeKind.Utc);
}
