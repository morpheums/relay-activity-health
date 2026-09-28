using Microsoft.EntityFrameworkCore;
using Relay.Infrastructure.Tests.Fixtures;

namespace Relay.Infrastructure.Tests.Schema;

[Collection(SqlServerTestGroup.Name)]
public sealed class InitialCreateSchemaTests(SqlServerFixture fixture) : SqlServerTest(fixture)
{
    [Fact]
    public async Task InitialCreateAccountOccurredIndexHasAccountThenOccurredAtKeys()
    {
        var keyColumns = await QueryDbContext.Database.SqlQuery<string>($"""
            SELECT indexed_column.name AS Value
            FROM sys.indexes AS table_index
            JOIN sys.index_columns AS index_column
              ON index_column.object_id = table_index.object_id AND index_column.index_id = table_index.index_id
            JOIN sys.columns AS indexed_column
              ON indexed_column.object_id = index_column.object_id AND indexed_column.column_id = index_column.column_id
            WHERE table_index.object_id = OBJECT_ID(N'dbo.activity_events')
              AND table_index.name = N'IX_activity_events_account_occurred'
              AND index_column.is_included_column = 0
            ORDER BY index_column.key_ordinal
            """).ToListAsync(CancellationToken);

        keyColumns.ShouldBe(["account_id", "occurred_at"]);
    }

    [Fact]
    public async Task InitialCreateAccountOccurredIndexIncludesEveryDeduplicationColumn()
    {
        var includedColumns = await QueryDbContext.Database.SqlQuery<string>($"""
            SELECT indexed_column.name AS Value
            FROM sys.indexes AS table_index
            JOIN sys.index_columns AS index_column
              ON index_column.object_id = table_index.object_id AND index_column.index_id = table_index.index_id
            JOIN sys.columns AS indexed_column
              ON indexed_column.object_id = index_column.object_id AND indexed_column.column_id = index_column.column_id
            WHERE table_index.object_id = OBJECT_ID(N'dbo.activity_events')
              AND table_index.name = N'IX_activity_events_account_occurred'
              AND index_column.is_included_column = 1
            """).ToListAsync(CancellationToken);

        includedColumns.ShouldBe(["location", "event_type", "duration_seconds", "outcome"], ignoreOrder: true);
    }

    [Fact]
    public async Task InitialCreateActivityEventsHasNoUniqueIndexOtherThanPrimaryKey()
    {
        var uniqueNonPrimaryIndexCounts = await QueryDbContext.Database.SqlQuery<int>($"""
            SELECT COUNT(*) AS Value
            FROM sys.indexes
            WHERE object_id = OBJECT_ID(N'dbo.activity_events')
              AND is_unique = 1
              AND is_primary_key = 0
            """).ToListAsync(CancellationToken);

        uniqueNonPrimaryIndexCounts.ShouldBe([0]);
    }
}
