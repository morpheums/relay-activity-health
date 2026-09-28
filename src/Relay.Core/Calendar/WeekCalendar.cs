namespace Relay.Core.Calendar;

public sealed class WeekCalendar : IWeekCalendar
{
    private const int DaysPerWeek = 7;

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

    private static DateTime LocalMidnightToUtc(DateOnly localDay, TimeZoneInfo timeZone)
    {
        var localMidnight = localDay.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        if (!timeZone.IsInvalidTime(localMidnight))
        {
            return TimeZoneInfo.ConvertTimeToUtc(localMidnight, timeZone);
        }

        // Midnight skipped by a DST jump: the day starts at the jump, which is midnight read with the previous day's offset.
        var offsetBeforeJump = timeZone.GetUtcOffset(localMidnight.AddDays(-1));
        return DateTime.SpecifyKind(localMidnight - offsetBeforeJump, DateTimeKind.Utc);
    }
}
