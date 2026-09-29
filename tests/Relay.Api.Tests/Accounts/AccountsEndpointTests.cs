using System.Net;
using Relay.Api.Tests.Fixtures;

namespace Relay.Api.Tests.Accounts;

[Collection(SeededApiTestGroup.Name)]
public sealed class AccountsEndpointTests(SeededApiFixture fixture) : SeededApiTest(fixture)
{
    [Fact]
    public async Task ListAccountsReturnsAllTwentyAccountsIncludingTheEmptyOneOrderedByNameOrdinal()
    {
        int[] accountIdsInOrdinalNameOrder = [14, 3, 18, 4, 7, 17, 10, 2, 9, 8, 6, 11, 16, 5, 20, 12, 19, 15, 1, 13];

        var response = await GetAsync("/api/accounts");

        response.StatusCode.ShouldBe(HttpStatusCode.OK, response.Body);
        response.MediaType.ShouldBe(ApiResponse.JsonMediaType);
        var accounts = response.Json.EnumerateArray().ToList();
        accounts.Select(account => account.GetProperty("id").GetInt32()).ShouldBe(accountIdsInOrdinalNameOrder);
        foreach (var account in accounts)
        {
            account.EnumerateObject().Select(property => property.Name).ShouldBe(["id", "name", "timezone"], ignoreOrder: true);
        }

        var emptyAccount = accounts.Single(account => account.GetProperty("id").GetInt32() == 20);
        emptyAccount.GetProperty("name").GetString().ShouldBe("Quiet Harbor Spa");
        emptyAccount.GetProperty("timezone").GetString().ShouldBe("America/Los_Angeles");
    }
}
