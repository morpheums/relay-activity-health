using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Relay.Api.Composition;

namespace Relay.Api.Tests.Fixtures;

public sealed class DotEnvApiFactory(string contentRoot, string environmentName) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(environmentName);
        builder.UseContentRoot(contentRoot);
        builder.ConfigureTestServices(services =>
        {
            var migratorRegistrations = services
                .Where(descriptor => descriptor.ImplementationType == typeof(DevelopmentDatabaseMigrator))
                .ToList();
            migratorRegistrations.ForEach(descriptor => services.Remove(descriptor));
        });
    }
}
