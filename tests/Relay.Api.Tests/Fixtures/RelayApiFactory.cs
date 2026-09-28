using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Relay.Infrastructure.Composition;

namespace Relay.Api.Tests.Fixtures;

public sealed class RelayApiFactory(
    string? connectionString,
    string environmentName,
    Action<IServiceCollection>? overrideServices = null) : WebApplicationFactory<Program>
{
    public const string ConnectionStringSetting = $"ConnectionStrings:{InfrastructureServiceCollectionExtensions.ConnectionStringName}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(environmentName);
        builder.ConfigureAppConfiguration(configuration =>
            configuration.AddInMemoryCollection([new KeyValuePair<string, string?>(ConnectionStringSetting, connectionString)]));
        if (overrideServices is not null)
        {
            builder.ConfigureTestServices(overrideServices);
        }
    }
}
