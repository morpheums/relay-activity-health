using Relay.Core.Accounts;

namespace Relay.Core.ActivityHealth;

public sealed record ActivityHealthReport(
    AccountListItem Account,
    ActivityType EventType,
    DateOnly WeekStart,
    DateOnly WeekEnd,
    DateTime DataAsOfUtc,
    DateOnly LatestCompleteWeek,
    DateOnly EarliestWeek,
    int BaselineWeeks,
    int MinimumEligibleWeeks,
    AccountSummary Summary,
    IReadOnlyList<LocationHealth> Locations);
