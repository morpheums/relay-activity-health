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
| `485c1256` | 18:45– | Session 2: four-agent design debate (statistician, product, architect, reviewer talking directly via `SendMessage`), user approval, PLAN §13 |

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

15. **ACCEPTED (reviewer on Sonnet caught gaps in the Opus-written plan)** — The adversarial review reproduced every golden value independently and confirmed the drop-to-zero blind spot (8). New findings accepted for the plan revision:
    `baseline: null` for insufficient data leaves the UI's "3 of 4 weeks" with nowhere to live → always return `weeksUsed`; `earliestWeek` undefined and the before-earliest case unspecified;
    **no agent owned the README's required "how to run" and "stack choices" sections** → assigned; ranking treats a lucky busy week like a location going quiet → sent to the statistician;
    band calibration only simulated on full 8-week baselines, never at the 4-week minimum → sent to the statistician.
    Partly **REJECTED**: "Docker SQL Server + Testcontainers is over-investment" — the user already runs SQL Server containers and the battle test showed a ~1 s seed load; kept, but DB-layer extras (DST at the SQL layer, plan-shape assertions) moved into the cut line.

16. **REDIRECTED** — User: *"make sure the agents talk directly between each other and they agree on a design, do not intervene in their conversation, they should debate and reach an agreement."*
    The coordinator only launches the debate (inputs, protocol, stopping rule) and brings the agreed design plus any recorded dissent to the user; it does not relay or steer messages.
    Inputs: statistician report, industry survey (17 products; recommends ±3 band, ≥6 weeks, "too few to judge" below median 2 — simulated on synthetic Poisson data, not the seed), independent-implementation ambiguities, SQL Server findings, adversarial review.

17. **ACCEPTED as input to the debate / AI CAUGHT** — Statistician (literature: c-charts, XmR, EARS, Farrington/Noufaily, Iglewicz–Hoaglin, Anscombe, Freeman–Tukey; simulations on the seed) recommends R2\* (Anscombe robust z, k = 2, floor 1).
    It **caught a bug in the coordinator's own comparison script** (`alt.py` back-transform used `floor(lo)+1`/`ceil(hi)-1` instead of `ceil`/`floor`; harmless on this data) and a trap any implementer would hit
    (squaring a negative lower edge produces a false positive bound → mandatory guard). Consequence the user must weigh: account 12's Site F (default account D5) is no longer flagged in the default week.
    The coordinator re-ran the statistician's golden script from the repo copy: output identical. Not applied to PLAN.md — goes to the agent debate (session 2).

### Session 485c1256 — design debate
18. **REDIRECTED (twice) / REJECTED** — The coordinator launched the four specialists as named background agents talking via `SendMessage`, per the kickoff.
    User: *"Instead of background agents/named agents, make sure you use one shot sub agents, so we dont keep all those idle"* → the coordinator stopped them and
    proposed file-based rounds (one-shot agents reading/writing `docs/debate/`). User rejected that: *"I do not want agent to use files to communicate, they must call each
    other and direct communication."* The coordinator explained that one-shot subagents cannot message each other (no `SendMessage`/`Agent` in their tools) and offered
    named agents with auto-stop; user: *"Do not auto stop, because they must discuss … till they have an agreed design"* → relaunched as named agents that stay until
    all four sign-offs. The rejected protocol file had still been written to disk; the architect noticed it and the coordinator deleted it.
19. **ACCEPTED** — Debate ran without coordinator messages (≈ 5 min, Round 0 + one rebuttal round). All four signed `docs/design-consensus.md` AGREE, no dissent, no open items:
    R2\* (Anscombe robust z, k = 2, floor 1, ≥ 4 weeks, status from the displayed integer range); neutral copy ("Higher/Lower than usual", "Within usual range"),
    no severity tier; ranking flagged-first then |z|, below before above on ties; `baseline` always present with `weeksUsed`; one `minimumEligibleWeeks`;
    `earliestWeek` = week of first event, never null, earlier weeks → 400; type filter never changes the site list; all SQL Server findings adopted; default account 12 → 14.
    Positions that changed: architect's ranking key (|z| alone → statistician's flagged-first key); architect and product crossed on empty-account `earliestWeek`
    (each switched to the other's position in the same round) and settled on non-null; reviewer dropped row-level `eligibleWeeks`; product dropped per-row `weeksRequired`;
    product, statistician and reviewer all moved D5 from 12 to 14 after product's and reviewer's scripts showed account 14's Site B is the only flagged series in the default week.
20. **AI CAUGHT (agent on itself)** — The statistician told the others that T(median) and median(T) differ by < 0.01 for even baselines, then measured it and retracted:
    up to 0.289, one status flips in 7,136 cells. Decision unchanged, but the spec now names the centre explicitly and a golden case ([2,4,6,20] → 1–14, wrong reading 1–13) pins it.
    Reviewer (Sonnet) independently re-derived the account 6 / account 8 / [2,4,6,20] goldens and small-median rows before signing.
21. **ACCEPTED after coordinator verification** — Coordinator re-ran all 8 `analysis/debate/*.py` scripts and `analysis/statistician/golden.py`: every output byte-identical to the
    saved `*_out.md`; spot-checked the consensus figures (−2.16, 46/341, 34/99, 0.289, 1,161, 110, 253,149, even-count and guard goldens, acct 14 total 18–38) against them.
    Surfaced to the user: borderline default flag (z −2.16), seed not bursty (false flags could rise to 5–8 %/side), starter files at repo root not `db/`.
22. **USER DECIDED** — *"Agree with the plan as designed"*: consensus approved in full, including default account 14, `git mv` of the starter files into `db/` in Phase 0,
    and the README known limits. Appended to PLAN §13 (replacement text copied verbatim from the consensus); earlier sections left as written.

23. **AI CAUGHT** — Before committing, the coordinator's post-export check found the user's email surviving redaction in session cfcd6b64's transcript as regex-escaped
    copies (`…@outlook\\.com` at several JSON escape levels, typed while building the redaction in entry 14); literal redaction can't match them, and two earlier local
    commits already contain them (no remote, never pushed). Fix: the bare username is now a redaction literal in the git-ignored `.ai-log-redact`; re-export → 0 hits,
    all JSONL valid. Whether to rewrite the two local commits is left to the user.

## Reflection
_(written at the end)_

## Tools and models
_(written at the end)_
