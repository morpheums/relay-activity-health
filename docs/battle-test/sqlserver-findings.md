# SQL Server battle test — PLAN §5.1 / §5.3 steps 1–2 / §6 IActivityQueries

Environment: `mcr.microsoft.com/mssql/server:2022-latest` (16.0.4265.3, CU26, amd64 under Rosetta on arm64), DB `relay`,
default collation `SQL_Latin1_General_CP1_CI_AS`, compat level 160. Driver: pymssql 2.4.2 calling `sp_executesql` with typed
parameters (the same wire shape SqlClient/EF uses), plus an EF Core 10.0.5 `Database.SqlQuery<T>` probe (`efprobe/`).
Scripts: `schema_mssql.sql`, `setup.py`, `queries.py`, `run.py`, `checks.py`, `plan.py`, `variants.py`; raw outputs `*.out`, plans `plan_*.xml`.

## 1. Schema
See `schema_mssql.sql`: `int`/`varchar(n)`/`datetime2` (defaults to `datetime2(7)`), PKs on `id`, FK events→accounts, no unique
constraint, `IX_activity_events_account_occurred (account_id, occurred_at) INCLUDE (location, event_type)`.

## 2. Seed load
`seed.sql` unmodified, one `execute()` = one batch (12,646 INSERTs, 2.4 MB, no `GO`, no `{}` braces), inside a transaction.
- **No errors.** Time: 1.36 s cold, 0.96–0.98 s on 3 reloads (emulated amd64).
- Counts: **20 accounts, 12,626 events**. MIN/MAX occurred_at `2026-02-01 10:57:44` / `2026-07-27 22:20:34` (matches PLAN §2).

## 3. Queries
(a) data anchor
```sql
SELECT MIN(occurred_at) AS first_occurred_at, MAX(occurred_at) AS last_occurred_at FROM activity_events
```
Plan: Index Scan of IX_activity_events_account_occurred (12,626 rows) → Stream Aggregate. Fine at this size.

(b) sites
```sql
SELECT location, MIN(occurred_at) AS first_occurred_at
FROM activity_events
WHERE account_id = @accountId AND occurred_at < @beforeUtc
GROUP BY location
```
Plan: **Index Seek** IX_activity_events_account_occurred on (account_id, occurred_at) → Hash Match aggregate. Covered.

(c) weekly counts — recommended text
```sql
SELECT deduplicated.location, deduplicated.window_start, COUNT(*) AS event_count
FROM (
    SELECT DISTINCT windows.window_start, events.location, events.event_type,
           events.occurred_at, events.duration_seconds, events.outcome
    FROM OPENJSON(@windows) WITH (window_start datetime2 '$.start', window_end datetime2 '$.end') AS windows
    JOIN activity_events AS events
      ON events.account_id = @accountId
     AND events.occurred_at >= windows.window_start
     AND events.occurred_at <  windows.window_end
    WHERE @eventType IS NULL OR events.event_type = @eventType
) AS deduplicated
GROUP BY deduplicated.location, deduplicated.window_start
```
- `@windows` = JSON `[{"start":"2026-01-26T05:00:00Z","end":"2026-02-02T05:00:00Z"}, …]`, UTC.
- De-dup = `SELECT DISTINCT` over every column except `id` (DISTINCT treats NULL = NULL). `window_start` is in the DISTINCT only so
  the dedup happens per window; duplicates share `occurred_at`, so they always fall in the same window. `account_id` is fixed by the predicate.
- Works verbatim through **EF Core 10 `Database.SqlQuery<WeeklyLocationCount>($"...")`**: EF sent `@p0` nvarchar(4000) JSON, `@p1` int,
  `@p2/@p3` (the interpolated `eventType` appears twice → two params; `null` → `DBNull`, works). Account 6 results via EF: all 375 rows / 2,627
  events, call_received 356 / 1,590 — identical to `counts.csv`.
- `System.Text.Json` serialises `DateTime` with `Kind=Utc` as `...Z`, which OPENJSON accepts.

Alternatives: (1) TVP (`SqlParameter{SqlDbType=Structured, TypeName="dbo.utc_window"}`) — strongly typed, no JSON, but needs a
user-defined table type in a migration and a DataTable/IEnumerable<SqlDataRecord>; heavier for no gain here. (2) Since the windows
are contiguous, a single `@rangeStart/@rangeEnd` plus bucketing in C# — moves the bucketing out of SQL, contradicting CLAUDE.md rule 7.
**Recommend OPENJSON** (it's what EF itself uses for primitive collections; requires compat ≥ 130 — 2022 default is 160).

## 4. counts.csv
5,851 non-zero rows (`account_id,location,event_type,week_start,count`), 20 accounts × 4 types × local Mondays 2026-01-26…2026-07-20,
windows built with Python `zoneinfo`. Independently recomputed in Python from raw rows (set-dedup, same windows): **identical**.
Account 6 week 2026-06-01 = 880, week 2026-07-20 = 87 (PLAN §2/§5.2 values). Account 20: 0 rows.

## 5. Checks
**Duplicates.** `GROUP BY account_id, location, event_type, occurred_at, duration_seconds, outcome HAVING COUNT(*) > 1` →
**12 groups, all pairs, 12 excess rows** (12,614 distinct rows). Same 12 under `Latin1_General_BIN2`, so CI collation merges nothing extra.
Pairs: 1523/1524, 1538/1539, 2236/2237, 5851/5852, 5856/5857, 6325/6326, 6966/6967, 7524/7525, 8738/8739, 9595/9596, 9603/9604, 11266/11267.
- 1538/1539 (account 4, Site B, call, `outcome` NULL): week 2026-02-23 all raw 8 → **7**; call_received raw 5 → **4**. Counted once.
- 5856/5857 (account 1, Site F, appointment, `duration_seconds` NULL): week 2026-04-27 all raw 14 → **13**; appointment_set raw 3 → **2**. Counted once.

**Naive dedup misses NULL duplicates.** Self-join `a.id < b.id AND a.col = b.col …` finds only **8** pairs; `NOT EXISTS (… b.id < a.id AND a.col = b.col …)`
keeps **12,618** rows instead of 12,614. The 4 missed (1538/1539, 5856/5857, 6325/6326, 7524/7525) all have a NULL column.
Correct join form if ever needed: `a.duration_seconds IS NOT DISTINCT FROM b.duration_seconds` (SQL Server 2022) → 12 pairs.
Rule to enforce: dedup only via `DISTINCT`/`GROUP BY` over all non-id columns, or `IS NOT DISTINCT FROM`; never `=` on nullable columns.

**Window boundary.** In a rolled-back transaction, inserted account-12 events at exactly `2026-07-20 07:00:00` (window start, America/Chicago Monday)
and 100 ns earlier. Result: the exact-start event is in window 2026-07-20, the earlier one in window 2026-07-13. `[start, end)` holds.

**Execution plan for (c)** — actual plans, account 6, 26 windows:
- With the PLAN's index `INCLUDE (location, event_type)`: **no seek.** `Clustered Index Scan PK_activity_events` (residual `account_id = @accountId`)
  → Nested Loops with `Table-valued function` (OPENJSON) on the **inner** side, executed **2,641 times** (68,666 rows) → Filter → Distinct Sort →
  Stream Aggregate. ~200 ms. Cause: the DISTINCT needs `duration_seconds` and `outcome`, which the index doesn't cover, so the optimiser drops it.
- Adding scalar `@rangeStart/@rangeEnd` helps (OPENJSON outer, 1 execution; clustered scan + Eager Index Spool; ~13 ms) but still no seek on our index.
- With `INCLUDE (location, event_type, duration_seconds, outcome)`: `Table-valued function` (26 rows, 1 execution) → Nested Loops →
  **Index Seek** `IX_…` seek keys `account_id = @accountId, occurred_at >= window_start, occurred_at < window_end`, **26 executions**, 2,631 rows;
  residual `@eventType IS NULL OR event_type = @eventType` → Distinct Sort → Stream Aggregate. ~7 ms. No range params needed.

**Datatype / collation surprises.**
- Collation is case- and trailing-space-insensitive: `event_type = 'CALL_RECEIVED'` matches 7,780 rows; `location = 'site a   '` matches all 3,692 `Site A` rows.
  GROUP BY/DISTINCT would merge such variants (C# `string` equality would not). Seed is clean (15 locations CI = 15 BIN2, 3 event types, no whitespace),
  so no current impact; the API must validate `type` against the exact lower-case set rather than rely on SQL matching.
- EF sends C# strings as **nvarchar(4000)**; against `varchar` columns this puts `CONVERT_IMPLICIT(nvarchar(40), event_type)` on the column. Harmless here
  (event_type is a residual predicate, not a seek key) but it would block a seek if it ever were. Fix: `HasColumnType("varchar(n)")` + pass a typed
  `SqlParameter` (`SqlDbType.VarChar`) or make the columns nvarchar; not needed for correctness.
- `occurred_at` is `datetime2(7)`; seed has whole seconds only. Seed literals `'YYYY-MM-DD hh:mm:ss'` are language-safe for datetime2 but **not** for
  `datetime` (under `SET LANGUAGE british`, `'2026-02-13 10:00:00'` as datetime fails out-of-range) — keep datetime2.
- `DATEADD(nanosecond, …, '<string literal>')` infers `datetime` and errors; cast to datetime2 first (test-fixture gotcha).
- **OPENJSON `WITH (… datetime2)` silently drops offsets**: `"2026-01-26T01:00:00-05:00"` → `01:00`, not `06:00`. Windows must be serialised as UTC
  (`DateTime.Kind=Utc` → `Z`, or `DateTimeOffset` at offset 0). A non-UTC offset produces wrong buckets with no error.
- datetime2 read back through SqlClient/EF has `DateTimeKind.Unspecified` (verified); the Infrastructure layer must `SpecifyKind(…, Utc)` (or use a
  value converter) before handing instants to Core.
- EF composes `SqlQuery` results into a subquery when you add operators (`SingleAsync` produced `SELECT TOP(2) … FROM (<sql>) AS d`): the raw SQL
  must not end with `;` or contain `ORDER BY` without TOP if it is ever composed. Materialise (b)/(c) with `ToListAsync()` only.

## 6. Plan deltas
1. §5.1 index should be `INCLUDE (location, event_type, duration_seconds, outcome)` (covering the dedup), otherwise (c) never seeks.
2. Name/record exact-dup rule as DISTINCT-over-all-non-id-columns; forbid `=`-based self-join/NOT EXISTS dedup (misses 4 of 12 pairs).
3. Infra contract: windows passed as UTC JSON (`Z`), results `SpecifyKind(Utc)`.
4. §6 says the anchor query returns "data anchor + first event"; (a) returns MIN and MAX globally. The account's first event (needed for account-total
   eligibility, §5.3 step 2) is MIN over (b)'s rows — no separate query needed, or state which query supplies it.
5. (b) ignores event type: sites exist irrespective of the type filter. Confirm that is the intended §5.3 step 1 semantics.
