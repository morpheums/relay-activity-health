using Microsoft.EntityFrameworkCore;
using Relay.Infrastructure.Persistence;

namespace Relay.Api.Tests.Fixtures;

public sealed class ScratchDatabase(string connectionString) : IAsyncDisposable
{
    public string ConnectionString { get; } = connectionString;

    public RelayDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<RelayDbContext>().UseSqlServer(ConnectionString).Options);

    public async ValueTask DisposeAsync()
    {
        await using var dbContext = CreateDbContext();
        await dbContext.Database.EnsureDeletedAsync(CancellationToken.None);
    }
}
