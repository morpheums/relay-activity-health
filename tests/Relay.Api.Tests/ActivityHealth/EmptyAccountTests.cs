using System.Net;
using System.Text.Json;
using Relay.Api.Tests.Fixtures;

namespace Relay.Api.Tests.ActivityHealth;

[Collection(SeededApiTestGroup.Name)]
public sealed class EmptyAccountTests(SeededApiFixture fixture) : SeededApiTest(fixture)
{
    private const string EmptyAccountPath = "/api/accounts/20/activity-health";

    [Fact]
    public async Task GetActivityHealthEmptyAccountSummaryIsInsufficientWithZeroCount()
    {
        var report = await GetReportAsync(EmptyAccountPath);

        report.Summary.ShouldBeInsufficient(count: 0, weeksUsed: 0);
    }

    [Fact]
    public async Task GetActivityHealthEmptyAccountHasNoLocations()
    {
        var report = await GetReportAsync(EmptyAccountPath);

        report.Root.GetProperty("locations").ValueKind.ShouldBe(JsonValueKind.Array);
        report.Locations.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetActivityHealthEmptyAccountEarliestWeekEqualsLatestCompleteWeek()
    {
        var report = await GetReportAsync(EmptyAccountPath);

        report.LatestCompleteWeek.ShouldBe("2026-07-20");
        report.EarliestWeek.ShouldBe("2026-07-20");
        report.WeekStart.ShouldBe("2026-07-20");
    }

    [Fact]
    public async Task GetActivityHealthEmptyAccountDataAsOfIsTheGlobalAnchor()
    {
        var report = await GetReportAsync(EmptyAccountPath);

        report.DataAsOf.ValueKind.ShouldBe(JsonValueKind.String);
        report.DataAsOf.GetString().ShouldBe("2026-07-27T22:20:34Z");
    }

    [Fact]
    public async Task GetActivityHealthEmptyAccountWeekBeforeEarliestWeekIsBadRequest()
    {
        var response = await GetAsync($"{EmptyAccountPath}?week=2026-03-02");

        response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }
}
