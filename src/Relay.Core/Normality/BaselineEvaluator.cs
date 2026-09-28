namespace Relay.Core.Normality;

public sealed class BaselineEvaluator(NormalityOptions normalityOptions) : IBaselineEvaluator
{
    // Scales a MAD so it is comparable with a standard deviation.
    private const double MadConsistencyConstant = 1.4826;
    private const double AnscombeOffset = 0.375;
    private const double AnscombeScale = 2.0;

    private static readonly double TransformedZero = AnscombeTransform(0);

    public SeriesHealth Evaluate(IReadOnlyList<int> eligibleWeekCounts, int selectedWeekCount)
    {
        if (eligibleWeekCounts.Count < normalityOptions.MinimumEligibleWeeks)
        {
            return new SeriesHealth(
                selectedWeekCount,
                new Baseline(eligibleWeekCounts.Count, null, null, null),
                HealthStatus.InsufficientData,
                null);
        }

        var median = MedianOf(eligibleWeekCounts.Select(count => (double)count));
        var centre = AnscombeTransform(median);
        var spread = SpreadOf(eligibleWeekCounts, centre);
        var (low, high) = RangeFor(centre, spread);

        return new SeriesHealth(
            selectedWeekCount,
            new Baseline(eligibleWeekCounts.Count, median, low, high),
            StatusFor(selectedWeekCount, low, high),
            DeviationOf(selectedWeekCount, centre, spread));
    }

    private static double AnscombeTransform(double count) => AnscombeScale * Math.Sqrt(count + AnscombeOffset);

    private static double InverseAnscombeTransform(double transformed) =>
        Math.Pow(transformed / AnscombeScale, 2) - AnscombeOffset;

    private static double MedianOf(IEnumerable<double> values)
    {
        var sortedValues = values.Order().ToList();
        var middleIndex = sortedValues.Count / 2;
        return sortedValues.Count % 2 == 1
            ? sortedValues[middleIndex]
            : (sortedValues[middleIndex - 1] + sortedValues[middleIndex]) / 2.0;
    }

    private double SpreadOf(IReadOnlyList<int> eligibleWeekCounts, double centre)
    {
        var madT = MedianOf(eligibleWeekCounts.Select(count => Math.Abs(AnscombeTransform(count) - centre)));
        return Math.Max(MadConsistencyConstant * madT, normalityOptions.SpreadFloor);
    }

    private (int Low, int High) RangeFor(double centre, double spread)
    {
        var lowT = centre - normalityOptions.BandWidth * spread;
        var highT = centre + normalityOptions.BandWidth * spread;
        var low = lowT <= TransformedZero ? 0 : (int)Math.Ceiling(InverseAnscombeTransform(lowT));
        var high = (int)Math.Floor(InverseAnscombeTransform(highT));
        return (low, high);
    }

    private static HealthStatus StatusFor(int selectedWeekCount, int low, int high)
    {
        if (selectedWeekCount < low)
        {
            return HealthStatus.Below;
        }

        return selectedWeekCount > high ? HealthStatus.Above : HealthStatus.Normal;
    }

    private static double DeviationOf(int selectedWeekCount, double centre, double spread) =>
        (AnscombeTransform(selectedWeekCount) - centre) / spread;
}
