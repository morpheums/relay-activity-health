namespace Relay.Api.Endpoints;

public static class RelayEndpoints
{
    public static IEndpointRouteBuilder MapRelayEndpoints(this IEndpointRouteBuilder routes) =>
        routes
            .MapAccountEndpoints()
            .MapActivityHealthEndpoints();
}
