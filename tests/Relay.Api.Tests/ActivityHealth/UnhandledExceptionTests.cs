using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Relay.Api.Tests.Fixtures;
using Relay.Core.ActivityHealth;

namespace Relay.Api.Tests.ActivityHealth;

[Collection(SeededApiTestGroup.Name)]
public sealed class UnhandledExceptionTests(SeededApiFixture fixture) : SeededApiTest(fixture)
{
    [Fact]
    public async Task GetActivityHealthServiceThrowsReturnsInternalServerErrorProblem()
    {
        var response = await GetWithThrowingServiceAsync();

        response.ShouldBeProblem(HttpStatusCode.InternalServerError);
    }

    [Fact]
    public async Task GetActivityHealthServiceThrowsBodyHasNoExceptionMessageOrStackTrace()
    {
        var response = await GetWithThrowingServiceAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        response.Body.ShouldNotContain(ThrowingActivityHealthService.SensitiveMessage);
        response.Body.ShouldNotContain("hunter2");
        response.Body.ShouldNotContain(nameof(InvalidOperationException));
        response.Body.ShouldNotContain(nameof(ThrowingActivityHealthService));
        response.Body.ShouldNotContain("stack", Case.Insensitive);
    }

    private async Task<ApiResponse> GetWithThrowingServiceAsync()
    {
        await using var factory = Fixture.CreateFactory(services =>
            services.AddScoped<IActivityHealthService, ThrowingActivityHealthService>());
        using var client = factory.CreateClient();
        return await ApiResponse.GetAsync(client, "/api/accounts/14/activity-health?week=2026-07-20", CancellationToken);
    }
}
