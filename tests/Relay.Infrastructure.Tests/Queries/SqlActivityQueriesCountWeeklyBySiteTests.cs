using System.Globalization;
using Relay.Core.Calendar;
using Relay.Core.Queries;
using Relay.Infrastructure.Queries;
using Relay.Infrastructure.Tests.Fixtures;
using static Relay.Infrastructure.Tests.Fixtures.EventRow;
using static Relay.Infrastructure.Tests.Fixtures.TestWeekWindows;

namespace Relay.Infrastructure.Tests.Queries;

[Collection(SqlServerTestGroup.Name)]
public sealed class SqlActivityQueriesCountWeeklyBySiteTests(SqlServerFixture fixture) : SqlServerTest(fixture)
{
    private const int OtherAccountId = 2;

    private static readonly DateOnly WeekOf20260223 = new(2026, 2, 23);
    private static readonly DateOnly WeekOf20260302 = new(2026, 3, 2);
    private static readonly DateOnly WeekOf20260309 = new(2026, 3, 9);

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        await Database.InsertAccountsAsync(
            [
                new AccountRow(DefaultAccountId, "Target Account", "America/Chicago"),
                new AccountRow(OtherAccountId, "Other Account", "America/Chicago"),
            ],
            CancellationToken);
    }

    [Theory]
    [InlineData(null, null, null, null, 1)]
    [InlineData(95, 95, "connected", "connected", 1)]
    [InlineData(null, 0, "connected", "connected", 1)]
    [InlineData(95, 95, null, "", 1)]
    [InlineData(95, 120, "connected", "connected", 2)]
    [InlineData(95, 95, "connected", "missed", 2)]
    public async Task CountWeeklyBySiteTwoRowsAtTheSameInstantAreOneEventOnlyWhenEveryColumnMatchesWithNullEqualToZeroOrEmpty(
        int? firstDurationSeconds,
        int? secondDurationSeconds,
        string? firstOutcome,
        string? secondOutcome,
        int expectedCount)
    {
        var sharedInstantEvent = EventAt("2026-03-04T15:30:00Z");
        await Database.InsertEventsAsync(
            [
                sharedInstantEvent with { DurationSeconds = firstDurationSeconds, Outcome = firstOutcome },
                sharedInstantEvent with { DurationSeconds = secondDurationSeconds, Outcome = secondOutcome },
            ],
            CancellationToken);

        var weeklyCounts = await CountAsync([ChicagoWeekOf20260302DstStart], ActivityType.All);

        weeklyCounts.ShouldBe([new WeeklySiteCount("Site A", WeekOf20260302, expectedCount)]);
    }

    [Fact]
    public async Task CountWeeklyBySiteNearDuplicatesOneSecondApartAreCountedTwice()
    {
        var firstEvent = EventAt("2026-03-04T15:30:00Z");
        var nearDuplicate = firstEvent with { OccurredAtUtc = firstEvent.OccurredAtUtc.AddSeconds(1) };
        await Database.InsertEventsAsync([firstEvent, nearDuplicate], CancellationToken);

        var weeklyCounts = await CountAsync([ChicagoWeekOf20260302DstStart], ActivityType.All);

        weeklyCounts.ShouldBe([new WeeklySiteCount("Site A", WeekOf20260302, 2)]);
    }

    [Theory]
    [InlineData(ActivityType.All, 2)]
    [InlineData(ActivityType.CallReceived, 1)]
    [InlineData(ActivityType.LeadCreated, 1)]
    public async Task CountWeeklyBySiteExactDuplicateIsCountedOnceUnderEveryFilterAndEventTypeIsPartOfTheDuplicateKey(
        ActivityType eventType,
        int expectedCount)
    {
        var callEvent = EventAt("2026-03-04T15:30:00Z") with { DurationSeconds = null, Outcome = null };
        await Database.InsertEventsAsync([callEvent, callEvent, callEvent with { EventType = LeadCreated }], CancellationToken);

        var weeklyCounts = await CountAsync([ChicagoWeekOf20260302DstStart], eventType);

        weeklyCounts.ShouldBe([new WeeklySiteCount("Site A", WeekOf20260302, expectedCount)]);
    }

    [Theory]
    [InlineData("2026-03-02T06:00:00Z", "2026-03-02")]
    [InlineData("2026-03-09T05:00:00Z", "2026-03-09")]
    [InlineData("2026-03-09T04:59:59Z", "2026-03-02")]
    public async Task CountWeeklyBySiteWindowsAreHalfOpenSoAnInstantOnABoundaryBelongsToTheLaterWindow(
        string occurredAtUtc,
        string expectedWeekStart)
    {
        await Database.InsertEventsAsync([EventAt(occurredAtUtc)], CancellationToken);

        var weeklyCounts = await CountAsync(
            [ChicagoWeekOf20260223, ChicagoWeekOf20260302DstStart, ChicagoWeekOf20260309],
            ActivityType.All);

        var expectedWeek = DateOnly.ParseExact(expectedWeekStart, "yyyy-MM-dd", CultureInfo.InvariantCulture);
        weeklyCounts.ShouldBe([new WeeklySiteCount("Site A", expectedWeek, 1)]);
    }

    [Theory]
    [InlineData(ActivityType.All, 6)]
    [InlineData(ActivityType.CallReceived, 1)]
    [InlineData(ActivityType.LeadCreated, 2)]
    [InlineData(ActivityType.AppointmentSet, 3)]
    public async Task CountWeeklyBySiteSpecificTypeCountsOnlyRowsOfThatType(ActivityType eventType, int expectedCount)
    {
        await Database.InsertEventsAsync(
            [
                EventAt("2026-03-03T14:00:00Z"),
                EventAt("2026-03-03T15:00:00Z") with { EventType = LeadCreated },
                EventAt("2026-03-03T16:00:00Z") with { EventType = LeadCreated },
                EventAt("2026-03-04T14:00:00Z") with { EventType = AppointmentSet },
                EventAt("2026-03-04T15:00:00Z") with { EventType = AppointmentSet },
                EventAt("2026-03-04T16:00:00Z") with { EventType = AppointmentSet },
            ],
            CancellationToken);

        var weeklyCounts = await CountAsync([ChicagoWeekOf20260302DstStart], eventType);

        weeklyCounts.ShouldBe([new WeeklySiteCount("Site A", WeekOf20260302, expectedCount)]);
    }

    [Fact]
    public async Task CountWeeklyBySiteMultipleWindowsInOneCallAreCountedPerWindowAndLocation()
    {
        await Database.InsertEventsAsync(
            [
                EventAt("2026-02-24T12:00:00Z"),
                EventAt("2026-03-03T12:00:00Z"),
                EventAt("2026-03-06T12:00:00Z"),
                EventAt("2026-03-10T12:00:00Z") with { Location = "Site B" },
                EventAt("2026-03-11T12:00:00Z") with { Location = "Site B" },
                EventAt("2026-03-15T23:00:00Z") with { Location = "Site B" },
                EventAt("2026-02-28T12:00:00Z") with { Location = "Site C" },
            ],
            CancellationToken);

        var weeklyCounts = await CountAsync(
            [ChicagoWeekOf20260223, ChicagoWeekOf20260302DstStart, ChicagoWeekOf20260309],
            ActivityType.All);

        weeklyCounts.ShouldBe(
            [
                new WeeklySiteCount("Site A", WeekOf20260223, 1),
                new WeeklySiteCount("Site A", WeekOf20260302, 2),
                new WeeklySiteCount("Site B", WeekOf20260309, 3),
                new WeeklySiteCount("Site C", WeekOf20260223, 1),
            ],
            ignoreOrder: true);
    }

    [Fact]
    public async Task CountWeeklyBySiteOtherAccountsRowsAreIgnored()
    {
        var targetAccountEvent = EventAt("2026-03-04T15:30:00Z");
        await Database.InsertEventsAsync(
            [
                targetAccountEvent,
                targetAccountEvent with { AccountId = OtherAccountId },
                EventAt("2026-03-05T15:30:00Z") with { AccountId = OtherAccountId },
                EventAt("2026-03-05T15:30:00Z") with { AccountId = OtherAccountId, Location = "Site Z" },
            ],
            CancellationToken);

        var weeklyCounts = await CountAsync([ChicagoWeekOf20260302DstStart], ActivityType.All);

        weeklyCounts.ShouldBe([new WeeklySiteCount("Site A", WeekOf20260302, 1)]);
    }

    [Fact]
    public async Task CountWeeklyBySiteEventsOutsideEveryRequestedWindowAreIgnored()
    {
        await Database.InsertEventsAsync(
            [
                EventAt("2026-02-23T05:59:59Z"),
                EventAt("2026-02-25T12:00:00Z"),
                EventAt("2026-03-04T12:00:00Z"),
                EventAt("2026-03-12T12:00:00Z"),
                EventAt("2026-03-16T05:00:00Z"),
                EventAt("2026-07-27T22:20:34Z"),
            ],
            CancellationToken);

        var weeklyCounts = await CountAsync([ChicagoWeekOf20260223, ChicagoWeekOf20260309], ActivityType.All);

        weeklyCounts.ShouldBe(
            [
                new WeeklySiteCount("Site A", WeekOf20260223, 1),
                new WeeklySiteCount("Site A", WeekOf20260309, 1),
            ],
            ignoreOrder: true);
    }

    [Fact]
    public async Task CountWeeklyBySiteAccountWithoutEventsReturnsEmpty()
    {
        await Database.InsertEventsAsync(
            [EventAt("2026-03-04T15:30:00Z") with { AccountId = OtherAccountId }],
            CancellationToken);

        var weeklyCounts = await CountAsync(
            [ChicagoWeekOf20260223, ChicagoWeekOf20260302DstStart, ChicagoWeekOf20260309],
            ActivityType.All);

        weeklyCounts.ShouldBeEmpty();
    }

    [Fact]
    public async Task CountWeeklyBySiteWeekStartIsTheWindowsLocalMondayNotTheUtcDate()
    {
        await Database.InsertEventsAsync([EventAt("2026-03-01T15:00:00Z")], CancellationToken);

        var weeklyCounts = await CountAsync([TokyoWeekOf20260302StartingOnUtcSunday], ActivityType.All);

        weeklyCounts.ShouldBe([new WeeklySiteCount("Site A", WeekOf20260302, 1)]);
    }

    private Task<IReadOnlyList<WeeklySiteCount>> CountAsync(IReadOnlyList<WeekWindow> weekWindows, ActivityType eventType) =>
        new SqlActivityQueries(QueryDbContext).CountWeeklyBySiteAsync(DefaultAccountId, weekWindows, eventType, CancellationToken);
}
