using System.Net;
using System.Text.Json;
using Relay.Api.Tests.Fixtures;

namespace Relay.Api.Tests.ActivityHealth;

[Collection(SeededApiTestGroup.Name)]
public sealed class ResponseShapeTests(SeededApiFixture fixture) : SeededApiTest(fixture)
{
    private const string DefaultAccountPath = "/api/accounts/14/activity-health";
    private const string Account6ExamplePath = "/api/accounts/6/activity-health?week=2026-07-20";

    private static readonly string[] TopLevelPropertyNames =
    [
        "account", "eventType", "week", "dataAsOf", "latestCompleteWeek", "earliestWeek",
        "baselineWeeks", "minimumEligibleWeeks", "summary", "locations",
    ];

    private static readonly string[] SeriesPropertyNames = ["count", "baseline", "status", "deviation"];
    private static readonly string[] LocationPropertyOrder = ["location", "count", "baseline", "status", "deviation"];
    private static readonly string[] BaselinePropertyNames = ["weeksUsed", "median", "low", "high"];

    [Fact]
    public async Task GetActivityHealthTopLevelHasExactlyTheContractProperties()
    {
        var report = await GetReportAsync(DefaultAccountPath);

        PropertyNames(report.Root).ShouldBe(TopLevelPropertyNames, ignoreOrder: true);
    }

    [Fact]
    public async Task GetActivityHealthAccountHasIdNameAndTimezone()
    {
        var report = await GetReportAsync(DefaultAccountPath);

        var account = report.Root.GetProperty("account");
        PropertyNames(account).ShouldBe(["id", "name", "timezone"], ignoreOrder: true);
        account.GetProperty("id").GetInt32().ShouldBe(14);
        account.GetProperty("name").GetString().ShouldBe("Beacon Home Security");
        account.GetProperty("timezone").GetString().ShouldBe("America/New_York");
    }

    [Fact]
    public async Task GetActivityHealthWeekHasStartAndEnd()
    {
        var report = await GetReportAsync(DefaultAccountPath);

        PropertyNames(report.Root.GetProperty("week")).ShouldBe(["start", "end"], ignoreOrder: true);
    }

    [Fact]
    public async Task GetActivityHealthSummaryHasSeriesPropertiesWithoutLocation()
    {
        var report = await GetReportAsync(DefaultAccountPath);

        PropertyNames(report.Root.GetProperty("summary")).ShouldBe(SeriesPropertyNames, ignoreOrder: true);
    }

    [Fact]
    public async Task GetActivityHealthBaselineHasWeeksUsedMedianLowHigh()
    {
        var report = await GetReportAsync(DefaultAccountPath);

        PropertyNames(report.Root.GetProperty("summary").GetProperty("baseline")).ShouldBe(BaselinePropertyNames, ignoreOrder: true);
        foreach (var location in report.Root.GetProperty("locations").EnumerateArray())
        {
            PropertyNames(location.GetProperty("baseline")).ShouldBe(BaselinePropertyNames, ignoreOrder: true);
        }
    }

    [Fact]
    public async Task GetActivityHealthLocationPropertiesAreInContractOrder()
    {
        var report = await GetReportAsync(Account6ExamplePath);

        var locations = report.Root.GetProperty("locations").EnumerateArray().ToList();
        locations.Count.ShouldBe(15);
        foreach (var location in locations)
        {
            PropertyNames(location).ShouldBe(LocationPropertyOrder);
        }
    }

    [Fact]
    public async Task GetActivityHealthNumbersAreJsonNumbersComparedByValue()
    {
        var report = await GetReportAsync(Account6ExamplePath);

        var siteM = report.Root.GetProperty("locations")[0];
        siteM.GetProperty("deviation").ValueKind.ShouldBe(JsonValueKind.Number);
        siteM.GetProperty("deviation").GetDouble().ShouldBe(1.30, SeriesJson.Tolerance);
        var summaryMedian = report.Root.GetProperty("summary").GetProperty("baseline").GetProperty("median");
        summaryMedian.ValueKind.ShouldBe(JsonValueKind.Number);
        summaryMedian.GetDouble().ShouldBe(72.5, SeriesJson.Tolerance);
        report.Root.GetProperty("baselineWeeks").ValueKind.ShouldBe(JsonValueKind.Number);
        report.Root.GetProperty("summary").GetProperty("count").ValueKind.ShouldBe(JsonValueKind.Number);
    }

    [Fact]
    public async Task GetActivityHealthInsufficientBaselineKeepsNullPropertiesInTheBody()
    {
        var report = await GetReportAsync("/api/accounts/8/activity-health?week=2026-03-02");

        var summary = report.Root.GetProperty("summary");
        var baseline = summary.GetProperty("baseline");
        baseline.GetProperty("median").ValueKind.ShouldBe(JsonValueKind.Null);
        baseline.GetProperty("low").ValueKind.ShouldBe(JsonValueKind.Null);
        baseline.GetProperty("high").ValueKind.ShouldBe(JsonValueKind.Null);
        summary.GetProperty("deviation").ValueKind.ShouldBe(JsonValueKind.Null);
    }

    [Fact]
    public async Task GetActivityHealthDataAsOfIsTheExactUtcAnchorString()
    {
        var report = await GetReportAsync(DefaultAccountPath);

        report.DataAsOf.ValueKind.ShouldBe(JsonValueKind.String);
        report.DataAsOf.GetString().ShouldBe("2026-07-27T22:20:34Z");
    }

    [Fact]
    public async Task GetActivityHealthWeekDatesAreIsoDateStrings()
    {
        var report = await GetReportAsync(DefaultAccountPath);

        report.WeekStart.ShouldBe("2026-07-20");
        report.WeekEnd.ShouldBe("2026-07-26");
        report.LatestCompleteWeek.ShouldBe("2026-07-20");
        report.EarliestWeek.ShouldBe("2026-01-26");
    }

    [Fact]
    public async Task GetActivityHealthWithoutTypeReturnsTheSameBodyAsTypeAll()
    {
        var withoutType = await GetAsync("/api/accounts/14/activity-health?week=2026-07-20");
        var withTypeAll = await GetAsync("/api/accounts/14/activity-health?week=2026-07-20&type=all");

        withoutType.StatusCode.ShouldBe(HttpStatusCode.OK, withoutType.Body);
        withoutType.Body.ShouldBe(withTypeAll.Body);
        withoutType.ShouldBeHealthReport().EventType.ShouldBe("all");
    }

    [Theory]
    [InlineData("call_received")]
    [InlineData("lead_created")]
    [InlineData("appointment_set")]
    [InlineData("all")]
    public async Task GetActivityHealthEventTypeEchoesTheSnakeCaseWireName(string type)
    {
        var report = await GetReportAsync($"/api/accounts/14/activity-health?type={type}");

        report.EventType.ShouldBe(type);
    }

    [Theory]
    [InlineData("/api/accounts/14/activity-health")]
    [InlineData("/api/accounts/14/activity-health?week=2026-03-02")]
    [InlineData("/api/accounts/14/activity-health?week=2026-02-02")]
    [InlineData("/api/accounts/6/activity-health?week=2026-06-01")]
    [InlineData("/api/accounts/6/activity-health?week=2026-07-20&type=call_received")]
    [InlineData("/api/accounts/8/activity-health?week=2026-03-02")]
    [InlineData("/api/accounts/20/activity-health")]
    public async Task GetActivityHealthEveryStatusIsALowerCaseWireName(string path)
    {
        var report = await GetReportAsync(path);

        WireStatus.All.ShouldContain(report.Summary.Status);
        report.Locations.ShouldAllBe(location => WireStatus.All.Contains(location.Status));
    }

    [Fact]
    public async Task GetActivityHealthConstantsAreEightBaselineWeeksAndFourMinimum()
    {
        var report = await GetReportAsync(DefaultAccountPath);

        report.Root.GetProperty("baselineWeeks").GetInt32().ShouldBe(8);
        report.Root.GetProperty("minimumEligibleWeeks").GetInt32().ShouldBe(4);
    }

    private static List<string> PropertyNames(JsonElement jsonObject) =>
        [.. jsonObject.EnumerateObject().Select(property => property.Name)];
}
