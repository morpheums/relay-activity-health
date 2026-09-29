# Relay — Activity health (DASH-247)

A dashboard feature for a Relay customer admin on Monday morning. For last week, it shows whether the account's inbound activity (calls, leads, appointments) was **normal for this account**, and which **locations need attention**. Each location is compared with its own last 8 full weeks and gets a plain-language status and a "Usually X–Y a week" range; unusual locations are listed first. Backend: .NET 10 Minimal API over SQL Server 2022 (EF Core migrations load the provided seed). Frontend: Angular 22.

## What you'll see

Open http://localhost:4200 (it redirects to `/dashboard`). The URL is rewritten to `/dashboard?account=14&week=2026-07-20&type=all`:
- "Viewing as" shows **Beacon Home Security** (account 14).
- The week control reads **Mon Jul 20 – Sun Jul 26, 2026**.
- The summary reads **26 inbound events · usually 18–38 a week**, "Within usual range".
- The first row of the locations table is **Site B**: 2 events, "Usually 3–12 a week", **"▼ Lower than usual"**. It is followed by Sites C, A and D, all "Within usual range".
- The footer, "About these numbers", shows **Data as of Mon Jul 27, 2026**.

## Quick start

First, on any OS:
1. `cp .env.example .env` (on Windows: `copy .env.example .env`)
2. Set `RELAY_DB_SA_PASSWORD` in `.env`. If it contains `$`, single-quote it: `RELAY_DB_SA_PASSWORD='Pa$w0rd…'`.

**One command (macOS, Linux):** `scripts/dev.sh`. It stops early if `.env` is missing or if port 5080 or 4200 is already in use. Otherwise it starts the DB and waits until it is healthy, runs `npm ci` in `web` on the first run, starts the API and waits for it (up to 120 s), then serves the web app. Open http://localhost:4200. Ctrl+C stops the API and web. The DB keeps running: stop it with `docker compose down`, or `docker compose down -v` to wipe the data.

**Step by step (Windows, or any OS):** after steps 1–2, no shell-specific setup is needed; in Development the API reads the repo-root `.env` itself.

3. `docker compose up -d --wait db`
4. `dotnet run --project src/Relay.Api` (terminal 1, http://localhost:5080)
5. `cd web && npm start` (terminal 2, http://localhost:4200/dashboard; run `npm ci` in `web` once first)

| URL | What |
|---|---|
| http://localhost:4200/dashboard | The dashboard |
| http://localhost:5080/api/accounts | The account list (20 accounts) |
| http://localhost:5080/api/accounts/14/activity-health | Account 14, latest complete week ([docs/api.md](docs/api.md)) |

**Tests:** `dotnet test` (Docker running) and `cd web && npm test`. The Playwright end-to-end layer is deferred (see [Another day](#deliberately-deferred-and-another-day)); there is no `npm run e2e` on `main`.

Full setup, configuration and troubleshooting: [docs/running.md](docs/running.md).

## The dashboard

One page, desktop only (no mobile layout) and no pagination: every location of the account is listed. The approved mockups and design spec are in [docs/design/](docs/design/) (see [its README](docs/design/README.md) for the artboards and the choices behind them).

| Part | What the admin sees |
|---|---|
| Header | `Relay` · `Customer admin`, plain text (not a link, so it never resets the URL) |
| Filters | "Viewing as" (account), "Week" and "Activity type" (All activity, Calls, Leads, Appointments) |
| Week control | `◀ Previous week`, a week button showing the week label, and `Next week ▶` |
| Week picker | The week button opens a calendar (Angular Material). Only Mondays from the account's first week to the latest complete week can be chosen, and the chosen week is shaded Monday to Sunday. Helper lines: "Weeks run Monday to Sunday." and "Weeks from Mon Jan 26 to Mon Jul 20, 2026" (both dates come from the API). A **Latest week** button jumps to the latest complete week; it is disabled when you are already there, and it never picks the week in progress. Works by keyboard; Escape closes it |
| Summary | The account total, its usual range and its status |
| Locations table | "Locations — most unusual first": count, "Usually X–Y a week" and status per location |
| Footer | "About these numbers", with the "Data as of" date beside the heading. Three fact tiles: "Compared with the last 8 full weeks at this location", "Inbound events, not unique customers", "Exact duplicates counted once". Under **Keep in mind**: "Locations that usually get 2 or fewer events a week can't show 'lower than usual'", plus "Per-type counts at a single location are small; only large changes show up." when a single activity type is selected |

- **Status is never colour alone.** Every status shows a symbol and words ("▲ Higher than usual", "▼ Lower than usual", "Within usual range", "Not enough history yet (N of 4 weeks needed)"). Colour only adds direction: light red for higher, blue for lower. The green of the fact tiles is the one colour not tied to status.
- **Every filter lives in the URL**, so a reload, Back or a shared link shows the same view. An invalid parameter is reset to its default and the URL is rewritten.
- **States:** "Loading…" while loading; an error card, "We couldn't load this week's activity. Try again.", with a "Try again" button; for account 20, "No activity recorded for this account yet." with the whole week control disabled.

## Interpretation of the ticket

**For the admin:** "Is last week normal for us?" is answered per location and for the account, each against **its own** last 8 full weeks. "Last week" is the last complete Monday–Sunday week in the account's time zone; the week in progress is never judged, because on a Monday it would always look like a collapse.

**Precisely:** counts are compared on the Anscombe scale T(x) = 2·√(x + 3/8), with a band of the median ± 2 robust spreads (floor 1.0), turned back into a whole-number "usually low–high" range. The status is read from that range, so the two can never disagree. With fewer than 4 usable weeks the page says "Not enough history yet" instead of guessing. Not built: alerting, forecasting (out of scope per product), cross-account benchmarks (a different persona). Full rule and ranking: [docs/interpretation.md](docs/interpretation.md).

## Key assumptions

Each is a working assumption from PLAN §4, with seed evidence in [docs/interpretation.md](docs/interpretation.md#key-assumptions).

| Assumption | Evidence in the seed |
|---|---|
| "Now" is the latest event in the data, not the clock; the default week is **2026-07-20** | Data ends Mon 2026-07-27 22:20:34 UTC; the system clock is in September 2026 |
| Only complete weeks, Monday start, in the account's IANA time zone | The last week holds one day of data |
| Account 6's 880-event week is shown, never excluded | 880 events in the week of Jun 1 (805 on Jun 3) against about 70 a week |
| Exact duplicates are ingestion errors, counted once | 12 pairs with adjacent ids and every column equal: 12,626 rows → 12,614 events |
| A site does not exist before its first event | Every site starts in the week of Jan 26 or Feb 2 |
| "All activity" counts events, not customers | There is no customer id |

## Design decisions and trade-offs

The main ones; every rejected option and number is in [docs/decisions.md](docs/decisions.md).

| Decision | Rejected | Why |
|---|---|---|
| Compare each location with its own history | Share of the account; % change against the mean | Share says "all normal" when the whole account moves (spike week). The mean is poisoned by the spike (171 vs a median of 72.5). % change cries wolf on counts of 3–6 |
| Band rule R2\* (Anscombe scale, k = 2) | The first plan's median ± 2·spread | The first rule never flagged a location dropping to zero for leads or appointments (calls: 37 %). R2\* catches 98 % (all) / 96 % (calls) and flags 4.3 % of site-weeks |
| One method for account and locations | A second statistic for siblings | One set of edge cases; still separates "the account moved" from "one site moved" |
| SQL only counts; statistics in C# | Everything in SQL; LINQ | Product rules are unit-tested without a database |
| Default account 14 | Account 12 | Its Site B is the only flagged series in the whole seed for the default week |

**Notable stack choices** (details in [docs/architecture.md](docs/architecture.md)): EF Core for the schema, migrations and seed load, with hand-written SQL for the counting query (exact de-duplication and index use); Angular signals with the URL as the only UI state (no store library); Angular Material only for the week picker.

## Known limits

What the dashboard can and cannot detect. Quoted verbatim from the statistician (`docs/design-consensus.md` §1):

- A location that usually gets ≤ 2 a week can never show "lower".
- A drop to 0 is caught about 90 % of the time at 4+ a week, and about 65 % at 3.
- With a 4-week baseline, false flags rise by about 1 point per side.
- Design flag rate: about 4 % of site-weeks; 13 % of account-weeks show at least one flagged location (`product_monday_view_out.md`).
- A halving at a single site is usually not caught in one week. Of 99 real halvings at median ≥ 6, 34 were flagged and 65 were not (`reviewer_default_week_out.md`).
- A spike stays in the baseline for 7 weeks and widens ranges by about 20–30 % (true of every rule).
- Per-type filters at site level are thin.

Also:
- **By design, about 4 % of location-weeks are flagged** (4.3 % measured) even when nothing has changed. That is the cost of catching 96–98 % of drops to zero.
- **"All activity" can hide a single-type change.** Combined totals have wider ranges, so a change in one type at one location may only show when that type is selected.
- More in [docs/interpretation.md](docs/interpretation.md#known-limits): the seed is not bursty, and time zones with a DST change at local midnight are out of scope.

## Data handling

Duplicates counted once; account 6's spike kept and absorbed by the median; account 20 is a valid empty state (200, not an error); only complete weeks; weeks bucketed in each account's time zone (DST-aware); silent weeks count as 0; early weeks say "Not enough history yet". Details: [docs/interpretation.md](docs/interpretation.md#data-handling).

## Deliberately deferred and another day

Full tables with the reason for each: [docs/decisions.md](docs/decisions.md#deliberately-deferred).

**Deferred:**

| Item | Why |
|---|---|
| Outcome rates (missed-call rate, conversion, no-shows) | Worse small-number problem; NULL outcomes need product decisions first |
| Playwright e2e smoke tests | Deferred by user decision (2026-09-29). The unmerged branch `e2e-scaffold` has the tooling and a placeholder spec; it starts only `ng serve`, so the API must already be running |
| Days from adjacent months in the week picker | They stay blank: Angular Material's calendar can't show them (user decision) |
| Mobile layout, pagination | Desktop only by design decision; accounts have at most 15 locations |
| Second severity tier, sibling-share statistic, near-duplicate policy, spike annotation, sparklines, auth, caching | See [docs/decisions.md](docs/decisions.md#deliberately-deferred) |

**With another day**, in priority order:
1. **The e2e smoke layer**, starting from `e2e-scaffold`: the only test that proves browser, API and seeded DB work together.
2. **Outcome rates, starting with the missed-call rate:** the most actionable thing the data holds.
3. **Recalibrate on bursty data:** the ≈ 4 % flag rate holds only for Poisson-like data.
4. **Make the data-anchor query cheap:** it scans the index on every request.
5. **Show an error when the account list fails to load:** today "Viewing as" is left empty.
6. **A per-type hint in "All activity":** point out when one type is unusual even though the total is within range.
7. **Second severity tier, sparklines, midnight-DST time zones:** once there is real usage to calibrate on, or customers outside the seed's US zones.

## Documentation

| File | What it covers |
|---|---|
| [docs/running.md](docs/running.md) | Prerequisites and versions, `.env` configuration and precedence, running, stopping and resetting the app, EF Core commands, troubleshooting |
| [docs/testing.md](docs/testing.md) | Every test suite, its command, what it needs (Docker or not) and what it checks |
| [docs/api.md](docs/api.md) | `GET /api/accounts` and `GET /api/accounts/{id}/activity-health`: parameters, validation order, status codes, example response |
| [docs/interpretation.md](docs/interpretation.md) | How the ticket was read, how "normal" is decided, key assumptions with seed evidence, data handling, known limits |
| [docs/decisions.md](docs/decisions.md) | Design decisions and rejected options, the numbers behind the band rule, later PLAN §13 decisions, what was deferred, what comes with another day |
| [docs/architecture.md](docs/architecture.md) | Stack with versions and reasons, project structure |
| [docs/design/](docs/design/README.md) | The approved UI mockups and design spec (session 3), including the rejected picker and footer options |
| [PLAN.md](PLAN.md) | The spec, written before the code; §13 is the append-only log of later decisions |
| [docs/acceptance-criteria.md](docs/acceptance-criteria.md) | Given/When/Then acceptance criteria per slice and the approved UI copy |
| [docs/design-consensus.md](docs/design-consensus.md) | The four-agent debate that chose the normality rule, the contract and the data-layer fixes |
| [docs/battle-test/](docs/battle-test/) | Pre-code verification of the plan against the seed: independent re-implementation, SQL Server findings, plan review, statistician report, industry survey |
| [analysis/](analysis/) | Python models and scripts behind every number in these documents and the golden tests |

## How AI was used

The work was agent-first with Claude Code. A coordinator on the main thread dispatched specialist agents ([`.claude/agents/`](.claude/agents/): architect, backend, database, frontend, test-author, product, statistician, designer, reviewer). Each worked in its own git worktree, bound by [CLAUDE.md](CLAUDE.md). The reviewer deliberately ran on a different model from the authors.

The plan was battle-tested against the seed before any code, and the normality rule was reopened when that test found the drop-to-zero blind spot. Every accept, reject, redirect and catch is in [AI_LOG.md](AI_LOG.md), with raw transcripts in [ai-log/raw/](ai-log/raw/) and readable renders in [ai-log/sessions/](ai-log/sessions/), exported by `scripts/export-ai-log.sh`.
