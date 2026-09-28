
### 🧑 USER — 2026-09-28T20:11:51.011Z

Review the calendar-contract simplification for DASH-247 (user decision at contract review, option A). Read-only. Three branches off main:
- `worktree-agent-ac9381cc42ed979cb` (cb04c68): contract change — `IWeekCalendar` reduced to `Window`, `WeekContaining`, `LatestCompleteWeek`; `NotAWeekStartException` deleted.
- `worktree-agent-a73b2ea8c833f1f05` (d4fcbad): `docs/acceptance-criteria.md` — BL-41g/h updated, BL-46 added.
- `governance/calendar` (4ce063b): PLAN §13 entry "Calendar contract simplified".
Use `git diff main...<branch>` for each; build in a scratch copy if useful.

Check: the three are consistent with each other and with PLAN §13; no dangling references to removed members anywhere (src, web, docs, PLAN §13 text, .claude/agents); every PLAN §7 calendar edge case is still testable with the three members (DST start/end Chicago, Phoenix, UTC, boundary instant, latest-complete-week anchor cases, invalid IANA id, non-Monday); BL-46 dates are correct (W = 2026-03-16 → 8 preceding Mondays, 2026-03-02 week is 167 h in America/Chicago); `InvalidWeekReason.NotAWeekStart` still exists for BL-43; the three branches merge without conflict. Verdict APPROVED / CHANGES REQUIRED, findings split (a) objective / (b) user decision. CLAUDE.md handoff format, keep it short.


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-aab69ccac33797947\"; for b in worktree-agent-ac9381cc42ed979cb worktree-agent-a73b2ea8c833f1f05 governance/calendar; do echo \"=== $b\"; git diff main...$b; done; git branch -a | head -30"}
```


<details><summary>tool result</summary>

```
This agent is isolated in the worktree /Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-aab69ccac33797947, but this command names git in a form too complex to verify that it stays inside the worktree. Refusing to run it — a worktree-isolated agent's git operations must target its own worktree. Split it into plain, separate commands and run them from /Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-aab69ccac33797947.
```
</details>


**tool call — Bash**
```json
{"command":"git diff main...worktree-agent-ac9381cc42ed979cb"}
```


<details><summary>tool result</summary>

```
diff --git a/src/Relay.Core/Calendar/IWeekCalendar.cs b/src/Relay.Core/Calendar/IWeekCalendar.cs
index 0552f6e..a79a849 100644
--- a/src/Relay.Core/Calendar/IWeekCalendar.cs
+++ b/src/Relay.Core/Calendar/IWeekCalendar.cs
@@ -2,13 +2,9 @@ namespace Relay.Core.Calendar;
 
 public interface IWeekCalendar
 {
-    bool IsWeekStart(DateOnly localDate);
+    WeekWindow Window(DateOnly weekStart, string timeZoneId);
 
     DateOnly WeekContaining(DateTime instantUtc, string timeZoneId);
 
     DateOnly LatestCompleteWeek(DateTime dataAnchorUtc, string timeZoneId);
-
-    WeekWindow Window(DateOnly weekStart, string timeZoneId);
-
-    IReadOnlyList<WeekWindow> BaselineWindows(DateOnly selectedWeek, int baselineWeeks, string timeZoneId);
 }
diff --git a/src/Relay.Core/Calendar/NotAWeekStartException.cs b/src/Relay.Core/Calendar/NotAWeekStartException.cs
deleted file mode 100644
index dd38e2b..0000000
--- a/src/Relay.Core/Calendar/NotAWeekStartException.cs
+++ /dev/null
@@ -1,26 +0,0 @@
-namespace Relay.Core.Calendar;
-
-public sealed class NotAWeekStartException : ArgumentException
-{
-    public NotAWeekStartException()
-    {
-    }
-
-    public NotAWeekStartException(string message)
-        : base(message)
-    {
-    }
-
-    public NotAWeekStartException(string message, Exception innerException)
-        : base(message, innerException)
-    {
-    }
-
-    public NotAWeekStartException(DateOnly localDate, string parameterName)
-        : base($"{localDate:yyyy-MM-dd} is not a M …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"git diff main...worktree-agent-a73b2ea8c833f1f05"}
```


**tool call — Bash**
```json
{"command":"git diff main...governance/calendar"}
```


<details><summary>tool result</summary>

```
diff --git a/docs/acceptance-criteria.md b/docs/acceptance-criteria.md
index a6fc28f..8c54b8d 100644
--- a/docs/acceptance-criteria.md
+++ b/docs/acceptance-criteria.md
@@ -6,12 +6,13 @@ User: a customer admin of one Relay account, on Monday morning, who has to act o
 
 ## How to read this document
 
-**Precedence.** `PLAN.md` §13 has six entries dated 2026-09-28. Five of them apply here:
+**Precedence.** `PLAN.md` §13 has several entries dated 2026-09-28. These apply here:
 - **"Revised design from the four-agent debate (approved by the user)"** overrides §5.1 (index bullet), §5.2, §5.3, the §5.4 status labels and footnote, the §7 Evaluator/Ranking/API bullets, the §7 golden table, D5 and D7.
 - **"Phase 0 decisions and promoted golden values (user decisions)"** adds ten golden scenarios and fixes the API port at 5080.
 - **"Input handling and UI copy decisions (user decisions, validated by the architect)"** decides malformed input, URL normalisation, account switching, the empty and error states, and the additional copy. It is cited below as §13 "Input handling and UI copy".
 - **"Contract decisions from the Phase 0 contract review (user decisions, validated by the architect)"** covers the empty database (`dataAsOf` null), the exact `dataAsOf` string, more malformed-week variants, the capitalised account method line, and more promoted goldens. It is cited below as §13 "Contract decisions".
 - **"Last Phase 0 clarifications (user decisions, validated by the architect)"** cov …[clipped in markdown; full text in raw/]
```
</details>


<details><summary>tool result</summary>

```
diff --git a/PLAN.md b/PLAN.md
index 3db0d3e..fe8906f 100644
--- a/PLAN.md
+++ b/PLAN.md
@@ -485,3 +485,12 @@ Count 14 → `normal` (z 1.98); 15 → `above`; 0 → `below`. (Under the wrong
 - **Empty `?week=`** (present but empty) → 400, like `?type=`. "Latest complete week" is requested by omitting the parameter; the UI never emits `week=`.
 - **Account 20's empty-state page** still shows the full footnote, including "Data as of Mon Jul 27, 2026"; the "Data as of" line is hidden only when `dataAsOf` is null.
 
+### 2026-09-28 — Calendar contract simplified (user decision at contract review, proposed by the architect)
+
+**Reason.** User at contract review: `IWeekCalendar` was *"a little bit overengineered"*. Supersedes the `IWeekCalendar` row in §6.
+- `IWeekCalendar` has three members: `Window(weekStart, timeZoneId)` (DST-correct local Monday → UTC half-open window), `WeekContaining(instantUtc, timeZoneId)`,
+  `LatestCompleteWeek(dataAnchorUtc, timeZoneId)`. The time-zone id stays a string resolved inside the calendar.
+- The Monday check (`InvalidWeek(NotAWeekStart)`) and the list of 8 baseline windows (oldest first, one `Window` per preceding Monday) live in `ActivityHealthService`.
+- Failure contract: an invalid IANA id → `TimeZoneNotFoundException`; a non-Monday `weekStart` passed to `Window` → `ArgumentException`. No custom exception type.
+- Rejected: dropping `LatestCompleteWeek` (moves a named domain rule into the service); a per-time-zone calendar factory (two interfa …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"git merge-tree --write-tree worktree-agent-ac9381cc42ed979cb worktree-agent-a73b2ea8c833f1f05 governance/calendar 2>&1 | head; echo ---; git grep -nE \"IsWeekStart|BaselineWindows|NotAWeekStartException\" main -- . | head -40"}
```


<details><summary>tool result</summary>

```
<persisted-output>
Output too large (479.4KB). Full output saved to: /Users/josemejia/.claude-alt/projects/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/485c1256-5727-457d-8aec-65bc845ee171/tool-results/bqwyaf0v3.txt

Preview (first 2KB):
usage: git merge-tree [--write-tree] [<options>] <branch1> <branch2>
   or: git merge-tree [--trivial-merge] <base-tree> <branch1> <branch2>

    --write-tree          do a real merge instead of a trivial merge
    --trivial-merge       do a trivial merge only
    --[no-]messages       also show informational/conflict messages
    -z                    separate paths with the NUL character
    --name-only           list filenames without modes/oids/stages
    --allow-unrelated-histories
                          allow merging unrelated histories
---
main:ai-log/raw/485c1256-5727-457d-8aec-65bc845ee171.jsonl:674:{"type":"queue-operation","operation":"enqueue","timestamp":"2026-09-28T19:20:39.842Z","sessionId":"485c1256-5727-457d-8aec-65bc845ee171","content":"<task-notification>\n<task-id>a9f256c5a869136f3</task-id>\n<tool-use-id>toolu_01XDBMhqNBc2QEDcwvqchuHJ</tool-use-id>\n<output-file>/private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/485c1256-5727-457d-8aec-65bc845ee171/tasks/a9f256c5a869136f3.output</output-file>\n<status>completed</status>\n<summary>Agent \"Review acceptance criteria\" finished</summary>\n<note>A task-notification fires each time this  …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"git grep -nE \"IsWeekStart|BaselineWindows|NotAWeekStartException\" worktree-agent-ac9381cc42ed979cb -- . | head; echo ---; git grep -nE \"IWeekCalendar|IsWeekStart|NotAWeekStart\" governance/calendar -- PLAN.md .claude | head -30"}
```


<details><summary>tool result</summary>

```
<persisted-output>
Output too large (126.5KB). Full output saved to: /Users/josemejia/.claude-alt/projects/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/485c1256-5727-457d-8aec-65bc845ee171/tool-results/bjdq4pqtg.txt

Preview (first 2KB):
worktree-agent-ac9381cc42ed979cb:ai-log/raw/485c1256-5727-457d-8aec-65bc845ee171.jsonl:674:{"type":"queue-operation","operation":"enqueue","timestamp":"2026-09-28T19:20:39.842Z","sessionId":"485c1256-5727-457d-8aec-65bc845ee171","content":"<task-notification>\n<task-id>a9f256c5a869136f3</task-id>\n<tool-use-id>toolu_01XDBMhqNBc2QEDcwvqchuHJ</tool-use-id>\n<output-file>/private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/485c1256-5727-457d-8aec-65bc845ee171/tasks/a9f256c5a869136f3.output</output-file>\n<status>completed</status>\n<summary>Agent \"Review acceptance criteria\" finished</summary>\n<note>A task-notification fires each time this agent stops with no live background children of its own. The user can send it another message and resume it, so the same task-id may notify more than once.</note>\n<result>## Summary\nThe acceptance criteria are mostly accurate. All GOLDEN/SPEC values I recomputed match PLAN §13, and I found no wrong SEED number. The defects are in classification and ambiguity rather than arithmetic: one copy string is filed as approved but is not in PLAN, several tags overstate what PLAN pins, and some criteria are untestable as written.  …[clipped in markdown; full text in raw/]
```
</details>


### 🤖 ASSISTANT — 2026-09-28T20:11:59.568Z

Need to exclude ai-log.


**tool call — Bash**
```json
{"command":"for b in main worktree-agent-ac9381cc42ed979cb worktree-agent-a73b2ea8c833f1f05 governance/calendar; do echo \"== $b\"; git grep -nE \"IsWeekStart|BaselineWindows|NotAWeekStart|IWeekCalendar\" $b -- . ':!ai-log' | cut -c1-260; done"}
```


<details><summary>tool result</summary>

```
This agent is isolated in the worktree /Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-aab69ccac33797947, but this command names git in a form too complex to verify that it stays inside the worktree. Refusing to run it — a worktree-isolated agent's git operations must target its own worktree. Split it into plain, separate commands and run them from /Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-aab69ccac33797947.
```
</details>


**tool call — Bash**
```json
{"command":"git merge-base main worktree-agent-ac9381cc42ed979cb; git merge-base main worktree-agent-a73b2ea8c833f1f05; git merge-base main governance/calendar; git diff --stat main...governance/calendar | tail -2"}
```


<details><summary>tool result</summary>

```
5895b6be4c8ca5fbc543597b9b6e23661899bea3
5895b6be4c8ca5fbc543597b9b6e23661899bea3
5895b6be4c8ca5fbc543597b9b6e23661899bea3
 PLAN.md | 9 +++++++++
 1 file changed, 9 insertions(+)
```
</details>


**tool call — Bash**
```json
{"command":"git grep -nE \"IsWeekStart|BaselineWindows|NotAWeekStart|IWeekCalendar\" worktree-agent-ac9381cc42ed979cb -- . ':!ai-log' | cut -c1-300"}
```


<details><summary>tool result</summary>

```
worktree-agent-ac9381cc42ed979cb:.claude/agents/architect.md:22:- Interfaces exactly as PLAN §6 (`IWeekCalendar`, `IWeeklyGridBuilder`, `IBaselineEvaluator`, `ILocationRanker`, `IAccountService`, `IActivityHealthService`, `IActivityQueries`, `IAccountQueries`).
worktree-agent-ac9381cc42ed979cb:.claude/agents/backend.md:28:- `WeekCalendar : IWeekCalendar` — IANA zone via `TimeZoneInfo.FindSystemTimeZoneById`; a week is local Monday 00:00 → next Monday 00:00, converted to UTC (half-open);
worktree-agent-ac9381cc42ed979cb:PLAN.md:150:| `IWeekCalendar` | Core | Local Monday ↔ UTC `[start, end)` windows (DST-correct via IANA `TimeZoneInfo`), week containing an instant, latest complete week for an anchor, "is a week start" |
worktree-agent-ac9381cc42ed979cb:src/Relay.Api/Composition/CoreServiceCollectionExtensions.cs:23:        services.AddSingleton<IWeekCalendar, WeekCalendar>();
worktree-agent-ac9381cc42ed979cb:src/Relay.Core/ActivityHealth/ActivityHealthService.cs:10:    IWeekCalendar weekCalendar,
worktree-agent-ac9381cc42ed979cb:src/Relay.Core/ActivityHealth/ActivityHealthService.cs:19:    private readonly IWeekCalendar _weekCalendar = weekCalendar;
worktree-agent-ac9381cc42ed979cb:src/Relay.Core/ActivityHealth/InvalidWeekReason.cs:5:    NotAWeekStart,
worktree-agent-ac9381cc42ed979cb:src/Relay.Core/Calendar/IWeekCalendar.cs:3:public interface IWeekCalendar
worktree-agent-ac9381cc42ed979cb:src/Relay.Core/Calendar/WeekCalendar.cs:3:public sealed class WeekCalendar : IWeekCalend …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"git grep -nE \"IsWeekStart|BaselineWindows|NotAWeekStart|IWeekCalendar\" governance/calendar -- . ':!ai-log' | cut -c1-300"}
```


<details><summary>tool result</summary>

```
governance/calendar:.claude/agents/architect.md:22:- Interfaces exactly as PLAN §6 (`IWeekCalendar`, `IWeeklyGridBuilder`, `IBaselineEvaluator`, `ILocationRanker`, `IAccountService`, `IActivityHealthService`, `IActivityQueries`, `IAccountQueries`).
governance/calendar:.claude/agents/backend.md:28:- `WeekCalendar : IWeekCalendar` — IANA zone via `TimeZoneInfo.FindSystemTimeZoneById`; a week is local Monday 00:00 → next Monday 00:00, converted to UTC (half-open);
governance/calendar:PLAN.md:150:| `IWeekCalendar` | Core | Local Monday ↔ UTC `[start, end)` windows (DST-correct via IANA `TimeZoneInfo`), week containing an instant, latest complete week for an anchor, "is a week start" |
governance/calendar:PLAN.md:490:**Reason.** User at contract review: `IWeekCalendar` was *"a little bit overengineered"*. Supersedes the `IWeekCalendar` row in §6.
governance/calendar:PLAN.md:491:- `IWeekCalendar` has three members: `Window(weekStart, timeZoneId)` (DST-correct local Monday → UTC half-open window), `WeekContaining(instantUtc, timeZoneId)`,
governance/calendar:PLAN.md:493:- The Monday check (`InvalidWeek(NotAWeekStart)`) and the list of 8 baseline windows (oldest first, one `Window` per preceding Monday) live in `ActivityHealthService`.
governance/calendar:src/Relay.Api/Composition/CoreServiceCollectionExtensions.cs:23:        services.AddSingleton<IWeekCalendar, WeekCalendar>();
governance/calendar:src/Relay.Core/ActivityHealth/ActivityHealthService.cs:10:    IWeekCalendar weekCalend …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"git grep -n -iE \"calendar|Monday|DST|Phoenix\" worktree-agent-ac9381cc42ed979cb -- PLAN.md | sed -n 1,60p | cut -c1-330"}
```


<details><summary>tool result</summary>

```
worktree-agent-ac9381cc42ed979cb:PLAN.md:11:**Who:** a customer admin of one Relay account (single- or multi-location), Monday morning.
worktree-agent-ac9381cc42ed979cb:PLAN.md:30:| Data ends **Monday 2026-07-27**; system clock is Sep 2026 | Anchor "now" to the data, not the clock (D1). Default week = Jul 20–26 |
worktree-agent-ac9381cc42ed979cb:PLAN.md:60:| Does "this week" mean the current partial week? | No — the last complete week; the partial one would read as a collapse every Monday |
worktree-agent-ac9381cc42ed979cb:PLAN.md:61:| Week start day | Monday (ISO), in the account's IANA timezone |
worktree-agent-ac9381cc42ed979cb:PLAN.md:83:| `week` | Optional local Monday. Default = latest complete week. Not a Monday → 400. After latest complete week → 400 |
worktree-agent-ac9381cc42ed979cb:PLAN.md:121:- Single route `/dashboard?account=12&week=2026-07-20&type=all`. **URL is the source of truth**: `DashboardState` maps query params → signals and writes changes via
worktree-agent-ac9381cc42ed979cb:PLAN.md:150:| `IWeekCalendar` | Core | Local Monday ↔ UTC `[start, end)` windows (DST-correct via IANA `TimeZoneInfo`), week containing an instant, latest complete week for an anchor, "is a week start" |
worktree-agent-ac9381cc42ed979cb:PLAN.md:173:| 5 Frontend | `web` (Vitest) | `DashboardState` URL round-trip + normalisation; `LocationTable`/`AccountSummary` states |
worktree-agent-ac9381cc42ed979cb:PLAN.md:176:- Calendar: DST start week (Mar 8 2026) and end week (Nov 1 2026) in  …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"git show worktree-agent-ac9381cc42ed979cb:src/Relay.Core/ActivityHealth/ActivityHealthService.cs; git show worktree-agent-ac9381cc42ed979cb:src/Relay.Core/Calendar/WeekWindow.cs; git ls-tree -r --name-only worktree-agent-ac9381cc42ed979cb | grep -i -E \"test|Calendar\""}
```


<details><summary>tool result</summary>

```
using Relay.Core.Calendar;
using Relay.Core.Normality;
using Relay.Core.Queries;

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
    private readonly IAccountQueries _accountQueries = accountQueries;
    private readonly IActivityQueries _activityQueries = activityQueries;
    private readonly IWeekCalendar _weekCalendar = weekCalendar;
    private readonly IWeeklyGridBuilder _weeklyGridBuilder = weeklyGridBuilder;
    private readonly IBaselineEvaluator _baselineEvaluator = baselineEvaluator;
    private readonly ILocationRanker _locationRanker = locationRanker;
    private readonly NormalityOptions _normalityOptions = normalityOptions;
    private readonly TimeProvider _timeProvider = timeProvider;

    public Task<ActivityHealthResult> GetAsync(ActivityHealthQuery query, CancellationToken cancellationToken) =>
        throw new NotImplementedException();
}
namespace Relay.Core.Calendar;

public sealed record WeekWindow(DateOnly WeekStart, DateTime StartUtc, DateTime EndUtc);
.claude/agents/test-author.md
docs/battle-test/README.md
docs/battle-test/independent-implementation-ambiguities.md
docs/battle-test/industry-survey.m …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"git grep -n -iE \"week.?start|baseline windows|isweek\" worktree-agent-a73b2ea8c833f1f05 -- docs/acceptance-criteria.md docs/design-consensus.md .claude | cut -c1-300"}
```


<details><summary>tool result</summary>

```
worktree-agent-a73b2ea8c833f1f05:docs/acceptance-criteria.md:15:- **"Calendar contract simplified"** reduces `IWeekCalendar` to `Window`, `WeekContaining` and `LatestCompleteWeek`; the Monday check and the 8 baseline windows move into `ActivityHealthService`. It is cited below as §13 "Calendar contr
worktree-agent-a73b2ea8c833f1f05:docs/acceptance-criteria.md:61:| C-17 | Week label | `Mon Jul 20 – Sun Jul 26, 2026` (from `week.start`/`week.end`) | SPEC §13 "Input handling and UI copy" |
worktree-agent-a73b2ea8c833f1f05:docs/acceptance-criteria.md:136:| BL-41h | `Window` for week start 2026-07-21 (a Tuesday), America/New_York | Throws `ArgumentException`. The user-facing Monday rule is BL-43 (service → `InvalidWeek`, `NotAWeekStart`) | SPEC §7 Calendar; SPEC §13 "Calendar contract s
worktree-agent-a73b2ea8c833f1f05:docs/acceptance-criteria.md:141:| BL-46 | Service, account in America/Chicago, W = 2026-03-16 | The service requests counts for W and for the 8 preceding Mondays as baseline (2026-01-19, 01-26, 02-02, 02-09, 02-16, 02-23, 03-02, 03-09), baseline windows oldest first,
worktree-agent-a73b2ea8c833f1f05:docs/acceptance-criteria.md:195:| API-01b | `GET …/accounts/14/activity-health` (JSON names) | Top level exactly: `account{id,name,timezone}`, `eventType`, `week{start,end}`, `dataAsOf` (a string, or null only for an empty database, API-32), `latestCompleteWeek`, `ea
worktree-agent-a73b2ea8c833f1f05:docs/acceptance-criteria.md:198:| API-10 | Default: `GET $API/api/accoun …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"git show worktree-agent-ac9381cc42ed979cb:.claude/agents/backend.md | sed -n 24,34p; git show worktree-agent-ac9381cc42ed979cb:.claude/agents/test-author.md | grep -n -i -E \"calendar|monday\""}
```


<details><summary>tool result</summary>

```
- `Program.cs`: minimal host that builds and runs (the architect adds the `AddRelay…` calls with the contracts).
- Done: `dotnet build` 0 warnings, `dotnet test` runs (0 tests).

# Relay.Core — what you implement
- `WeekCalendar : IWeekCalendar` — IANA zone via `TimeZoneInfo.FindSystemTimeZoneById`; a week is local Monday 00:00 → next Monday 00:00, converted to UTC (half-open);
  handles DST (23 h / 25 h days) by converting each boundary, never by adding 7×24 h. Latest complete week = the latest week whose UTC end ≤ the data anchor.
- `WeeklyGridBuilder : IWeeklyGridBuilder` — zero-fills every (site, week); marks eligibility per PLAN §5.3; excludes sites whose first activity is after the selected week.
- `BaselineEvaluator : IBaselineEvaluator` — PLAN §5.3 exactly, constants only from the injected `NormalityOptions` (plain data; bound and validated by `AddRelayCore` in `Relay.Api/Composition` — PLAN §13). Keep each step a small, named private method
  (`MedianOf`, `SpreadOf`, `StatusFor`, `RangeFor`) so the code reads like the plan.
- `LocationRanker : ILocationRanker` — PLAN §5.3 ranking; stable, culture-invariant name ordering (`StringComparer.Ordinal`).
- `AccountService : IAccountService` — lists accounts through `IAccountQueries`.
24:| Core service | `tests/Relay.Core.Tests` | `ActivityHealthService` with **real** calendar/grid/evaluator/ranker and hand-written fakes of `IActivityQueries`/`IAccountQueries` |
```
</details>


### 🤖 ASSISTANT — 2026-09-28T20:12:25.571Z

## Summary
The three branches are consistent with each other and with PLAN §13. No merge conflicts, no dangling code references, and every PLAN §7 calendar edge case is still testable. The one stale reference is a PLAN §6 row. Verdict: **APPROVED**, with one small objective fix and one optional user decision.

## Changed
Nothing. This was a read-only review.

## Verification
- **Diffs.** I ran `git diff main...<branch>` for all three branches.
- **Merge check.** All three branch from `5895b6b`. `governance/calendar` touches only `PLAN.md`, the acceptance-criteria branch touches only `docs/acceptance-criteria.md`, and the contract branch touches only `src/Relay.Core/Calendar/*`. The file sets are disjoint, so a merge cannot conflict. I did not run a build; the contract change is three stub members and a deleted file.
- **Dangling references.** I ran `git grep -nE "IsWeekStart|BaselineWindows|NotAWeekStart|IWeekCalendar"` on the contract branch, excluding `ai-log`.
  - The removed members no longer appear in `src`, `.claude/agents`, `web` or `tests`.
  - `NotAWeekStartException` has no remaining reference.
  - `IWeekCalendar` is still used in `ActivityHealthService`, the DI registration, `backend.md` and `architect.md`. Those uses do not name the removed members.
  - On `governance/calendar` alone, the old members still appear in `src`. That is expected because the contract change is a separate branch.
- **`InvalidWeekReason.NotAWeekStart`.** It is still present at `src/Relay.Core/ActivityHealth/InvalidWeekReason.cs:5`, so BL-43 is intact.
- **BL-46 dates.**
  - The 8 Mondays before 2026-03-16, oldest first, are 01-19, 01-26, 02-02, 02-09, 02-16, 02-23, 03-02, 03-09. This matches the row.
  - DST began on Sunday 2026-03-08. Monday 03-02 is CST, so its window starts at 06:00Z. Monday 03-09 is CDT, so its window ends at 05:00Z.
  - 7 × 24 − 1 = 167 h. The window is [2026-03-02T06:00Z, 2026-03-09T05:00Z). This matches the row.
- **PLAN §7 calendar edge cases with the three members.**
  - DST start and end weeks in Chicago (167 h and 169 h), Phoenix and UTC: `Window`.
  - Event exactly at a boundary: `WeekContaining`, BL-41e.
  - Latest complete week with anchor on Monday, on Sunday 23:59:59 local, and at exactly Monday 00:00 local: `LatestCompleteWeek`, BL-41f.
  - Invalid IANA id: BL-41g, any member throws `TimeZoneNotFoundException`.
  - Non-Monday week: `Window` throws `ArgumentException` (BL-41h), and the service returns `InvalidWeek` (BL-43).

## Findings
(a) Objective
1. **Minor.** `PLAN.md:150` (§6 `IWeekCalendar` row) still lists "is a week start". The §13 entry says it supersedes that row, so nothing is wrong, but the row reads as current to a reader of §6 alone. Fix it in the same governance commit, or accept the supersession note.
2. **Nit.** BL-41h says "Throws `ArgumentException`" without saying whether subclasses count. The test-author should use `ThrowsAny<ArgumentException>`. An exact-type `Throws` would fail on an `ArgumentOutOfRangeException` for no good reason.

(b) User decision
1. **Optional.** BL-46 and PLAN §13 hard-code "8 baseline windows". The value should come from `NormalityOptions.BaselineWeeks`, and the test should read the same option. This is only a note for the test-author, not a change to the docs.

## Deviations
None.

## Uncertain
- I did not build or run tests. There are no calendar tests yet, and the stubs are trivially valid.

## Next
- Merge order does not matter.
- The test-author can now write the `WeekCalendar` red suite and BL-46 against the three members. BL-46 needs the real calendar and a fake `IActivityQueries` that records the requested windows.

