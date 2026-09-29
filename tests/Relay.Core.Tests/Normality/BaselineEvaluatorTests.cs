using Relay.Core.Normality;

namespace Relay.Core.Tests.Normality;

public sealed class BaselineEvaluatorTests
{
    private const double Tolerance = 1e-9;

    private static SeriesHealth Evaluate(IReadOnlyList<int> eligibleWeekCounts, int selectedWeekCount) =>
        new BaselineEvaluator(new NormalityOptions()).Evaluate(eligibleWeekCounts, selectedWeekCount);

    [Theory]
    [InlineData(new[] { 11, 11, 11, 8 }, 11, 11.0, 6, 18, HealthStatus.Normal, 0.0)]
    [InlineData(new[] { 11, 11, 11, 8 }, 6, 11.0, 6, 18, HealthStatus.Normal, -1.6956163124349821)]
    [InlineData(new[] { 11, 11, 11, 8 }, 5, 11.0, 6, 18, HealthStatus.Below, -2.108559533868169)]
    [InlineData(new[] { 11, 11, 11, 8 }, 18, 11.0, 6, 18, HealthStatus.Normal, 1.827845318125103)]
    [InlineData(new[] { 11, 11, 11, 8 }, 19, 11.0, 6, 18, HealthStatus.Above, 2.058039649213484)]
    [InlineData(new[] { 2, 4, 6, 20 }, 14, 5.0, 1, 14, HealthStatus.Normal, 1.9790679585482758)]
    [InlineData(new[] { 2, 4, 6, 20 }, 15, 5.0, 1, 14, HealthStatus.Above, 2.153269134381721)]
    [InlineData(new[] { 2, 4, 6, 20 }, 0, 5.0, 1, 14, HealthStatus.Below, -2.2921098270714397)]
    [InlineData(new[] { 0, 0, 0, 0 }, 0, 0.0, 0, 2, HealthStatus.Normal, 0.0)]
    [InlineData(new[] { 0, 0, 0, 0 }, 3, 0.0, 0, 2, HealthStatus.Above, 2.4494897427831783)]
    [InlineData(new[] { 2, 2, 2, 2 }, 0, 2.0, 0, 6, HealthStatus.Normal, -1.857462130092899)]
    [InlineData(new[] { 3, 3, 3, 3 }, 0, 3.0, 1, 7, HealthStatus.Below, -2.4494897427831783)]
    [InlineData(new[] { 0, 1, 5, 9 }, 3, 3.0, 0, 21, HealthStatus.Normal, 0.0)]
    [InlineData(new[] { 1, 1, 1, 1 }, 0, 1.0, 0, 4, HealthStatus.Normal, -1.120463008520126)]
    [InlineData(new[] { 53, 880, 102, 59, 76, 69, 79, 50 }, 87, 72.5, 30, 134, HealthStatus.Normal, 0.5304079203105068)]
    [InlineData(new[] { 3, 9, 4, 6, 5 }, 5, 5.0, 2, 10, HealthStatus.Normal, 0.0)]
    public void EvaluateEligibleBaselineGivesPlanMedianRangeStatusAndDeviation(
        int[] eligibleWeekCounts,
        int selectedWeekCount,
        double expectedMedian,
        int expectedLow,
        int expectedHigh,
        HealthStatus expectedStatus,
        double expectedDeviation)
    {
        var seriesHealth = Evaluate(eligibleWeekCounts, selectedWeekCount);

        seriesHealth.Count.ShouldBe(selectedWeekCount);
        seriesHealth.Baseline.WeeksUsed.ShouldBe(eligibleWeekCounts.Length);
        seriesHealth.Baseline.Median.ShouldNotBeNull().ShouldBe(expectedMedian, Tolerance);
        seriesHealth.Baseline.Low.ShouldBe(expectedLow);
        seriesHealth.Baseline.High.ShouldBe(expectedHigh);
        seriesHealth.Status.ShouldBe(expectedStatus);
        seriesHealth.Deviation.ShouldNotBeNull().ShouldBe(expectedDeviation, Tolerance);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    public void EvaluateFewerThanFourEligibleWeeksIsInsufficientDataWithCountShownAndNullRange(int eligibleWeeks)
    {
        var eligibleWeekCounts = Enumerable.Repeat(5, eligibleWeeks).ToList();

        var seriesHealth = Evaluate(eligibleWeekCounts, 8);

        seriesHealth.Count.ShouldBe(8);
        seriesHealth.Status.ShouldBe(HealthStatus.InsufficientData);
        seriesHealth.Baseline.ShouldBe(new Baseline(eligibleWeeks, null, null, null));
        seriesHealth.Deviation.ShouldBeNull();
    }
}
