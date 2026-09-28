namespace Relay.Core.ActivityHealth;

public abstract record ActivityHealthResult
{
    private ActivityHealthResult()
    {
    }

    public sealed record Found(ActivityHealthReport Report) : ActivityHealthResult;

    public sealed record AccountNotFound : ActivityHealthResult;

    public sealed record InvalidWeek(InvalidWeekReason Reason) : ActivityHealthResult;
}
