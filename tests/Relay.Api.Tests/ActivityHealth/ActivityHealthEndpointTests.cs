using System.Net;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Relay.Api.Tests.Fixtures;
using Relay.Core.ActivityHealth;
using Relay.Core.Queries;
using static Relay.Api.Tests.Fixtures.ReportBuilder;

namespace Relay.Api.Tests.ActivityHealth;

[Collection(SeededApiTestGroup.Name)]
public sealed class ActivityHealthEndpointTests(SeededApiFixture fixture) : SeededApiTest(fixture)
{
    private static readonly ActivityHealthReport MidpointReport = Report(
        SummaryWithDeviation(0.125),
        LocationWithDeviation("Site B", -0.125),
        LocationWithDeviation("Site A", 0.125));

    [Fact]
    public async Task GetActivityHealthFoundReportRoundsSummaryDeviationAwayFromZero()
    {
        var (response, _) = await GetWithCannedResultAsync(new ActivityHealthResult.Found(MidpointReport), "/api/accounts/14/activity-health");

        var report = response.ShouldBeHealthReport();
        report.Summary.ShouldHaveDeviation(0.13);
    }

    [Fact]
    public async Task GetActivityHealthFoundReportRoundsLocationDeviationsAwayFromZero()
    {
        var (response, _) = await GetWithCannedResultAsync(new ActivityHealthResult.Found(MidpointReport), "/api/accounts/14/activity-health");

        var report = response.ShouldBeHealthReport();
        report.Location("Site B").ShouldHaveDeviation(-0.13);
        report.Location("Site A").ShouldHaveDeviation(0.13);
    }

    [Fact]
    public async Task GetActivityHealthFoundReportKeepsTheServiceLocationOrder()
    {
        var (response, _) = await GetWithCannedResultAsync(new ActivityHealthResult.Found(MidpointReport), "/api/accounts/14/activity-health");

        response.ShouldBeHealthReport().LocationNames.ShouldBe(["Site B", "Site A"]);
    }

    [Fact]
    public async Task GetActivityHealthFoundReportSerialisesCoreRecordsInContractShape()
    {
        var (response, _) = await GetWithCannedResultAsync(new ActivityHealthResult.Found(MidpointReport), "/api/accounts/14/activity-health");

        var root = response.ShouldBeHealthReport().Root;
        root.GetProperty("dataAsOf").GetString().ShouldBe("2026-07-27T22:20:34Z");
        root.GetProperty("eventType").GetString().ShouldBe("all");
        root.GetProperty("week").GetProperty("start").GetString().ShouldBe("2026-07-20");
        root.GetProperty("summary").GetProperty("status").GetString().ShouldBe(WireStatus.Normal);
        root.GetProperty("summary").GetProperty("baseline").GetProperty("weeksUsed").GetInt32().ShouldBe(8);
    }

    [Fact]
    public async Task GetActivityHealthFoundReportWithNullDataAsOfSerialisesNull()
    {
        var reportWithoutData = MidpointReport with { DataAsOf = null };

        var (response, _) = await GetWithCannedResultAsync(new ActivityHealthResult.Found(reportWithoutData), "/api/accounts/14/activity-health");

        response.ShouldBeHealthReport().DataAsOf.ValueKind.ShouldBe(JsonValueKind.Null);
    }

    [Fact]
    public async Task GetActivityHealthAccountNotFoundReturnsNotFoundProblem()
    {
        var (response, _) = await GetWithCannedResultAsync(new ActivityHealthResult.AccountNotFound(), "/api/accounts/14/activity-health");

        response.ShouldBeProblem(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData(InvalidWeekReason.NotAWeekStart)]
    [InlineData(InvalidWeekReason.AfterLatestCompleteWeek)]
    [InlineData(InvalidWeekReason.BeforeEarliestWeek)]
    public async Task GetActivityHealthInvalidWeekReturnsBadRequestProblem(InvalidWeekReason reason)
    {
        var (response, _) = await GetWithCannedResultAsync(new ActivityHealthResult.InvalidWeek(reason), "/api/accounts/14/activity-health?week=2026-07-20");

        response.ShouldBeProblem(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetActivityHealthWithoutQueryAsksServiceForDefaultWeekAndAllTypes()
    {
        var (_, service) = await GetWithCannedResultAsync(new ActivityHealthResult.Found(MidpointReport), "/api/accounts/14/activity-health");

        service.Requests.ShouldBe([(14, (DateOnly?)null, ActivityType.All)]);
    }

    [Theory]
    [InlineData("call_received", ActivityType.CallReceived)]
    [InlineData("lead_created", ActivityType.LeadCreated)]
    [InlineData("appointment_set", ActivityType.AppointmentSet)]
    [InlineData("all", ActivityType.All)]
    public async Task GetActivityHealthWithQueryPassesParsedWeekAndTypeToService(string type, ActivityType expectedType)
    {
        var (_, service) = await GetWithCannedResultAsync(
            new ActivityHealthResult.Found(MidpointReport),
            $"/api/accounts/6/activity-health?week=2026-06-01&type={type}");

        service.Requests.ShouldBe([(6, (DateOnly?)new DateOnly(2026, 6, 1), expectedType)]);
    }

    [Fact]
    public async Task GetActivityHealthMalformedWeekNeverReachesTheService()
    {
        var (response, service) = await GetWithCannedResultAsync(
            new ActivityHealthResult.Found(MidpointReport),
            "/api/accounts/14/activity-health?week=abc");

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        service.Requests.ShouldBeEmpty();
    }

    private async Task<(ApiResponse Response, CannedActivityHealthService Service)> GetWithCannedResultAsync(ActivityHealthResult cannedResult, string path)
    {
        var cannedService = new CannedActivityHealthService(cannedResult);
        await using var factory = Fixture.CreateFactory(services => services.AddSingleton<IActivityHealthService>(cannedService));
        using var client = factory.CreateClient();
        var response = await ApiResponse.GetAsync(client, path, CancellationToken);
        return (response, cannedService);
    }
}
