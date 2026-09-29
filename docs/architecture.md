# Architecture and stack

[← README](../README.md) · [Running](running.md) · [Testing](testing.md) · [API](api.md) · [Interpretation](interpretation.md) · [Decisions](decisions.md) · [Architecture](architecture.md)

Stack components with versions and reasons, and the project layout.

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
| **Angular Material + CDK** (`MatCalendar`, CDK overlay) | 22.2.0 (pinned) | The week picker only. A maintained calendar with keyboard and screen-reader support, instead of a hand-written one |
| `@angular/material-date-fns-adapter`, **date-fns** | 22.2.0, 4.4.0 | The picker's date adapter: `enUS` locale with `weekStartsOn: 1`, so weeks start on Monday like the API |
| **@fontsource/geist** | 5.3.0 | Geist, the only typeface, self-hosted with the app instead of loaded from a font CDN |
| **URL query params as the only UI state** | — | The brief requires that state survives a reload. With the URL as the source of truth, reload, Back and shared links all work without extra code, and invalid params are rewritten to defaults |
| **Week picker**: `MatCalendar` in a CDK connected overlay | — | Opened by our own trigger, which owns the overlay, focus return and the selected week. Monday-first (`enUS`), and only Mondays from the earliest week to the latest complete week are selectable |
| **Lazy dashboard route** (`loadComponent`) | — | The dashboard, with Material and date-fns, loads as its own chunk, which keeps the initial bundle small |
| **Abstract-class DI tokens** for the API clients | — | Components depend on an abstraction, so tests swap in a fake without HTTP mocks |
| **Vitest** via Angular's `@angular/build:unit-test`, jsdom | 5.0.2, 30 | Current Angular default, fast, no browser needed |
| TypeScript | 6.0.3 | Required by Angular 22 |
| No mapper library, no DTO layer | — | Core records are shaped like the JSON and serialisation is configured once in the API, so a contract change is one edit |

---

## Project structure

```
db/                        schema.sql, seed.sql — the starter files, never modified (CLAUDE.md rule)
src/
  Relay.Core/              no project references, no packages
    Calendar/              IWeekCalendar: local Monday ↔ UTC window (DST-aware), week containing an instant, latest complete week
    Normality/             zero-filled weekly grid, baseline evaluator (R2*), location ranker, NormalityOptions
    ActivityHealth/        IActivityHealthService: orchestration, the report records, invalid-week reasons
    Accounts/              IAccountService: account list ordered by name
    Queries/               IActivityQueries, IAccountQueries (implemented in Infrastructure), ActivityType
  Relay.Infrastructure/
    Composition/           AddRelayInfrastructure (DbContext, connection string "Relay")
    Persistence/           RelayDbContext, entity configurations, UTC converter
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
    components/            DashboardFilters, WeekPicker (week-picker.ts), AccountSummary, LocationTable, icon.ts (inline SVG icons)
  testing/                 fakes, fixtures, DOM and router helpers
docs/                      acceptance-criteria.md, design-consensus.md, battle-test/, handoff/
  design/                  session 3 mockups and design spec (Spec.dc.html: tokens, type scale, states, picker anatomy)
analysis/                  Python models and scripts behind every number here and in the golden tests
ai-log/                    raw/ transcripts (JSONL) and sessions/ readable renders
scripts/export-ai-log.sh   exports and redacts the AI log
.claude/agents/            agent definitions; CLAUDE.md holds the rules every agent follows
```

| Document | What it is |
|---|---|
| [`PLAN.md`](../PLAN.md) | The spec, written before the code. §13 is the append-only log of later decisions and their reasons |
| [`docs/acceptance-criteria.md`](acceptance-criteria.md) | Given/When/Then criteria per slice, and the approved UI copy |
| [`docs/design-consensus.md`](design-consensus.md) | The four-agent debate that chose the R2\* rule, the contract and the data-layer fixes |
| [`docs/battle-test/`](battle-test/) | Pre-code verification: independent re-implementation, SQL Server findings, adversarial plan review, statistician report, industry survey |
| [`AI_LOG.md`](../AI_LOG.md) | Curated decision log |
