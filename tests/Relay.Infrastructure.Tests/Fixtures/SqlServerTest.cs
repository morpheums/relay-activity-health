using Relay.Infrastructure.Persistence;

namespace Relay.Infrastructure.Tests.Fixtures;

public abstract class SqlServerTest(SqlServerFixture fixture) : IAsyncLifetime
{
    protected RelayTestDatabase Database { get; } = new(fixture);

    protected RelayDbContext QueryDbContext { get; } = fixture.CreateDbContext();

    protected static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    public virtual async ValueTask InitializeAsync() => await Database.DeleteAllRowsAsync(CancellationToken);

    public async ValueTask DisposeAsync()
    {
        await QueryDbContext.DisposeAsync();
        await Database.DeleteAllRowsAsync(CancellationToken.None);
        GC.SuppressFinalize(this);
    }
}
