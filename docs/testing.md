# Testing

[← README](../README.md) · [Running](running.md) · [Testing](testing.md) · [API](api.md) · [Interpretation](interpretation.md) · [Decisions](decisions.md) · [Architecture](architecture.md)

Every test suite, how to run it, what it needs and what it checks.

## Run the tests

**One line:** `dotnet test` (from the repo root, with **Docker running**) and `cd web && npm test`.

| Suite | Command (repo root unless noted) | Needs | What it checks |
|---|---|---|---|
| All backend | `dotnet test` | Docker | The three projects below |
| Business logic + service | `dotnet test --project tests/Relay.Core.Tests` | Nothing (no DB, no Docker) | Week calendar (US DST weeks, Phoenix, UTC, week boundaries), zero-fill and eligibility, the normality rule, ranking. Pure unit tests with no mocks; the service tests use hand-written fakes |
| Data | `dotnet test --project tests/Relay.Infrastructure.Tests` | Docker | The counting SQL against a real SQL Server 2022 in Testcontainers (never InMemory or SQLite): de-duplication, window boundaries, type filter, other accounts ignored |
| API + golden values | `dotnet test --project tests/Relay.Api.Tests` | Docker | The endpoints against the real seed: golden values from the independent Python model (PLAN §7, §13), contract shape, 400/404/500 rules, migrate-on-start, missing connection string, seed de-duplication total (12,614) |
| Frontend | `cd web && npm test` | Node, after `npm ci` | Vitest via `ng test` (175 tests): URL-as-state round trip and normalisation, request sequencing, component states and copy. The WeekPicker suite drives the real Angular Material datepicker through component harnesses: only Mondays in range can be chosen, keyboard navigation, re-selecting the current week, the "Latest week" button. The dashboard suite also covers the footer grouping (data-as-of tag, fact tiles, Keep in mind list) |
| End-to-end smoke (**deferred by user decision, 2026-09-29**) | Not on `main`; there is no `npm run e2e` here. Tooling plus a placeholder test live on the unmerged `e2e-scaffold` branch | Docker, Chromium, and the API already running via `scripts/dev.sh` (the scaffold starts only `ng serve`) | Browser, Angular, API and seeded DB together (PLAN §13, "End-to-end smoke layer") |

- **Where expectations come from.** Test expectations come from PLAN.md and the independent Python models in `analysis/`. They were never taken from running the code under test.
- **What is not tested, by decision.** The starter files `db/schema.sql` and `db/seed.sql` stay unmodified by rule (CLAUDE.md), not by a checksum test. Loading `.env` (DotNetEnv) is verified by the run steps in [running.md](running.md), not by startup tests. See PLAN §13 "Test suite pruned to business value".
- **Build settings.** The build uses `TreatWarningsAsErrors` and `AnalysisLevel latest-recommended`, so an analyzer warning fails the build.
