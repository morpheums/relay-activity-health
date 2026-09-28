using Relay.Api.Http;
using Relay.Api.Tests.Fixtures;
using static Relay.Api.Tests.Fixtures.ReportBuilder;

namespace Relay.Api.Tests.ActivityHealth;

public sealed class DisplayDeviationRoundingTests
{
    [Theory]
    [InlineData(0.125, 0.13)]
    [InlineData(-0.125, -0.13)]
    [InlineData(0.375, 0.38)]
    [InlineData(-2.625, -2.63)]
    [InlineData(12.7449, 12.74)]
    [InlineData(-3.186, -3.19)]
    [InlineData(1.2999999999, 1.3)]
    [InlineData(0, 0)]
    public void WithDisplayDeviationsSummaryRoundsToTwoDecimalsAwayFromZero(double fullPrecisionDeviation, double expectedDisplayDeviation)
    {
        var report = Report(SummaryWithDeviation(fullPrecisionDeviation));

        var displayReport = report.WithDisplayDeviations();

        displayReport.Summary.Deviation.ShouldNotBeNull();
        displayReport.Summary.Deviation.Value.ShouldBe(expectedDisplayDeviation, SeriesJson.Tolerance);
    }

    [Theory]
    [InlineData(0.125, 0.13)]
    [InlineData(-0.125, -0.13)]
    [InlineData(0.875, 0.88)]
    [InlineData(-0.375, -0.38)]
    public void WithDisplayDeviationsLocationRoundsToTwoDecimalsAwayFromZero(double fullPrecisionDeviation, double expectedDisplayDeviation)
    {
        var report = Report(SummaryWithDeviation(0), LocationWithDeviation("Site A", fullPrecisionDeviation));

        var displayReport = report.WithDisplayDeviations();

        displayReport.Locations[0].Deviation.ShouldNotBeNull();
        displayReport.Locations[0].Deviation!.Value.ShouldBe(expectedDisplayDeviation, SeriesJson.Tolerance);
    }

    [Fact]
    public void WithDisplayDeviationsNullDeviationStaysNull()
    {
        var report = Report(SummaryWithDeviation(null), LocationWithDeviation("Site A", null));

        var displayReport = report.WithDisplayDeviations();

        displayReport.Summary.Deviation.ShouldBeNull();
        displayReport.Locations[0].Deviation.ShouldBeNull();
    }

    [Fact]
    public void WithDisplayDeviationsKeepsLocationOrderAndEveryOtherLocationField()
    {
        var report = Report(
            SummaryWithDeviation(0.125),
            LocationWithDeviation("Site C", -0.125),
            LocationWithDeviation("Site A", 0.125),
            LocationWithDeviation("Site B", null));

        var displayReport = report.WithDisplayDeviations();

        displayReport.Locations.ShouldBe(
            [
                LocationWithDeviation("Site C", -0.13),
                LocationWithDeviation("Site A", 0.13),
                LocationWithDeviation("Site B", null),
            ]);
    }

    [Fact]
    public void WithDisplayDeviationsKeepsEveryNonDeviationReportField()
    {
        var report = Report(SummaryWithDeviation(0.125), LocationWithDeviation("Site A", 0.125));

        var displayReport = report.WithDisplayDeviations();

        (displayReport with { Summary = report.Summary, Locations = report.Locations }).ShouldBe(report);
        displayReport.Summary.ShouldBe(SummaryWithDeviation(0.13));
    }

    [Fact]
    public void WithDisplayDeviationsLeavesTheFullPrecisionReportUnchanged()
    {
        var location = LocationWithDeviation("Site A", -0.125);
        var report = Report(SummaryWithDeviation(0.125), location);

        _ = report.WithDisplayDeviations();

        report.Summary.Deviation.ShouldBe(0.125);
        report.Locations.ShouldBe([location]);
        location.Deviation.ShouldBe(-0.125);
    }
}
