using Relay.Core.ActivityHealth;
using Relay.Core.Queries;

namespace Relay.Api.Tests.Fixtures;

public sealed class ThrowingActivityHealthService : IActivityHealthService
{
    public const string SensitiveMessage = "Sensitive internal detail 7f3a: Server=db;Password=hunter2";

    public Task<ActivityHealthResult> GetAsync(int accountId, DateOnly? week, ActivityType eventType, CancellationToken cancellationToken) =>
        throw new InvalidOperationException(SensitiveMessage);
}
