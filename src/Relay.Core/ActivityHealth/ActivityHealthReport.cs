using Relay.Core.Calendar;
using Relay.Core.Normality;
using Relay.Core.Queries;

namespace Relay.Core.ActivityHealth;

public sealed record ActivityHealthReport(
    AccountListItem Account,
    ActivityType EventType,
    WeekRange Week,
    DateTime? DataAsOf,
    DateOnly LatestCompleteWeek,
    DateOnly EarliestWeek,
    int BaselineWeeks,
    int MinimumEligibleWeeks,
    SeriesHealth Summary,
    IReadOnlyList<LocationHealth> Locations);
