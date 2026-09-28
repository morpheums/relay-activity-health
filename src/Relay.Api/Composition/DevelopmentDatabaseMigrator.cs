using Microsoft.EntityFrameworkCore;
using Relay.Infrastructure.Composition;
using Relay.Infrastructure.Persistence;

namespace Relay.Api.Composition;

public sealed partial class DevelopmentDatabaseMigrator(
    IConfiguration configuration,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<DevelopmentDatabaseMigrator> logger) : IHostedService
{
    private const string ConnectionStringSetting = $"ConnectionStrings:{InfrastructureServiceCollectionExtensions.ConnectionStringName}";
    private const string ConnectionStringEnvironmentVariable = $"ConnectionStrings__{InfrastructureServiceCollectionExtensions.ConnectionStringName}";

    // The ~2.4 MB seed runs as one migration and can exceed EF's default 30 s command timeout.
    private static readonly TimeSpan MigrationCommandTimeout = TimeSpan.FromMinutes(10);

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        EnsureConnectionStringIsSet();

        await using var scope = serviceScopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RelayDbContext>();
        dbContext.Database.SetCommandTimeout(MigrationCommandTimeout);

        LogApplyingMigrations(logger);
        await dbContext.Database.MigrateAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private void EnsureConnectionStringIsSet()
    {
        if (!string.IsNullOrWhiteSpace(configuration.GetConnectionString(InfrastructureServiceCollectionExtensions.ConnectionStringName)))
        {
            return;
        }

        throw new InvalidOperationException(
            $"The connection string '{ConnectionStringSetting}' is missing or empty. Copy .env.example to .env at the repository root (or set the environment variable '{ConnectionStringEnvironmentVariable}') before starting the API.");
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Applying Relay database migrations on start (Development)")]
    private static partial void LogApplyingMigrations(ILogger logger);
}
