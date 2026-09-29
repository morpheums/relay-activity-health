using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Relay.Api.Tests.Fixtures;
using Relay.Core.ActivityHealth;

namespace Relay.Api.Tests.ActivityHealth;

[Collection(SeededApiTestGroup.Name)]
public sealed class UnhandledExceptionTests(SeededApiFixture fixture) : SeededApiTest(fixture)
{
    [Fact]
    public async Task GetActivityHealthServiceThrowsReturnsInternalServerErrorProblemWithoutExceptionDetails()
    {
        await using var factory = Fixture.CreateFactory(
            Environments.Development,
            services => services.AddScoped<IActivityHealthService, ThrowingActivityHealthService>());
        using var client = factory.CreateClient();

        var response = await ApiResponse.GetAsync(client, "/api/accounts/14/activity-health?week=2026-07-20", CancellationToken);

        response.ShouldBeProblem(HttpStatusCode.InternalServerError);
        response.Body.ShouldNotContain(ThrowingActivityHealthService.SensitiveMessage);
        response.Body.ShouldNotContain("hunter2");
        response.Body.ShouldNotContain(nameof(InvalidOperationException));
        response.Body.ShouldNotContain(nameof(ThrowingActivityHealthService));
        response.Body.ShouldNotContain("stack", Case.Insensitive);
    }
}
