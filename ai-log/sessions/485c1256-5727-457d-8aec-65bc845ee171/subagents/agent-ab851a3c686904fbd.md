
### 🧑 USER — 2026-09-28T23:53:58.359Z

The user approved your proposal. Add one Core test, `GetAsyncWeeklyCountAndSiteRowOrderDoesNotChangeTheReport`, to `tests/Relay.Core.Tests/ActivityHealth/ActivityHealthServiceTests.cs`. The architect is validating this in parallel, and I will discard the branch if it objects.

**Setup:** in your worktree, run `git checkout -B tests-row-order-invariance backend-infra-design-cleanup`.

**Test design:**
- Use a scenario with several sites and weeks in which the ranking and baselines matter. At least one site should be flagged, and at least two should tie on name order or deviation, so an order-dependent implementation would give a different report.
- Build the report once with the rows in natural order. Build it again with the fake returning the weekly-count and site-first-activity rows reversed.
- Assert the two reports are equal. Compare structurally, the same way the other tests in that file do.
- Expected outcomes come from PLAN, not from running the implementation. In this test they amount to equality.
- Add the test-only reversal option to `FakeActivityQueries`, or wherever the fake lives. Test files only; no contract or implementation changes.

**Prove the test can fail:**
- Temporarily inject an order dependence into `ActivityHealthService` or the grid builder. For example, take the first row's site as the ranking tiebreak, or rely on input order where the service sorts.
- Show the test goes red, then revert the injection.
- Paste the red output. Do not commit the injection.

**Verification:**
- `dotnet test --project tests/Relay.Core.Tests` passes.
- Full `dotnet test` passes, 403 tests. Docker is required.
- Test name in PascalCase, no comment blocks.
- Commit messages end with `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.

Reply with the CLAUDE.md handoff report and include your branch name.


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-ab851a3c686904fbd\" && git checkout -B tests-row-order-invariance backend-infra-design-cleanup && git log --oneline -3 && ls tests/Relay.Core.Tests tests/Relay.Core.Tests/ActivityHealth && wc -l tests/Relay.Core.Tests/ActivityHealth/*.cs","description":"Create branch and list test files"}
```


<details><summary>tool result</summary>

```
Switched to a new branch 'tests-row-order-invariance'
d2e12ae Drop EF Core Design package from Relay.Infrastructure; EF tools use Relay.Api as startup project
860fd64 Remove RelayDesignTimeDbContextFactory; dotnet ef resolves RelayDbContext from the Relay.Api host, which loads .env in Development (PLAN §13)
3c3dce0 Load repo-root .env as a Development configuration source via DotNetEnv; Api references EF Design (private); migrator message points at .env
tests/Relay.Core.Tests:
Accounts
ActivityHealth
Calendar
Normality
Relay.Core.Tests.csproj
TestDoubles

tests/Relay.Core.Tests/ActivityHealth:
ActivityHealthServiceTests.cs
     418 tests/Relay.Core.Tests/ActivityHealth/ActivityHealthServiceTests.cs
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-ab851a3c686904fbd\" && cat tests/Relay.Core.Tests/ActivityHealth/ActivityHealthServiceTests.cs; ls tests/Relay.Core.Tests/TestDoubles; cat tests/Relay.Core.Tests/TestDoubles/*.cs","description":"Read service tests and fakes"}
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
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-ab851a3c686904fbd\" && ls -R src/Relay.Core | head -60; cat src/Relay.Core/ActivityHealth/*.cs src/Relay.Core/Normality/LocationRanker.cs src/Relay.Core/Normality/WeeklyGridBuilder.cs 2>/dev/null; grep -n -i \"rank\\|tiebreak\\|tie\" PLAN.md | head -40","description":"Inspect Core sources and PLAN ranking rules"}
```


<details><summary>tool result</summary>

```
Accounts
ActivityHealth
Calendar
Normality
Queries
Relay.Core.csproj

src/Relay.Core/Accounts:
AccountService.cs
IAccountService.cs

src/Relay.Core/ActivityHealth:
ActivityHealthReport.cs
ActivityHealthResult.cs
ActivityHealthService.cs
IActivityHealthService.cs
InvalidWeekReason.cs

src/Relay.Core/Calendar:
IWeekCalendar.cs
WeekCalendar.cs
WeekLength.cs
WeekRange.cs
WeekWindow.cs

src/Relay.Core/Normality:
Baseline.cs
BaselineEvaluator.cs
HealthStatus.cs
IBaselineEvaluator.cs
ILocationRanker.cs
IWeeklyGridBuilder.cs
LocationHealth.cs
LocationRanker.cs
LocationSeries.cs
NormalityOptions.cs
SeriesHealth.cs
WeeklyGridBuilder.cs
WeeklySeries.cs

src/Relay.Core/Queries:
AccountListItem.cs
ActivityType.cs
IAccountQueries.cs
IActivityQueries.cs
SiteFirstActivity.cs
SiteFirstActivityExtensions.cs
WeeklySiteCount.cs
using Relay.Core.Calendar;
using Relay.Core.Normality;
using Relay.Core.Queries;

namespace Relay.Core.ActivityHealth;

public sealed record ActivityHealthReport(
    AccountListItem Account,
    ActivityType EventType,
    WeekRange Week,
    DateTime? DataAsOf,
    DateOnly LatestCompleteWeek,
    DateOnly EarliestWeek,
    int BaselineWeeks,
    int MinimumEligibleWeeks,
    SeriesHealth Summary,
    IReadOnlyList<LocationHealth> Locations);
namespace Relay.Core.ActivityHealth;

public abstract record ActivityHealthResult
{
    private ActivityHealthResult()
    {
    }

    public sealed record Found(ActivityHealthReport Report) : ActivityHealthResult;

    public sea …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-ab851a3c686904fbd\" && sed -n 95,120p PLAN.md; echo ----; sed -n 315,345p PLAN.md; echo ---; sed -n 395,410p PLAN.md; cat src/Relay.Core/Normality/BaselineEvaluator.cs src/Relay.Core/Normality/LocationHealth.cs src/Relay.Core/Normality/SeriesHealth.cs src/Relay.Core/Normality/Baseline.cs","description":"Read PLAN rules and evaluator"}
```


<details><summary>tool result</summary>

```
  "earliestWeek": "2026-01-26",
  "baselineWeeks": 8,
  "summary": { "count": 87, "baseline": { "median": 72.5, "low": 24, "high": 121, "weeksUsed": 8 }, "status": "normal", "deviation": 0.59 },
  "locations": [ { "location": "Site M", "count": 7, "baseline": { … }, "status": "normal", "deviation": 1.57 } ]
}
```
`status ∈ above | below | normal | insufficient_data`. `baseline` is `null` when insufficient. `locations` is returned sorted (§5.3).
Empty account → 200, `summary.count = 0`, `insufficient_data`, `locations: []`.

### 5.3 Normality rules (the product logic — pure C#)
For the account total and for each site, for selected week `W`:
1. **Sites** = distinct locations whose first event is before the end of `W`.
2. **Baseline weeks** = the 8 local weeks before `W`, **zero-filled**. A week is *eligible* only if it starts **after** the week
   containing that series' first event (site → site's first event; account total → account's first event).
3. Fewer than **4** eligible weeks → `insufficient_data` (count still shown).
4. `median` = median of eligible weeks (mean of the middle two when even).
5. `spread` = max(1.4826 × MAD, √max(median, 1)). The √ term is the Poisson noise floor so a steady or quiet series never gets a zero-width band.
6. Band: `below` iff count < median − 2·spread; `above` iff count > median + 2·spread; else `normal`.
   Displayed integer range `low = max(0, ⌈median − 2·spread⌉)`, `high = ⌊median + 2·spread⌋` (equivalent to the rule for integer counts). …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-ab851a3c686904fbd\" && python3 - <<'EOF'\np='tests/Relay.Core.Tests/TestDoubles/FakeActivityQueries.cs'\ns=open(p).read()\ns=s.replace(\"\"\"    private DateTime? _dataAnchorUtc;\n\"\"\",\"\"\"    private DateTime? _dataAnchorUtc;\n    private bool _rowsReversed;\n\"\"\")\ns=s.replace(\"\"\"    public FakeActivityQueries WithSite(\"\"\",\"\"\"    public FakeActivityQueries WithRowsReversed()\n    {\n        _rowsReversed = true;\n        return this;\n    }\n\n    public FakeActivityQueries WithSite(\"\"\")\ns=s.replace(\"\"\"            _siteFirstActivities.Where(entry => entry.AccountId == accountId).Select(entry => entry.SiteFirstActivity).ToList());\"\"\",\"\"\"            InReturnOrder(_siteFirstActivities.Where(entry => entry.AccountId == accountId).Select(entry => entry.SiteFirstActivity)));\"\"\")\ns=s.replace(\"\"\"        IReadOnlyList<WeeklySiteCount> matchingCounts = _weeklySiteCounts\n            .Where(entry => entry.AccountId == accountId\n                && entry.EventType == eventType\n                && requestedWeekStarts.Contains(entry.WeeklySiteCount.WeekStart))\n            .Select(entry => entry.WeeklySiteCount)\n            .ToList();\n        return Task.FromResult(matchingCounts);\n    }\n\"\"\",\"\"\"        var matchingCounts = _weeklySiteCounts\n            .Where(entry => entry.AccountId == accountId\n                && entry.E …[clipped in markdown; full text in raw/]
```


<details><summary>tool result</summary>

```
 .../ActivityHealth/ActivityHealthServiceTests.cs   | 32 ++++++++++++++++++++++
 .../TestDoubles/FakeActivityQueries.cs             | 19 +++++++++----
 2 files changed, 46 insertions(+), 5 deletions(-)
/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-ab851a3c686904fbd/tests/Relay.Core.Tests/TestDoubles/FakeActivityQueries.cs(79,33): error CA1859: Change return type of method 'InReturnOrder' from 'System.Collections.Generic.IReadOnlyList<TRow>' to 'System.Collections.Generic.List<TRow>' for improved performance (https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1859) [/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-ab851a3c686904fbd/tests/Relay.Core.Tests/Relay.Core.Tests.csproj]
Get projects properties with MSBuild didn't execute properly with exit code: 1.
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-ab851a3c686904fbd\" && f=tests/Relay.Core.Tests/TestDoubles/FakeActivityQueries.cs && sed -i '' 's/private IReadOnlyList<TRow> InReturnOrder/private List<TRow> InReturnOrder/; s/return Task.FromResult(InReturnOrder(matchingCounts));/return Task.FromResult<IReadOnlyList<WeeklySiteCount>>(InReturnOrder(matchingCounts));/' $f && dotnet test --project tests/Relay.Core.Tests 2>&1 | tail -5","description":"Fix analyzer error and rerun Core tests"}
```


<details><summary>tool result</summary>

```
This agent is isolated in the worktree /Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-ab851a3c686904fbd, but this command runs sed with a value computed at runtime (the variable f) where an option may stand (a value that is not double-quoted, or whose first character is matched or computed rather than spelled out, may begin with -; put -- before it) inside a construct too complex to verify, so what it runs cannot be shown not to be git. Refusing to run it — a worktree-isolated agent's git operations must target its own worktree. Split it into plain, separate commands and run them from /Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-ab851a3c686904fbd.
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-ab851a3c686904fbd\" && sed -i '' 's/private IReadOnlyList<TRow> InReturnOrder/private List<TRow> InReturnOrder/; s/return Task.FromResult(InReturnOrder(matchingCounts));/return Task.FromResult<IReadOnlyList<WeeklySiteCount>>(InReturnOrder(matchingCounts));/' \"tests/Relay.Core.Tests/TestDoubles/FakeActivityQueries.cs\"","description":"Fix analyzer error in fake"}
```


<details><summary>tool result</summary>

```
(Bash completed with no output)
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-ab851a3c686904fbd\" && dotnet test --project tests/Relay.Core.Tests 2>&1 | tail -5","description":"Run Core tests"}
```


<details><summary>tool result</summary>

```
  total: 136
  failed: 0
  succeeded: 136
  skipped: 0
  duration: 661ms
```
</details>


### 🤖 ASSISTANT — 2026-09-28T23:54:55.940Z

Now the failure proof: I'll temporarily remove the ranker's name tiebreak.


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-ab851a3c686904fbd\" && sed -i '' '/ThenBy(location => location.Location, StringComparer.Ordinal)/d' src/Relay.Core/Normality/LocationRanker.cs && git diff src && dotnet test --project tests/Relay.Core.Tests 2>&1 | grep -v \"^\\s*$\" | grep -A25 -i \"failed Relay\\|error\" | head -40; dotnet test --project tests/Relay.Core.Tests 2>&1 | tail -5","description":"Inject order dependence and run tests"}
```


<details><summary>tool result</summary>

```
diff --git a/src/Relay.Core/Normality/LocationRanker.cs b/src/Relay.Core/Normality/LocationRanker.cs
index 6026af8..f288193 100644
--- a/src/Relay.Core/Normality/LocationRanker.cs
+++ b/src/Relay.Core/Normality/LocationRanker.cs
@@ -14,7 +14,6 @@ public sealed class LocationRanker : ILocationRanker
             .OrderBy(location => RankGroupOf(location.Status))
             .ThenByDescending(location => Math.Abs(location.Deviation ?? 0.0))
             .ThenBy(location => location.Status == HealthStatus.Below ? 0 : 1)
-            .ThenBy(location => location.Location, StringComparer.Ordinal)
             .ToList();
 
     private static RankGroup RankGroupOf(HealthStatus status) => status switch
failed Relay.Core.Tests.Normality.LocationRankerTests.RankAccount14AppointmentTiesAtZeroDeviationAreOrderedByName (91ms)
  from /Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-ab851a3c686904fbd/tests/Relay.Core.Tests/bin/Debug/net10.0/Relay.Core.Tests.dll (net10.0|arm64)
  Shouldly.ShouldAssertException : rankedNames
      should be
  ["Site A", "Site B", "Site C", "Site D"]
      but was (case sensitive comparison)
  ["Site A", "Site D", "Site C", "Site B"]
      difference
  ["Site A", *"Site D"*, "Site C", *"Site B"*]
    at Relay.Core.Tests.Normality.LocationRankerTests.RankAccount14AppointmentTiesAtZeroDeviationAreOrderedByName() in /Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/work …[clipped in markdown; full text in raw/]
```
</details>


### 🤖 ASSISTANT — 2026-09-28T23:55:09.389Z

The injection makes the new test fail. Two existing ranker tests fail too, as they should. Reverting the injection now.


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-ab851a3c686904fbd\" && git checkout -- src/Relay.Core/Normality/LocationRanker.cs && git status --short && dotnet test --project tests/Relay.Core.Tests 2>&1 | tail -5","description":"Revert injection and rerun Core tests"}
```


<details><summary>tool result</summary>

```
 M tests/Relay.Core.Tests/ActivityHealth/ActivityHealthServiceTests.cs
 M tests/Relay.Core.Tests/TestDoubles/FakeActivityQueries.cs
  total: 136
  failed: 0
  succeeded: 136
  skipped: 0
  duration: 933ms
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-ab851a3c686904fbd\" && sed -i '' 's/            .ThenBy(location => location.Location, StringComparer.Ordinal)/            .ThenBy(location => location.Location == locations[^1].Location ? 0 : 1)\\n            .ThenBy(location => location.Location, StringComparer.Ordinal)/' src/Relay.Core/Normality/LocationRanker.cs && git diff src && dotnet test --project tests/Relay.Core.Tests 2>&1 | grep -B2 -A16 \"^failed\"; dotnet test --project tests/Relay.Core.Tests 2>&1 | tail -6","description":"Inject input-order tiebreak and run tests"}
```


<details><summary>tool result</summary>

```
diff --git a/src/Relay.Core/Normality/LocationRanker.cs b/src/Relay.Core/Normality/LocationRanker.cs
index 6026af8..9dbfc5b 100644
--- a/src/Relay.Core/Normality/LocationRanker.cs
+++ b/src/Relay.Core/Normality/LocationRanker.cs
@@ -14,6 +14,7 @@ public sealed class LocationRanker : ILocationRanker
             .OrderBy(location => RankGroupOf(location.Status))
             .ThenByDescending(location => Math.Abs(location.Deviation ?? 0.0))
             .ThenBy(location => location.Status == HealthStatus.Below ? 0 : 1)
+            .ThenBy(location => location.Location == locations[^1].Location ? 0 : 1)
             .ThenBy(location => location.Location, StringComparer.Ordinal)
             .ToList();
 
Running tests from /Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-ab851a3c686904fbd/tests/Relay.Core.Tests/bin/Debug/net10.0/Relay.Core.Tests.dll (net10.0|arm64)
failed Relay.Core.Tests.ActivityHealth.ActivityHealthServiceTests.GetAsyncWeeklyCountAndSiteRowOrderDoesNotChangeTheReport (37ms)
  from /Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-ab851a3c686904fbd/tests/Relay.Core.Tests/bin/Debug/net10.0/Relay.Core.Tests.dll (net10.0|arm64)
  Shouldly.ShouldAssertException : reversedOrderReport.Locations
      should be
  [LocationHealth { Count = 0, Baseline = Baseline { WeeksUsed = 8, Median = 3, Low = 1, High = 7 }, Status = Below, Deviation = -2.4494897427831783, L …[clipped in markdown; full text in raw/]
```
</details>


### 🤖 ASSISTANT — 2026-09-28T23:55:29.543Z

The equality assertion (line 447) catches it. Reverting, then running the full suite.


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-ab851a3c686904fbd\" && git checkout -- src/Relay.Core/Normality/LocationRanker.cs && git status --short && docker ps --format '{{.Names}}' | head -3; dotnet test 2>&1 | grep -E \"total:|failed:|succeeded:|skipped:|Relay\\..*Tests.dll|exit code\" ","description":"Revert injection and run full test suite","timeout":600000}
```


<details><summary>tool result</summary>

```
 M tests/Relay.Core.Tests/ActivityHealth/ActivityHealthServiceTests.cs
 M tests/Relay.Core.Tests/TestDoubles/FakeActivityQueries.cs
relay-activity-health-db-1
taxguardian-e2e-sql
taxguardian-e2e-azurite
Running tests from /Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-ab851a3c686904fbd/tests/Relay.Core.Tests/bin/Debug/net10.0/Relay.Core.Tests.dll (net10.0|arm64)
Running tests from /Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-ab851a3c686904fbd/tests/Relay.Api.Tests/bin/Debug/net10.0/Relay.Api.Tests.dll (net10.0|arm64)
/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-ab851a3c686904fbd/tests/Relay.Core.Tests/bin/Debug/net10.0/Relay.Core.Tests.dll (net10.0|arm64) passed (264ms)
Running tests from /Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-ab851a3c686904fbd/tests/Relay.Infrastructure.Tests/bin/Debug/net10.0/Relay.Infrastructure.Tests.dll (net10.0|arm64)
/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-ab851a3c686904fbd/tests/Relay.Infrastructure.Tests/bin/Debug/net10.0/Relay.Infrastructure.Tests.dll (net10.0|arm64) passed (13s 888ms)
/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-ab851a3c686904fbd/tests/Relay.Api.Tests/bin/Debug/net10.0/Relay. …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-ab851a3c686904fbd\" && git add tests/Relay.Core.Tests/ActivityHealth/ActivityHealthServiceTests.cs tests/Relay.Core.Tests/TestDoubles/FakeActivityQueries.cs && git commit -q -m \"Add service test: weekly-count and site row order does not change the report\n\nFakeActivityQueries gains WithRowsReversed() so the same scenario can be\nserved in reversed row order; the report must be structurally equal.\n\nCo-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>\" && git log --oneline -2 && git status --short","description":"Commit the test and fake change"}
```


<details><summary>tool result</summary>

```
0fdd91e Add service test: weekly-count and site row order does not change the report
d2e12ae Drop EF Core Design package from Relay.Infrastructure; EF tools use Relay.Api as startup project
```
</details>


### 🤖 ASSISTANT — 2026-09-28T23:56:20.767Z

## Summary
I added `GetAsyncWeeklyCountAndSiteRowOrderDoesNotChangeTheReport` on branch `tests-row-order-invariance` (commit `0fdd91e`, based on `backend-infra-design-cleanup`). The test builds the report twice, once with the rows in natural order and once with the fake returning them reversed, and requires the two reports to be equal. It passes. Two temporary faults injected into the ranker each turned it red, and both were reverted before committing.

## Changed
- `/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-ab851a3c686904fbd/tests/Relay.Core.Tests/ActivityHealth/ActivityHealthServiceTests.cs`: the new test.
- `/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-ab851a3c686904fbd/tests/Relay.Core.Tests/TestDoubles/FakeActivityQueries.cs`: a test-only `WithRowsReversed()` option. When set, `ListSiteFirstActivitiesAsync` and `CountWeeklyBySiteAsync` return their rows in reverse insertion order. Nothing under `src/` changed.

**Scenario (UTC account 18, default week 2026-07-20, eight baseline weeks from 2026-05-25).** Sites are inserted in the order G, F, B, A, D, C. Expected results, worked out by hand from PLAN §13 §5.3:
- **C and D:** baseline 3 in all eight weeks, selected week 0. This gives range 1–7 and `below` (the §7 case [3,3,3,3]→0), with exactly the same deviation for both. So there is a flagged pair tied on deviation.
- **A and B:** baseline 10 in all eight weeks, selected week 10. Both are `normal` with deviation 0, so they tie too.
- **F and G:** first activity in the week of 2026-06-29, so only 2 weeks are eligible. Both are `insufficient_data` and are ordered by name only.
- **Expected ranking:** C, D, A, B, F, G. The test asserts this once as a check that the scenario really has these ties.

**Comparison.** It is structural, in two parts, as the architect's note 1 asks:
- `reversedOrderReport.Locations.ShouldBe(naturalOrderReport.Locations)` compares the locations element by element.
- `(reversedOrderReport with { Locations = naturalOrderReport.Locations }).ShouldBe(naturalOrderReport)` compares every other field.

## Verification
- **Injection 1:** removed `.ThenBy(location => location.Location, StringComparer.Ordinal)` from `LocationRanker`. 9 failed out of 136. The new test failed:
```
failed ...GetAsyncWeeklyCountAndSiteRowOrderDoesNotChangeTheReport
  naturalOrderReport.Locations.Select(location => location.Location) should be
  ["Site C", "Site D", "Site A", "Site B", "Site F", "Site G"]
  but was ["Site D", "Site C", "Site B", "Site A", "Site G", "Site F"]
```
- **Injection 2:** this one targets the equality assertion itself. I added `.ThenBy(location => location.Location == locations[^1].Location ? 0 : 1)` before the name tiebreak, so the last input row wins ties. Natural order still ranks correctly; reversed order does not. 3 failed out of 136, and the new test failed at line 447 on the equality assertion:
```
Shouldly.ShouldAssertException : reversedOrderReport.Locations should be
  [... Site B ..., LocationHealth { ... InsufficientData ..., Location = Site F }, LocationHealth { ... Location = Site G }]
  but was
  [... Site B ..., *LocationHealth { ... Location = Site G }*, *LocationHealth { ... Location = Site F }*]
```
- **Revert:** both injections were undone with `git checkout -- src/Relay.Core/Normality/LocationRanker.cs`. `git status` showed only the two test files as changed.
- `dotnet test --project tests/Relay.Core.Tests`: total 136, failed 0, succeeded 136.
- Full `dotnet test` (Docker up): Core, Infrastructure and Api all passed; total 403, failed 0, succeeded 403, skipped 0.

## Deviations
- **Scenario choice (architect note 2).** I did not use Account 14 `appointment_set`. Core tests use hand-built fakes, not the seed, so matching that golden would mean inventing baselines to hit dev −1.12 / 0–4. My scenario has order-sensitive ties in all three ranking groups: two flagged sites with equal deviation, two normal sites at deviation 0, and two insufficient sites. The Account 14 golden has ties only in the normal group, so it is not the "Storage-only A/B" case the architect called too weak. If the architect still wants Account 14, that ordering check belongs in the API golden suite, which runs against the seed.
- **The extra ranking assertion.** Besides the equality, I assert the natural-order ranking (C, D, A, B, F, G). The expected order comes from PLAN, not from running the code, and it proves the ties exist. Injection 2 shows that the equality assertion catches order dependence on its own.

## Uncertain
- **Rationale for the test (architect note 3).** It should rest on CLAUDE.md rule 7 (SQL only counts, with no ordering) and on the contract not specifying row order. I did not cite any §13 line about activity-query order. There are no comments in the code (rule 2).

## Next
- The coordinator can merge `tests-row-order-invariance` (commit `0fdd91e`). Main at `38a8578` already contains the base `d2e12ae`, so no rebase is needed.
- `WithRowsReversed()` can be reused in other service tests that need order-invariance checks.

**Traceability**

| PLAN item | Test |
|---|---|
| §13 §5.3 step 9 ranking ties (below before above, then name; insufficient by name) under any input order | `GetAsyncWeeklyCountAndSiteRowOrderDoesNotChangeTheReport` |
| CLAUDE.md rule 7: SQL returns unordered counts; Core must not depend on row order | same |

**Proposed additions:** none.

