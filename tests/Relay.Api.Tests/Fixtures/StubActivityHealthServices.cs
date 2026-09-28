using Relay.Core.ActivityHealth;
using Relay.Core.Queries;

namespace Relay.Api.Tests.Fixtures;

public sealed class ThrowingActivityHealthService : IActivityHealthService
{
    public const string SensitiveMessage = "Sensitive internal detail 7f3a: Server=db;Password=hunter2";

    public Task<ActivityHealthResult> GetAsync(int accountId, DateOnly? week, ActivityType eventType, CancellationToken cancellationToken) =>
        throw new InvalidOperationException(SensitiveMessage);
}

public sealed class CannedActivityHealthService(ActivityHealthResult cannedResult) : IActivityHealthService
{
    public List<(int AccountId, DateOnly? Week, ActivityType EventType)> Requests { get; } = [];

    public Task<ActivityHealthResult> GetAsync(int accountId, DateOnly? week, ActivityType eventType, CancellationToken cancellationToken)
    {
        Requests.Add((accountId, week, eventType));
        return Task.FromResult(cannedResult);
    }
}
