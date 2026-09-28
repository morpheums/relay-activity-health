
### 🧑 USER — 2026-09-28T20:09:04.095Z

The user reviewed the Phase 0 contracts on main and says `IWeekCalendar` (src/Relay.Core/Calendar/) is "a little bit overengineered". Propose a leaner calendar contract. **Proposal only — do not edit or commit anything.**

Current: `IsWeekStart(DateOnly)`, `WeekContaining(DateTime instantUtc, string tz)`, `LatestCompleteWeek(DateTime anchorUtc, string tz)`, `Window(DateOnly weekStart, string tz)`, `BaselineWindows(DateOnly selected, int baselineWeeks, string tz)`, plus `NotAWeekStartException` (4 constructors) and `WeekWindow` record.

The coordinator's starting observations (challenge them if wrong):
- `IsWeekStart` is `date.DayOfWeek == DayOfWeek.Monday` — probably not an interface member.
- `BaselineWindows` is `Window` applied to the 8 preceding Mondays — the caller (service or grid) could build it, or it stays if it removes duplication.
- `NotAWeekStartException` exists only to pin a failure contract; the service validates Monday before calling the calendar (`InvalidWeekReason.NotAWeekStart`), so a plain `ArgumentException` or no guard may suffice.
- What must remain: DST-correct local-week → UTC half-open window, week containing an instant (for earliestWeek / latest complete week), latest complete week for the anchor; invalid IANA id behaviour.

Constraints: CLAUDE.md rules (behaviour behind interfaces + DI, no comment blocks, descriptive names), PLAN §5–§7 and all §13 entries (PLAN §7 calendar edge cases must stay testable: DST start/end Chicago, Phoenix, UTC, boundary instant, latest complete week anchor cases, invalid IANA id). Also consider whether passing `string timeZoneId` on every call vs. resolving `TimeZoneInfo` once is simpler.

Give: (1) the proposed contract (exact C# signatures), (2) what moves where (e.g. baseline window list built in the service), (3) knock-on changes to other contracts, docs/acceptance-criteria.md criteria (BL-41a–h etc.) and PLAN, (4) one or two alternatives with trade-offs and your recommendation. Keep it short. CLAUDE.md handoff format.


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-aa6eafea36accc597\" && git log --oneline -1 && ls src/Relay.Core/Calendar && cat src/Relay.Core/Calendar/*.cs && grep -rn \"IWeekCalendar\\|NotAWeekStart\\|WeekWindow\\|BaselineWindows\\|LatestCompleteWeek\\|WeekContaining\\|IsWeekStart\" --include=*.cs src tests | grep -v \"^src/Relay.Core/Calendar\"","description":"Show calendar contracts and usages"}
```


<details><summary>tool result</summary>

```
5895b6b AI log entries 24-30 (Phase 0) and fresh export
IWeekCalendar.cs
NotAWeekStartException.cs
WeekCalendar.cs
WeekWindow.cs
namespace Relay.Core.Calendar;

public interface IWeekCalendar
{
    bool IsWeekStart(DateOnly localDate);

    DateOnly WeekContaining(DateTime instantUtc, string timeZoneId);

    DateOnly LatestCompleteWeek(DateTime dataAnchorUtc, string timeZoneId);

    WeekWindow Window(DateOnly weekStart, string timeZoneId);

    IReadOnlyList<WeekWindow> BaselineWindows(DateOnly selectedWeek, int baselineWeeks, string timeZoneId);
}
namespace Relay.Core.Calendar;

public sealed class NotAWeekStartException : ArgumentException
{
    public NotAWeekStartException()
    {
    }

    public NotAWeekStartException(string message)
        : base(message)
    {
    }

    public NotAWeekStartException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public NotAWeekStartException(DateOnly localDate, string parameterName)
        : base($"{localDate:yyyy-MM-dd} is not a Monday.", parameterName)
    {
        LocalDate = localDate;
    }

    public DateOnly? LocalDate { get; }
}
namespace Relay.Core.Calendar;

public sealed class WeekCalendar : IWeekCalendar
{
    public bool IsWeekStart(DateOnly localDate) => throw new NotImplementedException();

    public DateOnly WeekContaining(DateTime instantUtc, string timeZoneId) => throw new NotImplementedException();

    public DateOnly LatestCompleteWeek(DateTime dataAncho …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-aa6eafea36accc597\" && grep -rn \"IWeekCalendar\\|NotAWeekStart\\|WeekWindow\\|BaselineWindows\\|LatestCompleteWeek\\|WeekContaining\\|IsWeekStart\\|TimeZoneNotFound\\|IANA\" src tests docs PLAN.md 2>/dev/null | grep -v \"^src/Relay.Core/Calendar\" | head -80","description":"Find calendar usages across repo"}
```


<details><summary>tool result</summary>

```
src/Relay.Core/Queries/IActivityQueries.cs:13:        IReadOnlyList<WeekWindow> weekWindows,
src/Relay.Core/ActivityHealth/ActivityHealthService.cs:10:    IWeekCalendar weekCalendar,
src/Relay.Core/ActivityHealth/ActivityHealthService.cs:19:    private readonly IWeekCalendar _weekCalendar = weekCalendar;
src/Relay.Core/ActivityHealth/ActivityHealthReport.cs:12:    DateOnly LatestCompleteWeek,
src/Relay.Core/Normality/WeeklyGridBuilder.cs:9:        WeekWindow selectedWeek,
src/Relay.Core/Normality/WeeklyGridBuilder.cs:10:        IReadOnlyList<WeekWindow> baselineWindows,
src/Relay.Core/Normality/WeeklyGridBuilder.cs:16:        WeekWindow selectedWeek,
src/Relay.Core/Normality/WeeklyGridBuilder.cs:17:        IReadOnlyList<WeekWindow> baselineWindows,
src/Relay.Core/ActivityHealth/InvalidWeekReason.cs:5:    NotAWeekStart,
src/Relay.Core/ActivityHealth/InvalidWeekReason.cs:6:    AfterLatestCompleteWeek,
src/Relay.Infrastructure/Queries/SqlActivityQueries.cs:19:        IReadOnlyList<WeekWindow> weekWindows,
src/Relay.Core/Normality/IWeeklyGridBuilder.cs:9:        WeekWindow selectedWeek,
src/Relay.Core/Normality/IWeeklyGridBuilder.cs:10:        IReadOnlyList<WeekWindow> baselineWindows,
src/Relay.Core/Normality/IWeeklyGridBuilder.cs:15:        WeekWindow selectedWeek,
src/Relay.Core/Normality/IWeeklyGridBuilder.cs:16:        IReadOnlyList<WeekWindow> baselineWindows,
src/Relay.Api/Http/ActivityHealthResponse.cs:8:    DateOnly LatestCompleteWeek,
docs/design-consensus.md:304:## Nex …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-aa6eafea36accc597\" && sed -n 120,145p docs/acceptance-criteria.md; sed -n 140,185p PLAN.md; grep -n \"^## 13\\|calendar\\|Calendar\\|timezone\\|TimeZone\" PLAN.md | sed -n 1,60p; cat src/Relay.Core/ActivityHealth/ActivityHealthService.cs src/Relay.Core/Queries/IActivityQueries.cs src/Relay.Core/Accounts/AccountSummary.cs 2>/dev/null","description":"Read calendar-related criteria, PLAN sections, service"}
```


<details><summary>tool result</summary>

```
Exit code 1
| BL-32 | Equal \|deviation\| | `below` before `above`, then location name ascending (ordinal) | SPEC |
| BL-33 | Several `insufficient_data` rows | Sorted among themselves by name (ordinal) | SPEC |

### 1.4 Calendar and service

| Id | Given / When | Then | Tag |
|---|---|---|---|
| BL-40 | Anchor 2026-07-27T22:20:34Z, any of the seed timezones (New_York, Chicago, Denver, Phoenix, Los_Angeles, UTC) | Latest complete week = 2026-07-20 | GOLDEN |
| BL-41a | America/Chicago, week 2026-03-02 (contains DST start, Sun 2026-03-08) | UTC window [2026-03-02T06:00Z, 2026-03-09T05:00Z), **167 h** | SPEC §7 Calendar |
| BL-41b | America/Chicago, week 2026-10-26 (contains DST end, Sun 2026-11-01) | UTC window [2026-10-26T05:00Z, 2026-11-02T06:00Z), **169 h** | SPEC §7 Calendar |
| BL-41c | America/Phoenix, week 2026-03-02 | [2026-03-02T07:00Z, 2026-03-09T07:00Z), 168 h (no DST) | SPEC §7 Calendar |
| BL-41d | UTC, week 2026-03-02 | [2026-03-02T00:00Z, 2026-03-09T00:00Z), 168 h | SPEC §7 Calendar |
| BL-41e | America/Chicago, instant 2026-03-09T05:00:00Z (local Mon 00:00) | Belongs to week 2026-03-09, not 2026-03-02 | SPEC §7 Calendar |
| BL-41f | Latest complete week, America/New_York: anchor Mon 2026-07-27 00:00 local (04:00Z) / Sun 2026-07-26 23:59:59 local (2026-07-27T03:59:59Z) / Mon 2026-07-27 18:20:34 local (the seed anchor) | 2026-07-20 / 2026-07-13 / 2026-07-20 | SPEC §7 Calendar; consensus §3 ("week containing the anchor, minus 7 days") |
| BL-41g | Invalid IANA id  …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-aa6eafea36accc597\" && sed -n 255,280p PLAN.md; cat src/Relay.Core/Normality/IWeeklyGridBuilder.cs; ls tests/*/ ; grep -rln \"Calendar\" tests docs .claude/agents 2>/dev/null","description":"Read PLAN §13 head, grid builder, tests layout"}
```


<details><summary>tool result</summary>

```
## 13. Plan changes
_(append-only, dated, with reason)_

### 2026-09-28 — Revised design from the four-agent debate (approved by the user)

**Reason.** Battle-testing showed the original band (median ± 2·spread, √median floor) never flags a location dropping to zero for leads/appointments
and only 37 % of the time for calls (AI_LOG 8). `statistician`, `product`, `architect` and `reviewer` debated the fix directly and all signed
`docs/design-consensus.md` AGREE with no dissent; every number in it is backed by a script in `analysis/` whose output was re-run and reproduced
byte-for-byte by the coordinator. The user approved the design as written.

**This entry supersedes**, for implementation purposes: §5.1 index/de-dup bullet, §5.2 in full, §5.3 in full, the §5.4 status labels and footnote,
the §7 Evaluator / Ranking / API edge-case bullets and golden-values table, D5's default account and D7's numbers. Everything not listed stays in force,
including the §7 Calendar, Grid and SQL edge cases. Rationale, rejected options and evidence per item: `docs/design-consensus.md` §1–§4.

**Decision changes in short**
- D2/D7: the normality rule is R2\* — robust z on the Anscombe scale `T(x) = 2√(x + 3/8)`, k = 2, spread floor 1.0, minimum 4 eligible weeks,
  status read from the back-transformed integer range. Flags 4.3 % of site-weeks (was 4.8 %); drop-to-0 caught 98 % all / 96 % calls (was 78 / 37 %);
  account 6's spike week still 15/15 `above`; 0 status/range contradictions in 253,149  …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-aa6eafea36accc597\" && cat src/Relay.Core/Normality/WeeklyGridBuilder.cs src/Relay.Core/Queries/SiteFirstActivity.cs src/Relay.Core/Queries/WeeklySiteCount.cs src/Relay.Core/Accounts/AccountSummary.cs 2>&1 | head -60; grep -n \"earliestWeek\\|first-activity\\|firstActivity\" PLAN.md docs/design-consensus.md | head","description":"Check grid builder and related records"}
```


<details><summary>tool result</summary>

```
using Relay.Core.Calendar;
using Relay.Core.Queries;

namespace Relay.Core.Normality;

public sealed class WeeklyGridBuilder : IWeeklyGridBuilder
{
    public IReadOnlyList<LocationSeries> BuildLocationSeries(
        WeekWindow selectedWeek,
        IReadOnlyList<WeekWindow> baselineWindows,
        IReadOnlyList<SiteFirstActivity> siteFirstActivities,
        IReadOnlyList<WeeklySiteCount> weeklySiteCounts) =>
        throw new NotImplementedException();

    public WeeklySeries BuildAccountSeries(
        WeekWindow selectedWeek,
        IReadOnlyList<WeekWindow> baselineWindows,
        IReadOnlyList<SiteFirstActivity> siteFirstActivities,
        IReadOnlyList<WeeklySiteCount> weeklySiteCounts) =>
        throw new NotImplementedException();
}
namespace Relay.Core.Queries;

public sealed record SiteFirstActivity(string Location, DateTime FirstActivityUtc);
namespace Relay.Core.Queries;

public sealed record WeeklySiteCount(string Location, DateOnly WeekStart, int Count);
cat: src/Relay.Core/Accounts/AccountSummary.cs: No such file or directory
PLAN.md:62:| Does a site exist before its first event? | No — weeks before (and including) a site's first-activity week don't count toward its baseline |
PLAN.md:95:  "earliestWeek": "2026-01-26",
PLAN.md:124:- Components: `DashboardPage` (container), `DashboardFilters` (Viewing-as select, ◀ week ▶ stepper bounded by `earliestWeek`/`latestCompleteWeek`, type select),
PLAN.md:151:| `IWeeklyGridBuilder` | Core | Sparse `(site, week,  …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-aa6eafea36accc597\" && grep -n \"preceding\\|W-8\\|W−8\\|baselineWeeks\\|BaselineWeeks\\|8 weeks\\|eight\" docs/acceptance-criteria.md PLAN.md src/Relay.Core/Normality/NormalityOptions.cs | head; grep -n \"Calendar\\|IsWeekStart\\|NotAWeek\" .claude/agents/*.md docs/design-consensus.md | head","description":"Check baseline window criteria and other mentions"}
```


<details><summary>tool result</summary>

```
PLAN.md:32:| **Account 6: 880 events in week of Jun 1** (805 on Jun 3 alone) vs ~70/week, all 15 sites, plausible fields | A mean baseline is poisoned for 8 weeks afterwards (mean 171 vs median 72.5 for week of Jul 20). Use median + MAD |
PLAN.md:47:| D2 | Normal = own history (median ± 2 × robust spread over 8 weeks) | Sibling comparison (share of account); %-change vs mean | Sibling share is confounded when the whole account moves (spike week: all 15 sites keep their share → "all normal"), and useless for single-site accounts. Mean is poisoned by the spike; %-change cries wolf on small counts |
PLAN.md:96:  "baselineWeeks": 8,
PLAN.md:117:Constants live in `NormalityOptions { BaselineWeeks = 8, MinimumEligibleWeeks = 4, BandWidth = 2.0 }`.
PLAN.md:304:  "baselineWeeks": 8,
PLAN.md:337:Constants live in `NormalityOptions { BaselineWeeks = 8, MinimumEligibleWeeks = 4, BandWidth = 2.0, SpreadFloor = 1.0 }`.
src/Relay.Core/Normality/NormalityOptions.cs:10:    public int BaselineWeeks { get; init; } = 8;
docs/acceptance-criteria.md:193:| API-01b | `GET …/accounts/14/activity-health` (JSON names) | Top level exactly: `account{id,name,timezone}`, `eventType`, `week{start,end}`, `dataAsOf` (a string, or null only for an empty database, API-32), `latestCompleteWeek`, `earliestWeek`, `baselineWeeks`, `minimumEligibleWeeks`, `summary`, `locations`. Summary and each location: `count`, `baseline{weeksUsed,median,low,high}`, `status`, `deviation`; locations also `location`. camelCase as  …[clipped in markdown; full text in raw/]
```
</details>


### 🤖 ASSISTANT — 2026-09-28T20:10:13.067Z

## Summary
I agree with the coordinator: `IWeekCalendar` should shrink from five members plus a custom exception to three members plus the `WeekWindow` record. It keeps taking the time zone as a `string` on each call. Nothing was edited or committed.

## Changed
None. This is a proposal only.

## Verification
- Read the current contract at `/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-aa6eafea36accc597/src/Relay.Core/Calendar/` (`IWeekCalendar.cs`, `WeekCalendar.cs`, `WeekWindow.cs`, `NotAWeekStartException.cs`).
- Searched with `grep` for every use of the calendar across `src`, `tests`, `docs`, `PLAN.md` and `.claude/agents`:
  - `IWeekCalendar` is only used by `ActivityHealthService` (constructor) and the DI registration.
  - `WeekWindow` is used by `IWeeklyGridBuilder` and `IActivityQueries`.
  - `NotAWeekStartException`, `IsWeekStart` and `BaselineWindows` have no callers.
  - No test files exist yet, so nothing is lost by changing this now.
- I did not run a build because nothing changed.

### (1) Proposed contract
```csharp
namespace Relay.Core.Calendar;

public interface IWeekCalendar
{
    WeekWindow Window(DateOnly weekStart, string timeZoneId);

    DateOnly WeekContaining(DateTime instantUtc, string timeZoneId);

    DateOnly LatestCompleteWeek(DateTime dataAnchorUtc, string timeZoneId);
}

public sealed record WeekWindow(DateOnly WeekStart, DateTime StartUtc, DateTime EndUtc);
```
Failure behaviour:
- **Invalid IANA id:** `TimeZoneNotFoundException` from `TimeZoneInfo.FindSystemTimeZoneById` is allowed to propagate. There is no custom type and no fallback to UTC.
- **Non-Monday `weekStart` passed to `Window`:** a plain `ArgumentException`. This is a quick check for programming errors, not for user input.

Checking the coordinator's observations:
- **`IsWeekStart`:** agreed. It is `DayOfWeek == Monday` and has no dependency on time zones, so it doesn't belong on an interface.
- **`BaselineWindows`:** agreed. It is one line of LINQ over `Window` with a single caller, so it removes no duplication.
- **`NotAWeekStartException`:** agreed. The service returns `InvalidWeek(NotAWeekStart)` before it ever calls the calendar. The exception adds a public type and four constructors (needed for analyzer compliance) to guard a path users can't reach.
- **`LatestCompleteWeek`:** I'm challenging the implied option of dropping it. It is `WeekContaining(anchor).AddDays(-7)`, but it is a named domain concept used three ways (default week, upper-bound validation, `earliestWeek` fallback). PLAN §7 lists it as a calendar edge case, and keeping it means BL-41f stays a pure unit test.

On `string timeZoneId` versus resolving `TimeZoneInfo` once:
- **Keep the string.** The account record already carries the id as a string. The runtime caches zone lookups, so there is no performance reason to resolve once. And there is one place where string becomes zone (the calendar), so BL-41g stays a pure calendar test.
- **Passing `TimeZoneInfo` instead** would move id resolution into the service. BL-41g would then need a service test with fakes, and it would add a failure mode that `ActivityHealthResult` has no case for.

### (2) What moves where
- **Monday check:** moves into `ActivityHealthService`: `selectedWeek.DayOfWeek != DayOfWeek.Monday` → `InvalidWeek(NotAWeekStart)`. This covers BL-43.
- **Baseline windows:** built in `ActivityHealthService` as `Enumerable.Range(1, _normalityOptions.BaselineWeeks).Select(weeksBack => _weekCalendar.Window(selectedWeek.AddDays(-7 * weeksBack), timeZoneId))`, ordered oldest first. Each window is still built boundary by boundary, so baseline weeks that cross a DST change stay correct.
- **`earliestWeek`:** `WeekContaining(firstEventUtc, tz)` in the service, as today.

### (3) Knock-on changes
- **Compile surface:**
  - `IWeekCalendar.cs` and `WeekCalendar.cs` lose two members.
  - `NotAWeekStartException.cs` is deleted.
  - `IWeeklyGridBuilder`, `IActivityQueries`, `ActivityHealthService` (constructor), `SqlActivityQueries` and `CoreServiceCollectionExtensions` are unchanged.
- **`docs/acceptance-criteria.md`:**
  - BL-41a–f: unchanged.
  - BL-41g: make the "Then" column specific: "throws `TimeZoneNotFoundException`".
  - BL-41h: replace with "`Window` for 2026-07-21 (Tue) throws `ArgumentException`". The Monday check itself is tested by BL-43.
  - New BL-46 (suggested): "The service asks for counts for W plus the 8 preceding Mondays. Windows come from the calendar; for America/Chicago with W = 2026-03-16, the baseline includes the 167 h DST week." Without this, nothing pins the baseline window composition once it leaves the calendar.
- **PLAN:** no edit to §6, because §13 is append-only and rules. Add a §13 entry: "`IWeekCalendar` reduced to `Window` / `WeekContaining` / `LatestCompleteWeek`; the week-start check and the baseline window list move to the service; no custom exception". §7's "non-Monday week rejected" is still satisfied, by BL-43 plus the new BL-41h.
- **`.claude/agents/backend.md`:** line 28 is unaffected. `reviewer.md` line 20 still applies.

### (4) Alternatives
- **A. Recommended: three members, string id** (above). This is the smallest surface that keeps every PLAN §7 calendar edge case as a pure unit test.
- **B. Two members: drop `LatestCompleteWeek`.** It's the leanest possible, but the "minus 7" rule moves into the service. BL-41f would have to be rewritten as `WeekContaining` boundary cases (07-27 / 07-20 / 07-27) plus a service test for the subtraction. It saves one line and costs clarity.
- **C. Resolve once with a factory: `IWeekCalendarFactory.ForTimeZone(string)` returning an `IAccountWeekCalendar` with parameterless-zone members.** Call sites get shorter, but it adds two interfaces to remove one argument. That's more engineering, not less. Rejected.

## Deviations
None. The proposal is still within PLAN §6 (behaviour behind an interface, registered in DI) and §13.

## Uncertain
- **Invalid IANA id:** whether the user wants it surfaced through `ActivityHealthResult` rather than an exception. Seed time zones are all valid, so I proposed the exception, which becomes a 500 via `UseExceptionHandler`.
- **Order of `baselineWindows`:** oldest first is my choice. The grid tests should not depend on it, but BL-46 should state it.

## Next
If the user approves option A, the architect applies it:
- edit the two calendar files and delete the exception;
- run `dotnet build` (0 warnings) and `cd web && npm run build` (no web impact is expected);
- append the §13 entry;
- ask `product` to update BL-41g/h and add BL-46 before `test-author` starts Phase 1.

