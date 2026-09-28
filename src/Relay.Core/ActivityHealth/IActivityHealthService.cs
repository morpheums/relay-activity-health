namespace Relay.Core.ActivityHealth;

public interface IActivityHealthService
{
    Task<ActivityHealthResult> GetAsync(ActivityHealthQuery query, CancellationToken cancellationToken);
}
