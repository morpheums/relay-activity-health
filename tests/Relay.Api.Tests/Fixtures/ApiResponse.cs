using System.Net;
using System.Text.Json;

namespace Relay.Api.Tests.Fixtures;

public sealed record ApiResponse(HttpStatusCode StatusCode, string? MediaType, string Body)
{
    public const string ProblemJsonMediaType = "application/problem+json";
    public const string JsonMediaType = "application/json";

    public JsonElement Json => ParseJson(Body);

    public static async Task<ApiResponse> GetAsync(HttpClient client, string path, CancellationToken cancellationToken)
    {
        using var response = await client.GetAsync(new Uri(path, UriKind.Relative), cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        return new ApiResponse(response.StatusCode, response.Content.Headers.ContentType?.MediaType, body);
    }

    public HealthReportJson ShouldBeHealthReport()
    {
        StatusCode.ShouldBe(HttpStatusCode.OK, Body);
        MediaType.ShouldBe(JsonMediaType);
        return new HealthReportJson(Json);
    }

    public JsonElement ShouldBeProblem(HttpStatusCode expectedStatusCode)
    {
        StatusCode.ShouldBe(expectedStatusCode, Body);
        MediaType.ShouldBe(ProblemJsonMediaType);
        var problem = Json;
        problem.ValueKind.ShouldBe(JsonValueKind.Object);
        problem.GetProperty("status").GetInt32().ShouldBe((int)expectedStatusCode);
        problem.GetProperty("title").GetString().ShouldNotBeNullOrWhiteSpace();
        return problem;
    }

    public JsonElement ShouldBeValidationProblemFor(string parameterName)
    {
        var problem = ShouldBeProblem(HttpStatusCode.BadRequest);
        var errors = problem.GetProperty("errors");
        errors.EnumerateObject().Select(error => error.Name).ShouldBe([parameterName]);
        errors.GetProperty(parameterName).GetArrayLength().ShouldBeGreaterThan(0);
        return problem;
    }

    private static JsonElement ParseJson(string body)
    {
        using var document = JsonDocument.Parse(body);
        return document.RootElement.Clone();
    }
}
