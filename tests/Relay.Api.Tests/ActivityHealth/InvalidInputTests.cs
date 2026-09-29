using System.Net;
using Relay.Api.Tests.Fixtures;

namespace Relay.Api.Tests.ActivityHealth;

[Collection(SeededApiTestGroup.Name)]
public sealed class InvalidInputTests(SeededApiFixture fixture) : SeededApiTest(fixture)
{
    [Theory]
    [InlineData("/api/accounts/999/activity-health")]
    [InlineData("/api/accounts/abc/activity-health")]
    public async Task GetActivityHealthUnknownOrNonNumericAccountReturnsNotFoundProblem(string path)
    {
        var response = await GetAsync(path);

        response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData(14, "2026-07-21")]
    [InlineData(14, "2026-07-27")]
    [InlineData(14, "2026-01-19")]
    [InlineData(20, "2026-03-02")]
    public async Task GetActivityHealthWeekNotAMondayAfterLatestOrBeforeEarliestReturnsBadRequestProblem(int accountId, string week)
    {
        var response = await GetAsync($"/api/accounts/{accountId}/activity-health?week={week}");

        response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("week=")]
    [InlineData("week=abc")]
    [InlineData("week=2026-13-01")]
    [InlineData("week=2026-7-20")]
    public async Task GetActivityHealthMalformedWeekReturnsValidationProblemForWeek(string query)
    {
        var response = await GetAsync($"/api/accounts/14/activity-health?{query}");

        response.ShouldBeValidationProblemFor("Week");
    }

    [Theory]
    [InlineData("type=")]
    [InlineData("type=ALL")]
    [InlineData("type=Call_Received")]
    [InlineData("type=CallReceived")]
    [InlineData("type=1")]
    public async Task GetActivityHealthUnknownOrWrongCaseTypeReturnsValidationProblemForType(string query)
    {
        var response = await GetAsync($"/api/accounts/14/activity-health?{query}");

        response.ShouldBeValidationProblemFor("Type");
    }

    [Theory]
    [InlineData("week=abc")]
    [InlineData("type=ALL")]
    public async Task GetActivityHealthMalformedInputForUnknownAccountIsValidatedBeforeLookup(string query)
    {
        var response = await GetAsync($"/api/accounts/999/activity-health?{query}");

        response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("2026-07-21")]
    [InlineData("2026-07-27")]
    public async Task GetActivityHealthWellFormedWeekForUnknownAccountReturnsNotFoundBeforeWeekRules(string week)
    {
        var response = await GetAsync($"/api/accounts/999/activity-health?week={week}");

        response.ShouldBeProblem(HttpStatusCode.NotFound);
    }
}
