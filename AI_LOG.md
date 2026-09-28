# AI interaction log

Raw, unedited transcripts of every session and subagent live in `ai-log/raw/` (JSONL, exactly as Claude Code wrote them) with readable renders in `ai-log/sessions/`.
Exported by `scripts/export-ai-log.sh`. **The only edit** is literal redaction of personal email addresses and the SA password (`<redacted>`); long tool output is clipped
in the markdown renders only — the raw JSONL is complete.

This file is the curated index: where the AI was accepted, rejected or redirected, and why. Entries are written live during the work, not reconstructed afterwards.

| Session | When (UTC, 2026-09-28) | What |
|---|---|---|
| `52cccc9e` | 16:33–16:45 | First read of the brief (brainstorming) |
| `f8d4159a` | 17:22–17:42 | Brief playback, stack decisions, first AI-log design |
| `cfcd6b64` | 17:42– | Brief playback again, seed-data profiling, design, battle-testing the plan, agent team |
| `cfcd6b64/subagents/*` | | Battle-test agents: independent spec implementation, SQL Server check, adversarial plan review, statistician, industry survey |

## Decision and correction log

Legend: **ACCEPTED** (AI proposal taken as is) · **REDIRECTED** (human changed direction) · **REJECTED** (AI proposal dropped) · **AI CAUGHT** (the AI found its own mistake through verification) · **HUMAN CAUGHT**

### Session 52cccc9e — first pass at the brief
- **HUMAN CAUGHT** — The assistant started designing before looking at the seed data. User: *"Did you take a look at the seed data?"*, then *"Read again the original doc"* after the assistant under-used the product notes. Lesson carried forward: data first, brief re-read before any design.

### Session f8d4159a — stack
- **REDIRECTED** — Stack options were being weighed against locally installed tools. User: *"Do not rely on our local tools, but on what is being asked on the requirement documents first."*
- **REDIRECTED** — Mapper library: user owns DeltaMapper but said to avoid it if it adds complexity → hand-written projections, no mapper.
- **REJECTED (later)** — The assistant proposed a hook-heavy AI-log pipeline (Stop/SubagentStop/PreCompact/UserPromptSubmit/PreToolUse hooks, pre-push gate). In session cfcd6b64 the user rejected it: *"I do not want to overcomplicate with hooks that are not needed"* → one export script + this live file.

### Session cfcd6b64 — design and plan
1. **ACCEPTED** — Seed profiling before design surfaced the traps that drive the design: data ends Monday 2026-07-27 (so "now" must be data-anchored), account 6's 880-event spike week, 12 exact duplicate pairs, empty account 20, tiny per-site counts.
2. **ACCEPTED with refinement** — "Normal = own history". User asked the assistant to re-check against the brief whether sibling comparison should win; the assistant kept own-history as the only statistic but made the view sibling-friendly (account row + locations sorted by deviation) instead of adding a confounded second metric.
3. **REDIRECTED** — User challenged raw SQL on unit-testability. Assistant argued LINQ wouldn't make it unit-testable (InMemory/SQLite change semantics) and instead moved all logic out of SQL into pure C#; SQL only counts. User agreed.
4. **REDIRECTED** — User imposed code rules: SOLID, interfaces + DI, test-first with all tests before implementation, no comment blocks. Assistant pushed back on two literal readings (interfaces for plain data records; C# tests can't compile before types exist) → interfaces for behaviour only, stubs + red commit per layer. User accepted both refinements.
5. **AI CAUGHT** — The assistant's own first band rule ("2nd-lowest to 2nd-highest baseline week") was simulated before being written into the plan: it would flag **34.6 %** of sites every week. Replaced by median ± 2·robust spread (4.8 %).
6. **REDIRECTED** — User: Opus for every agent, not a split; then Sonnet for the reviewer so a different model checks Opus's work.
7. **REDIRECTED** — User: *"I need the plan to be battle tested against the data before writing any line of code … the plan is the most critical part."* → three parallel verification agents before any code.
8. **AI CAUGHT** — Full sweep of the plan's rule over all 9,044 cells: the lower edge of the band is 0 for 73.5 % of site-weeks, so **a location dropping to zero activity is never flagged** for leads/appointments and only 37 % of the time for calls — the worst possible miss for "which location needs attention". Golden values alone didn't reveal it. Rule reopened (see 13).
9. **ACCEPTED** — Independent re-implementation of PLAN §5.3 by an agent that saw only PLAN.md matched the reference model on **all 9,044 cells**; it listed 9 ambiguities (biggest: "first event" under a type filter changes 833 cells) that are now explicit rules.
10. **ACCEPTED / AI CAUGHT** — SQL Server battle test: seed loads as one migration batch in ~1 s; counts match Python on all 5,851 rows; **the planned index was never used** (dedup needs `duration_seconds`/`outcome`) — fixed with INCLUDE columns (200 ms scan → 7 ms seek); a naive `NOT EXISTS` dedup misses 4 of 12 duplicate pairs because of NULL columns — now forbidden in the database agent's rules.
11. **HUMAN CAUGHT** — User: the agent definitions were *"very thin"* → rewritten with role, inputs, deliverables, conventions, must-nevers, done criteria and report format.
12. **REDIRECTED** — Assistant proposed the accounts endpoint call `IAccountQueries` directly. User: *"The endpoint should not call the query directly, it should rely on the service"* → `IAccountService`; endpoints inject services only.
13. **REDIRECTED** — On the band rule, user: *"call one of your experts … also do a web research to see what's the industry standard"*, then *"instead of using a generic agent for the statistician create a specialized agent"* → `.claude/agents/statistician.md` written, generic run stopped and relaunched under that definition, plus a parallel industry survey.
14. **AI CAUGHT** — Writing the export script, the assistant hard-coded the user's email as the default redaction pattern (it would have been committed to a public repo) and a greedy `sed` pattern corrupted JSON escapes. Caught by running the export and validating every file with `jq` and grepping for the secret → literal `perl` redaction from a git-ignored `.ai-log-redact` file.

## Reflection
_(written at the end)_

## Tools and models
_(written at the end)_
