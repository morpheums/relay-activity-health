using Microsoft.AspNetCore.Http.HttpResults;
using Relay.Api.Http;
using Relay.Core.ActivityHealth;

namespace Relay.Api.Endpoints;

public static class ActivityHealthEndpoints
{
    public static IEndpointRouteBuilder MapActivityHealthEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/api/accounts/{accountId:int}/activity-health", GetActivityHealth);
        return routes;
    }

    private static Task<Results<Ok<ActivityHealthResponse>, ProblemHttpResult>> GetActivityHealth(
        [AsParameters] ActivityHealthRequest request,
        IActivityHealthService activityHealthService,
        CancellationToken cancellationToken) =>
        throw new NotImplementedException();
}
