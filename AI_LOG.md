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
| `74b25094` | 2026-09-29 | Session 3: design-first UI/UX redesign on a design canvas (designer agent), picker and footer follow-ups, /simplify, final docs |

## How AI was used

The work was agent-first with Claude Code. A coordinator on the main thread dispatched specialist agents ([`.claude/agents/`](.claude/agents/): architect, backend, database, frontend, test-author, product, statistician, designer, reviewer). Each worked in its own git worktree, bound by [CLAUDE.md](CLAUDE.md). The reviewer deliberately ran on a different model from the authors.

The plan was battle-tested against the seed before any code, and the normality rule was reopened when that test found the drop-to-zero blind spot. Every accept, reject, redirect and catch is in the log below, with raw transcripts in [ai-log/raw/](ai-log/raw/) and readable renders in [ai-log/sessions/](ai-log/sessions/), exported by `scripts/export-ai-log.sh`.

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

24. **REDIRECTED** — Phase 0 was dispatched to the `architect` exactly as PLAN §9 said (scaffold + contracts). User: *"architect is not an implementer … we do have a backend and a frontend specialist"*.
    The coordinator stopped it (its worktree held only a `git mv`, discarded) and asked how to split: user chose **backend scaffolds .NET, frontend scaffolds Angular, architect writes contracts only**,
    with project/package ownership per stack. CLAUDE.md, PLAN §13 and the agent definitions were changed in a worktree.
25. **REDIRECTED (workflow)** — User: every agent task in its own worktree, never on the main worktree; the reviewer after every feature; *"do not do it arbitrary, if there's something that is not clear
    come back to me"*; later *"make sure to pass all decisions through the architect for validation"*. All later decisions went user → architect validation → PLAN §13, and the
    coordinator worked in worktrees too (governance commits, verification builds, this log).
26. **AI CAUGHT (frontend agent)** — While wiring the dev proxy, frontend found macOS AirPlay Receiver answering on :5000 (403 `AirTunes`); the coordinator confirmed with `lsof`. User chose a fixed port 5080.
27. **AI CAUGHT (reviewer, Sonnet)** — Scaffold review: `TreatWarningsAsErrors` + CA1707 makes every snake_case test name a build error (user chose PascalCase), Angular strict mode missing,
    proxy and launch ports disagreed, template leftovers. Acceptance-criteria review: 17 defects incl. a string filed as "approved" that PLAN never approved, values tagged golden that PLAN didn't pin,
    untestable calendar/rounding criteria, a criterion contradicting another about the post-spike week; re-review caught `1.005` (not representable in binary → a correct `Math.Round` fails)
    and DATA-29 contradicting the consensus (type matching lives in the API because the collation is case-insensitive). No golden value was wrong in any review.
28. **AI CAUGHT (architect validating a user decision)** — The user chose `DateTimeOffset` for API instants; the architect checked the serializer output and reported a **conflict**: it writes
    `+00:00`, not the `Z` in PLAN §5.2. User reverted to `DateTime` + an exact-string golden test. The architect also proved with a throwaway test that a malformed `week` returned **500** in
    Development (binding throws); user delegated the fix → strict `yyyy-MM-dd` validation attribute, all malformed variants 400.
29. **ACCEPTED (goldens)** — User promoted 15 more seed scenarios to goldens (week after the spike, a site going silent, mixed history, ties, single-site, per-account de-duplicated totals, …).
    The coordinator recomputed every value from the statistician's model before writing it to PLAN §13 (`analysis/goldens/promoted_goldens.py`); per-account totals sum to 12,614.
30. **USER DECIDED (Phase 0)** — Core stays package-free (`AddRelayCore` in Api), Core-owned wire names, nullable `dataAsOf` only for an empty database (clock fallback via `TimeProvider`),
    UI copy and input-handling rules (all in PLAN §13). Contracts and acceptance criteria both ended **APPROVED** by the reviewer and were merged; next stop is the user's contract review.

31. **HUMAN CAUGHT** — At contract review the user: *"I think IWeekCalendar is a little bit overengineered"*. The architect confirmed (two members had no caller, a custom exception
    guarded an unreachable path) and proposed three options; user chose three members (`Window`, `WeekContaining`, `LatestCompleteWeek`). The Monday check and the baseline window list
    moved to the service; product added BL-46 so the DST week inside the baseline stays pinned.
32. **REDIRECTED (/simplify before locking)** — User: *"Run the /simplify over it before locking in"*. Four reviewers (reuse, simplification, efficiency, altitude) ran in parallel.
    The coordinator dropped false positives with reasons (`TimeProvider` "dead" — it is the empty-DB fallback; `MinLength` "redundant" — proven needed; deriving the account series
    from location series — wrong under §5.3 eligibility; caching/parallel queries — out of budget). User decisions: JSON enum converter instead of Core name tables (reversing an earlier
    choice), Monday check stays in the service. On *"so we do have 3 places? entities, core and api dtos?"* the coordinator explained the layers; user: *"pass it over the architect,
    weighting tradeoffs vs SOLID, clean architecture patterns"* → architect removed the parallel DTO set (Core output records serialised directly, no serializer attributes in Core,
    rounding at the API boundary). This also brought the contracts back in line with backend.md, which already said "never a parallel DTO set". Net: 6 DTOs, 3 mappers, 2 name tables,
    3 small types deleted; reviewer re-ran the real API and confirmed the §5.2 JSON byte-for-byte.
33. **AI CAUGHT (architect)** — Removing the copied constructor fields fails the build while classes are stubs (CS9113 under warnings-as-errors); deferred to implementation instead of
    adding `nameof` workarounds.

34. **ACCEPTED (Phase 1 red suites)** — Three test-author tracks in parallel worktrees (Core 135, Infrastructure 52 on Testcontainers SQL Server, web 179) plus the database track
    (compose, EF model, migrations; seed loads 12,626 rows in ~1.3 s). Every suite was reviewed by the Sonnet reviewer, who recomputed golden values by hand; the user approved the
    combined red run on an integration branch before any implementation.
35. **AI CAUGHT (reviewer)** — Core suite: `AccountService` had no tests and nothing pinned an explicit `week == latestCompleteWeek` (an off-by-one would have passed). Web suite: a sweep
    test that a "Loading…" page would pass, fixture numbers that looked golden but weren't, and no check that user actions add history entries. All fixed before the checkpoint.
36. **AI CAUGHT (database agent)** — Its own probe showed `Database.SqlQuery<record>` returns `DateTime.Kind = Unspecified` while entity reads and scalar queries are `Utc`, and that
    each of the two UTC conversions covers a path the other misses — so it kept both with a one-line why-comment instead of "fixing" the duplicate.
37. **USER DECIDED (Phase 1)** — Against the recommendation: *"count null, empty, 0 as the same so we can dedup easily"* (coordinator verified on the seed first: 12,614 either way,
    no golden changes). Also: no committed dev password (compose requires `.env`, connection string only from the environment), accounts ordered by name, precedence account → Monday → range,
    `earliestWeek = min(first-event week, latest complete week)`, client-side `type` check, first-load-failure URL. All validated by the architect, recorded in PLAN §13.
38. **AI CAUGHT (coordinator)** — The old dev password survived in side-branch history after the env-only fix; the database track was squash-merged, every side branch deleted and the
    repository garbage-collected (object scan: 0 occurrences).

39. **AI CAUGHT (coordinator, own mistake)** — The Phase 1 log export committed the old dev password (10 transcript files, as agents had typed it without the `!`) because the
    commit step did not stop on a non-zero leak count. Caught by the post-commit check; the user approved replacing main's unpushed tip with a clean commit and garbage-collecting.
    The redaction list now holds the bare form, and the export step refuses to commit unless both leak counts are zero.
40. **ACCEPTED (Phase 2 green)** — backend (Core 135/135), database (Infrastructure 52/52; weekly query = one index seek per window, 3–6 ms, totals equal the goldens), frontend (web 179/179),
    each in its own worktree, no test edited, each reviewed by the Sonnet reviewer and verified by the coordinator before merge.
41. **AI CAUGHT (reviewer)** — Core: an untested branch for DST transitions at local midnight whose ambiguous-midnight reading disagreed with `WeekContaining`. User: *"Architect ruling"* →
    out of scope, branch removed, recorded in §13 and the README limits. Web: the logic preventing a duplicate request after the URL rewrite was correct but unpinned — test-author
    added exact request-sequence tests and proved them by mutation (removing the guard fails 6 tests that nothing caught before).
42. **USER DECIDED (Phase 2)** — Filters render before the first report (only the stepper waits); page heading "Activity health"; empty "Usual range" cell for insufficient rows;
    reviewer notes accepted as is (anchor query scans; Unspecified windows treated as UTC; account-list failure silent).
43. **DEVIATION (architect)** — Applying the optional-inputs contract, the architect also edited the two templates, so the behaviour existed before its tests. Recorded rather than hidden;
    test-author then wrote the specs and proved each one fails against a deliberately broken implementation (8 mutations) before the merge.

### Session 2 (late) — configuration, test prune, one-command start
44. **USER DECIDED** — OS-agnostic local configuration: in Development the API loads the repo-root `.env` via DotNetEnv (no clobbering real environment variables); the EF design-time
    factory was removed so `dotnet ef` resolves the context from the API host. Validated by the architect, recorded in PLAN §13.
45. **USER DECIDED** — Test suite pruned to business value: backend 403 → 173 (business rules, SQL correctness, client-facing API behaviour, seeded goldens), web 243 → 124
    (framework, pass-through and duplicate tests dropped; rule variants merged into tables). The four DotNetEnv startup tests went with it; the run steps document that behaviour.
46. **ACCEPTED** — `scripts/dev.sh` starts the database, API and web with one command; review fixes added a re-entry guard on cleanup, fail-fast on busy ports and detection of a partial `npm ci`.
47. **ACCEPTED** — A `designer` agent (mockups, design spec, UI library choice; never product code) so the redesign could be approved as a design before any Angular change.

### Session 74b25094 — UI/UX redesign, design first
48. **REDIRECTED** — While the designer was already drafting, the user: *"First run the application and take a look with playwright so you can first assess the current design"*. The
    coordinator screenshotted every state (default, spike, insufficient data, empty, loading, error, 390px) and sent an eight-point assessment to the running designer, who mapped each
    point to a fix in its handoff.
49. **USER DECIDED (design review)** — Desktop only (no mobile work), no pagination, Instrument Serif replaced by *"something more professional"* (delegated → Geist 600 throughout),
    "higher than usual" in light red instead of rust, the error-icon red allowed to share that family. Mockups published to the canvas and committed to `docs/design/`.
50. **HUMAN CAUGHT** — *"Remember the specialized agent must work on its own worktree and sub agent session, leave the main session free"*: the coordinator had been running `npm ci` and
    test runs on main after merges. From then on every build, install, test run, app launch and screenshot ran inside an agent's worktree; the coordinator only dispatched, merged and relayed.
51. **AI CAUGHT (reviewer)** — Re-selecting the current week left the picker open (Material emits `selectedChange` only on a change). The user approved adding one test to the frozen
    suite; fixed with MatCalendar's `_userSelection` output. The same review claimed a Material prerelease was installed; the implementer showed the lockfile was stable 22.2.0, the
    reviewer retracted, and the versions were pinned exactly anyway.
52. **USER DECIDED (review calls)** — Picker header "July 2026" (format override plus a one-line CSS fix for Material's forced upper case), weekday headers stay "M T W T F S S",
    the 15px gap between the loading placeholder and the real week control fixed so nothing shifts on load.
53. **ACCEPTED (explanation)** — User: *"i need to filter by appointments in order to see the red one? not in ALL activity, why?"* The coordinator checked the live API before answering:
    Site M's 3 appointments vs usually 0–2 is flagged, but its 7 events vs usually 1–9 in the combined view is not, because combined totals vary more (PLAN D2: each view against its own
    history). A per-type hint in "All activity" was offered as a future feature, not built.
54. **USER DECIDED (against the recommendation)** — "Latest week" button in the picker footer. For weeks crossing months, the designer confirmed Material cannot show adjacent-month
    days (angular/components #26768, #29549 open) and drew three options; the user kept Material (option A) rather than the recommended CDK week list.
55. **USER DECIDED (against the recommendation)** — Footer option B (icon fact strip) rather than the recommended C, with green tiles and a more prominent "Data as of" tag. The user edited
    the canvas live (moved the Leads state into its own piece); two publishes were refused as stale, and the coordinator re-read the live files and merged onto them instead of overwriting.
56. **AI CAUGHT (reviewer)** — Frontend added a narrow-screen media query despite "desktop only" → removed. The component style budget was raised from 4 kB to 6 kB because the footer
    must stay in `DashboardPage`; accepted as debt.
57. **REDIRECTED (/simplify)** — Four reviewers (reuse, simplification, efficiency, altitude). Applied: picker overlay styles moved into the component, shared badge/card/control styles,
    one status-icon helper, the picker filter reduced to `isMonday`, `CdkTrapFocus` instead of `A11yModule`, cached date formatters, Latin-only Geist (global CSS 19 → 12 kB). Before/after
    screenshots of six views were pixel-identical. Skipped with reasons: anything that would change user-approved behaviour or visuals, architect-owned contracts, frozen tests.
    The page style still measures 4.85 kB, so the 6 kB budget stays.
58. **USER DECIDED** — Playwright e2e deferred; the `e2e-scaffold` branch stays unmerged (tooling and a placeholder; its "API must already be running" deviation never adopted).
    Recorded in PLAN §13, the README and `docs/testing.md`.
59. **AI CAUGHT (coordinator)** — The final export failed on one transcript: the `SA_PASSWORD` redaction pattern treated the backslash of a JSON `\n` escape as part of the key and left
    an invalid `\<redacted>` escape. The pattern now accepts an escaped quote only as a whole `\"` pair; all 199 transcripts parse, and the secret, email and password-value leak counts are 0.

## Reflection
- **Data and rules first paid off.** Profiling the seed before designing surfaced the traps (data-anchored "now", the 880-event spike, duplicates, the empty account), and every
  number the UI shows traces to a PLAN golden or a live API read; nothing was invented for a mockup.
- **Test-first with frozen suites kept the agents honest.** Implementers could not move the goalposts; when a real gap appeared (re-select, Latest week) the user approved each new test.
- **The cross-model reviewer earned its place**, catching real defects (untested DST branch, unpinned request guard, re-select, scope creep), and it was also wrong at least once
  (the prerelease claim). Its findings were verified before acting, never applied blindly.
- **Design first was the right call for the UI.** Two review rounds on a canvas (font, colours, picker, footer) cost minutes; the same changes after implementation would have meant
  rewriting tests. Where the user overruled a recommendation (picker A, footer B) the trade-off was stated and the decision recorded.
- **What went wrong:** the coordinator leaked a secret into a log export once (entry 39) and ran work on the main session before the user corrected it (entry 50). Both became hard rules.
- **Left for another day:** Playwright e2e, the per-type hint in "All activity", adjacent-month days in the picker, trimming `DashboardPage` styles back under 4 kB.

## Tools and models
- **Claude Code** as the harness: one coordinator session dispatching one-shot subagents from `.claude/agents/`, each in its own git worktree, merged by the coordinator.
- **Claude Opus 5.5** for the coordinator and the authoring agents (architect, backend, database, frontend, test-author, product, statistician, designer);
  **Claude Sonnet** for the read-only `reviewer`, so reviews come from a different model than the code.
- **A design canvas artifact** (claude.ai) for reviewable mockups; sources mirrored in `docs/design/`.
- **Playwright** (from the e2e worktree) for screenshots and browser checks; **Testcontainers** SQL Server for SQL and golden tests; **Vitest** with Angular Material component harnesses for the web suite.
