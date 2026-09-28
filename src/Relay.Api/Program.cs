using Relay.Api.Composition;
using Relay.Api.Endpoints;
using Relay.Infrastructure.Composition;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddRelayCore(builder.Configuration)
    .AddRelayInfrastructure(builder.Configuration)
    .AddRelayApi();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.MapRelayEndpoints();

app.Run();
