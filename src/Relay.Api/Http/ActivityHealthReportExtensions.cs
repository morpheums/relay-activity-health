using Relay.Core.ActivityHealth;

namespace Relay.Api.Http;

public static class ActivityHealthReportExtensions
{
    private const int DisplayDeviationDecimals = 2;

    public static ActivityHealthReport WithDisplayDeviations(this ActivityHealthReport report) =>
        report with
        {
            Summary = report.Summary with { Deviation = DisplayDeviation(report.Summary.Deviation) },
            Locations = [.. report.Locations.Select(location => location with { Deviation = DisplayDeviation(location.Deviation) })],
        };

    private static double? DisplayDeviation(double? deviation) =>
        deviation is { } fullPrecisionDeviation
            ? Math.Round(fullPrecisionDeviation, DisplayDeviationDecimals, MidpointRounding.AwayFromZero)
            : null;
}
