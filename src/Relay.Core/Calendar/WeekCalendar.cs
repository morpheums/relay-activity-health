namespace Relay.Core.Calendar;

public sealed class WeekCalendar : IWeekCalendar
{
    public WeekWindow Window(DateOnly weekStart, string timeZoneId) => throw new NotImplementedException();

    public DateOnly WeekContaining(DateTime instantUtc, string timeZoneId) => throw new NotImplementedException();

    public DateOnly LatestCompleteWeek(DateTime dataAnchorUtc, string timeZoneId) => throw new NotImplementedException();
}
