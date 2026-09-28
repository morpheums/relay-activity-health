namespace Relay.Api.Tests.Fixtures;

public abstract class SeededApiTest(SeededApiFixture fixture)
{
    protected SeededApiFixture Fixture { get; } = fixture;

    protected static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    protected Task<ApiResponse> GetAsync(string path) => ApiResponse.GetAsync(Fixture.Client, path, CancellationToken);

    protected async Task<HealthReportJson> GetReportAsync(string path) => (await GetAsync(path)).ShouldBeHealthReport();
}
