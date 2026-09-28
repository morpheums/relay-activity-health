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
        var accounts = routes.MapGroup("/api/accounts");
        accounts.MapGet("/", ListAccounts);
        accounts.MapGet("/{accountId:int}/activity-health", GetActivityHealth);
        return routes;
    }

    private static async Task<Ok<IReadOnlyList<AccountListItem>>> ListAccounts(
        IAccountService accountService,
        CancellationToken cancellationToken) =>
        TypedResults.Ok(await accountService.ListAsync(cancellationToken));

    private static async Task<Results<Ok<ActivityHealthReport>, ProblemHttpResult>> GetActivityHealth(
        [AsParameters] ActivityHealthRequest request,
        IActivityHealthService activityHealthService,
        CancellationToken cancellationToken) =>
        (await activityHealthService.GetAsync(request.AccountId, request.ParsedWeek(), request.ParsedEventType(), cancellationToken))
            .ToHttpResult();
}
