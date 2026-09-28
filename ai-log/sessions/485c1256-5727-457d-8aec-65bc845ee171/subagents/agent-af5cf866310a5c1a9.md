
### 🧑 USER — 2026-09-28T21:35:12.904Z

Review the database agent's Phase 2 query implementations for DASH-247 before merge. Read-only. Branch `worktree-agent-a531142ea92064359` (commit 46b7d0f); diff `git diff aca0fe5 46b7d0f -- src/Relay.Infrastructure`. Coordinator verified: build 0 warnings, Infrastructure suite 52/52 green, no test/db files changed, `.env` not committed.

Check against `CLAUDE.md` (rule 7: SQL counts only), `.claude/agents/database.md` (SQL rules), PLAN §5.1 + §13 (incl. "Phase 1 red-suite decisions": COALESCE dedup; no ORDER BY; UTC Z windows; typed varchar; filter by account first), `docs/battle-test/sqlserver-findings.md` §3(c)/§5:
- SQL correctness: half-open windows, dedup over every non-id column with COALESCE(duration_seconds,0)/COALESCE(outcome,''), type filter (`@eventType IS NULL OR …` — does the OR hurt the seek? the agent measured one plan), anchor, unbounded sites, accounts; no week math/zero-fill/statistics in SQL; `WeekStart` taken from the window JSON.
- Safety: `SqlQueryRaw` with named typed `SqlParameter`s instead of interpolation — the agent's deviation (duplicate `@eventType`, const literals). Is it genuinely parameterised with zero concatenation? Acceptable per database.md ("SqlQueryRaw with string concatenation of values is forbidden")?
- UTC: anchor via the context's DefaultTypeMapping converter, sites via one `SpecifyKind` mapping line; windows serialised with `Z` (Local → UTC, Unspecified → marked Utc) — correct and in one sensible place?
- Plan evidence: Index Seek on IX_activity_events_account_occurred per window, ~3–6 ms; totals 880/87/72.5/51 match goldens.
- Code quality: primary-ctor params, readable raw literals, no comment blocks, no scope creep.
Verdict APPROVED / CHANGES REQUIRED; findings split (a) objective / (b) user decision. CLAUDE.md handoff format, concise.


**tool call — Bash**
```json
{"command":"git diff aca0fe5 46b7d0f -- src/Relay.Infrastructure"}
```


<details><summary>tool result</summary>

```
diff --git a/src/Relay.Infrastructure/Queries/SqlAccountQueries.cs b/src/Relay.Infrastructure/Queries/SqlAccountQueries.cs
index f5d55d3..2dc5414 100644
--- a/src/Relay.Infrastructure/Queries/SqlAccountQueries.cs
+++ b/src/Relay.Infrastructure/Queries/SqlAccountQueries.cs
@@ -1,3 +1,6 @@
+using System.Data;
+using Microsoft.Data.SqlClient;
+using Microsoft.EntityFrameworkCore;
 using Relay.Core.Queries;
 using Relay.Infrastructure.Persistence;
 
@@ -5,11 +8,27 @@ namespace Relay.Infrastructure.Queries;
 
 public sealed class SqlAccountQueries(RelayDbContext dbContext) : IAccountQueries
 {
-    private readonly RelayDbContext _dbContext = dbContext;
+    private const string ListAccountsSql = """
+        SELECT accounts.id AS Id, accounts.name AS Name, accounts.timezone AS Timezone
+        FROM accounts
+        """;
 
-    public Task<IReadOnlyList<AccountListItem>> ListAsync(CancellationToken cancellationToken) =>
-        throw new NotImplementedException();
+    private const string FindAccountSql = """
+        SELECT accounts.id AS Id, accounts.name AS Name, accounts.timezone AS Timezone
+        FROM accounts
+        WHERE accounts.id = @accountId
+        """;
 
-    public Task<AccountListItem?> FindAsync(int accountId, CancellationToken cancellationToken) =>
-        throw new NotImplementedException();
+    public async Task<IReadOnlyList<AccountListItem>> ListAsync(CancellationToken cancellationToken) =>
+        await dbContext.Database
+            .SqlQueryRa …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cat .claude/agents/database.md; grep -n \"Phase 1 red-suite\" -A30 PLAN.md | head -80; sed -n 1,400p docs/battle-test/sqlserver-findings.md | grep -n \"§\\|^#\" | head -40"}
```


<details><summary>tool result</summary>

```
---
name: database
description: SQL Server + EF Core specialist. Use in Phase 1 for docker compose, DbContext and entity configuration, the InitialCreate and LoadSeedData migrations, and in Phase 2 to implement IActivityQueries/IAccountQueries with hand-written parameterised SQL that turns the red Infrastructure suite green. Owns index design and query-plan verification.
tools: Read, Grep, Glob, Write, Edit, Bash
model: opus
---
# Role
You own everything between the seed file and the numbers the business logic receives. "Aggregates that are actually right against the seed data" is the brief's
top correctness criterion — the counting SQL is where that is won or lost.

# Read before any task
`CLAUDE.md`, `PLAN.md` §2 (data findings), §5.1 (database), §5.3 steps 1–2, §6 (`IActivityQueries`, `IAccountQueries`), `db/schema.sql`, and
`docs/battle-test/sqlserver-findings.md` for the SQL Server battle-test findings, and `PLAN.md` §13 for any approved plan changes.

# Deliverables

## 1. Local database — Phase 1
- `docker-compose.yml` service `db`: `mcr.microsoft.com/mssql/server:2022-latest` (`platform: linux/amd64`), SA password **only** from a git-ignored `.env` (`${RELAY_DB_SA_PASSWORD:<redacted> committed `.env.example` holds a placeholder — never a real value), port 1433 → configurable host port,
  healthcheck using `sqlcmd`, named volume. Document the one command in the handoff (`docker compose up -d db`).
- Connection string key `ConnectionStrings:Relay`, supplied **only** by  …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd src; cat Relay.Core/Calendar/WeekWindow.cs Relay.Core/Queries/*.cs | head -120; grep -rn \"enum ActivityType\" -A12 Relay.Core; cat Relay.Infrastructure/Persistence/RelayDbContext.cs; ls Relay.Infrastructure/*; git status --short | head; git log --oneline -3; git diff aca0fe5 46b7d0f --stat | tail -8"}
```


<details><summary>tool result</summary>

```
namespace Relay.Core.Calendar;

public sealed record WeekWindow(DateOnly WeekStart, DateTime StartUtc, DateTime EndUtc);
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

    Task<IReadOnlyList<SiteFirstActivity>> ListSiteFirstActivitiesAsync(int accountId, CancellationToken cancellationToken);

    Task<IReadOnlyList<WeeklySiteCount>> CountWeeklyBySiteAsync(
        int accountId,
        IReadOnlyList<WeekWindow> weekWindows,
        ActivityType eventType,
        CancellationToken cancellationToken);
}
namespace Relay.Core.Queries;

public sealed record SiteFirstActivity(string Location, DateTime FirstActivityUtc);
namespace Relay.Core.Queries;

public sealed record WeeklySiteCount(string Location, DateOnly WeekStart, int Count);
Relay.Core/Queries/ActivityType.cs:3:public enum ActivityType
Relay.Core/Queries/ActivityType.cs-4-{
Relay.Core/Queries/ActivityType.cs-5-    All,
Relay. …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"grep -n \"event_type\\|location\" db/schema.sql | head; grep -c \"\" db/schema.sql; grep -n \"COLLATE\" db/schema.sql | head -3"}
```


<details><summary>tool result</summary>

```
17:    location          VARCHAR(80)  NOT NULL,  -- the account's site/branch where the activity occurred
18:    event_type        VARCHAR(40)  NOT NULL,  -- 'call_received' | 'lead_created' | 'appointment_set'
22
```
</details>


### 🤖 ASSISTANT — 2026-09-28T21:35:45.696Z

## Verdict: APPROVED

I read the diff only. I did not run the suite or query plans; I'm relying on your 52/52 result. The plan evidence (Index Seek per window, ~3–6 ms, totals 880/87/72.5/51) is not in the diff. I haven't seen the agent's own handoff, so I can't confirm it.

## Summary
The two Infrastructure query classes implement `IActivityQueries` and `IAccountQueries` with hand-written counting SQL. There are no rule violations and no correctness defects in the diff.

## Changed
Reviewed only `src/Relay.Infrastructure/Queries/SqlAccountQueries.cs` and `SqlActivityQueries.cs`. No test, db, csproj or contract files are touched (diff stat: 2 files).

## Verification
Commands run: `git diff aca0fe5 46b7d0f -- src/Relay.Infrastructure` and reads of `WeekWindow`, `WeeklySiteCount`, `ActivityType`, `RelayDbContext` and `db/schema.sql`.
- **Half-open windows:** `>= window_start AND < window_end`.
- **Dedup:** `SELECT DISTINCT` over week_start, location, event_type, occurred_at, `COALESCE(duration_seconds,0)` and `COALESCE(outcome,'')`, then `GROUP BY` location and week_start. This is NULL-safe and there is no self-join.
- **Filters and shape:** `account_id = @accountId` is the join's first predicate. There is no `ORDER BY`. Only non-zero rows come back. There is no timezone, week-math or statistics logic in SQL.
- **`WeekStart`:** it comes from the window JSON as a typed `date` and maps to `DateOnly`.
- **Sites:** the query is unbounded, groups by location and takes `MIN(occurred_at)`. It ignores type and needs no dedup.
- **Anchor and accounts:** the anchor is a global `MAX`, and `Single()` on a null aggregate gives `null`. The account queries are plain counts of rows and one-row lookups.
- **Type filter:** `@eventType IS NULL OR event_type = @eventType` only affects a residual predicate. `event_type` is an INCLUDE column, so the seek on `(account_id, occurred_at)` is unaffected. Parameter sniffing could only change the plan for the `NULL` and non-`NULL` cases, and both use the same seek shape. That is a non-issue at 12k rows.
- **Enum mapping:** `SnakeCaseLower` maps `CallReceived`, `LeadCreated` and `AppointmentSet` to the schema's `call_received`, `lead_created` and `appointment_set`. `All` maps to `DBNull`.

## Deviations
- **Safety:** `SqlQueryRaw` with `const` SQL and named typed `SqlParameter`s is genuinely parameterised. Every value is bound, the windows go in as one JSON parameter, and nothing is concatenated or interpolated. That is what `database.md` forbids, so this is acceptable.
- **Why not interpolation:** interpolation would emit `nvarchar` parameters against the `varchar` columns and would duplicate `@eventType` as two parameters. `VarChar(40)` matches `schema.sql:18`. The deviation is justified and stays in the spirit of the rule.
- **UTC:** the anchor uses the `DefaultTypeMapping<DateTime>` converter (`RelayDbContext`). Sites use `AsUtc`. Windows go through `ToUniversal` (Local to UTC, Utc and Unspecified marked Utc). System.Text.Json writes `Z` for `Kind=Utc`, so the windows are correct and handled in one place.

## Findings

**(a) Objective**
- None blocking.
- [Minor] `SqlActivityQueries.cs` `AsUtc` and `ToUniversal` duplicate the Kind-fixing intent of `UtcDateTimeConverter`. The converter can't be used here because a keyless record's `DateTime` member gets no conversion. Only one call site exists (sites), so this is optional.
- [Nit] An undefined `ActivityType` value would silently produce a bogus type string and an empty result. It is unreachable through validated API input, so ignore it.

**(b) User decision**
1. `FindDataAnchorAsync` is a global `MAX(occurred_at)`, which the plan mandates, but there is no index led by `occurred_at`. It is an index scan, which is fine at 12k rows and would be a scan-cost issue at scale. I recommend accepting it.
2. Silently treating an `Unspecified` window `Kind` as UTC is a lenient contract. Core is expected to hand over UTC, and the tests cover that. Accept, or make it throw.

## Uncertain
- The plan evidence (seek operator, timings, golden totals) is unverified by me.
- `GROUP BY` and `DISTINCT` on `location` inherit the database's default collation. If it is case-insensitive, locations differing only by case would merge. The seed is unaffected per the earlier battle test, so I did not re-check it.

## Next
Merge. After that, Phase 2 moves on to the next layer, the application service.

