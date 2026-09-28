using System.Net;
using Relay.Api.Tests.Fixtures;

namespace Relay.Api.Tests.ActivityHealth;

[Collection(SeededApiTestGroup.Name)]
public sealed class InvalidInputTests(SeededApiFixture fixture) : SeededApiTest(fixture)
{
    private const string WeekErrorKey = "Week";
    private const string TypeErrorKey = "Type";

    [Theory]
    [InlineData("/api/accounts/999/activity-health")]
    [InlineData("/api/accounts/0/activity-health")]
    [InlineData("/api/accounts/-1/activity-health")]
    public async Task GetActivityHealthUnknownAccountReturnsNotFoundProblem(string path)
    {
        var response = await GetAsync(path);

        response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("/api/accounts/abc/activity-health")]
    [InlineData("/api/accounts/1.5/activity-health")]
    public async Task GetActivityHealthNonNumericAccountReturnsNotFoundProblem(string path)
    {
        var response = await GetAsync(path);

        response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("2026-07-21")]
    [InlineData("2026-07-26")]
    [InlineData("2026-03-04")]
    public async Task GetActivityHealthWeekNotAMondayReturnsBadRequestProblem(string week)
    {
        var response = await GetAsync($"/api/accounts/14/activity-health?week={week}");

        response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("2026-07-27")]
    [InlineData("2026-08-03")]
    [InlineData("2026-09-28")]
    public async Task GetActivityHealthMondayAfterLatestCompleteWeekReturnsBadRequestProblem(string week)
    {
        var response = await GetAsync($"/api/accounts/14/activity-health?week={week}");

        response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData(14, "2026-01-19")]
    [InlineData(14, "2025-12-29")]
    [InlineData(8, "2026-01-26")]
    [InlineData(20, "2026-03-02")]
    [InlineData(20, "2026-07-13")]
    public async Task GetActivityHealthMondayBeforeEarliestWeekReturnsBadRequestProblem(int accountId, string week)
    {
        var response = await GetAsync($"/api/accounts/{accountId}/activity-health?week={week}");

        response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetActivityHealthWeekBeforeAccount8EarliestIsValidForAccount14()
    {
        var response = await GetAsync("/api/accounts/14/activity-health?week=2026-01-26");

        response.StatusCode.ShouldBe(HttpStatusCode.OK, response.Body);
    }

    [Theory]
    [InlineData("week=")]
    [InlineData("week=abc")]
    [InlineData("week=2026-13-01")]
    [InlineData("week=20260720")]
    [InlineData("week=07/20/2026")]
    [InlineData("week=2026-7-20")]
    [InlineData("week=2026-02-30")]
    [InlineData("week=2026-07-20T00:00:00")]
    public async Task GetActivityHealthMalformedWeekReturnsValidationProblemForWeek(string query)
    {
        var response = await GetAsync($"/api/accounts/14/activity-health?{query}");

        response.ShouldBeValidationProblemFor(WeekErrorKey);
    }

    [Theory]
    [InlineData("type=")]
    [InlineData("type=ALL")]
    [InlineData("type=All")]
    [InlineData("type=Call_Received")]
    [InlineData("type=calls")]
    [InlineData("type=1")]
    [InlineData("type=foo")]
    [InlineData("type=CallReceived")]
    public async Task GetActivityHealthUnknownTypeReturnsValidationProblemForType(string query)
    {
        var response = await GetAsync($"/api/accounts/14/activity-health?{query}");

        response.ShouldBeValidationProblemFor(TypeErrorKey);
    }

    [Theory]
    [InlineData("week=abc")]
    [InlineData("week=")]
    [InlineData("type=ALL")]
    public async Task GetActivityHealthMalformedInputForUnknownAccountIsValidatedBeforeLookup(string query)
    {
        var response = await GetAsync($"/api/accounts/999/activity-health?{query}");

        response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("2026-07-21")]
    [InlineData("2026-07-27")]
    [InlineData("2026-01-19")]
    public async Task GetActivityHealthWellFormedWeekForUnknownAccountReturnsNotFound(string week)
    {
        var response = await GetAsync($"/api/accounts/999/activity-health?week={week}");

        response.ShouldBeProblem(HttpStatusCode.NotFound);
    }
}
