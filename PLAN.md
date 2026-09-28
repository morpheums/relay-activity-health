# PLAN — DASH-247 "Is this normal for us?"

Written before any implementation code. Built with Claude Code (Opus 5.5) across the planning sessions exported to `ai-log/raw/`
(`52cccc9e…`, `f8d4159a…`, `cfcd6b64…`). Per the brief this file stays as-written once approved; anything that changes during
implementation is appended to **§13 Plan changes** with the reason, never edited in place.

---

## 1. Interpretation of the ticket

**Who:** a customer admin of one Relay account (single- or multi-location), Monday morning.
**Question they need answered at a glance:**
1. "Was last week normal *for us*?" — the account as a whole, compared with its own recent history.
2. "Which location needs attention?" — every location compared with *its own* history, ranked so the most unusual one is on top.

**What "normal" means here:** a week's inbound activity count falls inside the range this location/account usually produces,
derived from its previous 8 complete weeks with robust statistics (median + MAD). Outside the range → "above normal" / "below normal".
Not enough history → say so instead of guessing.

**What we are *not* building:** alerting/notifications, forecasting/ML (out of scope per product), cross-account benchmarks
(different persona — that's an account-manager view), outcome rates (deferred, §11).

## 2. What the seed data told us

Profiled independently in Python/SQLite before design (scripts in the planning session log). 20 accounts, 12,626 events,
`2026-02-01 10:57:44` → `2026-07-27 22:20:34` UTC.

| Finding | Consequence for the design |
|---|---|
| Data ends **Monday 2026-07-27**; system clock is Sep 2026 | Anchor "now" to the data, not the clock (D1). Default week = Jul 20–26 |
| The current week holds one day of data; weekends ≈ 25% of a weekday | Only ever compare **complete** weeks |
| **Account 6: 880 events in week of Jun 1** (805 on Jun 3 alone) vs ~70/week, all 15 sites, plausible fields | A mean baseline is poisoned for 8 weeks afterwards (mean 171 vs median 72.5 for week of Jul 20). Use median + MAD |
| **12 exact-duplicate pairs** (adjacent ids, every column equal) | De-duplicate at query time; keep raw rows (use data as-is) |
| 27 near-duplicates within 60s | Look like natural traffic — not de-duplicated |
| **Account 20 has zero events** | Empty state is a valid 200, not an error |
| Per-site per-type weekly medians are 3–6 | %-change is noise. Band must scale with each series' own variability |
| `location` is clean free text ("Site A"…"Site O"), 1–15 per account; no locations table | Site list derived from events |
| Local-week vs UTC-week bucketing moves only 8 events; hours look generated near US-East/UTC | Bucket in account local time (it's cheap and correct), but it isn't where correctness lives |
| ~400 NULL outcomes, 313 NULL call durations, missed calls with durations | Irrelevant to counts; noted, and a reason rates are deferred |
| All sites' first activity is in week Jan 26 or Feb 2 | Eligibility rule (§5.3) only matters for early weeks |

## 3. Decisions (with rejected alternatives)

| # | Decision | Rejected | Why |
|---|---|---|---|
| D1 | "Now" = latest event in the whole dataset (global anchor). Default week = latest complete local week | System clock; fixed config date; per-account anchor | Clock shows nothing on static data. Per-account anchor would make an account that stopped sending data look normal |
| D2 | Normal = own history (median ± 2 × robust spread over 8 weeks) | Sibling comparison (share of account); %-change vs mean | Sibling share is confounded when the whole account moves (spike week: all 15 sites keep their share → "all normal"), and useless for single-site accounts. Mean is poisoned by the spike; %-change cries wolf on small counts |
| D3 | Sibling view = account summary row + locations table sorted by deviation (same method, no second statistic) | Separate sibling statistic | One method, one set of edge cases; still separates "whole account moved" from "one site moved" |
| D4 | Metric = inbound activity count; default all types, event-type filter | Per-type columns; outcome rates | Totals have enough volume; per-type columns are mostly noise; rates deferred (§11) |
| D5 | Account switcher labelled "Viewing as" (impersonation for demo, not auth). Default account **12** (Redline Tire & Service, 7 sites, one flagged in the default week) | Hardcode one account; cross-account overview | Lets an evaluator see account 6, 20 and single-site accounts in the app, not only in tests |
| D6 | SQL does counting only; week math, zero-fill, statistics, ranking in pure C# | Everything in SQL; LINQ | Keeps product rules unit-testable without a DB. LINQ doesn't change the test story (InMemory/SQLite give different semantics) |
| D7 | Band threshold **2** × spread | Rank bands (2nd lowest–2nd highest); min–max; 3 × spread | Simulated on the seed (every site-week with a full baseline, spike excluded): rank band flags **34.6%** of sites/week, min–max 15.1%, **±2 → 4.8%**, ±3 → 0.3%. ±2 and ±3 both flag 15/15 sites in the spike week |

## 4. Assumptions & open questions (not sent to recruiter — working assumption stated)

| Question | Working assumption |
|---|---|
| Is the Jun 3 spike real (storm, campaign) or a bad import? | Unknown from the data. We show it as "above normal" and never exclude it; the median keeps it from distorting later weeks |
| Are exact duplicates real repeated events? | No — identical to the second with adjacent ids = ingestion duplicates. Counted once. Stated in UI footnote + README |
| Does "this week" mean the current partial week? | No — the last complete week; the partial one would read as a collapse every Monday |
| Week start day | Monday (ISO), in the account's IANA timezone |
| Does a site exist before its first event? | No — weeks before (and including) a site's first-activity week don't count toward its baseline |
| Is "All" = customers? | No — inbound events; a call, lead and appointment may be the same person (no customer id). UI says "inbound events" |

## 5. Design

### 5.1 Database
- SQL Server 2022 via `docker compose` (`db` service). Connection string in `appsettings.Development.json`, overridable by env var.
- EF Core migration `InitialCreate` mirrors `schema.sql`; table/column names kept **snake_case** via explicit configuration so the seed runs verbatim.
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
  "dataAsOf": "2026-07-27T22:20:34Z",
  "latestCompleteWeek": "2026-07-20",
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
   Displayed integer range `low = max(0, ⌈median − 2·spread⌉)`, `high = ⌊median + 2·spread⌋` (equivalent to the rule for integer counts).
7. `deviation` = (count − median) / spread, rounded to 2 dp for display only.
8. Ranking: `insufficient_data` last; otherwise |deviation| descending, then location name ascending.

Constants live in `NormalityOptions { BaselineWeeks = 8, MinimumEligibleWeeks = 4, BandWidth = 2.0 }`.

### 5.4 Frontend
- Current stable Angular, standalone components, signals; Vitest.
- Single route `/dashboard?account=12&week=2026-07-20&type=all`. **URL is the source of truth**: `DashboardState` maps query params → signals and writes changes via
  `router.navigate` (merge). Invalid/missing params → defaults, URL rewritten so reload is reproducible.
- `ActivityHealthApi`, `AccountsApi` are abstract-class DI tokens; `Http…` implementations provided in `app.config.ts`.
- Components: `DashboardPage` (container), `DashboardFilters` (Viewing-as select, ◀ week ▶ stepper bounded by `earliestWeek`/`latestCompleteWeek`, type select),
  `AccountSummary`, `LocationTable`. Presentational components are input/output only.
- Status as text + symbol (never colour alone): "▲ Above normal", "▼ Below normal", "Normal", "Not enough history (3 of 4 weeks)".
- Footnote: method in plain English, "inbound events, not unique customers", "exact duplicates counted once", "data as of Mon Jul 27".
- Dev proxy `/api` → backend.

## 6. Architecture & code rules

```
relay-activity-health/
  db/seed.sql, db/schema.sql         (starter files, unmodified)
  src/Relay.Core                     business logic + application service + query interfaces (no dependencies)
  src/Relay.Infrastructure           EF Core, migrations, raw SQL implementations of Core's query interfaces
  src/Relay.Api                      Minimal API endpoints, DI composition root
  tests/Relay.Core.Tests             unit (no DB, no mocks for BL)
  tests/Relay.Infrastructure.Tests   integration (Testcontainers SQL Server)
  tests/Relay.Api.Tests              API + golden tests (WebApplicationFactory + Testcontainers)
  web/                               Angular app
  .claude/agents/                    agent team definitions
  scripts/export-ai-log.sh
```

Interfaces (behaviour) — records/DTOs/options are plain data and have none:

| Interface | Project | Responsibility |
|---|---|---|
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
4. Test expectations come from the spec and the independent Python profile — never from the implementation under test.

## 7. Test plan

| Layer | Project | Kind |
|---|---|---|
| 1 Business logic | `Relay.Core.Tests` | Unit, pure |
| 2 Service | `Relay.Core.Tests` | Unit, hand-written fakes of `IActivityQueries`/`IAccountQueries` |
| 3 Data | `Relay.Infrastructure.Tests` | Integration, Testcontainers, hand-built fixtures |
| 4 API + golden | `Relay.Api.Tests` | Integration against the real seed |
| 5 Frontend | `web` (Vitest) | `DashboardState` URL round-trip + normalisation; `LocationTable`/`AccountSummary` states |

**Edge cases that must have tests**
- Calendar: DST start week (Mar 8 2026) and end week (Nov 1 2026) in America/Chicago; America/Phoenix (no DST); UTC; event exactly at a week boundary (belongs to the new week);
  latest complete week when the anchor is Monday vs Sunday 23:59:59 local vs exactly Monday 00:00 local; non-Monday week rejected; invalid IANA id.
- Grid: site with zero events in `W` appears with 0; site silent for the whole baseline; weeks on/before first-activity week ineligible; site whose first event is after `W` excluded.
- Evaluator: < 4 eligible weeks; MAD = 0 (floor applies); median 0; even-count median; spike inside baseline; low clamped at 0; exactly on the band edge is `normal`.
- Ranking: insufficient last; ties by name; above and below ranked by magnitude together.
- SQL: exact duplicates counted once, near-duplicates not; boundary instant; type filter; other accounts' rows ignored; events outside windows ignored; no rows → empty.
- API: 404 unknown account; 400 non-Monday / future week / bad type; default week; empty account 200.

**Golden values (from the independent Python model, §2)**
| Scenario | Expected |
|---|---|
| Account 6, week 2026-06-01, all | total 880, median 66, range 35–97, `above`; **all 15 sites `above`**, top = Site C (67, dev 36.95) |
| Account 6, week 2026-07-20, all | total 87, median 72.5, range 24–121, `normal` (baseline contains the 880 week; mean would be 171); all 15 sites `normal` |
| Account 6, week 2026-07-20, `call_received` | total 51, median 42, range 13–71, `normal` |
| Account 12, week 2026-07-20, all | total 54, median 56, range 39–73, `normal`; Site F 11 vs 1–10 → `above` (dev 2.35), ranked first |
| Account 1, week 2026-07-06, Site C | 4 (raw rows 5 — one exact duplicate) |
| Account 8, week 2026-03-02 | `insufficient_data` (3 eligible weeks) |
| Account 8, week 2026-03-09 | baseline 11,11,11,8 → median 11, MAD 0, spread √11 = 3.317, range 5–17, `normal` |
| Account 20 | empty state |
| Default week (any account) | 2026-07-20 |

## 8. Agent team & working model

Definitions in `.claude/agents/`, all bound by `CLAUDE.md`.

| Agent | Model | Owns | Writes |
|---|---|---|---|
| `product` | Opus 5.5 | Interpretation, acceptance criteria per slice, README interpretation/assumptions/deferred | Docs |
| `architect` | Opus 5.5 | Solution/projects/packages, all interfaces, records, API contract, stubs; rules on cross-layer changes | Contracts |
| `test-author` | Opus 5.5 | Red test suites for every layer; **never implements** | Tests |
| `backend` | Opus 5.5 | Core implementations, service, endpoints, DI | Code |
| `database` | Opus 5.5 | EF config, migrations, seed load, raw SQL, index, compose, Testcontainers fixture | Code |
| `frontend` | Opus 5.5 | Angular state, services, components | Code |
| `reviewer` | **Sonnet 5** | Read-only adversarial review per layer: rules, SOLID, §5.3 conformance, golden values | Findings |

Main thread = **coordinator only**: dispatches with precise context (spec section, file scope, done criteria), reviews output, decides
accept/reject/redirect, merges worktrees, runs verification, keeps `AI_LOG.md`. It doesn't write product code.
Reviewer on a different model than the authors is deliberate — a second model doesn't share the author's blind spots.

## 9. Execution phases & parallelisation

```
Phase 0  sequential   architect: move starter files to db/, solution + 3 src + 3 test projects, packages, Angular shell (web/),
                      docker-compose, contracts (interfaces, records, API DTOs, TS models) + NotImplemented stubs
                      → commit "contracts" → USER reviews contracts
Phase 1  parallel     A  test-author: Core red suite (calendar, grid, evaluator, ranker, service w/ fakes)
         (worktrees)  B  test-author: Infrastructure red suite     ‖  database: migrations + seed load + compose up
                      C  test-author: web red suite
                      → ONE checkpoint: USER reviews all red suites
Phase 2  parallel     backend → A green  ‖  database → B green  ‖  frontend → C green (against fake API)
                      → reviewer on each track as it lands → fixes
Phase 3  sequential   test-author: API + golden red suite → backend: endpoints green → frontend wired to real API
                      → reviewer full pass → product: README → coordinator: export AI log, reflection
```
Tracks touch disjoint paths (`src/Relay.Core`+`tests/Relay.Core.Tests` | `src/Relay.Infrastructure`+`tests/Relay.Infrastructure.Tests` | `web/`).
Only the architect touches `.sln`, `.csproj`, `package.json`, `Directory.*` — in Phase 0 — so parallel tracks never collide.
Contract changes discovered later go back through the architect.

## 10. Time budget & cut line

Planning has used roughly 2h (first session 16:33 UTC). Remaining target ≈ 3h. If behind, cut in this order:
1. Frontend component tests (keep the `DashboardState` URL test).
2. Type filter UI (keep the API param + tests).
3. Week stepper → plain date input.
Never cut: aggregate correctness, BL + SQL tests, golden tests, README, AI log. If out of time: stop and document in README.

## 11. Deferred (deliberately)
- Outcome rates (missed-call rate, lead conversion, no-show) — more actionable, but worse small-number problem and NULL-outcome decisions. First "another day" item.
- Second severity tier (|dev| > 3 "very unusual").
- Sibling-share statistic (confounded, see D2).
- Near-duplicate policy, spike root-cause annotation, trend sparklines, auth, caching.

## 12. AI log (minimal, no hooks)
- `scripts/export-ai-log.sh` copies every session and subagent transcript for this work (both project folders under `$CLAUDE_CONFIG_DIR/projects`) into
  `ai-log/raw/`, redacts the user's email and any SA password, and renders a readable `ai-log/sessions/<session>.md` (prompts, responses, tool calls).
  The coordinator runs it at each phase checkpoint and before the final push.
- `AI_LOG.md` is written **live** by the coordinator: every dispatch (agent, prompt summary, link to raw), outcome, accept/reject/redirect + why,
  user overrides. Ends with the reflection and the one-line tools/models statement.

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
  account 6's spike week still 15/15 `above`; 0 status/range contradictions in 253,149 checks.
- D5: default account **14** (Beacon Home Security, 4 sites) instead of 12 — under R2\* account 12 flags nothing in 2026-07-20, and account 14's
  Site B (2 vs usually 3–12, `below`) is the only flagged series in the whole seed that week.
- Starter files: `schema.sql` and `seed.sql` are currently at the repo root; Phase 0 moves them to `db/` with `git mv`, content untouched (user-approved).
- README must carry the known limits in `docs/design-consensus.md` §1 verbatim and state the ≈ 4 % design flag rate.

#### §5.1 — index and de-duplication (replaces the index bullet)
**No unique constraint** (it would reject the duplicate rows). Index `IX_activity_events_account_occurred` on
`(account_id, occurred_at) INCLUDE (location, event_type, duration_seconds, outcome)` — covers the de-duplication so the weekly query seeks.
Exact duplicates are removed only by `DISTINCT`/`GROUP BY` over every non-id column (never `=` on nullable columns). Windows are sent as
UTC (`Z`) JSON; instants read back are marked `DateTimeKind.Utc`. Columns are explicit `varchar(n)`.

#### §5.2 — API (replaces §5.2)
`GET /api/accounts` → `[{ id, name, timezone }]` (includes account 20).

`GET /api/accounts/{accountId}/activity-health?week=YYYY-MM-DD&type=all`

| Param | Rule |
|---|---|
| `week` | Optional local Monday. Default = latest complete week. Not a Monday → 400. After `latestCompleteWeek` → 400. Before `earliestWeek` → 400 |
| `type` | Exactly `all` (default) \| `call_received` \| `lead_created` \| `appointment_set`, case-sensitive; else 400 |
| `accountId` | Unknown → 404 |

Errors are `ProblemDetails`. Response (account 6, 2026-07-20, all):
```json
{
  "account": { "id": 6, "name": "Metro Collision Centers", "timezone": "America/New_York" },
  "eventType": "all",
  "week": { "start": "2026-07-20", "end": "2026-07-26" },
  "dataAsOf": "2026-07-27T22:20:34Z",
  "latestCompleteWeek": "2026-07-20",
  "earliestWeek": "2026-01-26",
  "baselineWeeks": 8,
  "minimumEligibleWeeks": 4,
  "summary": { "count": 87, "baseline": { "weeksUsed": 8, "median": 72.5, "low": 30, "high": 134 }, "status": "normal", "deviation": 0.53 },
  "locations": [ { "location": "Site M", "count": 7, "baseline": { "weeksUsed": 8, "median": 3.5, "low": 1, "high": 9 }, "status": "normal", "deviation": 1.30 } ]
}
```
- `status ∈ above | below | normal | insufficient_data`.
- `baseline` is always present. When `insufficient_data`, `baseline = { weeksUsed: 0–3, median: null, low: null, high: null }` and `deviation: null`.
- `earliestWeek` = local Monday of the week containing the account's first event (any type); for an account with no events it equals `latestCompleteWeek` (never null).
- `deviation` is a z-score on the Anscombe scale, rounded to 2 dp (away from zero). `median` is unrounded (x or x.5). `low`/`high` are integers.
- `locations` is returned sorted (§5.3 step 9).
- Empty account → 200, `summary.count = 0`, `insufficient_data`, `baseline.weeksUsed = 0`, `earliestWeek = latestCompleteWeek`, `locations: []`.

#### §5.3 — normality rules (replaces §5.3)
Notation: T(x) = 2·√(x + 0.375) (Anscombe transform; makes small counts roughly equal-variance). T(0) = 2·√0.375 = 1.224744871391589.

For the account total and for each site, for selected week `W`:
1. **Sites** = distinct locations whose first event (any type) is before the end of `W` (local next-Monday 00:00 in UTC, exclusive). The type filter never changes the site list.
2. **Baseline weeks** = the 8 local weeks before `W`, **zero-filled**. A week is *eligible* only if it starts **after** the week containing the series'
   first event of any type (site → site's first event; account total → account's first event = MIN over its sites). `weeksUsed` = number of eligible weeks.
3. Fewer than **4** eligible weeks → `insufficient_data`: count shown; median, low, high and deviation are null.
4. `median` = median of the eligible weeks' counts only (mean of the middle two when even). This is the displayed median.
5. `centre = T(median)` — T of the raw median, **not** the median of the transformed values (they differ for even counts); `madT` = median of |T(cᵢ) − centre| over the eligible counts; `spread = max(1.4826 × madT, 1.0)`
   (1.4826 makes the MAD comparable to a standard deviation; 1.0 is the Poisson SD on this scale).
6. `lowT = centre − 2·spread`, `highT = centre + 2·spread`.
   `low = lowT ≤ T(0) ? 0 : ⌈(lowT/2)² − 0.375⌉` (the guard is mandatory: squaring a negative `lowT` would create a false lower edge);
   `high = ⌊(highT/2)² − 0.375⌋`.
7. Status from the integers only: `count < low` → `below`; `count > high` → `above`; otherwise `normal` (a count equal to `low` or `high` is `normal`).
   The displayed range "usually low–high" therefore can never contradict the status.
8. `deviation = (T(count) − centre) / spread`, full precision internally; rounded to 2 dp (away from zero) only in the API response.
9. Ranking of locations: `insufficient_data` last (among themselves by name); then flagged (`above`/`below`) before `normal`; then |deviation|
   descending on the unrounded value; then `below` before `above`; then location name ascending (ordinal).

Constants live in `NormalityOptions { BaselineWeeks = 8, MinimumEligibleWeeks = 4, BandWidth = 2.0, SpreadFloor = 1.0 }`.
Known limit: a series whose usual median is ≤ 2 can never be `below` (a drop to 0 is within normal variation there).

#### §5.4 — status copy and footnote (replaces the two §5.4 bullets on status and footnote)
| Status | Label |
|---|---|
| above | ▲ Higher than usual |
| below | ▼ Lower than usual |
| normal | Within usual range |
| insufficient_data | Not enough history yet (N of 4 weeks needed) |

Symbol and text are always shown together, never colour alone.

- **Range line:** "Usually X–Y a week", where X–Y is the API's `low`–`high`, never recomputed in the UI. Account row: "54 inbound events · usually 40–74 a week".
- **On screen:** no deviation, z, σ, "±" or median.
- **Severity tier:** none; stays deferred (§11).
- **Insufficient data:** the count is shown, with no range, and the label above. N is `baseline.weeksUsed` and 4 is `minimumEligibleWeeks`.
- **Account with no events:** "No activity recorded for this account yet." Shown when `locations == [] && summary.baseline.weeksUsed == 0`. The week stepper is disabled because `earliestWeek == latestCompleteWeek`.
- **Median:** not shown on screen (no "typical ~N"); "Usually X–Y" answers the question.
- The word "Normal" never appears alone on screen; "Within usual range" is used in the table, the summary and the footnote.
- **Footnote** (plain English):
  - method: "compared with the last 8 full weeks at this location";
  - "inbound events, not unique customers";
  - "exact duplicates counted once";
  - "Locations that usually get 2 or fewer events a week can't show 'lower than usual'";
  - "Data as of Mon Jul 27, 2026", with `dataAsOf` rendered in the account timezone.
- **Type ≠ all:** one extra line, "Per-type counts at a single location are small; only large changes show up."

#### §7 — golden values and Evaluator / Ranking / API edge cases (replace the corresponding parts of §7)
**Golden values (R2\*, from the independent Python model)**
| Scenario | Expected |
|---|---|
| Account 6, week 2026-06-01, all | total 880, median 66, range 39–101, `above`, dev 22.37; **all 15 sites `above`**; top = Site C (67, median 3, range 1–7, dev 12.74) |
| Account 6, week 2026-07-20, all | total 87, median 72.5, range 30–134, `normal`, dev 0.53 (baseline contains the 880 week); all 15 sites `normal`; Site M 7, median 3.5, range 1–9, dev 1.30 |
| Account 6, week 2026-07-20, `call_received` | total 51, median 42, range 17–79, `normal`, dev 0.54 |
| Account 12, week 2026-07-20, all | total 54, median 56, range 40–74, `normal`; Site F 11 vs 2–11 → `normal` (dev 1.90), ranked first |
| Account 14, week 2026-07-20, all (default account) | total 26, median 27, range 18–38, `normal`; **Site B 2 vs 3–12 → `below` (dev −2.16), ranked first**; Sites C, A, D `normal` |
| Account 1, week 2026-07-06, Site C | 4 (raw rows 5 — one exact duplicate) |
| Account 8, week 2026-03-02 | `insufficient_data`, `weeksUsed` 3 |
| Account 8, week 2026-03-09 | baseline 11,11,11,8 → median 11, madT 0, spread 1.0 (floor), range 6–18, `normal` |
| Account 20 | default week → 200 empty state, `earliestWeek` = `latestCompleteWeek` = 2026-07-20; `week=2026-03-02` → 400 |
| Default week (any account) | 2026-07-20 |
| `earliestWeek` | 2026-01-26 for accounts 1, 4, 5, 6, 7, 12, 14, 18; 2026-02-02 for 2, 3, 8–11, 13, 15–17, 19; 2026-07-20 for 20 |

Hand derivation, account 8 on 2026-03-09: median 11; centre = 2√11.375 = 6.745369; three of the four |T(cᵢ) − centre| are 0, so madT = 0 and spread = 1.0;
lowT = 4.745369 > T(0), so low = ⌈2.3726845² − 0.375⌉ = ⌈5.2546⌉ = 6; highT = 8.745369, so high = ⌊4.3726845² − 0.375⌋ = ⌊18.745⌋ = 18.

Even-count centre case (pins centre = T(raw median)), baseline [2,4,6,20]: median 5, centre 4.636809, madT 1.004056 (gaps 1.554602, 0.453509, 0.412943,
4.390926 → middle two average), spread 1.488613, lowT 1.659583 > T(0) → low ⌈0.3136⌉ = 1, highT 7.614035 → high ⌊14.118⌋ = 14.
Count 14 → `normal` (z 1.98); 15 → `above`; 0 → `below`. (Under the wrong centre = median of T the range is 1–13 and 14 reads `above`.)

**Evaluator edge cases (unit, no DB)**
- Fewer than 4 eligible weeks.
- madT = 0, so the floor applies.
- Even-count median.
- Spike inside the baseline.
- Count exactly `low` or `high` → `normal`.
- Rows from `statistician_evidence_out.md` §2:
  - [0,0,0,0]→0 gives 0–2 normal.
  - [0,0,0,0]→3 gives above.
  - [2,2,2,2]→0 gives 0–6 normal.
  - [3,3,3,3]→0 gives 1–7 **below** (z −2.45).
  - [11,11,11,8]→6 gives normal (edge).
  - [11,11,11,8]→5 gives below.
- **Low guard:** a case with lowT < 0 (e.g. median 1 with spread 3 → low 0), one with lowT in (0, T(0)] → low 0, and one just above T(0) ([2,4,6,20], lowT 1.6596 → low 1).
  Concrete discriminating baselines (`analysis/debate/statistician_guard_case.py` → `_out.md`): lowT < 0: [0,1,5,9] → median 3, lowT −1.927794, range **0–21**
  (without the guard the false edge gives low 1 — prefer this test); 0 < lowT ≤ T(0): [1,1,1,1] → lowT 0.345208, range 0–4; just above T(0): [2,4,6,20] → low 1, or [3,3,3,3] → 1–7.
  With the T(0) guard the ceiling argument is always > 0 (lowT > T(0) ⇒ (lowT/2)² − 0.375 > 0), so low ≥ 1 in that branch; a `Math.Max(0, …)` is defensive only.

**Ranking:** insufficient last (by name); flagged before normal; above and below by |deviation| together; equal |deviation| → below before above, then name.

**API:**
- 400 for a week before `earliestWeek`.
- 400 for `type=ALL` / `Call_Received` (case-sensitive).
- Account 20: default week → 200 empty with `earliestWeek` 2026-07-20; `week=2026-03-02` → 400.

### 2026-09-28 — Phase 0 split by specialty; project-file ownership per stack (user decision)

**Reason.** The user: *"architect is not an implementer"* — scaffolding belongs to the stack specialists. Supersedes the Phase 0 line in §9 and the "Writes/Owns" split in §8.
- **Phase 0** now runs as: `backend` (.NET solution, projects, `Directory.*.props`, central packages, `global.json`, minimal `Program.cs`, `git mv` of the starter files to `db/`)
  ‖ `frontend` (Angular shell, Vitest, dev proxy, `dashboard` route) in parallel worktrees → merge → `architect` adds **contracts only** (interfaces, records, DTOs, DI extension
  signatures, TS models, abstract API tokens) with `NotImplementedException` stubs → reviewer → **user reviews contracts**.
  In parallel, `product` writes `docs/acceptance-criteria.md`.
- **Ownership after Phase 0:** `backend` owns `.sln`, `*.csproj`, `Directory.*.props`, `global.json`; `frontend` owns `package.json`, `angular.json`; `architect` owns public contracts
  and never edits project files. Other agents request package/project changes through the coordinator.
- **Every** agent task runs in its own worktree (not only parallel ones); after each feature the `reviewer` runs and its findings are fixed, with unclear ones escalated to the user first.

### 2026-09-28 — Phase 0 decisions and promoted golden values (user decisions)

**Reason.** Raised by the scaffold and acceptance-criteria reviews; decided by the user.
- **API port 5080** (fixed, in `launchSettings.json` and `web/proxy.conf.json`): macOS AirPlay Receiver holds :5000 on the dev machine.
- **C# test names are PascalCase** (`MethodConditionExpectedOutcome`); CA1707 stays enabled for tests.
- **`dotnet test` exit code 8 with zero tests is accepted** until each red suite lands; it is not suppressed.
- **Promoted golden values.** Seed scenarios verified independently by the reviewer and recomputed by the coordinator are added to §7 goldens (R2\*; the §13 ranking rule).
  Source: `analysis/goldens/promoted_goldens.py` → `promoted_goldens_out.md`.

| Scenario | Expected |
|---|---|
| Account 6, week 2026-06-08, all (week after the spike) | total 102, median 66, range 37–104, `normal`; Site C 11 vs 1–8 `above` (ranked 1st), Site J 11 vs 2–10 `above` (2nd); the other 13 sites `normal` |
| Account 6, week 2026-06-29, all (location going silent) | total 69, range 41–111, `normal`; Site G 0 vs 2–9 `below` (dev −3.19), ranked first |
| Account 8, week 2026-07-20, all (single-site account) | total 7, median 10, range 5–17, `normal`; Site A identical |
| Account 8, week 2026-03-02, all | count 8, `insufficient_data`, `weeksUsed` 3 |
| Account 14, week 2026-01-26, all (earliest week) | total 2, `insufficient_data`; only Sites B and D listed (1 each), both `insufficient_data` |
| Account 14, week 2026-02-02, all | total 27, `insufficient_data`; all four sites listed, all `insufficient_data` (0 eligible weeks) |
| Account 14, week 2026-03-02, all (mixed history) | total 40, median 25, range 16–36, `above`; Site D 16 vs 2–11 `above` (1st), Site B 9 vs 2–10 `normal` (2nd), Sites A and C `insufficient_data` (last, by name) |
| Account 14, week 2026-07-20, all | ranking order B, C, A, D |
| Account 14, week 2026-07-20, `appointment_set` (ties) | total 2, range 1–8, `normal`; Site A 0 vs 0–4 (dev −1.12) first; Sites B, C, D have deviation 0 → ordered by name B, C, D |
| Account 18, week 2026-03-23, all (7 eligible weeks) | total 18, median 23, range 15–33, `normal`; Site C 0 vs 1–9 `below` (dev −2.90), ranked first |

### 2026-09-28 — Input handling and UI copy decisions (user decisions, validated by the architect)

**Reason.** Open items from the acceptance-criteria review (`docs/acceptance-criteria.md` §0.2 at the time). Each was decided by the user and validated by the
`architect` against the contracts (no conflicts). Supersedes the §5.4 status/footnote copy only where stated; the approved strings live verbatim in `docs/acceptance-criteria.md` §0.1.
- **API input:** malformed `week` (not `yyyy-MM-dd`, e.g. `2026-13-01`, `20260720`, `abc`) → 400 `ProblemDetails`; non-numeric `accountId` → 404 (route constraint `{accountId:int}`).
- **URL normalisation:** any invalid URL parameter is rewritten to its default (latest complete week, `all`, account 14) with `replaceUrl` — never snapped to the nearest Monday.
- **Account switch:** week and type are kept; if the API rejects the kept week (before the new account's `earliestWeek` → 400), the UI falls back to the latest complete week and rewrites the URL.
- **Empty account:** "No activity recorded for this account yet." replaces both the summary and the table; filters stay visible; the week stepper is disabled.
- **Load failure:** "We couldn't load this week's activity. Try again." with a "Try again" button (`DashboardState.reload()`); loading shows "Loading…".
- **Additional copy:** type select "Activity type" — "All activity" / "Calls" / "Leads" / "Appointments"; week label "Mon Jul 20 – Sun Jul 26, 2026"; stepper "◀ Previous week" / "Next week ▶";
  table headers "Location" / "Events" / "Usual range" / "Status"; summary heading "{account name} — all locations"; account method line "compared with the last 8 full weeks for this account".
- **Nouns by type:** "N calls" / "N leads" / "N appointments" (singular for 1) when filtered; "inbound events" / "1 inbound event" only for `all`.
- **Footnote lines** render with the first letter capitalised (wording unchanged). "Not enough history yet (0 of 4 weeks needed)" is kept for 0 weeks.

### 2026-09-28 — Contract decisions from the Phase 0 contract review (user decisions, validated by the architect)

**Reason.** Raised by the reviewer on the architect's contracts; decided by the user; validated by the `architect`.
- **Core stays package-free:** `AddRelayCore` lives in `Relay.Api/Composition`; Core receives `NormalityOptions` as plain data (still bound and `ValidateOnStart`-validated).
- **Wire names owned by Core:** one mapping for `ActivityType` (`all|call_received|lead_created|appointment_set`) and `HealthStatus` (`above|below|normal|insufficient_data`),
  used by the API mapper, the API `type` validation (pattern derived from it) and Infrastructure SQL.
- **Malformed `week` → 400** via a `yyyy-MM-dd` validation attribute on a `string? Week` (the architect's choice, delegated by the user): same validation-problem shape as `type`;
  `07/20/2026` and `2026-7-20` are rejected too.
- **Instants stay `DateTime` (UTC)** in Core and the API response, serialised as `…Z` exactly as §13 §5.2; an API golden test asserts the exact `dataAsOf` string. (`DateTimeOffset` was rejected: it serialises as `+00:00`.)
- **Empty database (no events at all):** 200 empty state; `dataAsOf` is **null** (the UI hides "Data as of"); the latest complete week falls back to the current clock via an injected
  `TimeProvider`. D1 still holds whenever data exists. Contract change: `dataAsOf` nullable in C#, TypeScript and §5.2 for this case only.
- **Account method line** is capitalised: "Compared with the last 8 full weeks for this account".
- **More promoted golden values** (source `analysis/goldens/promoted_goldens.py` → `promoted_goldens_out.md`):

| Scenario | Expected |
|---|---|
| De-duplicated events per account, all weeks | 1: 1221 · 2: 729 · 3: 477 · 4: 796 · 5: 884 · 6: 2637 · 7: 437 · 8: 260 · 9: 546 · 10: 342 · 11: 354 · 12: 1303 · 13: 205 · 14: 638 · 15: 499 · 16: 167 · 17: 323 · 18: 586 · 19: 210 · 20: 0 · total 12614 |
| Account 14 site first-activity weeks | Sites B and D: 2026-01-26; Sites A and C: 2026-02-02 |
| Account 14, week 2026-07-20, `appointment_set` | Site B 0, median 0, range 0–2, `normal` |
| Account 14, week 2026-07-20, `call_received` / `lead_created` | totals 16 (range 9–24, `normal`) / 8 (range 2–12, `normal`) |
| Account 6, week 2026-07-20, all | Site M is `locations[0]` (7, range 1–9, dev 1.30) |

### 2026-09-28 — Last Phase 0 clarifications (user decisions, validated by the architect)

- **§13 §5.2 `dataAsOf`** is `string` in the response, **or `null` only when the database has no events at all**; for every account (including account 20) it is the global anchor when data exists.
- **Empty `?week=`** (present but empty) → 400, like `?type=`. "Latest complete week" is requested by omitting the parameter; the UI never emits `week=`.
- **Account 20's empty-state page** still shows the full footnote, including "Data as of Mon Jul 27, 2026"; the "Data as of" line is hidden only when `dataAsOf` is null.

### 2026-09-28 — Calendar contract simplified (user decision at contract review, proposed by the architect)

**Reason.** User at contract review: `IWeekCalendar` was *"a little bit overengineered"*. Supersedes the `IWeekCalendar` row in §6.
- `IWeekCalendar` has three members: `Window(weekStart, timeZoneId)` (DST-correct local Monday → UTC half-open window), `WeekContaining(instantUtc, timeZoneId)`,
  `LatestCompleteWeek(dataAnchorUtc, timeZoneId)`. The time-zone id stays a string resolved inside the calendar.
- The Monday check (`InvalidWeek(NotAWeekStart)`) and the list of 8 baseline windows (oldest first, one `Window` per preceding Monday) live in `ActivityHealthService`.
- Failure contract: an invalid IANA id → `TimeZoneNotFoundException`; a non-Monday `weekStart` passed to `Window` → `ArgumentException`. No custom exception type.
- Rejected: dropping `LatestCompleteWeek` (moves a named domain rule into the service); a per-time-zone calendar factory (two interfaces to remove one argument).

### 2026-09-28 — Contract simplification (/simplify) (user decisions; the DTO question decided by the architect at the user's request)

**Reason.** The user ran `/simplify` over the contracts before locking them (four reviewers: reuse, simplification, efficiency, altitude).
Supersedes the §13 "Contract decisions…" bullet "Wire names owned by Core" and the §6 table's type names.
- **Wire names:** enums serialise via one `JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower, allowIntegerValues: false)` in `AddRelayApi`; the strict, case-sensitive `type`
  validation is built from `Enum.GetValues<ActivityType>()` through the same naming policy; Infrastructure SQL uses the same policy. No hand-written name tables.
- **No parallel DTO set** (architect's decision, weighed against SOLID/clean architecture): the application service's output records are serialised directly. Core records are shaped
  like the §5.2 JSON (`WeekRange`, `Baseline`, `SeriesHealth(Count, Baseline, Status, Deviation)`, `LocationHealth : SeriesHealth`, `ActivityHealthReport`) but carry no serializer
  attributes; casing, enum names and date formats are configured once in the API. Rounding `deviation` to 2 dp (away from zero) stays at the API boundary (`WithDisplayDeviations()`);
  ranking uses full precision. Rejected: a separate DTO layer (every §5.2 change in four places), serializer attributes in Core, a name-based rounding resolver.
- **Leaner types:** `WeeklySeries(SelectedWeekCount, EligibleWeekCounts)` (no per-week eligibility flags); `IActivityHealthService.GetAsync(accountId, week, eventType, ct)` (no query record);
  one endpoint-mapping class. Account-series eligibility still uses the account's own first event (§5.3), so it is not derived from location series.
- **JSON numbers** are compared numerically (the §5.2 example `1.30` serialises as `1.3`); `locations[]` property order is location, count, baseline, status, deviation (pinned by an API golden test).
- Unchanged by this pass: Monday check in the service, strict `yyyy-MM-dd` week attribute, `TimeProvider` fallback, `NormalityOptions`, `AddRelayCore` in Api, `DateTime` UTC instants.

### 2026-09-28 — Phase 1 red-suite decisions (user decisions, validated by the architect)

**Reason.** Raised by the reviews of the database work and the three red suites.
- **No committed secrets:** the dev SA password is never committed. Compose requires `RELAY_DB_SA_PASSWORD` from a git-ignored `.env` (`.env.example` holds a placeholder);
  the API connection string comes only from the environment (`ConnectionStrings__Relay`), not `appsettings.Development.json`. Supersedes §5.1's "connection string in
  `appsettings.Development.json`". Migrate-on-start fails fast with a clear message when it is missing. Testcontainers are unaffected. Compose keeps `platform: linux/amd64`.
- **De-duplication treats NULL, `''` and `0` as equal:** two rows identical except `outcome` NULL vs `''` or `duration_seconds` NULL vs `0` are one event
  (`DISTINCT`/`GROUP BY` over every non-id column with `COALESCE(duration_seconds, 0)` and `COALESCE(outcome, '')`; `=` self-joins still forbidden). Seed impact: none (still 12,614).
- **`GET /api/accounts` is ordered by name** (ordinal, ties by id), in `AccountService` — SQL stays free of `ORDER BY`.
- **Precedence:** malformed input (`week`/`type` shape) → 400 from validation before anything else; then unknown account → 404; then `NotAWeekStart`; then before-earliest / after-latest.
- **`earliestWeek = min(week of the first event, latestCompleteWeek)`:** an account whose first event is in the incomplete anchor week behaves like the empty account.
- **UI:** `type` is validated client-side against `EVENT_TYPES` and rewritten to `all` before any request (an invalid type is never sent). User actions add history entries
  (no `replaceUrl`); only normalisation rewrites replace. With no `week` in the URL and a failing first request, the URL keeps the known defaults without `week`, the load error and
  "Try again" show, and the week is filled in (replace) after the first successful load.
- **Component contracts:** `LocationTable` (`locations`, `minimumEligibleWeeks`), `AccountSummary` (`report`), `DashboardFilters` (accounts, accountId, week, earliestWeek,
  latestCompleteWeek, eventType → accountSelected, weekSelected, eventTypeSelected); the stepper's disabled state is derived; the footnote stays in `DashboardPage`.

### 2026-09-28 — DST transitions at local midnight are out of scope (architect ruling, delegated by the user)

**Reason.** Raised by the reviewer on the Core implementation: an untested branch handled zones whose DST transition falls at local midnight, and its ambiguous-midnight reading
disagreed with `WeekContaining`. Time zones whose DST transition falls at local midnight (Monday 00:00 skipped or repeated) are out of scope: a skipped midnight fails the request (500)
and a repeated one uses .NET's standard-time reading; no seed zone is affected (the US zones switch at 02:00). No tests are added for out-of-scope behaviour; the §7 US DST, Phoenix
and UTC cases stay covered. The README's known limits carry this line.

### 2026-09-28 — Phase 2 review decisions (user decisions, validated by the architect)

- **Filters without a report:** "Viewing as" and "Activity type" render even before a report has loaded (e.g. the first load fails); only the week stepper waits for data.
  Contract: `DashboardFilters` inputs `week`, `earliestWeek`, `latestCompleteWeek` are optional (`null` by default); the stepper is hidden until all three are set.
- **Page heading** `Activity health` (the page's only `<h1>`) is approved copy (C-23).
- **Insufficient rows** leave the "Usual range" cell empty (no placeholder).
- **Accepted as is (reviewer notes):** the global data-anchor query scans the index (fine at seed scale; a README "another day" item); windows with `DateTimeKind.Unspecified`
  are treated as UTC (Core always sends UTC); a failed account-list load leaves "Viewing as" empty without an error.

### 2026-09-28 — End-to-end smoke layer (user decision, validated by the architect)

**Reason.** The user asked whether E2E tests were part of the plan (they were manual only) and chose to add an automated smoke layer **as the very last step**. It proves the real browser,
Angular app, API and seeded SQL Server work together. Adds §7 layer 6 and a Phase 4 to §9.
- **Layer 6 — E2E smoke** (`web/e2e/*.e2e.ts`, Playwright, Chromium, 4–6 tests): default view (account 14, latest complete week 2026-07-20, Site B "▼ Lower than usual" with
  2 vs "Usually 3–12 a week" ranked first, order B, C, A, D — first §13 golden table); account 6 week 2026-06-01 (total 880, all 15 sites `above`); account 20 empty state;
  invalid `week=2026-07-21` rewritten to 2026-07-20 (replace); optionally a filter change undone by Back. Expectations only from §7/§13 golden values and approved copy;
  selectors by role and approved copy, no test ids.
- **Phase 4 (last):** only after the API + golden suite is green, frontend's manual end-to-end check, product acceptance and the reviewer's full pass.
  `frontend` adds `@playwright/test`, `playwright.config.ts`, the `e2e` script and ignores `test-results/`, `playwright-report/`; `test-author` writes the specs; the reviewer reviews them.
- **Red-first deviation:** the app is already green, so each spec proves it can fail by fault injection on a throwaway branch (one targeted mutation per spec, red output recorded,
  mutation discarded). A stopped API is not accepted as proof.
- **Running:** Playwright `webServer` starts the API (`dotnet run --project ../src/Relay.Api`, `ConnectionStrings__Relay` from the environment) and `ng serve`, reusing running servers;
  the DB via `docker compose up -d db`; the API migrates and seeds on start. No secrets committed. Excluded from `npm test`; runs via `npm run e2e`.
- **Cut line:** first item cut in §10, ahead of the frontend component tests.
- **README:** `docker compose up -d db && cd web && npx playwright install chromium && npm run e2e` (with `ConnectionStrings__Relay` set).

### 2026-09-28 — OS-agnostic local configuration via DotNetEnv (user decision, validated by the architect)

**Reason.** The run steps required `set -a; source .env; set +a` (bash/zsh only). User: *"We must find an env agnostic solution"* and *"Avoid hand written as much as possible;
if there is already a library for that, do not reinvent the wheel"* (the architect's first proposal was a hand-written reader).
- In Development only, the API adds the repo-root `.env` as a configuration source with **DotNetEnv 3.2.0** (`AddDotNetEnv(…, LoadOptions.TraversePath().NoClobber().NoEnvVars())`).
  `.env` is unchanged: its `ConnectionStrings__Relay` line is interpolated from `RELAY_DB_PORT` / `RELAY_DB_SA_PASSWORD`, the same file Docker Compose reads. Real environment variables and
  test configuration still win; the process environment is not modified; Production never reads `.env`.
- `RelayDesignTimeDbContextFactory` is removed; `dotnet ef … --startup-project src/Relay.Api` uses the same configuration (Api references `Microsoft.EntityFrameworkCore.Design`, private).
- Run steps are identical on Windows, macOS and Linux: `cp .env.example .env` → set the password → `docker compose up -d --wait db` → `dotnet run --project src/Relay.Api` →
  `cd web && npm start`. A password containing `$` must be single-quoted in `.env`.
- Test-first: four startup tests (interpolated connection string from `.env`; real env var wins; Production ignores `.env`; process environment untouched).

