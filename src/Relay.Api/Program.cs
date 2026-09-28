using DotNetEnv;
using DotNetEnv.Configuration;
using Relay.Api.Composition;
using Relay.Api.Endpoints;
using Relay.Infrastructure.Composition;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddDotNetEnv(
        Path.Combine(builder.Environment.ContentRootPath, ".env"),
        LoadOptions.TraversePath().NoClobber().NoEnvVars());
}

builder.Services
    .AddRelayCore(builder.Configuration)
    .AddRelayInfrastructure(builder.Configuration)
    .AddRelayApi();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHostedService<DevelopmentDatabaseMigrator>();
}

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.MapRelayEndpoints();

await app.RunAsync();
