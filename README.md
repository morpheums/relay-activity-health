# Relay — Activity health (DASH-247)

A Relay customer admin opens the dashboard on Monday morning and gets two answers:

1. **"Is last week normal for us?"** The account's total, compared with the account's own recent weeks.
2. **"Which of my locations needs attention?"** Every location compared with its own recent weeks. The unusual ones are listed first.

Spec: [`PLAN.md`](PLAN.md). It was written before the code and is append-only; later decisions are in §13 with a reason for each.
Acceptance criteria and approved UI copy: [`docs/acceptance-criteria.md`](docs/acceptance-criteria.md). AI interaction log: [`AI_LOG.md`](AI_LOG.md).

**Contents:** [Quick start](#quick-start) · [Prerequisites](#prerequisites) · [Configuration](#configuration) · [Run the app](#run-the-app) · [Run the tests](#run-the-tests) · [Troubleshooting](#troubleshooting) · [API reference](#api-reference) · [Interpretation of the ticket](#interpretation-of-the-ticket) · [Key assumptions](#key-assumptions) · [Design decisions and trade-offs](#design-decisions-and-trade-offs) · [Data handling](#data-handling) · [Known limits](#known-limits) · [Deliberately deferred](#deliberately-deferred) · [With another day](#with-another-day) · [Stack](#stack) · [Project structure](#project-structure) · [How AI was used](#how-ai-was-used)

---

## Quick start

```bash
cp .env.example .env                    # then set RELAY_DB_SA_PASSWORD in .env
docker compose up -d --wait db
set -a; source .env; set +a
dotnet run --project src/Relay.Api      # terminal 1, http://localhost:5080
cd web && npm ci && npm start           # terminal 2, http://localhost:4200/dashboard
```

Tests: `dotnet test` (Docker running) and `cd web && npm test`.

---

## Prerequisites

| Need | Version | Notes |
|---|---|---|
| .NET SDK | **10.0.201** (`global.json`, `rollForward: latestFeature`, so any 10.0.2xx or later feature band works) | Target framework `net10.0` (`Directory.Build.props`). `global.json` selects the Microsoft.Testing.Platform test runner |
| Node.js | `^22.22.3`, `^24.15.0` or `>=26.0.0`, the engines range of Angular 22.2 (`web/package-lock.json`). Verified with v26.0.0 | `web/package.json` declares no `engines` of its own |
| npm | **11.12.1** (`"packageManager": "npm@11.12.1"` in `web/package.json`) | `npm ci` installs from the lockfile |
| Docker | Any recent Docker Desktop or Engine with Compose v2. Verified with 29.5.3 | It runs the SQL Server container for the app. **The Infrastructure and API test suites also need Docker running**, because Testcontainers starts its own SQL Server containers |
| Global Angular CLI | **Not needed** | `npm start` and `npm test` run the project-local `ng` from `web/node_modules` |
| `dotnet-ef` tool | **Not needed to run** | The API applies migrations on startup in Development. You need `dotnet-ef` only to author new migrations; the repo has no tool manifest |

**Ports**

| Port | Used by | Set in |
|---|---|---|
| 1433 | SQL Server (host side) | `RELAY_DB_PORT` in `.env`, default 1433 (`docker-compose.yml`) |
| 5080 | API | `src/Relay.Api/Properties/launchSettings.json` (`applicationUrl`) and the dev proxy target in `web/proxy.conf.json` |
| 4200 | Angular dev server | Angular default (`ng serve`; `angular.json` sets no port) |

**Apple Silicon:** the SQL Server 2022 image has no arm64 build. `docker-compose.yml` sets `platform: linux/amd64`, so it runs under Rosetta. Enable "Use Rosetta for x86_64/amd64 emulation" in Docker Desktop. The first start is slower.

---

## Configuration

Nothing secret is committed. `.env` is git-ignored, and `.env.example` holds only a placeholder password.

| Variable | Purpose | Default | Read by |
|---|---|---|---|
| `RELAY_DB_SA_PASSWORD` | SQL Server `sa` password | **None, required.** Compose stops with `set RELAY_DB_SA_PASSWORD in .env` if it is missing | `docker compose`, which reads `.env` automatically and passes it to the container as `MSSQL_SA_PASSWORD`. It is also embedded in `ConnectionStrings__Relay` |
| `RELAY_DB_PORT` | Host port for SQL Server | `1433` | `docker compose` (port mapping); embedded in `ConnectionStrings__Relay` |
| `ConnectionStrings__Relay` | API connection string (`ConnectionStrings:Relay` in .NET configuration) | Built in `.env.example` from the two variables above: `Server=localhost,${RELAY_DB_PORT};Database=relay;User Id=sa;Password=${RELAY_DB_SA_PASSWORD};TrustServerCertificate=True` | The API, **only from the environment** (it is in no `appsettings*.json`), and the EF design-time factory (`RelayDesignTimeDbContextFactory`) when you run `dotnet-ef`. The API does **not** read `.env` itself; export it with `set -a; source .env; set +a` |
| `ASPNETCORE_ENVIRONMENT` | Environment name | `Development`, set by `launchSettings.json` for `dotnet run` | The API. Migrate-on-start and the missing-connection-string check run **only in Development**. In Production a missing connection string surfaces as a 500 on the first request |

The tests need none of these. Testcontainers creates its own databases and connection strings.

**Password rules:** SQL Server rejects weak `sa` passwords, and the container then exits. Use at least 8 characters from three of the four groups: upper case, lower case, digits, symbols. Avoid `$` (Compose and the shell expand it), `"`, and `;` (it ends a value in the connection string).

**Changing the password or resetting the data:** the `relay-db-data` volume keeps the password it was created with. Changing `.env` later does **not** change it. To reset, run `docker compose down -v` (this **deletes the database**), then `docker compose up -d --wait db`. The next API start recreates and reseeds the database.

---

## Run the app

1. **Configure:** `cp .env.example .env`, then set `RELAY_DB_SA_PASSWORD`.
2. **Database:** `docker compose up -d --wait db`. This returns when the container's `sqlcmd` health check passes (it retries for up to about 3 minutes).
3. **API** (terminal 1, repo root):
   ```bash
   set -a; source .env; set +a
   dotnet run --project src/Relay.Api
   ```
   It listens on **http://localhost:5080**. On the first start in Development it creates the `relay` database, applies `InitialCreate` (the schema) and `LoadSeedData` (runs `db/seed.sql`, about 12.6k rows, in about 1 s). It logs `Applying Relay database migrations on start (Development)`, and the port opens only after migration finishes, so the first start takes a few seconds longer. If `ConnectionStrings__Relay` is missing, it stops at startup with:
   `The connection string 'ConnectionStrings:Relay' is missing or empty. Set the environment variable 'ConnectionStrings__Relay' before starting the API.`
   Quick check: `curl -s http://localhost:5080/api/accounts` returns 20 accounts.
4. **Web** (terminal 2): `cd web && npm ci && npm start`. `ng serve` proxies `/api` to `http://localhost:5080` (`web/proxy.conf.json`).
5. **Open http://localhost:4200/dashboard.**

**What you should see.** The URL is rewritten to `/dashboard?account=14&week=2026-07-20&type=all`:
- "Viewing as" shows **Beacon Home Security** (account 14).
- The week label reads **Mon Jul 20 – Sun Jul 26, 2026**.
- The summary shows 26 inbound events, usually 18–38 a week, within the usual range.
- The first row of the locations table is **Site B**: 2 events, "Usually 3–12 a week", **"▼ Lower than usual"**. It is followed by Sites C, A and D, all "Within usual range".

Other views worth opening:

| View | URL | What it shows |
|---|---|---|
| The spike | `?account=6&week=2026-06-01` | 880 events; all 15 sites "▲ Higher than usual" |
| After the spike | `?account=6&week=2026-07-20` | 87 events, still within the usual range |
| Empty account | `?account=20` | The empty state |
| Early history | `?account=8&week=2026-03-02` | "Not enough history yet" |

Reloading any of these reproduces the same view.

**Stop and reset**

| To | Run |
|---|---|
| Stop the API or the web server | `Ctrl+C` in its terminal |
| Stop the DB and keep the data | `docker compose stop db` (or `docker compose down`) |
| Delete the DB and its data | `docker compose down -v`. The next API start reseeds it |

---

## Run the tests

**One line:** `dotnet test` (from the repo root, with **Docker running**) and `cd web && npm test`.

| Suite | Command (repo root unless noted) | Needs | What it checks |
|---|---|---|---|
| All backend | `dotnet test` | Docker | The three projects below |
| Business logic + service | `dotnet test --project tests/Relay.Core.Tests` | Nothing (no DB, no Docker) | Week calendar (US DST weeks, Phoenix, UTC, week boundaries), zero-fill and eligibility, the normality rule, ranking. Pure unit tests with no mocks; the service tests use hand-written fakes |
| Data | `dotnet test --project tests/Relay.Infrastructure.Tests` | Docker | The counting SQL against a real SQL Server 2022 in Testcontainers (never InMemory or SQLite): de-duplication, window boundaries, type filter, other accounts ignored |
| API + golden values | `dotnet test --project tests/Relay.Api.Tests` | Docker | The endpoints against the real seed: golden values from the independent Python model (PLAN §7, §13), contract shape, 400/404/500 rules, migrate-on-start, missing connection string, seed-file checksums |
| Frontend | `cd web && npm test` | Node, after `npm ci` | Vitest via `ng test`: URL-as-state round trip and normalisation, request sequencing, component states and copy |
| End-to-end smoke (**planned, not in the repo yet**) | `docker compose up -d --wait db && cd web && npx playwright install chromium && npm run e2e`, with `ConnectionStrings__Relay` exported | Docker, Chromium | Browser, Angular, API and seeded DB together (PLAN §13, "End-to-end smoke layer") |

- **Where expectations come from.** Test expectations come from PLAN.md and the independent Python models in `analysis/`. They were never taken from running the code under test.
- **Build settings.** The build uses `TreatWarningsAsErrors` and `AnalysisLevel latest-recommended`, so an analyzer warning fails the build.

---

## Troubleshooting

| Symptom | Cause and fix |
|---|---|
| The API won't bind, or the web proxy gets errors on port 5000 | On macOS the AirPlay Receiver holds :5000. That is why the API uses **5080** (`launchSettings.json`, `web/proxy.conf.json`). Start the API with `dotnet run --project src/Relay.Api` so the launch profile applies |
| The API stops at startup with `The connection string 'ConnectionStrings:Relay' is missing or empty. Set the environment variable 'ConnectionStrings__Relay' before starting the API.` | The connection string is not in the environment. Run `set -a; source .env; set +a` **in the same terminal** before `dotnet run`. This check runs only in Development; in Production the same problem shows up as a 500 on the first request |
| `Login failed for user 'sa'` | The volume was created with a different password. Put the old password back in `.env`, or reset with `docker compose down -v` (deletes the data) |
| `docker compose up` fails with `set RELAY_DB_SA_PASSWORD in .env` | `.env` is missing, or the variable is empty. Run `cp .env.example .env` and set it |
| The DB container exits or never turns healthy | The password is too weak for SQL Server; check `docker compose logs db`. On Apple Silicon, check that Rosetta emulation is enabled |
| Infrastructure/API tests fail with Docker or Testcontainers errors | Docker is not running. Start it, or run only `dotnet test --project tests/Relay.Core.Tests` |
| The first API start or first test run is slow | The SQL Server image is large and is pulled once, and under Rosetta it also starts more slowly. Seeding itself takes about 1 s |
| The dashboard shows "We couldn't load this week's activity. Try again." | The API is not running on 5080, or it can't reach the DB. Check terminal 1 |

---

## API reference

JSON over HTTP on `http://localhost:5080`. Errors are `application/problem+json` (RFC 9457 `ProblemDetails`). Enum values are snake_case and case-sensitive.

### `GET /api/accounts`

Returns every account, **including account 20, which has no events**. Ordered by name (ordinal; ties by id).

```json
[ { "id": 14, "name": "Beacon Home Security", "timezone": "America/New_York" }, … ]
```

### `GET /api/accounts/{accountId}/activity-health?week=YYYY-MM-DD&type=all`

Last week's health for the account and each of its locations.

| Parameter | Required | Default | Rule |
|---|---|---|---|
| `accountId` (route) | yes | — | Integer (`{accountId:int}`). Unknown or non-numeric → **404** |
| `week` (query) | no | `latestCompleteWeek` (2026-07-20 on the seed) | Strict `yyyy-MM-dd` date. It must be a Monday in the account's time zone, no earlier than `earliestWeek` and no later than `latestCompleteWeek`. To get the default, **omit** the parameter; `week=` (empty) is rejected |
| `type` (query) | no | `all` | Exactly one of `all`, `call_received`, `lead_created`, `appointment_set`, case-sensitive |

**Checks run in this order** (the first failure wins):

1. **Malformed input → 400 validation problem.** The body's `errors` holds exactly one key, `Week` or `Type`.
   - Rejected `week` values: `abc`, `2026-13-01`, `2026-02-30`, `20260720`, `07/20/2026`, `2026-7-20`, `2026-07-20T00:00:00`, and an empty value.
   - Rejected `type` values: `ALL`, `Call_Received`, `calls`, `1`, and an empty value.
2. **Unknown account → 404.**
3. **`week` is not a Monday → 400.**
4. **`week` is before `earliestWeek` or after `latestCompleteWeek` → 400.** For account 20 the only valid week is 2026-07-20.

| Status | When |
|---|---|
| 200 | Success. This includes an account with no events (empty state) |
| 400 | Validation problem (above), or one of the week rules |
| 404 | Unknown or non-numeric account |
| 500 | Unexpected error. `ProblemDetails` without the exception message, type or stack trace, in every environment. It also covers the out-of-scope case of a time zone whose DST change skips local midnight |

**Example** (account 6, week 2026-07-20, `all`; trimmed to one location, the §13 §5.2 contract):

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
  "locations": [
    { "location": "Site M", "count": 7, "baseline": { "weeksUsed": 8, "median": 3.5, "low": 1, "high": 9 }, "status": "normal", "deviation": 1.3 }
  ]
}
```

| Field | Meaning |
|---|---|
| `status` | `above` \| `below` \| `normal` \| `insufficient_data` |
| `baseline` | **Always present.** For `insufficient_data` it is `{ "weeksUsed": 0–3, "median": null, "low": null, "high": null }` and `deviation` is `null` |
| `low`, `high` | Integers: the "usually low–high" range. The status is read from them (`count < low` → `below`, `count > high` → `above`) |
| `median` | Unrounded (always x or x.5) |
| `deviation` | A z-score on the Anscombe scale, rounded to 2 dp away from zero. It is used for ranking and never shown on screen |
| `locations` | Already sorted (see [How "normal" is decided](#how-normal-is-decided)) |
| `earliestWeek` | Local Monday of the account's first event. Never null: for an account with no events it equals `latestCompleteWeek` |
| `dataAsOf` | The latest event in the whole dataset (UTC), the same for every account. It is `null` only when the database has no events at all |

**Empty account (20):** 200 with `summary.count` 0, `status: "insufficient_data"`, `baseline.weeksUsed` 0, `earliestWeek` = `latestCompleteWeek` = `2026-07-20`, and `locations: []`.

---

## Interpretation of the ticket

### For the admin

For each location, and for the account as a whole, the dashboard shows **last week's inbound events** next to what that location **usually** gets. "Usually" comes from that location's own last 8 full weeks.

| What the admin sees | Meaning |
|---|---|
| **▲ Higher than usual** | More than this location's usual range |
| **▼ Lower than usual** | Less than this location's usual range |
| **Within usual range** | Inside the range |
| **Not enough history yet (N of 4 weeks needed)** | Fewer than 4 full weeks of history. The count is shown, but no range and no judgement |

Every row has a line such as "Usually 3–12 a week". Locations that need attention are listed first. Status is always a symbol plus words, never colour alone.
The default view shows an example: at Beacon Home Security, **Site B had 2 events against a usual 3–12**. It is marked "▼ Lower than usual" and listed first, while the account as a whole (26, usually 18–38) is within its usual range.

"Last week" means the **last complete week** (Monday to Sunday in the account's time zone). The week in progress is never shown, because on a Monday morning it would always look like a collapse.

### How "normal" is decided

This is the precise rule (PLAN §13 §5.3, "R2\*").

- **Window.** For a selected week W, the baseline is the 8 local weeks before W, zero-filled. A baseline week counts only if it starts after the week of that series' first event (a site's own first event; for the account, its earliest event). With fewer than 4 such weeks the status is `insufficient_data`.
- **Scale.** Counts are compared on the Anscombe scale T(x) = 2·√(x + 3/8). This transform makes small counts roughly equally variable.
- **Band.** The centre is T(median of the eligible weeks). The spread is max(1.4826 × MAD on the T scale, 1.0), where 1.0 is the Poisson noise floor. The band is centre ± 2·spread, converted back to a whole-number range `low–high`.
- **Status comes from the integer range only.** `count < low` gives `below`, `count > high` gives `above`, and anything else, including exactly `low` or `high`, gives `normal`. The "Usually low–high" line therefore can never contradict the label.
- **Ranking.** Flagged rows come before normal ones. Within that, rows are ordered by how far they are from usual (|z| on the T scale, unrounded), then `below` before `above` on a tie, then location name. `insufficient_data` rows come last.
- **Metric.** Inbound activity events (calls received, leads created, appointments set). The default is all types, and a filter narrows it to one type.

**Not built:** alerting, forecasting and ML (out of scope per product); cross-account benchmarks, which serve a different persona (the account manager); outcome rates (deferred, see below).

---

## Key assumptions

None of these went to the recruiter. Each is a working assumption stated in PLAN §4, with the seed evidence behind it.

| Question | Working assumption | Evidence in the seed |
|---|---|---|
| What is "now"? | The latest event in the dataset. The default week is the last complete local week before it: **2026-07-20** for every account | Data ends **Mon 2026-07-27 22:20:34 UTC**, while the system clock is in September 2026. Measured from the clock, every account would look silent |
| Does "this week" mean the partial week? | No. Only complete weeks are compared | The final week holds one day of data. Weekends are about 25 % of a weekday |
| Week start | Monday (ISO), in the account's IANA time zone | Each account has an IANA `timezone`. Local-week and UTC-week bucketing differ by only 8 events, but local time is what the admin means |
| Is account 6's 880-event week real (a storm, a campaign) or a bad import? | Unknown. It is shown as "Higher than usual" and never excluded. The median keeps it from distorting later weeks | Week of Jun 1: 880 events (805 on Jun 3 alone) against about 70 a week, spread across all 15 sites, and the fields look plausible |
| Are exact duplicates real repeated events? | No. They are ingestion duplicates and are counted once | 12 pairs with adjacent ids and every column equal, down to the second |
| Does a site exist before its first event? | No. Weeks up to and including a site's first-activity week don't count toward its baseline | Every site's first activity is in the week of Jan 26 or Feb 2, so this only matters for early weeks |
| Is "All" a count of customers? | No. It counts inbound **events**; a call, a lead and an appointment can be the same person. The UI says "inbound events, not unique customers" | There is no customer id in `activity_events` |

---

## Design decisions and trade-offs

Main decisions from PLAN §3, as revised by §13. Rationale and evidence: [`docs/design-consensus.md`](docs/design-consensus.md). Every number below comes from a script in `analysis/`.

| # | Decision | Rejected | Why |
|---|---|---|---|
| D1 | "Now" = latest event in the whole dataset (one global anchor) | System clock; a fixed config date; a separate anchor per account | The clock shows nothing on static data. A per-account anchor would make an account that stopped sending data look normal |
| D2 | Normal = the location's own history | Sibling comparison (a site's share of the account); % change against the mean | Sibling share is confounded when the whole account moves: in the spike week all 15 sites keep their share, so it says "all normal". It is also useless for single-site accounts. The mean is poisoned by the spike: 171 vs a median of 72.5 for account 6 in the week of Jul 20. % change cries wolf on counts of 3–6 |
| D3 | One method at two levels: an account summary row plus locations sorted by how unusual they are | A second statistic for the sibling view | One set of edge cases. It still separates "the whole account moved" from "one site moved" |
| D4 | Metric = inbound event count, all types by default, with a type filter | A column per type; outcome rates | Totals have enough volume. Per-type counts per site are mostly noise. Rates are deferred |
| D5 | "Viewing as" account switcher (impersonation for the demo, not auth). The default is **account 14** | Account 12 (the original default); a hard-coded account | Under the final rule, account 14's Site B (2 vs usually 3–12) is the **only** flagged series in the whole seed for the default week, which shows a location going quiet. Account 12 flags nothing that week. The switcher also lets an evaluator open account 6 (the spike), account 20 (empty) and the single-site accounts |
| D6 | SQL only counts (de-duplicate, bucket into given UTC windows, group). Week math, zero-fill, statistics and ranking are in pure C# | Everything in SQL; LINQ | Product rules stay unit-testable without a database. LINQ would not help, because InMemory and SQLite have different semantics from SQL Server |
| D7 | Band rule **R2\*** (above) with k = 2, floor 1.0, and at least 4 eligible weeks | See the table below | See the table below |

### How the band rule was chosen (D7, revised)

The rule was simulated on the seed before any code was written. The simulation used every site-week with a full 8-week baseline and excluded the spike week.

| Candidate | Share of site-weeks flagged | Drop to 0 caught (all / calls) | Outcome |
|---|---|---|---|
| 2nd-lowest to 2nd-highest baseline week | **34.6 %** | — | Rejected: flags a third of all sites every week |
| Min–max of the baseline | 15.1 % | — | Rejected: too noisy |
| Median ± 3 × spread | 0.3 % | — | Rejected: misses too much |
| Median ± 2 × spread, √median floor (first plan) | 4.8 % | 78 % / **37 %** | Rejected **after** it was accepted. A full sweep showed that a location falling to **zero** was never flagged for leads or appointments, and was flagged for calls only 37 % of the time |
| **R2\*: Anscombe scale, k = 2, floor 1.0** | **4.3 %** (≈ 4 %) | **98 % / 96 %** | **Chosen.** 0 contradictions between status and range in 253,149 checks. Account 6's spike week still flags 15/15 sites, and 4 weeks later, with the spike still in the baseline, all 15 are within range |

Other options rejected in the debate: k = 1.75 (false "below" doubles to 5.5 %); k = 2.5 (drop-to-0 detection for calls falls to 70 %); Freeman–Tukey (7 % false "below" at median 2); exact Poisson/negative-binomial tails (drop-to-0 for calls only 56 %); EARS (based on mean and SD, and flags "above" only); Farrington/Noufaily (fitted models, out of scope); a fifth "too few to judge" status (costs contract, UI and test work; the footnote covers it).

**The trade-off accepted:** a location that usually gets 2 or fewer events a week can never show "Lower than usual". A week with 0 is ordinary at that level: P(0 | Poisson 2) = 13.5 %. The dashboard says so rather than raise false alarms.

### Later decisions (PLAN §13), with the reason for each

| Decision | Reason |
|---|---|
| Neutral copy: "Higher than usual" / "Lower than usual" / "Within usual range". No median, z or "±" on screen | 13 % of account-weeks show at least one flagged location, so the labels must not sound alarming. The admin needs a range, not statistics |
| `baseline` is always returned, with `weeksUsed`; `minimumEligibleWeeks` is a top-level field | The UI needs "N of 4 weeks" even when there is no range |
| A week before `earliestWeek` or after `latestCompleteWeek` returns 400. For an account with no events, `earliestWeek` equals `latestCompleteWeek` | The rule is the same on both sides, and the week stepper needs no special case for null |
| A site's existence and eligibility use its first event of **any** type. The type filter changes counts only | Using "first event of this type" changes 110 statuses on the seed, 99 of them from normal to insufficient, with no statistical gain |
| Exact duplicates are removed only with `DISTINCT`/`GROUP BY` over every non-id column. NULL, `''` and `0` count as equal | An `=`-based self-join misses 4 of the 12 pairs because of NULL columns. Treating NULL/''/0 as equal was a user decision; the total stays 12,614 on the seed |
| Covering index `(account_id, occurred_at) INCLUDE (location, event_type, duration_seconds, outcome)` | Without the extra columns the planned index went unused. With them the weekly query is one index seek per window (a 200 ms scan became about 7 ms of seeks) |
| Input precedence: a malformed `week` or `type` returns 400; then an unknown account returns 404; then a non-Monday week; then out of range. `type` is case-sensitive. A non-numeric account id returns 404 | Errors are predictable and testable, and each rule has exactly one place |
| An invalid URL parameter is reset to its default, with the URL rewritten using `replaceUrl` (never snapped to the nearest Monday). Switching account keeps the week and type, and falls back to the latest week if the new account can't show it. User actions add browser history entries | A reload or a shared link always reproduces the same view, and Back undoes a filter change |
| An empty database (no events at all) returns 200 with `dataAsOf: null`, and the clock is the fallback anchor | The one case where D1 has nothing to anchor on |
| No separate DTO layer. Core's output records are shaped like the JSON; casing, enum names and rounding are configured once in the API | Otherwise every contract change would be made in four places |
| API on port 5080. Connection string only from the environment | macOS holds port 5000. No committed secrets |
| Time zones whose DST change falls at local midnight are out of scope | No seed zone is affected, because the US zones switch at 02:00. An untested branch for them disagreed with the rest of the calendar, so it was removed rather than half-supported |
| Process: every agent works in its own git worktree; the red test suite is committed before the implementation; a reviewer on a different model after each layer; E2E smoke tests as the last step | See [How AI was used](#how-ai-was-used) |

---

## Data handling

| Messy part | What the app does |
|---|---|
| **Exact duplicates** | 12 pairs (adjacent ids, every column equal) are counted once at query time. The raw rows are kept, so the data is used as-is: 12,626 rows give 12,614 events. The footnote says "exact duplicates counted once" |
| **Near-duplicates** | 27 near-duplicates within 60 seconds look like ordinary traffic and are **kept** |
| **Account 6 spike** | 880 events in the week of Jun 1 (805 on Jun 3) against about 70 a week. That week shows ▲ Higher than usual at all 15 sites. It is never removed. Because the baseline uses the median, the week of Jul 20 still reads 87 vs usually 30–134, within range. A mean baseline would have been 171 |
| **Empty account (20)** | Zero events is a valid state, not an error. The API returns 200 with count 0, and the page shows "No activity recorded for this account yet." with the week stepper disabled |
| **Partial weeks** | Only complete weeks are shown. The data ends on a Monday, so the week of Jul 27 is never compared |
| **Time zones** | Weeks run Monday 00:00 to Monday 00:00 in the account's IANA time zone and are converted to UTC windows (DST-aware). The events are stored in UTC |
| **Silent weeks** | A week with no events at a site counts as 0. It is not skipped. A site with nothing in the selected week is still listed, with 0 |
| **Early history** | A baseline week counts only once the site has started sending data. With fewer than 4 such weeks the page says "Not enough history yet (N of 4 weeks needed)" instead of guessing. Example: account 8 in the week of Mar 2 |
| **NULL outcomes and call durations** | About 400 NULL outcomes and 313 NULL durations. They don't affect counts. They are one reason outcome rates are deferred |
| **Small counts** | Per-site weekly medians are 3–6, so ranges are wide by necessity. See the limits below |

## Known limits

What the dashboard can and cannot detect. These bullets are quoted verbatim from the statistician (`docs/design-consensus.md` §1):

- A location that usually gets ≤ 2 a week can never show "lower".
- A drop to 0 is caught about 90 % of the time at 4+ a week, and about 65 % at 3.
- With a 4-week baseline, false flags rise by about 1 point per side.
- Design flag rate: about 4 % of site-weeks; 13 % of account-weeks show at least one flagged location (`product_monday_view_out.md`).
- A halving at a single site is usually not caught in one week. Of 99 real halvings at median ≥ 6, 34 were flagged and 65 were not (`reviewer_default_week_out.md`).
- A spike stays in the baseline for 7 weeks and widens ranges by about 20–30 % (true of every rule).
- Per-type filters at site level are thin.

Also:
- **By design, about 4 % of location-weeks are flagged** (4.3 % measured) even when nothing has changed. That is the cost of catching 96–98 % of drops to zero.
- **The seed is not bursty.** Its week-to-week variance is about equal to its mean, which is what the rule was calibrated on. On synthetic bursty data, false flags rise to about 5–8 % per side.
- **Time zones whose DST change falls at local midnight** (Monday 00:00 skipped or repeated) are out of scope. No seed zone is affected.

---

## Deliberately deferred

From PLAN §11 and the design debate.

| Item | Why it was deferred |
|---|---|
| Outcome rates (missed-call rate, lead conversion, no-shows) | These are more actionable, but the small-number problem is worse for rates, and about 400 NULL outcomes and "missed" calls that have durations need product decisions first |
| A second severity tier (e.g. "very unusual") | It adds a threshold to calibrate and explain. With neutral labels and ranking, the worst row is already on top |
| A sibling-share statistic | Confounded when the whole account moves, and meaningless for single-site accounts (D2) |
| A near-duplicate policy | 27 near-duplicates within 60 seconds look like real traffic. Merging them would be a guess |
| Spike root-cause notes | The data can't say whether Jun 3 was a storm or a bad import. That is for a human to annotate |
| Trend sparklines | Useful context, but "Usually X–Y" already answers the Monday question |
| Auth | Out of scope per the brief. "Viewing as" is impersonation for the demo |
| Caching | Not needed at seed scale (the weekly query takes milliseconds) |
| A "too few to judge" status | Covered by the footnote, at no cost to the contract or tests |

## With another day

In priority order.

1. **Finish and harden the E2E smoke layer.** It is the only test that proves the browser, API and seeded DB work together; the current tests stop at each boundary.
2. **Outcome rates, starting with the missed-call rate.** "Calls are normal but we missed half of them" is the most actionable thing the data holds.
3. **Recalibrate on bursty data.** The ≈ 4 % flag rate holds only for Poisson-like data. Real customers with campaigns would see 5–8 % false flags per side; a negative-binomial check or a per-account k would address that.
4. **Make the global data-anchor query cheap.** `MAX(occurred_at)` scans the index on every request. That is fine for 12k rows but not for production, so it should be cached or indexed.
5. **Show an error when the account list fails to load.** Today "Viewing as" is just left empty (an accepted reviewer note).
6. **Add a second severity tier.** Once there is real usage data to calibrate it on.
7. **Add a trend sparkline per location.** It helps tell a one-off week from a slide.
8. **Support time zones with a DST change at midnight.** Needed before onboarding customers outside the US zones in the seed.

---

## Stack

Versions are read from `global.json`, `Directory.Packages.props`, `web/package-lock.json` and `docker-compose.yml`.

| Component | Version | Why |
|---|---|---|
| .NET, **Minimal APIs** | SDK 10.0.201, `net10.0` | The required stack; .NET 10 is the current LTS. Two endpoints don't need controllers. Each endpoint calls exactly one application service |
| **EF Core** (`Microsoft.EntityFrameworkCore.SqlServer`) | 10.0.12 | Schema, migrations and the seed load. Migrations are the standard tooling, and the seed is loaded as a migration. Table and column names stay snake_case so `db/seed.sql` runs verbatim |
| **Hand-written SQL** for counting (`Database.SqlQuery<T>`) | — | The counting query needs exact control over de-duplication (`GROUP BY` over every non-id column, NULL-safe) and index use, which LINQ hides. All logic beyond counting is in pure C#, so it can be unit-tested |
| **SQL Server 2022** | `mcr.microsoft.com/mssql/server:2022-latest`, Developer edition | It is the team's database. Docker Compose with a health check |
| **Testcontainers** (`Testcontainers.MsSql`) | 4.15.0 | SQL is tested only against the real engine, because InMemory and SQLite differ on collation, NULL handling and `OPENJSON`. The seed loads in about 1 s |
| **xUnit v3**, Shouldly, `Microsoft.AspNetCore.Mvc.Testing` | 4.0.1, 4.3.0, 10.0.12 | Standard .NET testing, and in-process API tests (`WebApplicationFactory`). Test names are PascalCase, with the analyzers left on |
| **Angular** (standalone components, signals) | 22.2.0 | The required stack. Signals are enough for one page, so there is no store library |
| **URL query params as the only UI state** | — | The brief requires that state survives a reload. With the URL as the source of truth, reload, Back and shared links all work without extra code, and invalid params are rewritten to defaults |
| **Abstract-class DI tokens** for the API clients | — | Components depend on an abstraction, so tests swap in a fake without HTTP mocks |
| **Vitest** via Angular's `@angular/build:unit-test`, jsdom | 5.0.2, 30 | Current Angular default, fast, no browser needed |
| TypeScript | 6.0.3 | Required by Angular 22 |
| No mapper library, no DTO layer | — | Core records are shaped like the JSON and serialisation is configured once in the API, so a contract change is one edit |

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
  app/core/api/            AccountsApi, ActivityHealthApi (abstract tokens + HTTP implementations)
  app/features/dashboard/  DashboardPage, DashboardState (URL ↔ signals), copy, week helpers
    components/            DashboardFilters, AccountSummary, LocationTable
  testing/                 fakes, fixtures, DOM and router helpers
docs/                      acceptance-criteria.md, design-consensus.md, battle-test/, handoff/
analysis/                  Python models and scripts behind every number here and in the golden tests
ai-log/                    raw/ transcripts (JSONL) and sessions/ readable renders
scripts/export-ai-log.sh   exports and redacts the AI log
.claude/agents/            agent definitions; CLAUDE.md holds the rules every agent follows
```

| Document | What it is |
|---|---|
| [`PLAN.md`](PLAN.md) | The spec, written before the code. §13 is the append-only log of later decisions and their reasons |
| [`docs/acceptance-criteria.md`](docs/acceptance-criteria.md) | Given/When/Then criteria per slice, and the approved UI copy |
| [`docs/design-consensus.md`](docs/design-consensus.md) | The four-agent debate that chose the R2\* rule, the contract and the data-layer fixes |
| [`docs/battle-test/`](docs/battle-test/) | Pre-code verification: independent re-implementation, SQL Server findings, adversarial plan review, statistician report, industry survey |
| [`AI_LOG.md`](AI_LOG.md) | Curated decision log |

---

## How AI was used

The work was agent-first with Claude Code. The coordinator ran on the main thread and dispatched specialist agents (`.claude/agents/`: architect, backend, database, frontend, test-author, product, statistician, reviewer), each in its own git worktree, all bound by [`CLAUDE.md`](CLAUDE.md). The reviewer ran on a different model from the authors, deliberately.

The plan was battle-tested against the seed before any code, and the normality rule was reopened when that test found the drop-to-zero blind spot. Every accept, reject, redirect and catch is in [`AI_LOG.md`](AI_LOG.md), with raw transcripts in `ai-log/raw/` and readable renders in `ai-log/sessions/`.
