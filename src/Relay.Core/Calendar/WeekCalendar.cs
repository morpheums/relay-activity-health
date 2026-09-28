using static Relay.Core.Calendar.WeekLength;

namespace Relay.Core.Calendar;

public sealed class WeekCalendar : IWeekCalendar
{
    public WeekWindow Window(DateOnly weekStart, string timeZoneId)
    {
        if (weekStart.DayOfWeek != DayOfWeek.Monday)
        {
            throw new ArgumentException($"Week start {weekStart:yyyy-MM-dd} is not a Monday.", nameof(weekStart));
        }

        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        return new WeekWindow(
            weekStart,
            LocalMidnightToUtc(weekStart, timeZone),
            LocalMidnightToUtc(weekStart.AddDays(DaysPerWeek), timeZone));
    }

    public DateOnly WeekContaining(DateTime instantUtc, string timeZoneId)
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        var utcInstant = DateTime.SpecifyKind(instantUtc, DateTimeKind.Utc);
        var localDay = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(utcInstant, timeZone));
        return MondayOnOrBefore(localDay);
    }

    public DateOnly LatestCompleteWeek(DateTime dataAnchorUtc, string timeZoneId) =>
        WeekContaining(dataAnchorUtc, timeZoneId).AddDays(-DaysPerWeek);

    private static DateOnly MondayOnOrBefore(DateOnly localDay)
    {
        var daysSinceMonday = ((int)localDay.DayOfWeek - (int)DayOfWeek.Monday + DaysPerWeek) % DaysPerWeek;
        return localDay.AddDays(-daysSinceMonday);
    }

    private static DateTime LocalMidnightToUtc(DateOnly localDay, TimeZoneInfo timeZone) =>
        TimeZoneInfo.ConvertTimeToUtc(localDay.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified), timeZone);
}
