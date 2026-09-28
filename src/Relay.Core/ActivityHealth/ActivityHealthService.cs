using Relay.Core.Calendar;
using Relay.Core.Normality;
using Relay.Core.Queries;

namespace Relay.Core.ActivityHealth;

public sealed class ActivityHealthService(
    IAccountQueries accountQueries,
    IActivityQueries activityQueries,
    IWeekCalendar weekCalendar,
    IWeeklyGridBuilder weeklyGridBuilder,
    IBaselineEvaluator baselineEvaluator,
    ILocationRanker locationRanker,
    NormalityOptions normalityOptions) : IActivityHealthService
{
    private readonly IAccountQueries _accountQueries = accountQueries;
    private readonly IActivityQueries _activityQueries = activityQueries;
    private readonly IWeekCalendar _weekCalendar = weekCalendar;
    private readonly IWeeklyGridBuilder _weeklyGridBuilder = weeklyGridBuilder;
    private readonly IBaselineEvaluator _baselineEvaluator = baselineEvaluator;
    private readonly ILocationRanker _locationRanker = locationRanker;
    private readonly NormalityOptions _normalityOptions = normalityOptions;

    public Task<ActivityHealthResult> GetAsync(ActivityHealthQuery query, CancellationToken cancellationToken) =>
        throw new NotImplementedException();
}
