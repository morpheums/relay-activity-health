using Relay.Core.ActivityHealth;
using Relay.Core.Calendar;
using Relay.Core.Normality;
using Relay.Core.Queries;

namespace Relay.Api.Tests.Fixtures;

public static class ReportBuilder
{
    public static readonly AccountListItem BeaconHomeSecurity = new(14, "Beacon Home Security", "America/New_York");

    public static readonly DateTime DataAnchorUtc = new(2026, 7, 27, 22, 20, 34, DateTimeKind.Utc);

    public static Baseline FullBaseline { get; } = new(WeeksUsed: 8, Median: 27, Low: 18, High: 38);

    public static SeriesHealth SummaryWithDeviation(double? deviation) =>
        new(Count: 26, FullBaseline, HealthStatus.Normal, deviation);

    public static LocationHealth LocationWithDeviation(string location, double? deviation) =>
        new(location, Count: 9, FullBaseline, HealthStatus.Normal, deviation);

    public static ActivityHealthReport Report(SeriesHealth summary, params LocationHealth[] locations) =>
        new(
            BeaconHomeSecurity,
            ActivityType.All,
            new WeekRange(new DateOnly(2026, 7, 20), new DateOnly(2026, 7, 26)),
            DataAnchorUtc,
            LatestCompleteWeek: new DateOnly(2026, 7, 20),
            EarliestWeek: new DateOnly(2026, 1, 26),
            BaselineWeeks: 8,
            MinimumEligibleWeeks: 4,
            summary,
            locations);
}
