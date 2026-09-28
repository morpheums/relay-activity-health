using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Relay.Infrastructure.Composition;

namespace Relay.Infrastructure.Persistence;

public sealed class RelayDesignTimeDbContextFactory : IDesignTimeDbContextFactory<RelayDbContext>
{
    private const string ConnectionStringVariable = $"ConnectionStrings__{InfrastructureServiceCollectionExtensions.ConnectionStringName}";

    public RelayDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable(ConnectionStringVariable);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Environment variable {ConnectionStringVariable} is not set. Copy .env.example to .env, set the password, then run: set -a; source .env; set +a");
        }

        var options = new DbContextOptionsBuilder<RelayDbContext>().UseSqlServer(connectionString).Options;
        return new RelayDbContext(options);
    }
}
