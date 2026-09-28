namespace Relay.Core.Normality;

public interface IBaselineEvaluator
{
    BaselineAssessment Evaluate(IReadOnlyList<int> eligibleWeekCounts, int selectedWeekCount);
}
