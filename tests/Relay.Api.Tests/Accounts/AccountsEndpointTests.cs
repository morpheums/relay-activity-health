using System.Net;
using System.Text.Json;
using Relay.Api.Tests.Fixtures;

namespace Relay.Api.Tests.Accounts;

[Collection(SeededApiTestGroup.Name)]
public sealed class AccountsEndpointTests(SeededApiFixture fixture) : SeededApiTest(fixture)
{
    private const string AccountsPath = "/api/accounts";

    [Fact]
    public async Task ListAccountsSeededDatabaseReturnsTwentyAccounts()
    {
        var response = await GetAsync(AccountsPath);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, response.Body);
        response.MediaType.ShouldBe(ApiResponse.JsonMediaType);
        response.Json.GetArrayLength().ShouldBe(20);
    }

    [Fact]
    public async Task ListAccountsEachItemHasExactlyIdNameAndTimezone()
    {
        var response = await GetAsync(AccountsPath);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, response.Body);
        foreach (var account in response.Json.EnumerateArray())
        {
            account.EnumerateObject().Select(property => property.Name).ShouldBe(["id", "name", "timezone"], ignoreOrder: true);
        }
    }

    [Theory]
    [InlineData(14, "Beacon Home Security", "America/New_York")]
    [InlineData(20, "Quiet Harbor Spa", "America/Los_Angeles")]
    [InlineData(6, "Metro Collision Centers", "America/New_York")]
    [InlineData(18, "Capital City Storage", "UTC")]
    public async Task ListAccountsSeededDatabaseIncludesAccount(int accountId, string expectedName, string expectedTimezone)
    {
        var response = await GetAsync(AccountsPath);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, response.Body);
        var account = response.Json.EnumerateArray().Single(item => item.GetProperty("id").GetInt32() == accountId);
        account.GetProperty("name").GetString().ShouldBe(expectedName);
        account.GetProperty("timezone").GetString().ShouldBe(expectedTimezone);
    }

    [Fact]
    public async Task ListAccountsSeededDatabaseIsOrderedByNameOrdinal()
    {
        int[] accountIdsInOrdinalNameOrder = [14, 3, 18, 4, 7, 17, 10, 2, 9, 8, 6, 11, 16, 5, 20, 12, 19, 15, 1, 13];

        var response = await GetAsync(AccountsPath);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, response.Body);
        response.Json.EnumerateArray().Select(account => account.GetProperty("id").GetInt32()).ShouldBe(accountIdsInOrdinalNameOrder);
    }

    [Fact]
    public async Task ListAccountsIdIsAJsonNumber()
    {
        var response = await GetAsync(AccountsPath);

        response.StatusCode.ShouldBe(HttpStatusCode.OK, response.Body);
        response.Json.EnumerateArray().ShouldAllBe(account => account.GetProperty("id").ValueKind == JsonValueKind.Number);
    }
}
