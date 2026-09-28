using Relay.Core.Calendar;

namespace Relay.Infrastructure.Tests.Fixtures;

public static class TestWeekWindows
{
    public static WeekWindow ChicagoWeekOf20260223 { get; } =
        new(new DateOnly(2026, 2, 23), Utc.At("2026-02-23T06:00:00Z"), Utc.At("2026-03-02T06:00:00Z"));

    public static WeekWindow ChicagoWeekOf20260302DstStart { get; } =
        new(new DateOnly(2026, 3, 2), Utc.At("2026-03-02T06:00:00Z"), Utc.At("2026-03-09T05:00:00Z"));

    public static WeekWindow ChicagoWeekOf20260309 { get; } =
        new(new DateOnly(2026, 3, 9), Utc.At("2026-03-09T05:00:00Z"), Utc.At("2026-03-16T05:00:00Z"));

    public static WeekWindow TokyoWeekOf20260302StartingOnUtcSunday { get; } =
        new(new DateOnly(2026, 3, 2), Utc.At("2026-03-01T15:00:00Z"), Utc.At("2026-03-08T15:00:00Z"));
}
