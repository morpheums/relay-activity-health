using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Relay.Infrastructure.Persistence;
using Testcontainers.MsSql;

namespace Relay.Infrastructure.Tests.Fixtures;

public sealed class SqlServerFixture : IAsyncLifetime
{
    public const string InitialCreateMigration = "InitialCreate";

    private const string SqlServerImage = "mcr.microsoft.com/mssql/server:2022-latest";
    private const string DatabaseName = "relay_infrastructure_tests";

    private readonly MsSqlContainer _container = new MsSqlBuilder(SqlServerImage).Build();
    private string _connectionString = string.Empty;

    public RelayDbContext CreateDbContext() =>
        new(new DbContextOptionsBuilder<RelayDbContext>().UseSqlServer(_connectionString).Options);

    public async ValueTask InitializeAsync()
    {
        await _container.StartAsync(TestContext.Current.CancellationToken);
        _connectionString = new SqlConnectionStringBuilder(_container.GetConnectionString())
        {
            InitialCatalog = DatabaseName,
        }.ConnectionString;

        await using var dbContext = CreateDbContext();
        await dbContext.Database.MigrateAsync(InitialCreateMigration, TestContext.Current.CancellationToken);
    }

    public async ValueTask DisposeAsync() => await _container.DisposeAsync();
}
