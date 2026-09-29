# Relay — Activity health (DASH-247)

A dashboard for a Relay customer admin on Monday morning. It answers two questions about last week's inbound activity (calls, leads, appointments): **is this number normal for our account**, and **which locations need attention**. Each location is compared with its own last 8 full weeks, gets a plain-language status and a "Usually X–Y a week" range, and unusual locations are listed first. Stack: .NET 10 Minimal API, EF Core and SQL Server 2022, Angular 22.

## Quick start

**One command (macOS, Linux):**

```bash
cp .env.example .env        # then set RELAY_DB_SA_PASSWORD in .env
scripts/dev.sh              # DB + API + web; open http://localhost:4200
```

- **Needs:** Docker (Compose v2), .NET 10 SDK, Node.js 22.22+/24.15+/26 with npm. Exact versions: [docs/running.md](docs/running.md#prerequisites). If the password contains `$`, single-quote it: `RELAY_DB_SA_PASSWORD='Pa$w0rd…'`.
- **What it does:** stops early if `.env` is missing or port 5080 or 4200 is in use. Otherwise it starts the DB and waits until it is healthy, runs `npm ci` in `web` on the first run, starts the API (waits up to 120 s), then serves the web app.
- **Stop:** Ctrl+C stops the API and web. The DB keeps running: `docker compose down`, or `docker compose down -v` to wipe the data.

**Step by step (Windows, or any OS):** see [docs/running.md](docs/running.md#step-by-step-windows-or-any-os), which also has configuration and troubleshooting.

**Tests:** `dotnet test` (Docker running) and `cd web && npm test`. The Playwright end-to-end layer is deferred ([docs/deferred.md](docs/deferred.md)); there is no `npm run e2e` on `main`.

## The dashboard

- Opens on account 14, **Beacon Home Security**, week of **Mon Jul 20, 2026**: Site B is listed first, "▼ Lower than usual" (2 events, usually 3–12).
- Filters: "Viewing as" (account), week (stepper plus a Monday-only picker) and activity type. Every filter lives in the URL, so reload, Back and shared links keep the view.
- Status is never colour alone: always a symbol and words, plus "Not enough history yet" when a location has under 4 weeks of history.
- Desktop only, no pagination. Approved mockups: [docs/design/](docs/design/README.md); every part of the page: [docs/dashboard.md](docs/dashboard.md).

## Known limits

What the dashboard can and cannot detect. Quoted verbatim from the statistician (`docs/design-consensus.md` §1):

- A location that usually gets ≤ 2 a week can never show "lower".
- A drop to 0 is caught about 90 % of the time at 4+ a week, and about 65 % at 3.
- With a 4-week baseline, false flags rise by about 1 point per side.
- Design flag rate: about 4 % of site-weeks; 13 % of account-weeks show at least one flagged location (`product_monday_view_out.md`).
- A halving at a single site is usually not caught in one week. Of 99 real halvings at median ≥ 6, 34 were flagged and 65 were not (`reviewer_default_week_out.md`).
- A spike stays in the baseline for 7 weeks and widens ranges by about 20–30 % (true of every rule).
- Per-type filters at site level are thin.

By design, **about 4 % of location-weeks are flagged** (4.3 % measured) even when nothing has changed; that is the cost of catching 96–98 % of drops to zero. More limits (All activity hiding a single-type change, bursty data, midnight-DST zones): [docs/interpretation.md](docs/interpretation.md#known-limits).

## Documentation

| Doc | What it covers |
|---|---|
| [Running](docs/running.md) | Prerequisites, `.env` configuration, step-by-step run, stop and reset, EF Core commands, troubleshooting |
| [API](docs/api.md) | The two endpoints: parameters, validation order, status codes, example response |
| [Interpretation](docs/interpretation.md) | How the ticket was read, how "normal" is decided, key assumptions with seed evidence, data handling, known limits |
| [Decisions](docs/decisions.md) | Design decisions and trade-offs, rejected options, the numbers behind the band rule, later PLAN §13 decisions |
| [Architecture](docs/architecture.md) | Stack with versions and reasons, project structure, the other reference documents |
| [Testing](docs/testing.md) | Every test suite, its command, what it needs and what it checks |
| [Dashboard](docs/dashboard.md) · [Design](docs/design/README.md) | What each part of the page shows; the approved mockups and design spec |
| [Acceptance criteria](docs/acceptance-criteria.md) | Given/When/Then criteria per slice and the approved UI copy |
| [Deferred](docs/deferred.md) | What was left out on purpose, and what comes with another day, in priority order |
| [PLAN.md](PLAN.md) | The spec, written before the code; §13 logs later decisions |
| [AI_LOG.md](AI_LOG.md) | How AI was used: agents, worktrees, every accept, reject and correction, raw transcripts |
