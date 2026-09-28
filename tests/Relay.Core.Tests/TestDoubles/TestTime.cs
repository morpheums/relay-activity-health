using System.Globalization;
using Relay.Core.Calendar;

namespace Relay.Core.Tests.TestDoubles;

internal static class TestTime
{
    public static DateTime Utc(string isoInstant) =>
        DateTime.Parse(
            isoInstant,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);

    public static DateOnly Day(string isoDate) =>
        DateOnly.ParseExact(isoDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

    public static WeekWindow UtcMidnightWeek(string weekStart)
    {
        var weekStartDay = Day(weekStart);
        var startUtc = weekStartDay.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        return new WeekWindow(weekStartDay, startUtc, startUtc.AddDays(7));
    }

    public static IReadOnlyList<WeekWindow> UtcMidnightBaselineBefore(string selectedWeekStart, int baselineWeeks)
    {
        var selectedWeekDay = Day(selectedWeekStart);
        return Enumerable.Range(1, baselineWeeks)
            .Reverse()
            .Select(weeksBack => UtcMidnightWeek(selectedWeekDay.AddDays(-7 * weeksBack).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)))
            .ToList();
    }
}
