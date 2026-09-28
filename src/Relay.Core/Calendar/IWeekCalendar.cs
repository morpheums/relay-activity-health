namespace Relay.Core.Calendar;

public interface IWeekCalendar
{
    WeekWindow Window(DateOnly weekStart, string timeZoneId);

    DateOnly WeekContaining(DateTime instantUtc, string timeZoneId);

    DateOnly LatestCompleteWeek(DateTime dataAnchorUtc, string timeZoneId);
}
