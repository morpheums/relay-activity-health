
### 🧑 USER — 2026-09-28T23:53:51.735Z

The user has made a decision; validate it. This is read-only and you may not edit anything.

**Decision:** add one Core service test, `GetAsyncWeeklyCountAndSiteRowOrderDoesNotChangeTheReport`, in `tests/Relay.Core.Tests/ActivityHealth/ActivityHealthServiceTests.cs`.

**What it does:**
- Build the same scenario twice. The second time, the fakes (`FakeActivityQueries`) return the weekly-count rows and site-first-activity rows in reverse order.
- Assert that the two `GetAsync` reports are equal.

**Why:** PLAN §13 says the SQL has no ORDER BY, so query row order is not part of the `IActivityQueries` contract. But the Core fakes always return rows in the order they were inserted, so a service that depended on row order would only be caught by the seeded API goldens.

**Your job:**
- Check that the test is consistent with PLAN §13 and the current contracts: `IActivityQueries` return types, `ActivityHealthService`, and the records.
- Check that no public contract changes are needed. Can the fake reverse its rows with a test-only option, or does that need a contract change?
- Flag any conflict.

Answer in one line: **VALID**, **CONFLICT: …** or **CONTRACT IMPACT: …**. Then give a short rationale.


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-aeccf37963c2bd3d2\"; git log --oneline -1; ls src/Relay.Core/Queries tests/Relay.Core.Tests tests/Relay.Core.Tests/*; cat src/Relay.Core/Queries/*.cs; grep -n -i \"order by\\|row order\\|ORDER\" PLAN.md | head -40","description":"Inspect query contracts, tests, and PLAN ordering mentions"}
```


<details><summary>tool result</summary>

```
e90b938 Record OS-agnostic configuration via DotNetEnv in PLAN §13
tests/Relay.Core.Tests/Relay.Core.Tests.csproj

src/Relay.Core/Queries:
AccountListItem.cs
ActivityType.cs
IAccountQueries.cs
IActivityQueries.cs
SiteFirstActivity.cs
SiteFirstActivityExtensions.cs
WeeklySiteCount.cs

tests/Relay.Core.Tests:
Accounts
ActivityHealth
Calendar
Normality
Relay.Core.Tests.csproj
TestDoubles

tests/Relay.Core.Tests/Accounts:
AccountServiceTests.cs

tests/Relay.Core.Tests/ActivityHealth:
ActivityHealthServiceTests.cs

tests/Relay.Core.Tests/Calendar:
WeekCalendarTests.cs

tests/Relay.Core.Tests/Normality:
BaselineEvaluatorTests.cs
LocationRankerTests.cs
WeeklyGridBuilderTests.cs

tests/Relay.Core.Tests/TestDoubles:
FakeAccountQueries.cs
FakeActivityQueries.cs
FixedTimeProvider.cs
TestTime.cs
namespace Relay.Core.Queries;

public sealed record AccountListItem(int Id, string Name, string Timezone);
namespace Relay.Core.Queries;

public enum ActivityType
{
    All,
    CallReceived,
    LeadCreated,
    AppointmentSet,
}
namespace Relay.Core.Queries;

public interface IAccountQueries
{
    Task<IReadOnlyList<AccountListItem>> ListAsync(CancellationToken cancellationToken);

    Task<AccountListItem?> FindAsync(int accountId, CancellationToken cancellationToken);
}
using Relay.Core.Calendar;

namespace Relay.Core.Queries;

public interface IActivityQueries
{
    Task<DateTime?> FindDataAnchorAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<SiteFirstActivity>> ListSiteF …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-aeccf37963c2bd3d2\"; cat tests/Relay.Core.Tests/TestDoubles/FakeActivityQueries.cs; grep -n -i \"no ORDER\\|ORDER BY\\|order\" PLAN.md | sed -n 1,60p; ls src/Relay.Core/ActivityHealth; cat src/Relay.Core/ActivityHealth/ActivityHealthReport.cs src/Relay.Core/ActivityHealth/LocationHealth.cs 2>/dev/null; grep -n \"Report\\|Equal\" tests/Relay.Core.Tests/ActivityHealth/ActivityHealthServiceTests.cs | head -30","description":"Inspect fake, records, and existing tests"}
```


<details><summary>tool result</summary>

```
using Relay.Core.Calendar;
using Relay.Core.Queries;

namespace Relay.Core.Tests.TestDoubles;

internal sealed record WeeklyCountRequest(int AccountId, IReadOnlyList<WeekWindow> WeekWindows, ActivityType EventType);

internal sealed class FakeActivityQueries : IActivityQueries
{
    private readonly List<(int AccountId, SiteFirstActivity SiteFirstActivity)> _siteFirstActivities = [];
    private readonly List<(int AccountId, ActivityType EventType, WeeklySiteCount WeeklySiteCount)> _weeklySiteCounts = [];
    private DateTime? _dataAnchorUtc;

    public List<WeeklyCountRequest> WeeklyCountRequests { get; } = [];

    public IReadOnlyList<WeekWindow> RequestedWindows =>
        WeeklyCountRequests.SelectMany(request => request.WeekWindows).ToList();

    public FakeActivityQueries WithDataAnchor(string dataAnchorUtc)
    {
        _dataAnchorUtc = TestTime.Utc(dataAnchorUtc);
        return this;
    }

    public FakeActivityQueries WithSite(int accountId, string location, string firstActivityUtc)
    {
        _siteFirstActivities.Add((accountId, new SiteFirstActivity(location, TestTime.Utc(firstActivityUtc))));
        return this;
    }

    public FakeActivityQueries WithWeeklyCounts(
        int accountId,
        string location,
        string firstWeekStart,
        ActivityType eventType,
        params int[] consecutiveWeekCounts)
    {
        var firstWeekDay = TestTime.Day(firstWeekStart);
        for (var weekIndex = 0; weekIndex < consecutiveWeekCounts.Length; …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-aeccf37963c2bd3d2\"; cat src/Relay.Core/ActivityHealth/ActivityHealthService.cs src/Relay.Core/ActivityHealth/ActivityHealthResult*.cs; ls src/Relay.Core/Normality; cat src/Relay.Core/Normality/LocationSeries.cs src/Relay.Core/Normality/WeeklySeries.cs src/Relay.Core/Normality/SeriesHealth.cs src/Relay.Core/Normality/Baseline.cs 2>/dev/null; grep -rn \"Sort\\|OrderBy\\|Order\" src/Relay.Core --include=*.cs","description":"Inspect service, records, ordering in Core"}
```


<details><summary>tool result</summary>

```
using Relay.Core.Calendar;
using Relay.Core.Normality;
using Relay.Core.Queries;
using static Relay.Core.Calendar.WeekLength;

namespace Relay.Core.ActivityHealth;

public sealed class ActivityHealthService(
    IAccountQueries accountQueries,
    IActivityQueries activityQueries,
    IWeekCalendar weekCalendar,
    IWeeklyGridBuilder weeklyGridBuilder,
    IBaselineEvaluator baselineEvaluator,
    ILocationRanker locationRanker,
    NormalityOptions normalityOptions,
    TimeProvider timeProvider) : IActivityHealthService
{
    public async Task<ActivityHealthResult> GetAsync(
        int accountId,
        DateOnly? week,
        ActivityType eventType,
        CancellationToken cancellationToken)
    {
        var account = await accountQueries.FindAsync(accountId, cancellationToken);
        if (account is null)
        {
            return new ActivityHealthResult.AccountNotFound();
        }

        if (week is { DayOfWeek: not DayOfWeek.Monday })
        {
            return new ActivityHealthResult.InvalidWeek(InvalidWeekReason.NotAWeekStart);
        }

        var dataAnchorUtc = await activityQueries.FindDataAnchorAsync(cancellationToken);
        var latestCompleteWeek = weekCalendar.LatestCompleteWeek(dataAnchorUtc ?? timeProvider.GetUtcNow().UtcDateTime, account.Timezone);
        var siteFirstActivities = await activityQueries.ListSiteFirstActivitiesAsync(accountId, cancellationToken);
        var earliestWeek = EarliestWeekFor(siteFirstActivities, latestCompl …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-aeccf37963c2bd3d2\"; sed -n 1,60p tests/Relay.Core.Tests/ActivityHealth/ActivityHealthServiceTests.cs; grep -n \"^## \\|^### \" PLAN.md | tail -20; grep -n -i \"row order\\|unordered\\|any order\\|ORDER BY\\|ordering\" PLAN.md src/Relay.Infrastructure -r; grep -n \"Sort\\|OrderBy\\|ThenBy\" -r src/Relay.Core","description":"Inspect test setup and PLAN ordering statements"}
```


<details><summary>tool result</summary>

```
using Relay.Core.ActivityHealth;
using Relay.Core.Calendar;
using Relay.Core.Normality;
using Relay.Core.Queries;
using Relay.Core.Tests.TestDoubles;

namespace Relay.Core.Tests.ActivityHealth;

public sealed class ActivityHealthServiceTests
{
    private const double Tolerance = 1e-9;
    private const string SeedDataAnchor = "2026-07-27T22:20:34Z";
    private const string MondayClockUtc = "2026-09-28T16:00:00Z";

    private const int StorageAccountId = 18;
    private const int CollisionAccountId = 6;
    private const int SecurityAccountId = 14;
    private const int AutoGroupAccountId = 1;
    private const int SpaAccountId = 20;

    private static CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    private static ActivityHealthService CreateService(
        FakeAccountQueries accountQueries,
        FakeActivityQueries activityQueries,
        NormalityOptions? normalityOptions = null,
        string clockUtc = MondayClockUtc)
    {
        var options = normalityOptions ?? new NormalityOptions();
        return new ActivityHealthService(
            accountQueries,
            activityQueries,
            new WeekCalendar(),
            new WeeklyGridBuilder(),
            new BaselineEvaluator(options),
            new LocationRanker(),
            options,
            new FixedTimeProvider(new DateTimeOffset(TestTime.Utc(clockUtc))));
    }

    private static FakeAccountQueries StorageAccount() =>
        new FakeAccountQueries().Wit …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-aeccf37963c2bd3d2\"; sed -n 255,600p PLAN.md | grep -n -i \"sql\\|order\\|sorted\" ; grep -rn -i \"order\" src/Relay.Infrastructure --include=*.cs | head; cat src/Relay.Core/Normality/WeeklyGridBuilder.cs | head -60","description":"Check PLAN SQL ordering statements and grid builder"}
```


<details><summary>tool result</summary>

```
13:including the §7 Calendar, Grid and SQL edge cases. Rationale, rejected options and evidence per item: `docs/design-consensus.md` §1–§4.
21:- Starter files: `schema.sql` and `seed.sql` are currently at the repo root; Phase 0 moves them to `db/` with `git mv`, content untouched (user-approved).
60:- `locations` is returned sorted (§5.3 step 9).
188:| Account 14, week 2026-07-20, all | ranking order B, C, A, D |
189:| Account 14, week 2026-07-20, `appointment_set` (ties) | total 2, range 1–8, `normal`; Site A 0 vs 0–4 (dev −1.12) first; Sites B, C, D have deviation 0 → ordered by name B, C, D |
211:  used by the API mapper, the API `type` validation (pattern derived from it) and Infrastructure SQL.
248:  validation is built from `Enum.GetValues<ActivityType>()` through the same naming policy; Infrastructure SQL uses the same policy. No hand-written name tables.
255:- **JSON numbers** are compared numerically (the §5.2 example `1.30` serialises as `1.3`); `locations[]` property order is location, count, baseline, status, deviation (pinned by an API golden test).
266:- **`GET /api/accounts` is ordered by name** (ordinal, ties by id), in `AccountService` — SQL stays free of `ORDER BY`.
294:Angular app, API and seeded SQL Server work together. Adds §7 layer 6 and a Phase 4 to §9.
296:  2 vs "Usually 3–12 a week" ranked first, order B, C, A, D — first §13 golden table); account 6 week 2026-06-01 (total 880, all 15 sites `above`); account 20 empty state;
(eval):1: no matches found …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-aeccf37963c2bd3d2\"; grep -rn -i \"order by\" src/ | head; grep -n \"ShouldBeEquivalentTo\\|Shouldly\" -r tests/Relay.Core.Tests | head -3","description":"Check SQL ORDER BY and assertion library"}
```


<details><summary>tool result</summary>

```
tests/Relay.Core.Tests/Relay.Core.Tests.csproj:11:    <Using Include="Shouldly" />
tests/Relay.Core.Tests/Relay.Core.Tests.csproj:21:    <PackageReference Include="Shouldly" />
```
</details>


### 🤖 ASSISTANT — 2026-09-28T23:54:33.148Z

**VALID.** The test fits PLAN §13 and the current contracts, and it needs no public contract change. Two notes on how it is written, and one wording fix to the rationale.

**Rationale**
- **Contracts.** `IActivityQueries.ListSiteFirstActivitiesAsync` and `CountWeeklyBySiteAsync` return `IReadOnlyList<SiteFirstActivity>` and `IReadOnlyList<WeeklySiteCount>`. Neither the interface nor the records promise any order. The Infrastructure SQL has no `ORDER BY` anywhere under `src/`. `ActivityHealthService` only passes the rows to `WeeklyGridBuilder`, which uses them order-free (dictionary lookups, sums, `Min`), and then `LocationRanker`. The ranker's sort always ends with an ordinal name tiebreak, so the output order is fully fixed. The report should therefore be the same, and the test pins a property the contract already implies.
- **Fake change.** `FakeActivityQueries` is an `internal sealed` class in `tests/Relay.Core.Tests/TestDoubles/`. A test-only builder option (for example `WithRowsReversed()`, which reverses both returned lists) is test code, not a public contract. No change to Core, Infrastructure, the API or the TypeScript side.

**Things to check (not conflicts)**
1. **Comparing the reports.** `ActivityHealthReport` is a record, but `Locations` is an `IReadOnlyList<LocationHealth>`. Record equality compares that list by reference, so `reversedReport.ShouldBe(originalReport)` will fail even when the contents match. Compare `Locations` item by item (e.g. `reversed.Locations.ShouldBe(original.Locations)`, which compares the elements), and compare the rest with `with { Locations = [] }` on both sides. Alternatively, use Shouldly's `ShouldBeEquivalentTo`. The other nested records (`SeriesHealth`, `Baseline`, `WeekRange`, `AccountListItem`) have no collection members, so ordinary equality works for them.
2. **Scenario choice.** The Storage account only has Sites A and B, which the ranker separates anyway. Account 14 with `appointment_set` is a stronger scenario: Sites B, C and D tie at deviation 0 and are ordered by name (a §13 golden). That scenario actually exercises the tiebreak when the input order is reversed.
3. **The PLAN citation.** PLAN §13 only says "SQL stays free of `ORDER BY`" for `GET /api/accounts` (PLAN.md line 520). For the activity queries, the unordered result follows from CLAUDE.md rule 7 ("SQL does counting only") and from the contract saying nothing about order. The rationale should cite those. If you want it stated outright, you could add a one-line §13 entry saying `IActivityQueries` results are unordered.

The test will pass against the current implementation as soon as it is written. That is fine for a regression guard, but it will not have a red phase.

Relevant files:
- `/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-aeccf37963c2bd3d2/src/Relay.Core/Queries/IActivityQueries.cs`
- `/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-aeccf37963c2bd3d2/src/Relay.Core/ActivityHealth/ActivityHealthService.cs`
- `/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-aeccf37963c2bd3d2/src/Relay.Core/ActivityHealth/ActivityHealthReport.cs`
- `/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-aeccf37963c2bd3d2/src/Relay.Core/Normality/WeeklyGridBuilder.cs`
- `/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-aeccf37963c2bd3d2/src/Relay.Core/Normality/LocationRanker.cs`
- `/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-aeccf37963c2bd3d2/tests/Relay.Core.Tests/TestDoubles/FakeActivityQueries.cs`
- `/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-aeccf37963c2bd3d2/tests/Relay.Core.Tests/ActivityHealth/ActivityHealthServiceTests.cs`

