using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Relay.Core.Queries;
using Relay.Infrastructure.Persistence;
using Relay.Infrastructure.Queries;

namespace Relay.Infrastructure.Composition;

public static class InfrastructureServiceCollectionExtensions
{
    public const string ConnectionStringName = "Relay";

    public static IServiceCollection AddRelayInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<RelayDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString(ConnectionStringName)));
        services.AddScoped<IActivityQueries, SqlActivityQueries>();
        services.AddScoped<IAccountQueries, SqlAccountQueries>();
        return services;
    }
}
