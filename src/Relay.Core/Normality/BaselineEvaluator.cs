namespace Relay.Core.Normality;

public sealed class BaselineEvaluator(NormalityOptions normalityOptions) : IBaselineEvaluator
{
    private readonly NormalityOptions _normalityOptions = normalityOptions;

    public SeriesHealth Evaluate(IReadOnlyList<int> eligibleWeekCounts, int selectedWeekCount) =>
        throw new NotImplementedException();
}
