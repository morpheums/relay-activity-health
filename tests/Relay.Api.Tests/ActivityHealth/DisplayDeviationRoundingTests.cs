using Relay.Api.Http;
using Relay.Api.Tests.Fixtures;
using static Relay.Api.Tests.Fixtures.ReportBuilder;

namespace Relay.Api.Tests.ActivityHealth;

public sealed class DisplayDeviationRoundingTests
{
    [Theory]
    [InlineData(0.125, 0.13)]
    [InlineData(-0.125, -0.13)]
    [InlineData(-2.625, -2.63)]
    [InlineData(12.7449, 12.74)]
    public void WithDisplayDeviationsRoundsSummaryAndLocationsToTwoDecimalsAwayFromZero(double fullPrecisionDeviation, double expectedDisplayDeviation)
    {
        var report = Report(SummaryWithDeviation(fullPrecisionDeviation), LocationWithDeviation("Site A", fullPrecisionDeviation));

        var displayReport = report.WithDisplayDeviations();

        displayReport.Summary.Deviation.ShouldNotBeNull().ShouldBe(expectedDisplayDeviation, SeriesJson.Tolerance);
        displayReport.Locations[0].Deviation.ShouldNotBeNull().ShouldBe(expectedDisplayDeviation, SeriesJson.Tolerance);
    }
}
