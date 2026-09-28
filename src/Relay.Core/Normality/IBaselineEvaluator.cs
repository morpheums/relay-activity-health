namespace Relay.Core.Normality;

public interface IBaselineEvaluator
{
    SeriesHealth Evaluate(IReadOnlyList<int> eligibleWeekCounts, int selectedWeekCount);
}
