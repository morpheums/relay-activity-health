using Microsoft.AspNetCore.Http.HttpResults;
using Relay.Api.Http;
using Relay.Core.Accounts;

namespace Relay.Api.Endpoints;

public static class AccountEndpoints
{
    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder routes)
    {
        routes.MapGet("/api/accounts", ListAccounts);
        return routes;
    }

    private static Task<Ok<IReadOnlyList<AccountResponse>>> ListAccounts(
        IAccountService accountService,
        CancellationToken cancellationToken) =>
        throw new NotImplementedException();
}
