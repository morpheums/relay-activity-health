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
6. C# test names are PascalCase without underscores (analyzer CA1707 stays on). Business-logic tests use no database and no mocks. SQL is tested only against real SQL Server (Testcontainers) — never EF InMemory or SQLite.
7. SQL does counting only (de-dup, bucket into given UTC windows, group). Week math, zero-fill, statistics and ranking live in `Relay.Core`.

## Boundaries
- `Relay.Core` references nothing else in the solution. `Relay.Infrastructure` implements Core's query interfaces. `Relay.Api` is the composition root.
- Only the `architect` edits public contracts (interfaces, records, API DTOs, TS models, abstract API tokens); it never scaffolds or edits project files.
  `backend` owns `.sln`, `*.csproj`, `Directory.*.props`, `global.json`; `frontend` owns `package.json`, `angular.json`.
  Anyone else who needs a contract, package or project change stops and reports it.
- `db/seed.sql` and `db/schema.sql` are never modified.
- Stay inside the file scope you were given.

## Working model
- The main thread is the coordinator: it dispatches subagents from `.claude/agents/`, reviews, merges, verifies. It does not write product code.
- Every agent task runs in its own git worktree — never on the main worktree; each subagent commits only its own scope, and the coordinator merges.
- After every feature lands, the `reviewer` reviews it and the owning agent fixes the findings. Findings that are unclear, conflict with PLAN.md, or need a product/design call go to the user before any fix.
- Every commit message ends with `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.
- Endpoints call exactly one application service; they never inject query interfaces or `DbContext`.

## Handoff report (every agent, every task)
```
## Summary        one or two sentences: what was done
## Changed        paths created/edited
## Verification   exact commands run + trimmed output (never claim green without it)
## Deviations     anything that differs from PLAN.md/CLAUDE.md, and why
## Uncertain      interpretations you had to make; things the coordinator should check
## Next           what the next agent needs to know
```

## Commands
- DB: `docker compose up -d db`
- Backend tests: `dotnet test` (exits 8 while a test project has zero tests — expected until its red suite lands)
- API: `dotnet run --project src/Relay.Api` (fixed port **5080**; the Angular proxy targets it)
- Frontend: `cd web && npm start` · tests: `cd web && npm test`
- AI log export: `scripts/export-ai-log.sh`
