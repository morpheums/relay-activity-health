using System.Text.Json;
using Relay.Api.Tests.Fixtures;

namespace Relay.Api.Tests.ActivityHealth;

[Collection(SeededApiTestGroup.Name)]
public sealed class EmptyAccountTests(SeededApiFixture fixture) : SeededApiTest(fixture)
{
    [Fact]
    public async Task GetActivityHealthEmptyAccountReturnsEmptyStateAtLatestCompleteWeekWithGlobalDataAsOf()
    {
        var report = await GetReportAsync("/api/accounts/20/activity-health");

        report.Summary.ShouldBeInsufficient(count: 0, weeksUsed: 0);
        report.Root.GetProperty("locations").ValueKind.ShouldBe(JsonValueKind.Array);
        report.Locations.ShouldBeEmpty();
        report.LatestCompleteWeek.ShouldBe("2026-07-20");
        report.EarliestWeek.ShouldBe("2026-07-20");
        report.WeekStart.ShouldBe("2026-07-20");
        report.DataAsOf.GetString().ShouldBe("2026-07-27T22:20:34Z");
    }
}
