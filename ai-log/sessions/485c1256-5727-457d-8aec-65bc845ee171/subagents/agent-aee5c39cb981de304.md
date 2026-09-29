
### 🧑 USER — 2026-09-29T00:01:01.835Z

Review two small, independent branches against main (`38a8578`). Keep it proportionate and report only real defects: things that are wrong, misleading, or break a rule in CLAUDE.md or PLAN.md. Do not raise optional nice-to-haves.

**1. `tests-row-order-invariance`** (commit `0fdd91e`). It adds the Core test `GetAsyncWeeklyCountAndSiteRowOrderDoesNotChangeTheReport` and a test-only `WithRowsReversed()` option on `FakeActivityQueries`. Check:
- the hand-derived expected ranking C, D, A, B, F, G against PLAN §13 §5.3 (R2* rule, ranking);
- that the equality comparison is structural and not reference-based;
- that nothing under `src/` changed;
- naming and comment rules.

Run `git diff main...tests-row-order-invariance` and `dotnet test --project tests/Relay.Core.Tests`.

**2. `readme-env-setup`** (commits `454fd45` and `26c03e6`). This is docs and `.env.example` only: the five-step OS-agnostic setup, EF commands with `--startup-project src/Relay.Api`, password quoting rules, and precedence. Check:
- every command and claim against the actual code on main: `Program.cs` DotNetEnv setup, `DevelopmentDatabaseMigrator` message, `docker-compose.yml`, `.env.example`, the port 5080 and web proxy setup;
- the docs against PLAN §13;
- that no bash-only steps remain;
- that no real secrets appear;
- that the root README stays short and links out to `docs/`.

Run `git diff main...readme-env-setup`.

Use your worktree; the checkouts can be detached. Report in the CLAUDE.md handoff format with a verdict per branch: APPROVED or CHANGES REQUESTED. List findings with file:line.


**tool call — Bash**
```json
{"command":"git diff main...tests-row-order-invariance; git diff main...readme-env-setup"}
```


<details><summary>tool result</summary>

```
<persisted-output>
Output too large (55.3KB). Full output saved to: /Users/josemejia/.claude-alt/projects/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/485c1256-5727-457d-8aec-65bc845ee171/tool-results/b5f9bz25a.txt

Preview (first 2KB):
diff --git a/tests/Relay.Core.Tests/ActivityHealth/ActivityHealthServiceTests.cs b/tests/Relay.Core.Tests/ActivityHealth/ActivityHealthServiceTests.cs
index e5ee739..bddf3b8 100644
--- a/tests/Relay.Core.Tests/ActivityHealth/ActivityHealthServiceTests.cs
+++ b/tests/Relay.Core.Tests/ActivityHealth/ActivityHealthServiceTests.cs
@@ -415,4 +415,36 @@ public sealed class ActivityHealthServiceTests
         report.Locations[1].Baseline.ShouldBe(new Baseline(8, 0.0, 0, 2));
         report.Locations[1].Status.ShouldBe(HealthStatus.Normal);
     }
+
+    [Fact]
+    public async Task GetAsyncWeeklyCountAndSiteRowOrderDoesNotChangeTheReport()
+    {
+        FakeActivityQueries TiedSitesActivity() =>
+            new FakeActivityQueries()
+                .WithDataAnchor(SeedDataAnchor)
+                .WithSite(StorageAccountId, "Site G", "2026-07-01T10:00:00Z")
+                .WithSite(StorageAccountId, "Site F", "2026-06-30T10:00:00Z")
+                .WithSite(StorageAccountId, "Site B", "2026-05-12T09:00:00Z")
+                .WithSite(StorageAccountId, "Site A", "2026-05-12T09:00:00Z")
+                .WithSite(StorageAccountId, "Site D", "2026-05-12T09:00:00Z")
+                .WithSite(StorageAccountId …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"git diff main...tests-row-order-invariance --stat; git diff main...tests-row-order-invariance | sed -n 40,200p; git diff main...readme-env-setup --stat"}
```


<details><summary>tool result</summary>

```
 .../ActivityHealth/ActivityHealthServiceTests.cs   | 32 ++++++++++++++++++++++
 .../TestDoubles/FakeActivityQueries.cs             | 19 +++++++++----
 2 files changed, 46 insertions(+), 5 deletions(-)
+    }
 }
diff --git a/tests/Relay.Core.Tests/TestDoubles/FakeActivityQueries.cs b/tests/Relay.Core.Tests/TestDoubles/FakeActivityQueries.cs
index 21d3bdc..412023c 100644
--- a/tests/Relay.Core.Tests/TestDoubles/FakeActivityQueries.cs
+++ b/tests/Relay.Core.Tests/TestDoubles/FakeActivityQueries.cs
@@ -10,6 +10,7 @@ internal sealed class FakeActivityQueries : IActivityQueries
     private readonly List<(int AccountId, SiteFirstActivity SiteFirstActivity)> _siteFirstActivities = [];
     private readonly List<(int AccountId, ActivityType EventType, WeeklySiteCount WeeklySiteCount)> _weeklySiteCounts = [];
     private DateTime? _dataAnchorUtc;
+    private bool _rowsReversed;
 
     public List<WeeklyCountRequest> WeeklyCountRequests { get; } = [];
 
@@ -22,6 +23,12 @@ internal sealed class FakeActivityQueries : IActivityQueries
         return this;
     }
 
+    public FakeActivityQueries WithRowsReversed()
+    {
+        _rowsReversed = true;
+        return this;
+    }
+
     public FakeActivityQueries WithSite(int accountId, string location, string firstActivityUtc)
     {
         _siteFirstActivities.Add((accountId, new SiteFirstActivity(location, TestTime.Utc(firstActivityUtc))));
@@ -50,7 +57,7 @@ internal sealed class FakeActivityQueries : IActivityQueries
 
     publ …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"git diff main...tests-row-order-invariance | sed -n 28,42p; git checkout -q tests-row-order-invariance 2>&1; dotnet test --project tests/Relay.Core.Tests 2>&1 | tail -8; grep -n \"R2\\|tie\\|Tie\\|rank\" PLAN.md | head -40"}
```


<details><summary>tool result</summary>

```
+        var naturalOrderService = CreateService(StorageAccount(), TiedSitesActivity());
+        var reversedOrderService = CreateService(StorageAccount(), TiedSitesActivity().WithRowsReversed());
+
+        var naturalOrderResult = await naturalOrderService.GetAsync(StorageAccountId, null, ActivityType.All, CancellationToken);
+        var reversedOrderResult = await reversedOrderService.GetAsync(StorageAccountId, null, ActivityType.All, CancellationToken);
+
+        var naturalOrderReport = naturalOrderResult.ShouldBeOfType<ActivityHealthResult.Found>().Report;
+        var reversedOrderReport = reversedOrderResult.ShouldBeOfType<ActivityHealthResult.Found>().Report;
+        naturalOrderReport.Locations.Select(location => location.Location)
+            .ShouldBe(["Site C", "Site D", "Site A", "Site B", "Site F", "Site G"]);
+        reversedOrderReport.Locations.ShouldBe(naturalOrderReport.Locations);
+        (reversedOrderReport with { Locations = naturalOrderReport.Locations }).ShouldBe(naturalOrderReport);
+    }
 }
diff --git a/tests/Relay.Core.Tests/TestDoubles/FakeActivityQueries.cs b/tests/Relay.Core.Tests/TestDoubles/FakeActivityQueries.cs
fatal: 'tests-row-order-invariance' is already used by worktree at '/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-ab851a3c686904fbd'
/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-aee5c39cb981de304/tests/Relay.Co …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"sed -n 150,185p PLAN.md; sed -n 262,290p PLAN.md; grep -n \"SeedDataAnchor\\|CreateService\" tests/Relay.Core.Tests/ActivityHealth/ActivityHealthServiceTests.cs | head -5; grep -rn \"SeedDataAnchor =\" tests | head"}
```


<details><summary>tool result</summary>

```
| `IWeekCalendar` | Core | Local Monday ↔ UTC `[start, end)` windows (DST-correct via IANA `TimeZoneInfo`), week containing an instant, latest complete week for an anchor, "is a week start" |
| `IWeeklyGridBuilder` | Core | Sparse `(site, week, count)` + site first-activity → zero-filled series with eligibility per week |
| `IBaselineEvaluator` | Core | Eligible baseline counts + current count → median, spread, band, status, deviation (§5.3 steps 3–7) |
| `ILocationRanker` | Core | §5.3 step 8 |
| `IActivityHealthService` | Core | Orchestrates the above + queries; returns `ActivityHealthResult` (Found / AccountNotFound / InvalidWeek) |
| `IActivityQueries` | Core (impl: Infrastructure) | Data anchor + first event; sites with first-event instant; weekly de-duplicated counts for given UTC windows and type |
| `IAccountQueries` | Core (impl: Infrastructure) | List accounts; get one |

**Code rules** (also in `CLAUDE.md`, binding on every agent):
1. SOLID, no tight coupling. Every class with behaviour depends on interfaces and is wired via DI. Plain data is exempt.
2. **No comment blocks — forbidden.** Self-explanatory code and descriptive names. A single-line comment only when truly needed (e.g. why 1.4826).
3. Test-first per layer: interfaces + records + `NotImplementedException` stubs → complete test suite (happy path + edge cases) → **commit red** →
   user reviews the tests → implement to green → reviewer → next layer. Business-logic tests run with no DB and no mocks.
4. Tes …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"grep -rn \"record \" src/Relay.Core --include=*.cs | grep -i \"LocationHealth\\|ActivityHealthReport\\|Baseline\\|Summary\"; grep -n \"public async Task\" tests/Relay.Core.Tests/ActivityHealth/ActivityHealthServiceTests.cs | head -4; git diff main...readme-env-setup -- README.md .env.example docs/running.md"}
```


<details><summary>tool result</summary>

```
(eval):1: no matches found: --include=*.cs
71:    public async Task GetAsyncUnknownAccountReturnsAccountNotFound()
81:    public async Task GetAsyncNoWeekUsesLatestCompleteWeekFromDataAnchorNotClock()
90:    public async Task GetAsyncExplicitWeekReturnsThatMondayToSundayAndKeepsLatestCompleteWeek()
99:    public async Task GetAsyncFoundReportCarriesAccountEventTypeDataAnchorAndOptions()
diff --git a/.env.example b/.env.example
index 18a677b..3db8989 100644
--- a/.env.example
+++ b/.env.example
@@ -1,3 +1,4 @@
+# Placeholder only; never put a real password here. If the password contains $, single-quote it: RELAY_DB_SA_PASSWORD=<redacted>
 RELAY_DB_SA_PASSWORD="<redacted>"
 RELAY_DB_PORT="1433"
 ConnectionStrings__Relay="Server=localhost,${RELAY_DB_PORT};Database=relay;User Id=sa;Password=${RELAY_DB_SA_PASSWORD};TrustServerCertificate=True"
diff --git a/README.md b/README.md
index 6d5a4d9..69a4d94 100644
--- a/README.md
+++ b/README.md
@@ -1,26 +1,47 @@
-# Relay — Take-Home Starter
+# Relay — Activity health (DASH-247)
 
-Seed data and context for the take-home exercise. **Copy the contents into a fresh repo of your own** (please don't fork — forks are publicly linked to the source), then follow the instructions in the take-home prompt you received by email.
+A dashboard feature for a Relay customer admin on Monday morning. For last week, it shows whether the account's inbound activity (calls, leads, appointments) was **normal for this account**, and which **locations need atte …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"git status | head -3; git log --oneline -1; cat src/Relay.Api/Program.cs | head -60; grep -rn \"missing or empty\\|Applying Relay\" src; cat docker-compose.yml; cat web/proxy.conf.json src/Relay.Api/Properties/launchSettings.json; cat global.json; grep -n \"packageManager\\|engines\\|\\\"start\\\"\\|\\\"test\\\"\" web/package.json; cat .gitignore | grep -n env; ls docs; ls .claude/agents AI_LOG.md ai-log 2>&1 | head -20; git ls-files | grep -i \"\\.env\""}
```


**tool call — Bash**
```json
{"command":"git diff main...readme-env-setup --stat -- README.md; git show readme-env-setup:README.md | wc -l; git show readme-env-setup --stat | head; git log main..readme-env-setup --oneline; git show readme-env-setup:docs/testing.md; git grep -n \"export \\|\\.sh\\|source \\|bash\\|~/\" readme-env-setup -- README.md docs/running.md docs/testing.md docs/api.md docs/architecture.md docs/decisions.md docs/interpretation.md | head -30"}
```


<details><summary>tool result</summary>

```
On branch worktree-agent-aee5c39cb981de304
nothing to commit, working tree clean
38a8578 Database agent brief: connection string also comes from Development .env via DotNetEnv
using DotNetEnv;
using DotNetEnv.Configuration;
using Relay.Api.Composition;
using Relay.Api.Endpoints;
using Relay.Infrastructure.Composition;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddDotNetEnv(
        Path.Combine(builder.Environment.ContentRootPath, ".env"),
        LoadOptions.TraversePath().NoClobber().NoEnvVars());
}

builder.Services
    .AddRelayCore(builder.Configuration)
    .AddRelayInfrastructure(builder.Configuration)
    .AddRelayApi();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHostedService<DevelopmentDatabaseMigrator>();
}

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.MapRelayEndpoints();

await app.RunAsync();
src/Relay.Api/Composition/DevelopmentDatabaseMigrator.cs:40:            $"The connection string '{ConnectionStringSetting}' is missing or empty. Copy .env.example to .env at the repository root (or set the environment variable '{ConnectionStringEnvironmentVariable}') before starting the API.");
src/Relay.Api/Composition/DevelopmentDatabaseMigrator.cs:43:    [LoggerMessage(Level = LogLevel.Information, Message = "Applying Relay database migrations on start (Development)")]
name: relay-activity-health

services:
  db:
    image: mcr.mi …[clipped in markdown; full text in raw/]
```
</details>


<details><summary>tool result</summary>

```
 README.md | 57 +++++++++++++++++++++++++++++++++++++++------------------
 1 file changed, 39 insertions(+), 18 deletions(-)
      47
commit 26c03e6fef07f3fef26ab8d2da02418f706725c4
Author: Jose Mejia <<redacted>>
Date:   Tue Sep 29 00:57:35 2026 +0100

    Docs: OS-agnostic five-step setup via .env (PLAN §13 DotNetEnv); EF commands use --startup-project src/Relay.Api; password quoting, variable order and precedence notes
    
    Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>

 .env.example      |  1 +
 README.md         | 16 +++++++-------
26c03e6 Docs: OS-agnostic five-step setup via .env (PLAN §13 DotNetEnv); EF commands use --startup-project src/Relay.Api; password quoting, variable order and precedence notes
454fd45 Merge branch 'main' into readme-env-setup
9c81445 Restructure README into a short entry point plus focused docs (running, testing, api, interpretation, decisions, architecture); env-setup lines marked ENV-SETUP
7cae3ef README review fixes: spike-baseline week, E2E marked planned, evidence sources
18381a9 README: verified startup behaviour, exact fail-fast message, Development-only checks; verify markers removed
a9bd789 Merge main into README branch
3b38418 README as full specification: prerequisites, configuration, run/stop, per-suite tests, troubleshooting, API reference, versions, project structure
01e2ac9 README: near-duplicate count worded as in PLAN §2
ead2d9b README for DASH-247: run, tests, interpretation, assumptions, trade-offs, data handli …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"grep -rn \"NoClobber\\|clobber\\|Precedence\\|precedence\" tests src PLAN.md | head; grep -n \"env\\b\\|\\.env\" PLAN.md | sed -n 1,40p | cut -c1-260"}
```


<details><summary>tool result</summary>

```
PLAN.md:521:- **Precedence:** malformed input (`week`/`type` shape) → 400 from validation before anything else; then unknown account → 404; then `NotAWeekStart`; then before-earliest / after-latest.
PLAN.md:566:- In Development only, the API adds the repo-root `.env` as a configuration source with **DotNetEnv 3.2.0** (`AddDotNetEnv(…, LoadOptions.TraversePath().NoClobber().NoEnvVars())`).
src/Relay.Api/Program.cs:13:        LoadOptions.TraversePath().NoClobber().NoEnvVars());
68:- SQL Server 2022 via `docker compose` (`db` service). Connection string in `appsettings.Development.json`, overridable by env var.
515:- **No committed secrets:** the dev SA password is never committed. Compose requires `RELAY_DB_SA_PASSWORD` from a git-ignored `.env` (`.env.example` holds a placeholder);
564:**Reason.** The run steps required `set -a; source .env; set +a` (bash/zsh only). User: *"We must find an env agnostic solution"* and *"Avoid hand written as much as possible;
566:- In Development only, the API adds the repo-root `.env` as a configuration source with **DotNetEnv 3.2.0** (`AddDotNetEnv(…, LoadOptions.TraversePath().NoClobber().NoEnvVars())`).
567:  `.env` is unchanged: its `ConnectionStrings__Relay` line is interpolated from `RELAY_DB_PORT` / `RELAY_DB_SA_PASSWORD`, the same file Docker Compose reads. Real environment variables and
568:  test configuration still win; the process environment is not modified; Production never reads `.env`.
570:- Run steps are identical on Windows,  …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"ls analysis | head -3; ls scripts; grep -n \"\\\"@angular/core\\\"\" web/package.json; sed -n 1,45p src/Relay.Api/Composition/DevelopmentDatabaseMigrator.cs | grep -n \"const\\|IsNullOr\"; grep -n \"ChangeOrigin\\|ng serve\\|\\\"port\\\"\" web/angular.json | head; git show readme-env-setup:docs/architecture.md | sed -n 40,71p; grep -rn \"5.5\\|Opus\" .claude/agents/reviewer.md | head -3; grep -n \"1.4826\\|3 hours\\|3 minutes\" docs/../PLAN.md | head -2"}
```


<details><summary>tool result</summary>

```
README.md
debate
goldens
export-ai-log.sh
16:    "@angular/core": "^22.2.0",
12:    private const string ConnectionStringSetting = $"ConnectionStrings:{InfrastructureServiceCollectionExtensions.ConnectionStringName}";
13:    private const string ConnectionStringEnvironmentVariable = $"ConnectionStrings__{InfrastructureServiceCollectionExtensions.ConnectionStringName}";
34:        if (!string.IsNullOrWhiteSpace(configuration.GetConnectionString(InfrastructureServiceCollectionExtensions.ConnectionStringName)))
    Composition/           AddRelayInfrastructure (DbContext, connection string "Relay")
    Persistence/           RelayDbContext, entity configurations, UTC converter, design-time factory
    Queries/               SqlActivityQueries, SqlAccountQueries — the hand-written counting SQL
    Migrations/            InitialCreate, LoadSeedData
  Relay.Api/
    Composition/           AddRelayCore, AddRelayApi (ProblemDetails, validation, JSON enum naming)
    Endpoints/             RelayEndpoints — the two GET endpoints
    Http/                  request binding, strict week/type validation, result → HTTP mapping, display rounding
tests/
  Relay.Core.Tests/            unit, no DB
  Relay.Infrastructure.Tests/  SQL against Testcontainers SQL Server
  Relay.Api.Tests/             endpoints and goldens against the real seed (Testcontainers)
web/src/
  app/core/models/         TypeScript contract models
  app/core/api/            AccountsApi, ActivityHealthApi (abstract tokens + H …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"ls src/Relay.Infrastructure/Persistence; git grep -n -i \"design-time\\|DesignTime\" readme-env-setup -- README.md docs/*.md | cut -c1-220; git grep -n \"Program.cs\\|extension\\|hosted\" readme-env-setup -- docs/architecture.md | head"}
```


<details><summary>tool result</summary>

```
Account.cs
AccountConfiguration.cs
ActivityEvent.cs
ActivityEventConfiguration.cs
RelayDbContext.cs
UtcDateTimeConverter.cs
```
</details>


**tool call — Bash**
```json
{"command":"git show readme-env-setup:docs/architecture.md | grep -n -i \"design-time\"; git show readme-env-setup:docs/architecture.md | sed -n 25,40p"}
```


<details><summary>tool result</summary>

```
41:    Persistence/           RelayDbContext, entity configurations, UTC converter, design-time factory

---

## Project structure

```
db/                        schema.sql, seed.sql — the starter files, unmodified (checksums pinned in tests)
src/
  Relay.Core/              no project references, no packages
    Calendar/              IWeekCalendar: local Monday ↔ UTC window (DST-aware), week containing an instant, latest complete week
    Normality/             zero-filled weekly grid, baseline evaluator (R2*), location ranker, NormalityOptions
    ActivityHealth/        IActivityHealthService: orchestration, the report records, invalid-week reasons
    Accounts/              IAccountService: account list ordered by name
    Queries/               IActivityQueries, IAccountQueries (implemented in Infrastructure), ActivityType
  Relay.Infrastructure/
    Composition/           AddRelayInfrastructure (DbContext, connection string "Relay")
```
</details>


### 🤖 ASSISTANT — 2026-09-29T00:01:51.935Z

## Handoff report

### Summary
I reviewed both branches against main (`38a8578`). `tests-row-order-invariance` is clean. `readme-env-setup` has one stale statement in `docs/architecture.md`, which contradicts the branch's own EF/DotNetEnv change.

### Changed
Nothing. This review was read-only.

### Verification
- `git diff main...tests-row-order-invariance --stat` shows only two files, both under `tests/Relay.Core.Tests`. Nothing under `src/` changed.
- `dotnet test --project tests/Relay.Core.Tests` from my worktree gave 135 passed, 0 failed. That run is on main plus this worktree, because git would not let me check out the branch (it is in use by another worktree). The new test's diff was read directly from `git diff`.
- For `readme-env-setup` I compared the docs with `Program.cs`, `DevelopmentDatabaseMigrator.cs`, `docker-compose.yml`, `web/proxy.conf.json`, `launchSettings.json`, `global.json`, `web/package.json`, `.gitignore` and PLAN §13.

### Findings

**Branch 1: `tests-row-order-invariance` — APPROVED**

Hand-derived ranking (PLAN §5.3 R2\*, ranking rule "insufficient last; ties by name; magnitude"):
- The anchor `2026-07-27` is a Tuesday, so the latest complete week is `2026-07-20`.
- **Sites F and G:** first activity is in week 06-29, which is ineligible. The eligible baseline is weeks 07-06 and 07-13, so 2 weeks, below the minimum of 4. Both are insufficient, ranked last, ordered by name: F, G.
- **Sites A and B:** the baseline is eight weeks of 10 and the current week is 10. Deviation is 0, so they tie and are ordered A, B.
- **Sites C and D:** the baseline is eight weeks of 3 and the current week has no row, so zero-fill gives 0.
  - T(0) = 2√0.375 ≈ 1.22 and T(3) = 2√3.375 ≈ 3.67.
  - The spread is floored at 1.0, so the deviation is nonzero and negative, and |dev| is greater than 0. They rank above A and B, ordered C, D.
- Result: C, D, A, B, F, G, which matches the test.

Other checks:
- **Test can fail:** natural insertion order is G, F, B, A, D, C, which is reverse-alphabetical. Reversing it gives alphabetical order. A ranker that skipped the name tie-break would therefore break either the natural-order or the reversed-order assertion.
- **Comparison is structural:** `Locations.ShouldBe(...)` compares element by element through record `Equals`. The `with { Locations = ... }` line then compares all remaining report fields. It passed without reference-equality tricks.
- **Naming and comments:** the test name is PascalCase and follows the existing `GetAsync…` pattern. There are no comments. `WithRowsReversed` and `InReturnOrder` are descriptive. Business logic uses no mocks.

No findings.

**Branch 2: `readme-env-setup` — CHANGES REQUESTED**

- [Major] `docs/architecture.md:41` — The project-structure tree lists `Persistence/ … design-time factory`. Main removed that factory in commit `860fd64`, and `ls src/Relay.Infrastructure/Persistence` shows no such file. `docs/decisions.md:52` and `docs/running.md` (EF section) both say the factory was removed, so the docs contradict each other. This is misleading and it is in the branch's own scope. Fix: drop ", design-time factory" from that line.

Verified as correct:
- **Five steps:** README steps 1–5 match PLAN §13 lines 570–571 word for word. Both `cp` and Windows `copy` are given.
- **DotNetEnv:** `Program.cs:11-13` uses `TraversePath().NoClobber().NoEnvVars()` in Development only. The docs' claims hold: parent-folder search, no process-environment change, real environment variables win, Production ignores `.env`. PLAN §13:566-568 says the same, and §13:572 lists four startup tests for it.
- **Error message:** the quoted text matches `DevelopmentDatabaseMigrator.cs:40` and the log line at `:43`.
- **`docker-compose.yml`:** the `${RELAY_DB_SA_PASSWORD:<redacted> RELAY_DB_SA_PASSWORD <redacted> .env}` message, the `RELAY_DB_PORT` default of 1433, `platform: linux/amd64`, the `sqlcmd` healthcheck and the `relay-db-data` volume all match.
- **Port 5080:** `launchSettings.json` and `web/proxy.conf.json` both use 5080. `ng serve` sets no port in `angular.json`, so 4200 is the Angular default.
- **Versions:** SDK 10.0.201 with `rollForward: latestFeature`, npm 11.12.1 and Angular `^22.2.0` all match.
- **EF commands:** all three use `--startup-project src/Relay.Api`.
- **Bash-only steps:** none remain. The only `source`/`export` hit is PLAN's history text. `.env.example:1` is a comment saying the password is a placeholder and giving the `$` single-quote rule. It does not hold a real secret.
- **README:** 47 lines. It links to the seven docs files and the other listed targets, all of which exist.

I did not run `dotnet ef` or the NETSDK1004 claim. It is an unverified troubleshooting note and I am not counting it as a defect.

### Deviations
None.

### Uncertain
- The `NoClobber` precedence claim rests on PLAN §13 and its startup tests. I did not run them.
- I could not check out `tests-row-order-invariance` because another worktree holds it. The 135-pass run therefore reflects main, not the branch. I verified the branch's test by reading its diff and hand-deriving the ranking.

### Next
Fix `docs/architecture.md:41` on `readme-env-setup`, then merge. `tests-row-order-invariance` can merge as it is.

