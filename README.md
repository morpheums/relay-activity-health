# Relay — Activity health (DASH-247)

A Relay customer admin opens the dashboard on Monday morning and gets two answers:

1. **"Is last week normal for us?"** The account's total, compared with the account's own recent weeks.
2. **"Which of my locations needs attention?"** Every location compared with its own recent weeks. The unusual ones are listed first.

Spec: [`PLAN.md`](PLAN.md). It was written before the code and is append-only; later changes are in §13 with a reason for each.
AI interaction log: [`AI_LOG.md`](AI_LOG.md) (see [How AI was used](#how-ai-was-used)).

---

## How to run

**Needs:** Docker, .NET SDK 10 (`global.json` pins 10.0.201, roll-forward to the latest feature band), Node + npm (see `web/package.json`).

No secrets are committed. The database password and the connection string come only from a git-ignored `.env`.

```bash
cp .env.example .env
# edit .env: set RELAY_DB_SA_PASSWORD to a strong password (see note below)

docker compose up -d --wait db          # SQL Server 2022; waits until the health check passes

set -a; source .env; set +a             # exports ConnectionStrings__Relay (and the password it embeds)
dotnet run --project src/Relay.Api      # http://localhost:5080

cd web && npm ci && npm start           # second terminal, from the repo root; /api is proxied to :5080
```

Then open **http://localhost:4200/dashboard**. It opens on account 14 (Beacon Home Security) and the week of Jul 20, 2026.

| Thing | Detail |
|---|---|
| `RELAY_DB_SA_PASSWORD` | Must meet SQL Server's complexity rules: at least 8 characters, using three of upper case, lower case, digits and symbols. Avoid `$`, `"` and `;`. Compose and the shell expand `$`, and `;` ends a value in the connection string |
| `RELAY_DB_PORT` | Host port for SQL Server, default `1433` |
| `ConnectionStrings__Relay` | Built in `.env` from the two values above. The API reads it **only** from the environment. If it is missing, the API stops at startup with a message that names the setting |
| Database schema and seed | In Development the API applies the EF Core migrations on startup. `InitialCreate` builds the schema from `db/schema.sql`. `LoadSeedData` runs `db/seed.sql`, which is committed unmodified |
| Changing the password later | The DB volume keeps the password it was first created with. To reset it, run `docker compose down -v`, which **deletes the data**; the next API start reseeds it |
| Apple Silicon | The SQL Server image has no arm64 build. Compose sets `platform: linux/amd64`, so it runs under Rosetta. The first start is slower |
| Port 5080 | Fixed, and set in `launchSettings.json` and `web/proxy.conf.json`. Port 5000 is taken by the macOS AirPlay Receiver |

## How to run the tests

**One line:** `dotnet test` (from the repo root; **Docker must be running**) and `cd web && npm test`.

| Layer | Project | What it checks |
|---|---|---|
| Business logic + service | `tests/Relay.Core.Tests` | Week calendar (US DST weeks, Phoenix, UTC, week boundaries), zero-fill and eligibility, the normality rule, ranking. Pure unit tests with no DB and no mocks; the service tests use hand-written fakes |
| Data | `tests/Relay.Infrastructure.Tests` | The counting SQL against a real SQL Server 2022 in Testcontainers (never InMemory or SQLite): de-duplication, window boundaries, type filter |
| API + golden values | `tests/Relay.Api.Tests` | The endpoints against the real seed in a Testcontainers SQL Server. Golden values from an independent Python model (PLAN §7, §13), the contract shape, 400/404 rules, migrate-on-start |
| Frontend | `web` (Vitest via `ng test`) | URL-as-state round trip and normalisation, request sequencing, component states and copy |

Test expectations come from PLAN.md and the independent Python models in `analysis/`. They were never taken from running the code under test.

**Planned, last step (not in the repo yet):** a Playwright end-to-end smoke layer (`web/e2e/`, run with `npm run e2e`) that drives the browser, Angular, the API and the seeded DB together (PLAN §13, "End-to-end smoke layer").

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

### Precisely (the rule, PLAN §13 §5.3, "R2\*")

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
| **Near-duplicates** | 27 pairs within 60 seconds look like ordinary traffic and are **kept** |
| **Account 6 spike** | 880 events in the week of Jun 1 (805 on Jun 3) against about 70 a week. That week shows ▲ Higher than usual at all 15 sites. It is never removed. Because the baseline uses the median, the week of Jul 20 still reads 87 vs usually 30–134, within range. A mean baseline would have been 171 |
| **Empty account (20)** | Zero events is a valid state, not an error. The API returns 200 with count 0, and the page shows "No activity recorded for this account yet." with the week stepper disabled |
| **Partial weeks** | Only complete weeks are shown. The data ends on a Monday, so the week of Jul 27 is never compared |
| **Time zones** | Weeks run Monday 00:00 to Monday 00:00 in the account's IANA time zone and are converted to UTC windows (DST-aware). The events are stored in UTC |
| **Silent weeks** | A week with no events at a site counts as 0. It is not skipped. A site with nothing in the selected week is still listed, with 0 |
| **Early history** | A baseline week counts only once the site has started sending data. With fewer than 4 such weeks the page says "Not enough history yet (N of 4 weeks needed)" instead of guessing. Example: account 8 in the week of Mar 2 |
| **NULL outcomes and call durations** | About 400 NULL outcomes and 313 NULL durations. They don't affect counts. They are one reason outcome rates are deferred |
| **Small counts** | Per-site weekly medians are 3–6, so ranges are wide by necessity. See the limits below |

### Known limits

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
| A near-duplicate policy | 27 pairs within 60 seconds look like real traffic. Merging them would be a guess |
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

## Stack choices

| Choice | Why |
|---|---|
| **.NET 10 Minimal APIs** | Required stack, current LTS. Two endpoints don't need controllers. Each endpoint calls exactly one application service |
| **EF Core for schema, migrations and seed; hand-written SQL for counting** (`Database.SqlQuery<T>`) | Migrations are the standard tooling, and the seed loads as a migration. The counting query needs exact control over de-duplication (`GROUP BY` over every non-id column, NULL-safe) and index use, which LINQ hides. All logic beyond counting is in pure C# so it can be unit-tested |
| **SQL Server 2022 in Docker; Testcontainers for tests** | It is the team's database. SQL is tested only against the real engine, because InMemory and SQLite differ on collation, NULL handling and `OPENJSON`. The seed loads in about 1 s |
| **Angular (standalone components, signals), URL query params as the only state** | The brief requires that state survives a reload. With the URL as the source of truth, reload, Back and shared links all work without extra code, and invalid params are rewritten to defaults. Signals are enough; no store library |
| **Abstract-class DI tokens for the API clients** | Components depend on an abstraction, so tests swap in a fake without HTTP mocks |
| **Vitest** (Angular's `unit-test` builder) | Current Angular default, fast |
| **xUnit v3, Shouldly** | Standard .NET testing. Test names are PascalCase, with the analyzers left on |
| **No mapper library, no DTO layer** | Core records are shaped like the JSON and serialisation is configured once, so one contract change is one edit |

### Repository map

| Path | What it is |
|---|---|
| `db/schema.sql`, `db/seed.sql` | The provided schema and seed, **unmodified**. They are loaded by the `InitialCreate` and `LoadSeedData` migrations, and their checksums are pinned in `tests/Relay.Api.Tests/Seed/StarterFileChecksumTests.cs` |
| `src/Relay.Core` | Normality rule, week calendar, ranking, application services, query interfaces. References nothing else |
| `src/Relay.Infrastructure` | EF Core model, migrations, the hand-written counting SQL |
| `src/Relay.Api` | Minimal API endpoints and the DI composition root |
| `tests/` | `Relay.Core.Tests`, `Relay.Infrastructure.Tests`, `Relay.Api.Tests` |
| `web/` | Angular dashboard |
| [`PLAN.md`](PLAN.md) | The spec, written before the code. §13 is the append-only log of later decisions |
| [`docs/acceptance-criteria.md`](docs/acceptance-criteria.md) | Given/When/Then criteria per slice, and the approved UI copy |
| [`docs/design-consensus.md`](docs/design-consensus.md) | The four-agent debate that chose the R2\* rule, the contract and the data-layer fixes |
| [`docs/battle-test/`](docs/battle-test/) | Pre-code verification: independent re-implementation, SQL Server findings, adversarial plan review, statistician report, industry survey |
| [`analysis/`](analysis/) | Python models and scripts behind every number in this README and the golden tests |
| [`AI_LOG.md`](AI_LOG.md), `ai-log/` | AI interaction log and raw transcripts |
| `.claude/agents/`, [`CLAUDE.md`](CLAUDE.md) | Agent definitions and the rules they follow |

---

## How AI was used

The work was agent-first with Claude Code. The coordinator ran on the main thread and dispatched specialist agents (`.claude/agents/`: architect, backend, database, frontend, test-author, product, statistician, reviewer), each in its own git worktree, all bound by [`CLAUDE.md`](CLAUDE.md). The reviewer ran on a different model from the authors, deliberately.

The plan was battle-tested against the seed before any code, and the normality rule was reopened when that test found the drop-to-zero blind spot. Every accept, reject, redirect and catch is in [`AI_LOG.md`](AI_LOG.md), with raw transcripts in `ai-log/raw/` and readable renders in `ai-log/sessions/`.
