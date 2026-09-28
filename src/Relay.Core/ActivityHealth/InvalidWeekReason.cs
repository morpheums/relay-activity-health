namespace Relay.Core.ActivityHealth;

public enum InvalidWeekReason
{
    NotAWeekStart,
    AfterLatestCompleteWeek,
    BeforeEarliestWeek,
}
