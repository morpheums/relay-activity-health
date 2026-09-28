using Microsoft.AspNetCore.Http.HttpResults;
using Relay.Api.Http;
using Relay.Core.Accounts;
using Relay.Core.ActivityHealth;
using Relay.Core.Queries;

namespace Relay.Api.Endpoints;

public static class RelayEndpoints
{
    public static IEndpointRouteBuilder MapRelayEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/api/accounts", ListAccounts);
        routes.MapGet("/api/accounts/{accountId:int}/activity-health", GetActivityHealth);
        return routes;
    }

    private static Task<Ok<IReadOnlyList<AccountListItem>>> ListAccounts(
        IAccountService accountService,
        CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    private static Task<Results<Ok<ActivityHealthReport>, ProblemHttpResult>> GetActivityHealth(
        [AsParameters] ActivityHealthRequest request,
        IActivityHealthService activityHealthService,
        CancellationToken cancellationToken) =>
        throw new NotImplementedException();
}
