# Relay — Activity health (DASH-247)

A dashboard feature for a Relay customer admin on Monday morning. For last week, it shows whether the account's inbound activity (calls, leads, appointments) was **normal for this account**, and which **locations need attention**. Each location is compared with its own last 8 full weeks and gets a plain-language status and a "Usually X–Y a week" range; unusual locations are listed first. Backend: .NET 10 Minimal API over SQL Server 2022 (EF Core migrations load the provided seed). Frontend: Angular 22.

## What you'll see

Open http://localhost:4200/dashboard. The URL is rewritten to `/dashboard?account=14&week=2026-07-20&type=all`:
- "Viewing as" shows **Beacon Home Security** (account 14).
- The week label reads **Mon Jul 20 – Sun Jul 26, 2026**.
- The summary shows 26 inbound events, usually 18–38 a week, within the usual range.
- The first row of the locations table is **Site B**: 2 events, "Usually 3–12 a week", **"▼ Lower than usual"**. It is followed by Sites C, A and D, all "Within usual range".

## Quick start

```bash
cp .env.example .env                    # then set RELAY_DB_SA_PASSWORD in .env
docker compose up -d --wait db
set -a; source .env; set +a      # ENV-SETUP: bash/zsh only, to be replaced with an OS-agnostic step
dotnet run --project src/Relay.Api      # terminal 1, http://localhost:5080
cd web && npm ci && npm start           # terminal 2, http://localhost:4200/dashboard
```

Tests: `dotnet test` (Docker running) and `cd web && npm test`.

Full setup, configuration and troubleshooting: [docs/running.md](docs/running.md).

## Documentation

| File | What it covers |
|---|---|
| [docs/running.md](docs/running.md) | Prerequisites and versions, environment variables, running, stopping and resetting the app, troubleshooting |
| [docs/testing.md](docs/testing.md) | Every test suite, its command, what it needs (Docker or not) and what it checks; the planned E2E smoke layer |
| [docs/api.md](docs/api.md) | `GET /api/accounts` and `GET /api/accounts/{id}/activity-health`: parameters, validation order, status codes, example response |
| [docs/interpretation.md](docs/interpretation.md) | How the ticket was read, how "normal" is decided, key assumptions with seed evidence, data handling, known limits |
| [docs/decisions.md](docs/decisions.md) | Design decisions and rejected options, the numbers behind the band rule, later PLAN §13 decisions, what was deferred, what comes with another day |
| [docs/architecture.md](docs/architecture.md) | Stack with versions and reasons, project structure |
| [PLAN.md](PLAN.md) | The spec, written before the code; §13 is the append-only log of later decisions |
| [docs/acceptance-criteria.md](docs/acceptance-criteria.md) | Given/When/Then acceptance criteria per slice and the approved UI copy |
| [docs/design-consensus.md](docs/design-consensus.md) | The four-agent debate that chose the normality rule, the contract and the data-layer fixes |
| [docs/battle-test/](docs/battle-test/) | Pre-code verification of the plan against the seed: independent re-implementation, SQL Server findings, plan review, statistician report, industry survey |
| [analysis/](analysis/) | Python models and scripts behind every number in these documents and the golden tests |

## How AI was used

The work was agent-first with Claude Code. A coordinator on the main thread dispatched specialist agents ([`.claude/agents/`](.claude/agents/): architect, backend, database, frontend, test-author, product, statistician, reviewer). Each worked in its own git worktree, bound by [CLAUDE.md](CLAUDE.md). The reviewer deliberately ran on a different model from the authors.

The plan was battle-tested against the seed before any code, and the normality rule was reopened when that test found the drop-to-zero blind spot. Every accept, reject, redirect and catch is in [AI_LOG.md](AI_LOG.md), with raw transcripts in [ai-log/raw/](ai-log/raw/) and readable renders in [ai-log/sessions/](ai-log/sessions/).
