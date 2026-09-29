using Microsoft.EntityFrameworkCore;
using Relay.Infrastructure.Tests.Fixtures;

namespace Relay.Infrastructure.Tests.Schema;

[Collection(SqlServerTestGroup.Name)]
public sealed class InitialCreateSchemaTests(SqlServerFixture fixture) : SqlServerTest(fixture)
{
    [Fact]
    public async Task InitialCreateAccountOccurredIndexKeysAccountThenOccurredAtAndCoversEveryDeduplicationColumn()
    {
        var indexColumns = await QueryDbContext.Database.SqlQuery<string>($"""
            SELECT CONCAT(CASE WHEN index_column.is_included_column = 1 THEN 'include:' ELSE 'key:' END, indexed_column.name) AS Value
            FROM sys.indexes AS table_index
            JOIN sys.index_columns AS index_column
              ON index_column.object_id = table_index.object_id AND index_column.index_id = table_index.index_id
            JOIN sys.columns AS indexed_column
              ON indexed_column.object_id = index_column.object_id AND indexed_column.column_id = index_column.column_id
            WHERE table_index.object_id = OBJECT_ID(N'dbo.activity_events')
              AND table_index.name = N'IX_activity_events_account_occurred'
            ORDER BY index_column.is_included_column, index_column.key_ordinal, indexed_column.name
            """).ToListAsync(CancellationToken);

        indexColumns.ShouldBe(
        [
            "key:account_id",
            "key:occurred_at",
            "include:duration_seconds",
            "include:event_type",
            "include:location",
            "include:outcome",
        ]);
    }
}
