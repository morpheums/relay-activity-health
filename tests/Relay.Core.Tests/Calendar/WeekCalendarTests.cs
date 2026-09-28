using Relay.Core.Calendar;
using Relay.Core.Tests.TestDoubles;

namespace Relay.Core.Tests.Calendar;

public sealed class WeekCalendarTests
{
    private const string InvalidTimeZoneId = "Mars/Olympus";

    private readonly WeekCalendar _weekCalendar = new();

    [Theory]
    [InlineData("America/Chicago", "2026-03-02", "2026-03-02T06:00:00Z", "2026-03-09T05:00:00Z")]
    [InlineData("America/Chicago", "2026-10-26", "2026-10-26T05:00:00Z", "2026-11-02T06:00:00Z")]
    [InlineData("America/Phoenix", "2026-03-02", "2026-03-02T07:00:00Z", "2026-03-09T07:00:00Z")]
    [InlineData("UTC", "2026-03-02", "2026-03-02T00:00:00Z", "2026-03-09T00:00:00Z")]
    [InlineData("America/New_York", "2026-07-20", "2026-07-20T04:00:00Z", "2026-07-27T04:00:00Z")]
    [InlineData("America/Chicago", "2026-03-09", "2026-03-09T05:00:00Z", "2026-03-16T05:00:00Z")]
    public void WindowLocalMondayReturnsHalfOpenUtcWindowFromLocalMidnightToNextLocalMidnight(
        string timeZoneId,
        string weekStart,
        string expectedStartUtc,
        string expectedEndUtc)
    {
        var window = _weekCalendar.Window(TestTime.Day(weekStart), timeZoneId);

        window.ShouldBe(new WeekWindow(TestTime.Day(weekStart), TestTime.Utc(expectedStartUtc), TestTime.Utc(expectedEndUtc)));
    }

    [Fact]
    public void WindowChicagoDstStartWeekSpans167Hours()
    {
        var window = _weekCalendar.Window(TestTime.Day("2026-03-02"), "America/Chicago");

        (window.EndUtc - window.StartUtc).ShouldBe(TimeSpan.FromHours(167));
    }

    [Fact]
    public void WindowChicagoDstEndWeekSpans169Hours()
    {
        var window = _weekCalendar.Window(TestTime.Day("2026-10-26"), "America/Chicago");

        (window.EndUtc - window.StartUtc).ShouldBe(TimeSpan.FromHours(169));
    }

    [Theory]
    [InlineData("America/Phoenix")]
    [InlineData("UTC")]
    public void WindowZoneWithoutDstSpans168HoursInDstStartWeek(string timeZoneId)
    {
        var window = _weekCalendar.Window(TestTime.Day("2026-03-02"), timeZoneId);

        (window.EndUtc - window.StartUtc).ShouldBe(TimeSpan.FromHours(168));
    }

    [Fact]
    public void WindowReturnsInstantsMarkedAsUtc()
    {
        var window = _weekCalendar.Window(TestTime.Day("2026-07-20"), "America/New_York");

        window.StartUtc.Kind.ShouldBe(DateTimeKind.Utc);
        window.EndUtc.Kind.ShouldBe(DateTimeKind.Utc);
    }

    [Theory]
    [InlineData("2026-07-21")]
    [InlineData("2026-07-26")]
    public void WindowNonMondayWeekStartThrowsArgumentException(string nonMonday)
    {
        Should.Throw<ArgumentException>(() => _weekCalendar.Window(TestTime.Day(nonMonday), "America/New_York"));
    }

    [Fact]
    public void WindowInvalidTimeZoneIdThrowsTimeZoneNotFoundException()
    {
        Should.Throw<TimeZoneNotFoundException>(() => _weekCalendar.Window(TestTime.Day("2026-07-20"), InvalidTimeZoneId));
    }

    [Theory]
    [InlineData("America/Chicago", "2026-03-09T05:00:00Z", "2026-03-09")]
    [InlineData("America/Chicago", "2026-03-09T04:59:59Z", "2026-03-02")]
    [InlineData("America/Chicago", "2026-03-02T06:00:00Z", "2026-03-02")]
    [InlineData("America/Chicago", "2026-03-02T05:59:59Z", "2026-02-23")]
    [InlineData("America/Chicago", "2026-11-02T06:00:00Z", "2026-11-02")]
    [InlineData("America/Chicago", "2026-11-02T05:59:59Z", "2026-10-26")]
    [InlineData("America/Chicago", "2026-11-01T07:30:00Z", "2026-10-26")]
    [InlineData("America/New_York", "2026-07-27T04:00:00Z", "2026-07-27")]
    [InlineData("America/New_York", "2026-07-27T03:59:59Z", "2026-07-20")]
    [InlineData("America/New_York", "2026-07-27T22:20:34Z", "2026-07-27")]
    [InlineData("America/New_York", "2026-02-02T03:00:00Z", "2026-01-26")]
    [InlineData("America/Phoenix", "2026-03-09T07:00:00Z", "2026-03-09")]
    [InlineData("America/Phoenix", "2026-03-09T06:59:59Z", "2026-03-02")]
    [InlineData("UTC", "2026-03-09T00:00:00Z", "2026-03-09")]
    [InlineData("UTC", "2026-03-08T23:59:59Z", "2026-03-02")]
    public void WeekContainingInstantReturnsLocalMondayOfItsWeekWithBoundaryInNewWeek(
        string timeZoneId,
        string instantUtc,
        string expectedWeekStart)
    {
        var weekStart = _weekCalendar.WeekContaining(TestTime.Utc(instantUtc), timeZoneId);

        weekStart.ShouldBe(TestTime.Day(expectedWeekStart));
    }

    [Fact]
    public void WeekContainingInvalidTimeZoneIdThrowsTimeZoneNotFoundException()
    {
        Should.Throw<TimeZoneNotFoundException>(() =>
            _weekCalendar.WeekContaining(TestTime.Utc("2026-07-27T22:20:34Z"), InvalidTimeZoneId));
    }

    [Theory]
    [InlineData("America/New_York")]
    [InlineData("America/Chicago")]
    [InlineData("America/Denver")]
    [InlineData("America/Phoenix")]
    [InlineData("America/Los_Angeles")]
    [InlineData("UTC")]
    public void LatestCompleteWeekSeedAnchorIsJuly20InEverySeedTimeZone(string timeZoneId)
    {
        var latestCompleteWeek = _weekCalendar.LatestCompleteWeek(TestTime.Utc("2026-07-27T22:20:34Z"), timeZoneId);

        latestCompleteWeek.ShouldBe(TestTime.Day("2026-07-20"));
    }

    [Theory]
    [InlineData("2026-07-27T04:00:00Z", "2026-07-20")]
    [InlineData("2026-07-27T03:59:59Z", "2026-07-13")]
    [InlineData("2026-07-27T22:20:34Z", "2026-07-20")]
    public void LatestCompleteWeekNewYorkAnchorAroundMondayMidnightIsWeekContainingAnchorMinusSevenDays(
        string dataAnchorUtc,
        string expectedLatestCompleteWeek)
    {
        var latestCompleteWeek = _weekCalendar.LatestCompleteWeek(TestTime.Utc(dataAnchorUtc), "America/New_York");

        latestCompleteWeek.ShouldBe(TestTime.Day(expectedLatestCompleteWeek));
    }

    [Fact]
    public void LatestCompleteWeekChicagoAnchorExactlyMondayMidnightLocalIsPreviousWeek()
    {
        var latestCompleteWeek = _weekCalendar.LatestCompleteWeek(TestTime.Utc("2026-07-27T05:00:00Z"), "America/Chicago");

        latestCompleteWeek.ShouldBe(TestTime.Day("2026-07-20"));
    }

    [Fact]
    public void LatestCompleteWeekClockOnMondaySeptember28NewYorkIsSeptember21()
    {
        var latestCompleteWeek = _weekCalendar.LatestCompleteWeek(TestTime.Utc("2026-09-28T16:00:00Z"), "America/New_York");

        latestCompleteWeek.ShouldBe(TestTime.Day("2026-09-21"));
    }

    [Fact]
    public void LatestCompleteWeekInvalidTimeZoneIdThrowsTimeZoneNotFoundException()
    {
        Should.Throw<TimeZoneNotFoundException>(() =>
            _weekCalendar.LatestCompleteWeek(TestTime.Utc("2026-07-27T22:20:34Z"), InvalidTimeZoneId));
    }
}
