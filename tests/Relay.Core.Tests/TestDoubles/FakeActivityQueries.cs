using Relay.Core.Calendar;
using Relay.Core.Queries;

namespace Relay.Core.Tests.TestDoubles;

internal sealed record WeeklyCountRequest(int AccountId, IReadOnlyList<WeekWindow> WeekWindows, ActivityType EventType);

internal sealed class FakeActivityQueries : IActivityQueries
{
    private readonly List<(int AccountId, SiteFirstActivity SiteFirstActivity)> _siteFirstActivities = [];
    private readonly List<(int AccountId, ActivityType EventType, WeeklySiteCount WeeklySiteCount)> _weeklySiteCounts = [];
    private DateTime? _dataAnchorUtc;
    private bool _rowsReversed;

    public List<WeeklyCountRequest> WeeklyCountRequests { get; } = [];

    public IReadOnlyList<WeekWindow> RequestedWindows =>
        WeeklyCountRequests.SelectMany(request => request.WeekWindows).ToList();

    public FakeActivityQueries WithDataAnchor(string dataAnchorUtc)
    {
        _dataAnchorUtc = TestTime.Utc(dataAnchorUtc);
        return this;
    }

    public FakeActivityQueries WithRowsReversed()
    {
        _rowsReversed = true;
        return this;
    }

    public FakeActivityQueries WithSite(int accountId, string location, string firstActivityUtc)
    {
        _siteFirstActivities.Add((accountId, new SiteFirstActivity(location, TestTime.Utc(firstActivityUtc))));
        return this;
    }

    public FakeActivityQueries WithWeeklyCounts(
        int accountId,
        string location,
        string firstWeekStart,
        ActivityType eventType,
        params int[] consecutiveWeekCounts)
    {
        var firstWeekDay = TestTime.Day(firstWeekStart);
        for (var weekIndex = 0; weekIndex < consecutiveWeekCounts.Length; weekIndex++)
        {
            var weeklySiteCount = new WeeklySiteCount(location, firstWeekDay.AddDays(7 * weekIndex), consecutiveWeekCounts[weekIndex]);
            _weeklySiteCounts.Add((accountId, eventType, weeklySiteCount));
        }

        return this;
    }

    public Task<DateTime?> FindDataAnchorAsync(CancellationToken cancellationToken) =>
        Task.FromResult(_dataAnchorUtc);

    public Task<IReadOnlyList<SiteFirstActivity>> ListSiteFirstActivitiesAsync(int accountId, CancellationToken cancellationToken) =>
        Task.FromResult<IReadOnlyList<SiteFirstActivity>>(
            InReturnOrder(_siteFirstActivities.Where(entry => entry.AccountId == accountId).Select(entry => entry.SiteFirstActivity)));

    public Task<IReadOnlyList<WeeklySiteCount>> CountWeeklyBySiteAsync(
        int accountId,
        IReadOnlyList<WeekWindow> weekWindows,
        ActivityType eventType,
        CancellationToken cancellationToken)
    {
        var requestedWindows = weekWindows.ToList();
        WeeklyCountRequests.Add(new WeeklyCountRequest(accountId, requestedWindows, eventType));
        var requestedWeekStarts = requestedWindows.Select(window => window.WeekStart).ToHashSet();
        var matchingCounts = _weeklySiteCounts
            .Where(entry => entry.AccountId == accountId
                && entry.EventType == eventType
                && requestedWeekStarts.Contains(entry.WeeklySiteCount.WeekStart))
            .Select(entry => entry.WeeklySiteCount);
        return Task.FromResult<IReadOnlyList<WeeklySiteCount>>(InReturnOrder(matchingCounts));
    }

    private List<TRow> InReturnOrder<TRow>(IEnumerable<TRow> rowsInInsertionOrder) =>
        _rowsReversed ? rowsInInsertionOrder.Reverse().ToList() : rowsInInsertionOrder.ToList();
}
