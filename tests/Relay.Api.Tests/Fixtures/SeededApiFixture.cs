using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Relay.Infrastructure.Persistence;
using Testcontainers.MsSql;

namespace Relay.Api.Tests.Fixtures;

public sealed class SeededApiFixture : IAsyncLifetime
{
    public const string InitialCreateMigration = "InitialCreate";
    public const string LoadSeedDataMigration = "LoadSeedData";

    private const string SqlServerImage = "mcr.microsoft.com/mssql/server:2022-latest";
    private const string SeededDatabaseName = "relay_api_seeded";

    private static readonly TimeSpan SeedLoadTimeout = TimeSpan.FromMinutes(10);

    private readonly MsSqlContainer _container = new MsSqlBuilder(SqlServerImage).Build();

    public string SeededConnectionString { get; private set; } = string.Empty;

    public RelayApiFactory Factory { get; private set; } = null!;

    public HttpClient Client { get; private set; } = null!;

    public RelayDbContext CreateSeededDbContext() => CreateDbContext(SeededConnectionString);

    public string ConnectionStringFor(string databaseName) =>
        new SqlConnectionStringBuilder(_container.GetConnectionString()) { InitialCatalog = databaseName }.ConnectionString;

    public async Task<ScratchDatabase> CreateScratchDatabaseAsync(string databaseName, string? targetMigration, CancellationToken cancellationToken)
    {
        var scratchDatabase = new ScratchDatabase(ConnectionStringFor(databaseName));
        if (targetMigration is not null)
        {
            await using var dbContext = scratchDatabase.CreateDbContext();
            dbContext.Database.SetCommandTimeout(SeedLoadTimeout);
            await dbContext.Database.MigrateAsync(targetMigration, cancellationToken);
        }

        return scratchDatabase;
    }

    public RelayApiFactory CreateFactory(Action<IServiceCollection> overrideServices) =>
        new(SeededConnectionString, Environments.Development, overrideServices);

    public async ValueTask InitializeAsync()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await _container.StartAsync(cancellationToken);
        SeededConnectionString = ConnectionStringFor(SeededDatabaseName);

        await using (var dbContext = CreateSeededDbContext())
        {
            dbContext.Database.SetCommandTimeout(SeedLoadTimeout);
            await dbContext.Database.MigrateAsync(LoadSeedDataMigration, cancellationToken);
        }

        Factory = new RelayApiFactory(SeededConnectionString, Environments.Development);
        Client = Factory.CreateClient();
    }

    public async ValueTask DisposeAsync()
    {
        Client?.Dispose();
        if (Factory is not null)
        {
            await Factory.DisposeAsync();
        }

        await _container.DisposeAsync();
    }

    private static RelayDbContext CreateDbContext(string connectionString) =>
        new(new DbContextOptionsBuilder<RelayDbContext>().UseSqlServer(connectionString).Options);
}
