namespace Relay.Core.Calendar;

public sealed class WeekCalendar : IWeekCalendar
{
    public bool IsWeekStart(DateOnly localDate) => throw new NotImplementedException();

    public DateOnly WeekContaining(DateTime instantUtc, string timeZoneId) => throw new NotImplementedException();

    public DateOnly LatestCompleteWeek(DateTime dataAnchorUtc, string timeZoneId) => throw new NotImplementedException();

    public WeekWindow Window(DateOnly weekStart, string timeZoneId) => throw new NotImplementedException();

    public IReadOnlyList<WeekWindow> BaselineWindows(DateOnly selectedWeek, int baselineWeeks, string timeZoneId) =>
        throw new NotImplementedException();
}
