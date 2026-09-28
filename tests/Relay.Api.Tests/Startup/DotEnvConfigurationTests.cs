using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Relay.Api.Tests.Fixtures;

namespace Relay.Api.Tests.Startup;

[Collection(ProcessEnvironmentTestGroup.Name)]
public sealed class DotEnvConfigurationTests : IDisposable
{
    private const string ConnectionStringVariable = "ConnectionStrings__Relay";
    private const string SaPasswordVariable = "RELAY_DB_SA_PASSWORD";
    private const string DatabasePortVariable = "RELAY_DB_PORT";
    private const string EnvironmentConnectionString = "Server=from-the-real-environment,1433;Database=relay;TrustServerCertificate=True";

    private readonly DotEnvDirectory dotEnvDirectory = new();
    private readonly EnvironmentVariableScope environmentWithoutRelayVariables = new(new Dictionary<string, string?>
    {
        [ConnectionStringVariable] = null,
        [SaPasswordVariable] = null,
        [DatabasePortVariable] = null,
    });

    [Fact]
    public async Task StartInDevelopmentResolvesTheDotEnvConnectionStringWithInterpolation()
    {
        await using var factory = new DotEnvApiFactory(dotEnvDirectory.ContentRoot, Environments.Development);

        var configuration = factory.Services.GetRequiredService<IConfiguration>();

        configuration[RelayApiFactory.ConnectionStringSetting].ShouldBe(DotEnvDirectory.InterpolatedConnectionString);
    }

    [Fact]
    public async Task StartInDevelopmentPrefersARealEnvironmentVariableOverTheDotEnvValue()
    {
        using var realConnectionString = new EnvironmentVariableScope(new Dictionary<string, string?>
        {
            [ConnectionStringVariable] = EnvironmentConnectionString,
        });
        await using var factory = new DotEnvApiFactory(dotEnvDirectory.ContentRoot, Environments.Development);

        var configuration = factory.Services.GetRequiredService<IConfiguration>();

        configuration[DatabasePortVariable].ShouldBe(DotEnvDirectory.DatabasePort);
        configuration[RelayApiFactory.ConnectionStringSetting].ShouldBe(EnvironmentConnectionString);
    }

    [Fact]
    public async Task StartInProductionIgnoresTheDotEnvThatDevelopmentLoads()
    {
        await using var developmentFactory = new DotEnvApiFactory(dotEnvDirectory.ContentRoot, Environments.Development);
        await using var productionFactory = new DotEnvApiFactory(dotEnvDirectory.ContentRoot, Environments.Production);

        var developmentConfiguration = developmentFactory.Services.GetRequiredService<IConfiguration>();
        var productionConfiguration = productionFactory.Services.GetRequiredService<IConfiguration>();

        developmentConfiguration[RelayApiFactory.ConnectionStringSetting].ShouldBe(DotEnvDirectory.InterpolatedConnectionString);
        productionConfiguration[RelayApiFactory.ConnectionStringSetting].ShouldBeNull();
        productionConfiguration[DatabasePortVariable].ShouldBeNull();
    }

    [Fact]
    public async Task StartInDevelopmentLoadsTheDotEnvWithoutSettingProcessEnvironmentVariables()
    {
        await using var factory = new DotEnvApiFactory(dotEnvDirectory.ContentRoot, Environments.Development);

        var configuration = factory.Services.GetRequiredService<IConfiguration>();

        configuration[SaPasswordVariable].ShouldBe(DotEnvDirectory.SaPassword);
        Environment.GetEnvironmentVariable(ConnectionStringVariable).ShouldBeNull();
        Environment.GetEnvironmentVariable(SaPasswordVariable).ShouldBeNull();
        Environment.GetEnvironmentVariable(DatabasePortVariable).ShouldBeNull();
    }

    public void Dispose()
    {
        environmentWithoutRelayVariables.Dispose();
        dotEnvDirectory.Dispose();
    }
}
