using Relay.Core.Calendar;
using Relay.Core.Queries;
using Relay.Infrastructure.Persistence;

namespace Relay.Infrastructure.Queries;

public sealed class SqlActivityQueries(RelayDbContext dbContext) : IActivityQueries
{
    private readonly RelayDbContext _dbContext = dbContext;

    public Task<DateTime?> FindDataAnchorAsync(CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<IReadOnlyList<SiteFirstActivity>> ListSiteFirstActivitiesAsync(int accountId, CancellationToken cancellationToken) =>
        throw new NotImplementedException();

    public Task<IReadOnlyList<WeeklySiteCount>> CountWeeklyBySiteAsync(
        int accountId,
        IReadOnlyList<WeekWindow> weekWindows,
        ActivityType eventType,
        CancellationToken cancellationToken) =>
        throw new NotImplementedException();
}
