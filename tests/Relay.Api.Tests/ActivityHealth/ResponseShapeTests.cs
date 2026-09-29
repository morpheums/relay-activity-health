using System.Text.Json;
using Relay.Api.Tests.Fixtures;

namespace Relay.Api.Tests.ActivityHealth;

[Collection(SeededApiTestGroup.Name)]
public sealed class ResponseShapeTests(SeededApiFixture fixture) : SeededApiTest(fixture)
{
    private static readonly string[] TopLevelPropertyNames =
    [
        "account", "eventType", "week", "dataAsOf", "latestCompleteWeek", "earliestWeek",
        "baselineWeeks", "minimumEligibleWeeks", "summary", "locations",
    ];

    private static readonly string[] SummaryPropertyNames = ["count", "baseline", "status", "deviation"];
    private static readonly string[] LocationPropertyOrder = ["location", "count", "baseline", "status", "deviation"];
    private static readonly string[] BaselinePropertyNames = ["weeksUsed", "median", "low", "high"];

    [Fact]
    public async Task GetActivityHealthResponseHasExactlyTheContractPropertiesWithLocationFieldsInContractOrder()
    {
        var report = await GetReportAsync("/api/accounts/6/activity-health?week=2026-07-20");

        var root = report.Root;
        PropertyNames(root).ShouldBe(TopLevelPropertyNames, ignoreOrder: true);
        PropertyNames(root.GetProperty("account")).ShouldBe(["id", "name", "timezone"], ignoreOrder: true);
        PropertyNames(root.GetProperty("week")).ShouldBe(["start", "end"], ignoreOrder: true);
        PropertyNames(root.GetProperty("summary")).ShouldBe(SummaryPropertyNames, ignoreOrder: true);
        PropertyNames(root.GetProperty("summary").GetProperty("baseline")).ShouldBe(BaselinePropertyNames, ignoreOrder: true);
        var locations = root.GetProperty("locations").EnumerateArray().ToList();
        locations.Count.ShouldBe(15);
        foreach (var location in locations)
        {
            PropertyNames(location).ShouldBe(LocationPropertyOrder);
            PropertyNames(location.GetProperty("baseline")).ShouldBe(BaselinePropertyNames, ignoreOrder: true);
        }
    }

    private static List<string> PropertyNames(JsonElement jsonObject) =>
        [.. jsonObject.EnumerateObject().Select(property => property.Name)];
}
