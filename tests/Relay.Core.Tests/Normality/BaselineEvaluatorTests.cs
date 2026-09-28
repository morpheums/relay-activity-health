using Relay.Core.Normality;

namespace Relay.Core.Tests.Normality;

public sealed class BaselineEvaluatorTests
{
    private const double Tolerance = 1e-9;

    private static SeriesHealth Evaluate(IReadOnlyList<int> eligibleWeekCounts, int selectedWeekCount) =>
        new BaselineEvaluator(new NormalityOptions()).Evaluate(eligibleWeekCounts, selectedWeekCount);

    private static SeriesHealth EvaluateWith(NormalityOptions normalityOptions, IReadOnlyList<int> eligibleWeekCounts, int selectedWeekCount) =>
        new BaselineEvaluator(normalityOptions).Evaluate(eligibleWeekCounts, selectedWeekCount);

    [Fact]
    public void EvaluateAccount8FloorCaseReturnsMedian11Range6To18Normal()
    {
        var seriesHealth = Evaluate([11, 11, 11, 8], 11);

        seriesHealth.Count.ShouldBe(11);
        seriesHealth.Baseline.WeeksUsed.ShouldBe(4);
        seriesHealth.Baseline.Median.ShouldBe(11.0);
        seriesHealth.Baseline.Low.ShouldBe(6);
        seriesHealth.Baseline.High.ShouldBe(18);
        seriesHealth.Status.ShouldBe(HealthStatus.Normal);
        seriesHealth.Deviation.ShouldNotBeNull().ShouldBe(0.0, Tolerance);
    }

    [Theory]
    [InlineData(6, HealthStatus.Normal, -1.6956163124349821)]
    [InlineData(5, HealthStatus.Below, -2.108559533868169)]
    [InlineData(18, HealthStatus.Normal, 1.827845318125103)]
    [InlineData(19, HealthStatus.Above, 2.058039649213484)]
    public void EvaluateAccount8BaselineCountOnRangeEdgeIsNormalAndOneBeyondIsFlagged(
        int selectedWeekCount,
        HealthStatus expectedStatus,
        double expectedDeviation)
    {
        var seriesHealth = Evaluate([11, 11, 11, 8], selectedWeekCount);

        seriesHealth.Status.ShouldBe(expectedStatus);
        seriesHealth.Deviation.ShouldNotBeNull().ShouldBe(expectedDeviation, Tolerance);
    }

    [Fact]
    public void EvaluateEvenCountBaselineCentresOnTransformOfRawMedianGivingRange1To14()
    {
        var seriesHealth = Evaluate([2, 4, 6, 20], 14);

        seriesHealth.Baseline.WeeksUsed.ShouldBe(4);
        seriesHealth.Baseline.Median.ShouldBe(5.0);
        seriesHealth.Baseline.Low.ShouldBe(1);
        seriesHealth.Baseline.High.ShouldBe(14);
        seriesHealth.Status.ShouldBe(HealthStatus.Normal);
        seriesHealth.Deviation.ShouldNotBeNull().ShouldBe(1.9790679585482758, Tolerance);
    }

    [Theory]
    [InlineData(15, HealthStatus.Above, 2.153269134381721)]
    [InlineData(0, HealthStatus.Below, -2.2921098270714397)]
    public void EvaluateEvenCountBaselineOutsideRange1To14IsFlagged(
        int selectedWeekCount,
        HealthStatus expectedStatus,
        double expectedDeviation)
    {
        var seriesHealth = Evaluate([2, 4, 6, 20], selectedWeekCount);

        seriesHealth.Status.ShouldBe(expectedStatus);
        seriesHealth.Deviation.ShouldNotBeNull().ShouldBe(expectedDeviation, Tolerance);
    }

    [Fact]
    public void EvaluateAllZeroBaselineWithZeroCountIsNormalWithRange0To2()
    {
        var seriesHealth = Evaluate([0, 0, 0, 0], 0);

        seriesHealth.Baseline.Median.ShouldBe(0.0);
        seriesHealth.Baseline.Low.ShouldBe(0);
        seriesHealth.Baseline.High.ShouldBe(2);
        seriesHealth.Status.ShouldBe(HealthStatus.Normal);
        seriesHealth.Deviation.ShouldNotBeNull().ShouldBe(0.0, Tolerance);
    }

    [Fact]
    public void EvaluateAllZeroBaselineWithCountThreeIsAbove()
    {
        var seriesHealth = Evaluate([0, 0, 0, 0], 3);

        seriesHealth.Baseline.Low.ShouldBe(0);
        seriesHealth.Baseline.High.ShouldBe(2);
        seriesHealth.Status.ShouldBe(HealthStatus.Above);
        seriesHealth.Deviation.ShouldNotBeNull().ShouldBe(2.4494897427831783, Tolerance);
    }

    [Fact]
    public void EvaluateMedianTwoBaselineDropToZeroIsNormalWithRange0To6()
    {
        var seriesHealth = Evaluate([2, 2, 2, 2], 0);

        seriesHealth.Baseline.Low.ShouldBe(0);
        seriesHealth.Baseline.High.ShouldBe(6);
        seriesHealth.Status.ShouldBe(HealthStatus.Normal);
        seriesHealth.Deviation.ShouldNotBeNull().ShouldBe(-1.857462130092899, Tolerance);
    }

    [Fact]
    public void EvaluateMedianThreeBaselineDropToZeroIsBelowWithRange1To7()
    {
        var seriesHealth = Evaluate([3, 3, 3, 3], 0);

        seriesHealth.Baseline.Low.ShouldBe(1);
        seriesHealth.Baseline.High.ShouldBe(7);
        seriesHealth.Status.ShouldBe(HealthStatus.Below);
        seriesHealth.Deviation.ShouldNotBeNull().ShouldBe(-2.4494897427831783, Tolerance);
    }

    [Fact]
    public void EvaluateNegativeLowTransformIsGuardedToLowZeroGivingRange0To21()
    {
        var seriesHealth = Evaluate([0, 1, 5, 9], 3);

        seriesHealth.Baseline.Median.ShouldBe(3.0);
        seriesHealth.Baseline.Low.ShouldBe(0);
        seriesHealth.Baseline.High.ShouldBe(21);
        seriesHealth.Status.ShouldBe(HealthStatus.Normal);
        seriesHealth.Deviation.ShouldNotBeNull().ShouldBe(0.0, Tolerance);
    }

    [Fact]
    public void EvaluateLowTransformBetweenZeroAndTransformOfZeroGivesLowZeroAndRange0To4()
    {
        var seriesHealth = Evaluate([1, 1, 1, 1], 0);

        seriesHealth.Baseline.Median.ShouldBe(1.0);
        seriesHealth.Baseline.Low.ShouldBe(0);
        seriesHealth.Baseline.High.ShouldBe(4);
        seriesHealth.Status.ShouldBe(HealthStatus.Normal);
        seriesHealth.Deviation.ShouldNotBeNull().ShouldBe(-1.120463008520126, Tolerance);
    }

    [Fact]
    public void EvaluateSpikeInsideBaselineUsesMedian72Point5AndRange30To134()
    {
        var seriesHealth = Evaluate([53, 880, 102, 59, 76, 69, 79, 50], 87);

        seriesHealth.Count.ShouldBe(87);
        seriesHealth.Baseline.WeeksUsed.ShouldBe(8);
        seriesHealth.Baseline.Median.ShouldBe(72.5);
        seriesHealth.Baseline.Low.ShouldBe(30);
        seriesHealth.Baseline.High.ShouldBe(134);
        seriesHealth.Status.ShouldBe(HealthStatus.Normal);
        seriesHealth.Deviation.ShouldNotBeNull().ShouldBe(0.5304079203105068, Tolerance);
    }

    [Fact]
    public void EvaluateOddCountBaselineUsesMiddleValueAsMedian()
    {
        var seriesHealth = Evaluate([3, 9, 4, 6, 5], 5);

        seriesHealth.Baseline.WeeksUsed.ShouldBe(5);
        seriesHealth.Baseline.Median.ShouldBe(5.0);
        seriesHealth.Baseline.Low.ShouldBe(2);
        seriesHealth.Baseline.High.ShouldBe(10);
        seriesHealth.Status.ShouldBe(HealthStatus.Normal);
    }

    [Fact]
    public void EvaluateSevenEligibleWeeksAccount18TotalGivesMedian23Range15To33Normal()
    {
        var seriesHealth = Evaluate([26, 24, 29, 23, 17, 19, 22], 18);

        seriesHealth.Baseline.WeeksUsed.ShouldBe(7);
        seriesHealth.Baseline.Median.ShouldBe(23.0);
        seriesHealth.Baseline.Low.ShouldBe(15);
        seriesHealth.Baseline.High.ShouldBe(33);
        seriesHealth.Status.ShouldBe(HealthStatus.Normal);
        seriesHealth.Deviation.ShouldNotBeNull().ShouldBe(-1.0963257031657339, Tolerance);
    }

    [Fact]
    public void EvaluateThreeEligibleWeeksIsInsufficientDataWithNullRangeAndDeviation()
    {
        var seriesHealth = Evaluate([9, 12, 10], 8);

        seriesHealth.Count.ShouldBe(8);
        seriesHealth.Status.ShouldBe(HealthStatus.InsufficientData);
        seriesHealth.Baseline.WeeksUsed.ShouldBe(3);
        seriesHealth.Baseline.Median.ShouldBeNull();
        seriesHealth.Baseline.Low.ShouldBeNull();
        seriesHealth.Baseline.High.ShouldBeNull();
        seriesHealth.Deviation.ShouldBeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void EvaluateFewerThanFourEligibleWeeksIsInsufficientDataReportingWeeksUsed(int eligibleWeeks)
    {
        var eligibleWeekCounts = Enumerable.Repeat(5, eligibleWeeks).ToList();

        var seriesHealth = Evaluate(eligibleWeekCounts, 5);

        seriesHealth.Status.ShouldBe(HealthStatus.InsufficientData);
        seriesHealth.Baseline.WeeksUsed.ShouldBe(eligibleWeeks);
        seriesHealth.Count.ShouldBe(5);
    }

    [Fact]
    public void EvaluateMinimumEligibleWeeksComesFromOptions()
    {
        var normalityOptions = new NormalityOptions { MinimumEligibleWeeks = 3 };

        var seriesHealth = EvaluateWith(normalityOptions, [9, 12, 10], 8);

        seriesHealth.Status.ShouldBe(HealthStatus.Normal);
        seriesHealth.Baseline.WeeksUsed.ShouldBe(3);
        seriesHealth.Baseline.Median.ShouldBe(10.0);
        seriesHealth.Baseline.Low.ShouldBe(5);
        seriesHealth.Baseline.High.ShouldBe(17);
        seriesHealth.Deviation.ShouldNotBeNull().ShouldBe(-0.6541309119674503, Tolerance);
    }

    [Fact]
    public void EvaluateBandWidthComesFromOptions()
    {
        var normalityOptions = new NormalityOptions { BandWidth = 3.0 };

        var seriesHealth = EvaluateWith(normalityOptions, [11, 11, 11, 8], 11);

        seriesHealth.Baseline.Low.ShouldBe(4);
        seriesHealth.Baseline.High.ShouldBe(23);
    }

    [Fact]
    public void EvaluateSpreadFloorComesFromOptions()
    {
        var normalityOptions = new NormalityOptions { SpreadFloor = 2.0 };

        var seriesHealth = EvaluateWith(normalityOptions, [11, 11, 11, 8], 11);

        seriesHealth.Baseline.Low.ShouldBe(2);
        seriesHealth.Baseline.High.ShouldBe(28);
    }
}
