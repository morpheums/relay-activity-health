namespace Relay.Api.Http;

public sealed record ActivityHealthResponse(
    AccountResponse Account,
    string EventType,
    WeekRangeResponse Week,
    DateTime DataAsOf,
    DateOnly LatestCompleteWeek,
    DateOnly EarliestWeek,
    int BaselineWeeks,
    int MinimumEligibleWeeks,
    SummaryResponse Summary,
    IReadOnlyList<LocationHealthResponse> Locations);
