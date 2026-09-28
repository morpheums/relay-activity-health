# CLAUDE.md — relay-activity-health

DASH-247: help a Relay customer admin answer "is this normal for us?" per account and per location. `PLAN.md` is the spec — read it before any work.
If a task conflicts with `PLAN.md`, stop and report; don't improvise.

## Stack
.NET 10 Minimal APIs · EF Core (schema, migrations, seed) · hand-written SQL for aggregates via `Database.SqlQuery<T>` · SQL Server 2022 (docker compose) ·
xUnit + Testcontainers · Angular (standalone, signals, URL query params as state) · Vitest.

## Non-negotiable code rules
1. **SOLID, no tight coupling.** Every class with behaviour (services, calculators, calendars, queries, API clients) depends on interfaces and is registered in DI.
   Records, DTOs, options and value objects are plain data — no interfaces for them. Angular: abstract-class DI tokens.
2. **No comment blocks. Forbidden.** No XML doc blocks, no banner comments, no multi-line explanations, no commented-out code.
   Code explains itself through names. A single-line comment only when the *why* is not expressible in code.
3. **Names are descriptive and domain-based**: `baselineWeeks`, `eligibleWeekCounts`, `latestCompleteWeekStart` — never `data`, `tmp`, `list2`, `x`.
4. **Test-first, per layer.** Interfaces + records + `NotImplementedException` stubs → the complete test suite (happy path + every edge case in PLAN §7) →
   commit **red** → user reviews the tests → implement to green → reviewer → next layer. Never write implementation and its tests in the same step.
5. **Test expectations come from PLAN.md** (§5.3 rules, §7 golden values), never from running the implementation.
6. Business-logic tests use no database and no mocks. SQL is tested only against real SQL Server (Testcontainers) — never EF InMemory or SQLite.
7. SQL does counting only (de-dup, bucket into given UTC windows, group). Week math, zero-fill, statistics and ranking live in `Relay.Core`.

## Boundaries
- `Relay.Core` references nothing else in the solution. `Relay.Infrastructure` implements Core's query interfaces. `Relay.Api` is the composition root.
- Only the `architect` agent edits `.sln`, `*.csproj`, `Directory.*.props`, `package.json`, `angular.json`, or public contracts (interfaces, records, API DTOs).
  Anyone else who needs a contract change stops and reports it.
- `db/seed.sql` and `db/schema.sql` are never modified.
- Stay inside the file scope you were given.

## Working model
- The main thread is the coordinator: it dispatches subagents from `.claude/agents/`, reviews, merges, verifies. It does not write product code.
- Parallel tracks run in separate git worktrees; each subagent commits only its own scope.
- Every commit message ends with `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.
- Report back: what changed (paths), test command + result, anything you were unsure about or deviated from. Never claim green without the command output.

## Commands
- DB: `docker compose up -d db`
- Backend tests: `dotnet test`
- API: `dotnet run --project src/Relay.Api`
- Frontend: `cd web && npm start` · tests: `cd web && npm test`
- AI log export: `scripts/export-ai-log.sh`
