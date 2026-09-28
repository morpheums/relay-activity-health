namespace Relay.Core.Calendar;

public interface IWeekCalendar
{
    bool IsWeekStart(DateOnly localDate);

    DateOnly WeekContaining(DateTime instantUtc, string timeZoneId);

    DateOnly LatestCompleteWeek(DateTime dataAnchorUtc, string timeZoneId);

    WeekWindow Window(DateOnly weekStart, string timeZoneId);

    IReadOnlyList<WeekWindow> BaselineWindows(DateOnly selectedWeek, int baselineWeeks, string timeZoneId);
}
