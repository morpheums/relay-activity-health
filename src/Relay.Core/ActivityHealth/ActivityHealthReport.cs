using Relay.Core.Normality;
using Relay.Core.Queries;

namespace Relay.Core.ActivityHealth;

public sealed record ActivityHealthReport(
    AccountListItem Account,
    ActivityType EventType,
    DateOnly WeekStart,
    DateOnly WeekEnd,
    DateTime? DataAsOfUtc,
    DateOnly LatestCompleteWeek,
    DateOnly EarliestWeek,
    int BaselineWeeks,
    int MinimumEligibleWeeks,
    AccountTotal Summary,
    IReadOnlyList<LocationHealth> Locations);
