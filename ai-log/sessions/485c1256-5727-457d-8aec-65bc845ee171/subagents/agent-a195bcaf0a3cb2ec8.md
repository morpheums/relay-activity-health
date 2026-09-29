
### 🧑 USER — 2026-09-29T00:14:51.332Z

Validate the user's ruling on test cleanup. This is read-only: do not edit anything.

**The ruling:** remove tests that do not test business logic or add real value. This includes the 4 DotEnv configuration tests.

**Two branches to check:**
- `tests-prune-backend` (commit `0b447d8`): 403 → 173 tests.
- `tests-prune-web` (commit `c56cf56`): 243 → 124 tests.

Diff each against `main`. No `src/` or `db/` changes are expected.

**Your job:** check that every rule in PLAN §5.3 and §7, and every §13 decision with observable behaviour, still has at least one surviving test. That covers:
- the R2* rule rows
- ranking
- eligibility
- the calendar and DST rules
- de-dup
- half-open windows
- precedence
- the goldens for accounts 1, 6, 8, 12, 14, 18 and 20, and the empty database
- rounding
- earliestWeek
- the UI URL-state rules and the copy

Also check the UI criteria in `docs/acceptance-criteria.md` that were marked GOLDEN or SPEC. Use the traceability tables in the test files and the test names, and spot-check by reading the tests.

**Points the test-author flagged:**
- The `StarterFileChecksumTests` guard on `db/` was removed.
- The `NormalityOptions` wiring tests were removed.
- The fail-fast test for a missing connection string and `MigrateOnStartTests` were kept.
- The web week-label across a year boundary is untested because C-17 doesn't define it.

**Answer:**
- **VALID**, or **GAP:** followed by a list of PLAN items that now have no test, each with its PLAN line.
- One line on whether PLAN §13 needs an append-only entry recording the ruling. It does for the DotEnv tests, because §13 lists four startup tests. Give the exact wording for that entry. The coordinator will append it.


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a195bcaf0a3cb2ec8\" && git log --oneline -1 main && git log --oneline -2 tests-prune-backend tests-prune-web; git diff --stat main...0b447d8; git diff --stat main...c56cf56; wc -l PLAN.md docs/acceptance-criteria.md","description":"Show branch commits and diff stats"}
```


<details><summary>tool result</summary>

```
5e9bed4 Architecture doc: drop removed design-time factory from project tree
0b447d8 Prune backend tests to business rules, SQL correctness, client-facing API behaviour and seeded goldens (403 -> 173)
c56cf56 Prune web Vitest suite from 243 to 124 tests: drop framework/pass-through/duplicate tests, merge rule variants into it.each tables, add pure unit tables for week, event-type guard and health copy
 .../Accounts/AccountsEndpointTests.cs              |  59 +---
 .../ActivityHealth/Account14GoldenTests.cs         | 111 ++------
 .../ActivityHealth/Account6GoldenTests.cs          |  84 +-----
 .../ActivityHealth/Account8GoldenTests.cs          |  31 +--
 .../ActivityHealth/ActivityHealthEndpointTests.cs  | 129 ---------
 .../ActivityHealthResultMappingTests.cs            |  45 ---
 .../ActivityHealth/DefaultWeekGoldenTests.cs       |  56 +---
 .../DisplayDeviationRoundingTests.cs               |  81 +-----
 .../ActivityHealth/EmptyAccountTests.cs            |  37 +--
 .../ActivityHealth/InvalidInputTests.cs            |  72 +----
 .../ActivityHealth/OtherAccountsGoldenTests.cs     |  27 +-
 .../ActivityHealth/ResponseShapeTests.cs           | 163 +----------
 .../ActivityHealth/UnhandledExceptionTests.cs      |  38 +--
 tests/Relay.Api.Tests/Fixtures/DotEnvApiFactory.cs |  23 --
 tests/Relay.Api.Tests/Fixtures/DotEnvDirectory.cs  |  25 --
 .../Fixtures/EnvironmentVariableScope.cs           |  23 --
 .../Fixtures/ProcessEnvironmentTestGroup.cs        |   7 -
 tests/Relay.Api.T …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a195bcaf0a3cb2ec8\" && grep -n '^#' PLAN.md","description":"List PLAN headings"}
```


<details><summary>tool result</summary>

```
1:# PLAN — DASH-247 "Is this normal for us?"
9:## 1. Interpretation of the ticket
23:## 2. What the seed data told us
42:## 3. Decisions (with rejected alternatives)
54:## 4. Assumptions & open questions (not sent to recruiter — working assumption stated)
65:## 5. Design
67:### 5.1 Database
76:### 5.2 API
104:### 5.3 Normality rules (the product logic — pure C#)
119:### 5.4 Frontend
130:## 6. Architecture & code rules
165:## 7. Test plan
197:## 8. Agent team & working model
215:## 9. Execution phases & parallelisation
234:## 10. Time budget & cut line
242:## 11. Deferred (deliberately)
248:## 12. AI log (minimal, no hooks)
255:## 13. Plan changes
258:### 2026-09-28 — Revised design from the four-agent debate (approved by the user)
278:#### §5.1 — index and de-duplication (replaces the index bullet)
284:#### §5.2 — API (replaces §5.2)
317:#### §5.3 — normality rules (replaces §5.3)
340:#### §5.4 — status copy and footnote (replaces the two §5.4 bullets on status and footnote)
365:#### §7 — golden values and Evaluator / Ranking / API edge cases (replace the corresponding parts of §7)
413:### 2026-09-28 — Phase 0 split by specialty; project-file ownership per stack (user decision)
424:### 2026-09-28 — Phase 0 decisions and promoted golden values (user decisions)
446:### 2026-09-28 — Input handling and UI copy decisions (user decisions, validated by the architect)
460:### 2026-09-28 — Contract decisions from the Phase 0 contract review (user decisions, validated by the architect) …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a195bcaf0a3cb2ec8\" && sed -n 70,573p PLAN.md","description":"Read PLAN from §5 onward"}
```


<details><summary>tool result</summary>

```
<persisted-output>
Output too large (44.3KB). Full output saved to: /Users/josemejia/.claude-alt/projects/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/485c1256-5727-457d-8aec-65bc845ee171/tool-results/b1pehpoba.txt

Preview (first 2KB):
  `TIMESTAMP` → `datetime2`; ids `ValueGeneratedNever`; `event_type` stays a string column.
- **No unique constraint** (it would reject the duplicate rows). Index `IX_activity_events_account_occurred` on
  `(account_id, occurred_at) INCLUDE (location, event_type)`.
- Migration `LoadSeedData` runs `db/seed.sql` (committed unmodified, embedded resource) via `migrationBuilder.Sql`. `Down()` deletes the rows.
- The API applies migrations at startup in Development. Integration tests apply `InitialCreate` only and insert their own fixtures.

### 5.2 API
`GET /api/accounts` → `[{ id, name, timezone }]` (includes account 20).

`GET /api/accounts/{accountId}/activity-health?week=YYYY-MM-DD&type=all`

| Param | Rule |
|---|---|
| `week` | Optional local Monday. Default = latest complete week. Not a Monday → 400. After latest complete week → 400 |
| `type` | `all` (default) \| `call_received` \| `lead_created` \| `appointment_set`; else 400 |
| `accountId` | Unknown → 404 |

Errors are `ProblemDetails`. Response:
```json
{
  "account": { "id": 6, "name": "Metro Collision Centers", "timezone": "America/New_York" },
  "eventType": "all",
  "week": { "start": "2026-07-20", "end": "2026-07-26" },
  "dataAsOf": "2026-07-27T2 …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Read**
```json
{"file_path":"/Users/josemejia/.claude-alt/projects/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/485c1256-5727-457d-8aec-65bc845ee171/tool-results/b1pehpoba.txt"}
```


<details><summary>tool result</summary>

```
1	  `TIMESTAMP` → `datetime2`; ids `ValueGeneratedNever`; `event_type` stays a string column.
2	- **No unique constraint** (it would reject the duplicate rows). Index `IX_activity_events_account_occurred` on
3	  `(account_id, occurred_at) INCLUDE (location, event_type)`.
4	- Migration `LoadSeedData` runs `db/seed.sql` (committed unmodified, embedded resource) via `migrationBuilder.Sql`. `Down()` deletes the rows.
5	- The API applies migrations at startup in Development. Integration tests apply `InitialCreate` only and insert their own fixtures.
6	
7	### 5.2 API
8	`GET /api/accounts` → `[{ id, name, timezone }]` (includes account 20).
9	
10	`GET /api/accounts/{accountId}/activity-health?week=YYYY-MM-DD&type=all`
11	
12	| Param | Rule |
13	|---|---|
14	| `week` | Optional local Monday. Default = latest complete week. Not a Monday → 400. After latest complete week → 400 |
15	| `type` | `all` (default) \| `call_received` \| `lead_created` \| `appointment_set`; else 400 |
16	| `accountId` | Unknown → 404 |
17	
18	Errors are `ProblemDetails`. Response:
19	```json
20	{
21	  "account": { "id": 6, "name": "Metro Collision Centers", "timezone": "America/New_York" },
22	  "eventType": "all",
23	  "week": { "start": "2026-07-20", "end": "2026-07-26" },
24	  "dataAsOf": "2026-07-27T22:20:34Z",
25	  "latestCompleteWeek": "2026-07-20",
26	  "earliestWeek": "2026-01-26",
27	  "baselineWeeks": 8,
28	  "summary": { "count": 87, "baseline": { "median": 72.5, "low": 24, "high": 121, "weeksUsed": …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a195bcaf0a3cb2ec8\" && S=/private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/485c1256-5727-457d-8aec-65bc845ee171/scratchpad; mkdir -p $S/be $S/web && git archive 0b447d8 tests | tar -x -C $S/be && git archive c56cf56 web/src docs | tar -x -C $S/web && git diff --name-status main 0b447d8 -- src db; git diff --name-status main c56cf56 -- src db web/src ':!*.spec.ts' ; find $S/be/tests -name '*Tests.cs' | sed \"s|$S/be/||\" | sort","description":"Extract branch test trees to scratchpad and list files"}
```


<details><summary>tool result</summary>

```
This agent is isolated in the worktree /Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a195bcaf0a3cb2ec8, but this command names git in a form too complex to verify that it stays inside the worktree. Refusing to run it — a worktree-isolated agent's git operations must target its own worktree. Split it into plain, separate commands and run them from /Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a195bcaf0a3cb2ec8.
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a195bcaf0a3cb2ec8\" && git diff --name-status main 0b447d8 -- src db web; echo ---; git diff --name-status main c56cf56 -- src db tests; echo ---; git diff --name-status main c56cf56 -- web","description":"Check for src/db changes on both branches"}
```


<details><summary>tool result</summary>

```
---
---
D	web/src/app/app.spec.ts
M	web/src/app/core/api/accounts.api.spec.ts
M	web/src/app/core/api/activity-health.api.spec.ts
M	web/src/app/features/dashboard/components/account-summary.spec.ts
M	web/src/app/features/dashboard/components/dashboard-filters.spec.ts
M	web/src/app/features/dashboard/components/location-table.spec.ts
M	web/src/app/features/dashboard/dashboard-state.spec.ts
M	web/src/app/features/dashboard/dashboard.page.spec.ts
A	web/src/app/features/dashboard/event-type-guard.spec.ts
A	web/src/app/features/dashboard/health-copy.spec.ts
A	web/src/app/features/dashboard/week.spec.ts
M	web/src/testing/fake-apis.ts
M	web/src/testing/router-testing.ts
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a195bcaf0a3cb2ec8\" && git archive -o /private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/485c1256-5727-457d-8aec-65bc845ee171/scratchpad/be.tar 0b447d8 tests","description":"Archive backend branch tests"}
```


<details><summary>tool result</summary>

```
(Bash completed with no output)
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a195bcaf0a3cb2ec8\" && git archive -o /private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/485c1256-5727-457d-8aec-65bc845ee171/scratchpad/web.tar c56cf56 web/src","description":"Archive web branch sources"}
```


<details><summary>tool result</summary>

```
(Bash completed with no output)
```
</details>


**tool call — Bash**
```json
{"command":"S=/private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/485c1256-5727-457d-8aec-65bc845ee171/scratchpad; mkdir -p $S/be $S/web; tar -xf $S/be.tar -C $S/be; tar -xf $S/web.tar -C $S/web; cd $S/be && find tests -name '*.cs' | sort | xargs wc -l; cd $S/web && find web/src -name '*.spec.ts' | xargs wc -l","description":"Extract and list test files"}
```


<details><summary>tool result</summary>

```
      29 tests/Relay.Api.Tests/Accounts/AccountsEndpointTests.cs
      75 tests/Relay.Api.Tests/ActivityHealth/Account14GoldenTests.cs
      83 tests/Relay.Api.Tests/ActivityHealth/Account6GoldenTests.cs
      38 tests/Relay.Api.Tests/ActivityHealth/Account8GoldenTests.cs
      83 tests/Relay.Api.Tests/ActivityHealth/DefaultWeekGoldenTests.cs
      23 tests/Relay.Api.Tests/ActivityHealth/DisplayDeviationRoundingTests.cs
      22 tests/Relay.Api.Tests/ActivityHealth/EmptyAccountTests.cs
      49 tests/Relay.Api.Tests/ActivityHealth/EmptyDatabaseTests.cs
      75 tests/Relay.Api.Tests/ActivityHealth/InvalidInputTests.cs
      41 tests/Relay.Api.Tests/ActivityHealth/OtherAccountsGoldenTests.cs
      41 tests/Relay.Api.Tests/ActivityHealth/ResponseShapeTests.cs
      29 tests/Relay.Api.Tests/ActivityHealth/UnhandledExceptionTests.cs
      52 tests/Relay.Api.Tests/Fixtures/ApiResponse.cs
       6 tests/Relay.Api.Tests/Fixtures/FixedTimeProvider.cs
      28 tests/Relay.Api.Tests/Fixtures/HealthReportJson.cs
      27 tests/Relay.Api.Tests/Fixtures/RelayApiFactory.cs
      34 tests/Relay.Api.Tests/Fixtures/ReportBuilder.cs
      18 tests/Relay.Api.Tests/Fixtures/ScratchDatabase.cs
      78 tests/Relay.Api.Tests/Fixtures/SeededApiFixture.cs
      12 tests/Relay.Api.Tests/Fixtures/SeededApiTest.cs
       7 tests/Relay.Api.Tests/Fixtures/SeededApiTestGroup.cs
      67 tests/Relay.Api.Tests/Fixtures/SeriesJson.cs
      12 tests/Relay.Api.Tests/Fixtures/ThrowingActivityHealthService.cs
   …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd /private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/485c1256-5727-457d-8aec-65bc845ee171/scratchpad/be/tests/Relay.Core.Tests && cat Normality/BaselineEvaluatorTests.cs Normality/LocationRankerTests.cs Calendar/WeekCalendarTests.cs","description":"Read evaluator, ranker, calendar tests"}
```


<details><summary>tool result</summary>

```
using Relay.Core.Normality;

namespace Relay.Core.Tests.Normality;

public sealed class BaselineEvaluatorTests
{
    private const double Tolerance = 1e-9;

    private static SeriesHealth Evaluate(IReadOnlyList<int> eligibleWeekCounts, int selectedWeekCount) =>
        new BaselineEvaluator(new NormalityOptions()).Evaluate(eligibleWeekCounts, selectedWeekCount);

    [Theory]
    [InlineData(new[] { 11, 11, 11, 8 }, 11, 11.0, 6, 18, HealthStatus.Normal, 0.0)]
    [InlineData(new[] { 11, 11, 11, 8 }, 6, 11.0, 6, 18, HealthStatus.Normal, -1.6956163124349821)]
    [InlineData(new[] { 11, 11, 11, 8 }, 5, 11.0, 6, 18, HealthStatus.Below, -2.108559533868169)]
    [InlineData(new[] { 11, 11, 11, 8 }, 18, 11.0, 6, 18, HealthStatus.Normal, 1.827845318125103)]
    [InlineData(new[] { 11, 11, 11, 8 }, 19, 11.0, 6, 18, HealthStatus.Above, 2.058039649213484)]
    [InlineData(new[] { 2, 4, 6, 20 }, 14, 5.0, 1, 14, HealthStatus.Normal, 1.9790679585482758)]
    [InlineData(new[] { 2, 4, 6, 20 }, 15, 5.0, 1, 14, HealthStatus.Above, 2.153269134381721)]
    [InlineData(new[] { 2, 4, 6, 20 }, 0, 5.0, 1, 14, HealthStatus.Below, -2.2921098270714397)]
    [InlineData(new[] { 0, 0, 0, 0 }, 0, 0.0, 0, 2, HealthStatus.Normal, 0.0)]
    [InlineData(new[] { 0, 0, 0, 0 }, 3, 0.0, 0, 2, HealthStatus.Above, 2.4494897427831783)]
    [InlineData(new[] { 2, 2, 2, 2 }, 0, 2.0, 0, 6, HealthStatus.Normal, -1.857462130092899)]
    [InlineData(new[] { 3, 3, 3, 3 }, 0, 3.0, 1, 7, HealthStatus.Below, -2.44948974278 …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd /private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/485c1256-5727-457d-8aec-65bc845ee171/scratchpad/be/tests/Relay.Core.Tests && cat Normality/WeeklyGridBuilderTests.cs ActivityHealth/ActivityHealthServiceTests.cs Accounts/AccountServiceTests.cs","description":"Read grid, service, account service tests"}
```


<details><summary>tool result</summary>

```
using Relay.Core.Calendar;
using Relay.Core.Normality;
using Relay.Core.Queries;
using Relay.Core.Tests.TestDoubles;

namespace Relay.Core.Tests.Normality;

public sealed class WeeklyGridBuilderTests
{
    private const string SelectedWeekStart = "2026-03-16";

    private static readonly WeekWindow SelectedWeek = TestTime.UtcMidnightWeek(SelectedWeekStart);
    private static readonly IReadOnlyList<WeekWindow> BaselineWindows = TestTime.UtcMidnightBaselineBefore(SelectedWeekStart, 8);

    private readonly WeeklyGridBuilder _weeklyGridBuilder = new();

    private static SiteFirstActivity SiteFirstSeen(string location, string firstActivityUtc) =>
        new(location, TestTime.Utc(firstActivityUtc));

    private static WeeklySiteCount CountFor(string location, string weekStart, int count) =>
        new(location, TestTime.Day(weekStart), count);

    private static WeeklySeries SeriesFor(IReadOnlyList<LocationSeries> locationSeries, string location) =>
        locationSeries.Single(series => series.Location == location).Series;

    [Fact]
    public void BuildLocationSeriesSiteWithNoEventsInSelectedWeekAppearsWithZeroCount()
    {
        var siteFirstActivities = new[] { SiteFirstSeen("Site A", "2026-01-05T10:00:00Z") };
        var weeklySiteCounts = BaselineWindows
            .Select(window => new WeeklySiteCount("Site A", window.WeekStart, 2))
            .ToList();

        var locationSeries = _weeklyGridBuilder.BuildLocationSeries(SelectedWeek, BaselineWindows, sit …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd /private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/485c1256-5727-457d-8aec-65bc845ee171/scratchpad/be/tests/Relay.Infrastructure.Tests && cat Queries/*.cs Schema/*.cs","description":"Read infrastructure tests"}
```


<details><summary>tool result</summary>

```
using Relay.Core.Queries;
using Relay.Infrastructure.Queries;
using Relay.Infrastructure.Tests.Fixtures;
using static Relay.Infrastructure.Tests.Fixtures.EventRow;

namespace Relay.Infrastructure.Tests.Queries;

[Collection(SqlServerTestGroup.Name)]
public sealed class SqlAccountQueriesTests(SqlServerFixture fixture) : SqlServerTest(fixture)
{
    private static readonly AccountRow MetroCollision = new(6, "Metro Collision Centers", "America/New_York");
    private static readonly AccountRow BeaconHomeSecurity = new(14, "Beacon Home Security", "America/New_York");
    private static readonly AccountRow AccountWithoutEvents = new(20, "Quiet Account", "America/Phoenix");

    public override async ValueTask InitializeAsync()
    {
        await base.InitializeAsync();
        await Database.InsertAccountsAsync([MetroCollision, BeaconHomeSecurity, AccountWithoutEvents], CancellationToken);
        await Database.InsertEventsAsync(
            [
                EventAt("2026-03-04T15:30:00Z") with { AccountId = MetroCollision.Id },
                EventAt("2026-03-04T15:30:00Z") with { AccountId = BeaconHomeSecurity.Id },
            ],
            CancellationToken);
    }

    [Fact]
    public async Task ListSeveralAccountsReturnsEveryAccountIncludingOneWithoutEvents()
    {
        var accounts = await Queries.ListAsync(CancellationToken);

        accounts.ShouldBe(
            [
                new AccountListItem(6, "Metro Collision Centers", "America/New_York"),
           …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a195bcaf0a3cb2ec8\" && git diff main 0b447d8 -- tests | grep -E '^-.*(public (async )?(Task|void) |\\[InlineData)' | sed 's/^-\\s*//' ","description":"List removed backend test methods and data rows"}
```


<details><summary>tool result</summary>

```
    public async Task ListAccountsSeededDatabaseReturnsTwentyAccounts()
    public async Task ListAccountsEachItemHasExactlyIdNameAndTimezone()
    [InlineData(14, "Beacon Home Security", "America/New_York")]
    [InlineData(20, "Quiet Harbor Spa", "America/Los_Angeles")]
    [InlineData(6, "Metro Collision Centers", "America/New_York")]
    [InlineData(18, "Capital City Storage", "UTC")]
    public async Task ListAccountsSeededDatabaseIncludesAccount(int accountId, string expectedName, string expectedTimezone)
    public async Task ListAccountsSeededDatabaseIsOrderedByNameOrdinal()
    public async Task ListAccountsIdIsAJsonNumber()
    public async Task GetActivityHealthEarliestWeekListsOnlySitesAlreadyActive()
    public async Task GetActivityHealthEarliestWeekEverySeriesIsInsufficientWithNoEligibleWeeks()
    public async Task GetActivityHealthFebruary2SummaryIsInsufficientWithNoEligibleWeeks()
    public async Task GetActivityHealthFebruary2AllFourSitesInsufficientOrderedByName()
    public async Task GetActivityHealthMixedHistorySummaryIsAbove()
    public async Task GetActivityHealthMixedHistoryRanksFlaggedThenNormalThenInsufficientByName()
    public async Task GetActivityHealthMixedHistorySitesWithFullHistoryHaveGoldenRanges()
    public async Task GetActivityHealthMixedHistorySitesFirstActiveInFebruaryHaveThreeWeeks()
    public async Task GetActivityHealthAppointmentsSummaryIsNormal()
    public async Task GetActivityHealthAppointmentsSiteAWithZeroIsNormalAndRanked …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd /private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/485c1256-5727-457d-8aec-65bc845ee171/scratchpad/be/tests/Relay.Api.Tests && cat ActivityHealth/*.cs Accounts/*.cs","description":"Read API tests"}
```


<details><summary>tool result</summary>

```
using Relay.Api.Tests.Fixtures;

namespace Relay.Api.Tests.ActivityHealth;

[Collection(SeededApiTestGroup.Name)]
public sealed class Account14GoldenTests(SeededApiFixture fixture) : SeededApiTest(fixture)
{
    [Fact]
    public async Task GetActivityHealthEarliestWeekListsOnlySitesAlreadyActiveAndEverySeriesIsInsufficient()
    {
        var report = await GetReportAsync("/api/accounts/14/activity-health?week=2026-01-26");

        report.LocationNames.ShouldBe(["Site B", "Site D"]);
        report.Summary.ShouldBeInsufficient(count: 2, weeksUsed: 0);
        report.Location("Site B").ShouldBeInsufficient(count: 1, weeksUsed: 0);
        report.Location("Site D").ShouldBeInsufficient(count: 1, weeksUsed: 0);
    }

    [Fact]
    public async Task GetActivityHealthFebruary2ListsAllFourSitesInsufficientWithNoEligibleWeeksOrderedByName()
    {
        var report = await GetReportAsync("/api/accounts/14/activity-health?week=2026-02-02");

        report.Summary.ShouldBeInsufficient(count: 27, weeksUsed: 0);
        report.LocationNames.ShouldBe(["Site A", "Site B", "Site C", "Site D"]);
        report.Locations.ShouldAllBe(location =>
            location.Status == WireStatus.InsufficientData && location.WeeksUsed == 0 && location.Deviation == null);
    }

    [Fact]
    public async Task GetActivityHealthMixedHistoryIsAboveAndRanksFlaggedThenNormalThenInsufficientByName()
    {
        var report = await GetReportAsync("/api/accounts/14/activity-health?week=2026-03-02");

   …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd /private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/485c1256-5727-457d-8aec-65bc845ee171/scratchpad/be/tests/Relay.Api.Tests && cat Seed/*.cs Startup/*.cs Fixtures/SeriesJson.cs Fixtures/HealthReportJson.cs","description":"Read seed, startup tests and JSON helpers"}
```


<details><summary>tool result</summary>

```
using Microsoft.Extensions.DependencyInjection;
using Relay.Api.Tests.Fixtures;
using Relay.Core.Calendar;
using Relay.Core.Queries;

namespace Relay.Api.Tests.Seed;

[Collection(SeededApiTestGroup.Name)]
public sealed class SeedLoadTests(SeededApiFixture fixture) : SeededApiTest(fixture)
{
    private const int WeeksFromJanuary26ThroughJuly27 = 27;
    private const int SeedAccountCount = 20;

    private static readonly DateOnly FirstSeedWeek = new(2026, 1, 26);

    [Fact]
    public async Task CountWeeklyBySiteEveryAccountAndSeedWeekSumsTo12614DeduplicatedEvents()
    {
        await using var scope = Fixture.Factory.Services.CreateAsyncScope();
        var weekCalendar = scope.ServiceProvider.GetRequiredService<IWeekCalendar>();
        var accountQueries = scope.ServiceProvider.GetRequiredService<IAccountQueries>();
        var activityQueries = scope.ServiceProvider.GetRequiredService<IActivityQueries>();
        var deduplicatedEvents = 0;

        for (var accountId = 1; accountId <= SeedAccountCount; accountId++)
        {
            var account = (await accountQueries.FindAsync(accountId, CancellationToken)).ShouldNotBeNull();
            var seedWeekWindows = Enumerable.Range(0, WeeksFromJanuary26ThroughJuly27)
                .Select(weekIndex => weekCalendar.Window(FirstSeedWeek.AddDays(7 * weekIndex), account.Timezone))
                .ToList();
            var weeklyCounts = await activityQueries.CountWeeklyBySiteAsync(accountId, seedWeekWindows, ActivityTyp …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a195bcaf0a3cb2ec8\" && git diff main 0b447d8 --stat -- tests | tail -3; git diff main 0b447d8 -- tests | grep -n -i 'ValidateOnStart\\|NormalityOptions' | head -20; git ls-files tests | grep -c '/obj/'","description":"Check NormalityOptions-related removals and tracked obj files"}
```


<details><summary>tool result</summary>

```
 ...lActivityQueriesListSiteFirstActivitiesTests.cs |  82 +-----
 .../Schema/InitialCreateSchemaTests.cs             |  51 +---
 36 files changed, 298 insertions(+), 2241 deletions(-)
1885:-        NormalityOptions? normalityOptions = null,
1889:-        var options = normalityOptions ?? new NormalityOptions();
1890:+        var normalityOptions = new NormalityOptions();
1897:+            new BaselineEvaluator(normalityOptions),
1901:+            normalityOptions,
1910:-    private static async Task<ActivityHealthReport> GetStorageReportAsync(DateOnly? week, NormalityOptions? normalityOptions = null)
1913:-        var service = CreateService(StorageAccount(), StorageActivity(), normalityOptions);
2210:-        var service = CreateService(AutoGroupAccount(), activityQueries, new NormalityOptions { BaselineWeeks = 3 });
2235:-    public async Task GetAsyncReportOptionsFieldsAndBaselineLengthFollowNormalityOptions()
2237:-        var report = await GetStorageReportAsync(null, new NormalityOptions { BaselineWeeks = 6, MinimumEligibleWeeks = 3 });
2444:         new BaselineEvaluator(new NormalityOptions()).Evaluate(eligibleWeekCounts, selectedWeekCount);
2446:-    private static SeriesHealth EvaluateWith(NormalityOptions normalityOptions, IReadOnlyList<int> eligibleWeekCounts, int selectedWeekCount) =>
2447:-        new BaselineEvaluator(normalityOptions).Evaluate(eligibleWeekCounts, selectedWeekCount);
2677:-        var normalityOptions = new NormalityOptions { MinimumEligibleWee …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a195bcaf0a3cb2ec8\" && git ls-tree -r --name-only 0b447d8 tests | grep -c '/obj/'; git ls-tree -r --name-only main tests | grep -c '/obj/'","description":"Check whether obj files are tracked in commits"}
```


<details><summary>tool result</summary>

```
0
0
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a195bcaf0a3cb2ec8\" && grep -n 'GOLDEN\\|SPEC\\|^#' docs/acceptance-criteria.md | head -150","description":"Find GOLDEN/SPEC criteria in acceptance doc"}
```


<details><summary>tool result</summary>

```
1:# DASH-247 — Acceptance criteria
7:## How to read this document
25:| `GOLDEN` | A value from the "Revised design" §7 golden table or its hand derivations | Yes. Tests must use these values |
26:| `GOLDEN-P` | A value from a promoted-goldens table in §13: the "Phase 0 decisions" entry or the "Contract decisions" entry | Yes |
27:| `SPEC` | Follows directly from a PLAN rule, cited next to it | Yes |
37:## 0. User-facing copy (verbatim; frontend copies these exactly)
39:### 0.1 Approved copy
48:| C-04 | Status `insufficient_data` | `Not enough history yet (N of 4 weeks needed)`. N = `baseline.weeksUsed`; 4 = `minimumEligibleWeeks` from the response. N = 0 renders `(0 of 4 weeks needed)` | §13 §5.4; SPEC §13 "Input handling and UI copy" (0 case kept) |
50:| C-06 | Account summary line, sufficient history | `{count} {noun} · usually X–Y a week`, e.g. `26 inbound events · usually 18–38 a week`, `51 calls · usually 17–79 a week` | §13 §5.4; SPEC §13 "Input handling and UI copy" (noun) |
51:| C-06a | `{noun}` by type, plural / singular (count = 1) | all: `inbound events` / `inbound event` · call_received: `calls` / `call` · lead_created: `leads` / `lead` · appointment_set: `appointments` / `appointment` | SPEC §13 "Input handling and UI copy" |
52:| C-06b | Account summary line, `insufficient_data` | `{count} {noun}`, with no "usually" part, followed by C-04 (e.g. `8 inbound events` + `Not enough history yet (3 of 4 weeks needed)`) | SPEC §13 "Input handling and UI copy" |
53:| C-0 …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a195bcaf0a3cb2ec8\" && sed -n 37,80p docs/acceptance-criteria.md; sed -n 280,399p docs/acceptance-criteria.md","description":"Read copy table and remaining UI criteria"}
```


<details><summary>tool result</summary>

```
## 0. User-facing copy (verbatim; frontend copies these exactly)

### 0.1 Approved copy

Sources: PLAN §13 §5.4, D5, §13 "Input handling and UI copy" and §13 "Phase 2 review decisions".

| Id | Where | Exact string | Source |
|---|---|---|---|
| C-01 | Status `above` | `▲ Higher than usual` | §13 §5.4 |
| C-02 | Status `below` | `▼ Lower than usual` | §13 §5.4 |
| C-03 | Status `normal` | `Within usual range` | §13 §5.4 |
| C-04 | Status `insufficient_data` | `Not enough history yet (N of 4 weeks needed)`. N = `baseline.weeksUsed`; 4 = `minimumEligibleWeeks` from the response. N = 0 renders `(0 of 4 weeks needed)` | §13 §5.4; SPEC §13 "Input handling and UI copy" (0 case kept) |
| C-05 | Location row range | `Usually X–Y a week`. X–Y = API `low`–`high`, with an en dash (–). For `insufficient_data` rows the "Usual range" cell is left empty (UI-45) | §13 §5.4; §13 "Phase 2 review decisions" (empty cell) |
| C-06 | Account summary line, sufficient history | `{count} {noun} · usually X–Y a week`, e.g. `26 inbound events · usually 18–38 a week`, `51 calls · usually 17–79 a week` | §13 §5.4; SPEC §13 "Input handling and UI copy" (noun) |
| C-06a | `{noun}` by type, plural / singular (count = 1) | all: `inbound events` / `inbound event` · call_received: `calls` / `call` · lead_created: `leads` / `lead` · appointment_set: `appointments` / `appointment` | SPEC §13 "Input handling and UI copy" |
| C-06b | Account summary line, `insufficient_data` | `{count} {noun}`, with no "usually" p …[clipped in markdown; full text in raw/]
```
</details>


### 🤖 ASSISTANT — 2026-09-29T00:16:31.722Z

Now the web specs.


**tool call — Bash**
```json
{"command":"cd /private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/485c1256-5727-457d-8aec-65bc845ee171/scratchpad/web/web/src/app && cat features/dashboard/dashboard-state.spec.ts features/dashboard/week.spec.ts features/dashboard/health-copy.spec.ts features/dashboard/event-type-guard.spec.ts","description":"Read web state and unit specs"}
```


<details><summary>tool result</summary>

```
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Params, Router, provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { AccountsApi } from '../../core/api/accounts.api';
import { ActivityHealthApi, ActivityHealthRequest } from '../../core/api/activity-health.api';
import { EVENT_TYPES } from '../../core/models';
import { beaconDefaultWeekReport } from '../../../testing/activity-health-fixtures';
import { FakeAccountsApi, FakeActivityHealthApi, serverError } from '../../../testing/fake-apis';
import { RecordedNavigation, currentQueryParams, queryParamsOf, recordNavigations, settle } from '../../../testing/router-testing';
import { DashboardState } from './dashboard-state';

@Component({
  selector: 'app-dashboard-route-stub',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: '',
})
class DashboardRouteStub {}

interface StateUnderTest {
  state: DashboardState;
  activityHealthApi: FakeActivityHealthApi;
  navigations: RecordedNavigation[];
  harness: RouterTestingHarness;
}

const DEFAULT_QUERY_PARAMS = { account: '14', week: '2026-07-20', type: 'all' };

async function openState(url: string, prepareApi?: (activityHealthApi: FakeActivityHealthApi) => void): Promise<StateUnderTest> {
  const activityHealthApi = new FakeActivityHealthApi();
  prepareApi?.(activityHealthApi);
  TestBed.configureTestingModule({
    providers: [
       …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd /private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/485c1256-5727-457d-8aec-65bc845ee171/scratchpad/web/web/src/app && cat features/dashboard/dashboard.page.spec.ts","description":"Read dashboard page spec"}
```


<details><summary>tool result</summary>

```
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { routes } from '../../app.routes';
import { AccountsApi } from '../../core/api/accounts.api';
import { ActivityHealthApi, ActivityHealthRequest } from '../../core/api/activity-health.api';
import { EventType } from '../../core/models';
import { BEACON_HOME_SECURITY, buildReport, quietHarborEmptyReport, withoutEnoughHistory } from '../../../testing/activity-health-fixtures';
import {
  cellTexts,
  chooseOption,
  collapsedText,
  findButton,
  getButton,
  getSelect,
  hasTable,
  isDisabled,
  locationRows,
  selectedOptionText,
  textOutsideTables,
} from '../../../testing/dom-queries';
import { FakeAccountsApi, FakeActivityHealthApi, networkFailure, serverError } from '../../../testing/fake-apis';
import { RecordedNavigation, currentQueryParams, queryParamsOf, recordNavigations, settle } from '../../../testing/router-testing';
import { DashboardState } from './dashboard-state';

interface PageUnderTest {
  root: HTMLElement;
  activityHealthApi: FakeActivityHealthApi;
  navigations: RecordedNavigation[];
  harness: RouterTestingHarness;
}

const PREVIOUS_WEEK = '◀ Previous week';
const NEXT_WEEK = 'Next week ▶';
const ABOVE = '▲ Higher than usual';
const BELOW = '▼ Lower than usual';
const WITHIN = 'Within usual range';
const EMPTY_ACCOUNT_MESSAGE = 'No activity recorded for this account yet.';
con …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd /private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/485c1256-5727-457d-8aec-65bc845ee171/scratchpad/web/web/src/app && cat features/dashboard/components/*.spec.ts core/api/*.spec.ts","description":"Read component and api specs"}
```


<details><summary>tool result</summary>

```
import { TestBed } from '@angular/core/testing';
import { ActivityHealthReport } from '../../../core/models';
import { BEACON_HOME_SECURITY, buildReport, withRange, withoutEnoughHistory } from '../../../../testing/activity-health-fixtures';
import { collapsedText } from '../../../../testing/dom-queries';
import { AccountSummary } from './account-summary';

async function renderSummary(report: ActivityHealthReport): Promise<string> {
  TestBed.configureTestingModule({ imports: [AccountSummary] });
  const fixture = TestBed.createComponent(AccountSummary);
  fixture.componentRef.setInput('report', report);
  await fixture.whenStable();
  return collapsedText(fixture.nativeElement as HTMLElement);
}

describe('AccountSummary', () => {
  it('shows "{count} {noun} · usually {low}–{high} a week" with the status label, taking low and high as given', async () => {
    const report = buildReport({ account: BEACON_HOME_SECURITY, weekStart: '2026-07-20', summary: withRange(10, 27, 18, 38, 'below', -3.5), locations: [] });

    const summaryText = await renderSummary(report);

    expect(summaryText).toContain('10 inbound events · usually 18–38 a week');
    expect(summaryText).toContain('▼ Lower than usual');
  });

  it('shows the count with no range and weeks needed from the report minimumEligibleWeeks when history is insufficient', async () => {
    const report = {
      ...buildReport({ account: BEACON_HOME_SECURITY, weekStart: '2026-07-20', eventType: 'call_received', summary: wit …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a195bcaf0a3cb2ec8\" && git diff main c56cf56 -- web/src | grep -E \"^-\\s*(it|it\\.each|describe)\\(|^-\\s*\\]\\)\\('|^-\\s*\\}?\\]?\\)?\\('\" | sed 's/^-\\s*//' | cut -c1-200","description":"List removed web test titles"}
```


<details><summary>tool result</summary>

```
describe('App', () => {
  it('creates the root component', () => {
  it('emits the accounts from the response body unchanged, including account 20', () => {
  it('sends GET /api/accounts/{accountId}/activity-health as a relative URL', () => {
  it('passes week and type as query params', () => {
  it('emits the report body unchanged', () => {
  it('surfaces a 400 ProblemDetails as an HttpErrorResponse with status 400 and the problem body', () => {
  it('shows the heading "Beacon Home Security — all locations"', async () => {
  it('shows the capitalised account method line', async () => {
  it('shows "26 inbound events · usually 18–38 a week" and "Within usual range" for the default week', async () => {
  it('shows "880 inbound events · usually 39–101 a week" and "▲ Higher than usual" for the spike week', async () => {
  it('shows "▼ Lower than usual" for a below summary', async () => {
  it('prints low and high exactly as given, never recomputed', async () => {
  it.each([
  ])('uses the plural noun for type $eventType: "$expectedLine"', async ({ report, expectedLine }) => {
  it('uses "calls", not "inbound events", for 51 calls at account 6', async () => {
  ])('uses the singular noun for a count of 1 with type $eventType: "$expectedLine"', async ({ eventType, expectedLine }) => {
  it('shows "8 inbound events" with no range and "Not enough history yet (3 of 4 weeks needed)" when history is insufficient', async () => {
  it('uses the type noun and minimumEligibleWeeks from th …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a195bcaf0a3cb2ec8\" && grep -rln \"NormalityOptions\\|Normality:\" tests src/Relay.Api | head; grep -rn \"ValidateOnStart\\|Validate\" src/Relay.Api/Composition/*.cs | head","description":"Find NormalityOptions tests and validation on main"}
```


<details><summary>tool result</summary>

```
tests/Relay.Core.Tests/ActivityHealth/ActivityHealthServiceTests.cs
tests/Relay.Core.Tests/Normality/BaselineEvaluatorTests.cs
src/Relay.Api/Composition/CoreServiceCollectionExtensions.cs
src/Relay.Api/Composition/CoreServiceCollectionExtensions.cs:15:            .ValidateDataAnnotations()
src/Relay.Api/Composition/CoreServiceCollectionExtensions.cs:16:            .Validate(
src/Relay.Api/Composition/CoreServiceCollectionExtensions.cs:19:            .ValidateOnStart();
```
</details>


### 🤖 ASSISTANT — 2026-09-29T00:17:28.846Z

**VALID.** Both prunes keep at least one test for every rule you listed, and I found no gap.

I checked this by reading every surviving test file on both branches, the list of removed test names, and PLAN §5.3, §7 and §13 alongside `docs/acceptance-criteria.md`. I did not run the suites. Neither branch changes `src/` or `db/`; the web branch only touches test helpers under `web/src/testing/`.

## Where each item is still tested

| Item | Surviving test |
|---|---|
| R2\* evaluator: all six statistician rows, the three low-guard cases, the madT = 0 floor, even and odd medians, the spike in the baseline, edges at low/high (6 and 18 normal, 5 below, 19 above), fewer than 4 weeks (0 and 3) | `BaselineEvaluatorTests` (two theories) |
| Ranking (§5.3 step 9) | `LocationRankerTests`, 8 tests |
| Eligibility and site list (steps 1–2) | `WeeklyGridBuilderTests` |
| Calendar and DST | `WeekCalendarTests`, plus `GetAsyncRequestsSelectedWeekAndEightPrecedingWindows…AcrossDst` |
| De-dup, including NULL = `''` = `0` and near-duplicates | `CountWeeklyBySite…` tests, the index test, and seed raw 12,626 / de-duplicated 12,614 |
| Half-open windows | `CountWeeklyBySiteWindowsAreHalfOpen…` |
| Precedence | `InvalidInputTests` (malformed input → 400 before the account lookup; unknown account → 404 before week rules) and the service's `…NotAWeekStartTakingPrecedence` |
| Account goldens | Account 1 Site C 5→4, account 6 on 06-01, 06-08, 06-29, 07-20 and with calls, account 8 on 07-20, 03-09 and 03-02, account 12, account 14 on 07-20 and 01-26, 02-02, 03-02, `appointment_set`, calls and leads, account 18 on 03-23, account 20 on the default week and 03-02 → 400 |
| Empty database | `EmptyDatabaseTests` |
| Rounding | `DisplayDeviationRoundingTests` (±0.125 → ±0.13), plus the API goldens that assert 2 dp |
| earliestWeek | The 20-account theory in `DefaultWeekGoldenTests`, and `min(first-event week, latestCompleteWeek)` in the service test |
| UI URL state, rewrites and copy | UI-01…07, 10…22, 30…45, C-04…C-23: `dashboard.page.spec.ts`, `dashboard-state.spec.ts`, the component specs, `health-copy.spec.ts`, `week.spec.ts` |

The C-17 week label is covered by the "Mon Jul 13 – Sun Jul 19, 2026" assertion in the UI-31 test.

## The four points you flagged

- **`StarterFileChecksumTests`:** acceptance criterion DATA-40 now has no automated test. That is a repository guard, not app behaviour. It needs the §13 entry below, and `docs/acceptance-criteria.md` DATA-40 plus its §6 "Starter files unchanged" row should be updated to match.
- **`NormalityOptions` wiring:** what was removed is the Core `…ComesFromOptions` tests and a separate API constants test. The API values (`baselineWeeks` 8, `minimumEligibleWeeks` 4) are still asserted in `DefaultWeekGoldenTests`. `ValidateOnStart` had no test on `main` before the prune either, so nothing was lost there.
- **Kept startup tests:** the missing-connection-string fail-fast test and `MigrateOnStartTests` are both present.
- **Week label across a year boundary:** I agree it's not a gap, because C-17 doesn't define a cross-year format. The year-boundary step itself is still tested by `addWeeks` (2025-12-29 → 2026-01-05).

## Smaller cuts that are still covered some other way

- **Per-account de-dup totals:** only the 12,614 overall total is still asserted. The per-account row in the §13 "Contract decisions" goldens is covered only through that sum.
- **`DateTimeKind.Utc` read-back:** the dedicated tests were removed. The exact `"2026-07-27T22:20:34Z"` assertion and the goldens still depend on it.
- **Before-earliestWeek → 400:** API-44's account 8 case was removed. The rule is still tested for account 14 (2026-01-19) and account 20 (2026-03-02).

## §13 entry

It is needed: §13 "OS-agnostic local configuration via DotNetEnv" lists four startup tests that no longer exist, and the starter-file checksum guard is gone too. Suggested wording:

```
### 2026-09-29 — Test suite pruned to business value (user decision, validated by the architect)

**Reason.** The user ruled that tests which do not test business logic or add real value are removed. Backend 403 → 173 tests (`0b447d8`), web 243 → 124 (`c56cf56`); no `src/` or `db/` change.
Every §5.3 rule, §7 edge case and golden, and every §13 decision with observable behaviour keeps at least one test (validated by the architect).
- Supersedes the last bullet of "OS-agnostic local configuration via DotNetEnv": the four DotEnv startup tests are removed; the DotNetEnv behaviour is verified by the documented run steps only.
- The starter-file SHA-256 guard (`StarterFileChecksumTests`, acceptance DATA-40) is removed; `db/schema.sql` and `db/seed.sql` stay unmodified by rule (CLAUDE.md), not by test.
- Kept: migrate-on-start fail-fast for a missing connection string, `MigrateOnStartTests`, the seed de-duplication total 12,614.
```

