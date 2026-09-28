using Relay.Core.Queries;

namespace Relay.Core.ActivityHealth;

public interface IActivityHealthService
{
    Task<ActivityHealthResult> GetAsync(
        int accountId,
        DateOnly? week,
        ActivityType eventType,
        CancellationToken cancellationToken);
}
