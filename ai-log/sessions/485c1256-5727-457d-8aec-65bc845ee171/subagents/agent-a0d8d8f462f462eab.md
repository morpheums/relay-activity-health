
### 🧑 USER — 2026-09-28T22:02:35.076Z

DASH-247: validate a user decision (VALID / CONFLICT / CONTRACT IMPACT) — no code, no commit; reply in ≤12 lines plus the exact PLAN §13 text you'd record.

Decision: add an automated end-to-end layer — a small Playwright smoke suite (4–6 tests) driving a real browser against the real API + Angular dev server on the seeded SQL Server — **as the very final step**, only after everything else is covered and validated (API+golden suite green, frontend's manual end-to-end check, product acceptance, reviewer's full pass). Candidate scenarios: default view (account 14, Site B "▼ Lower than usual", 2 vs "Usually 3–12 a week"), account 6 spike week 2026-06-01 (880, all sites above), account 20 empty state, an invalid URL param rewritten (e.g. `week=2026-07-21` → latest week), maybe a filter change with Back.

Check against CLAUDE.md (test-first per layer: red then green — how does that apply to an e2e layer written after the app works? propose how to prove the tests can fail, e.g. mutation or running against a stopped API), PLAN §7 test layers, §9 phases, §10 cut line (this becomes the first thing cut if time runs out?), ownership (`frontend` owns `web/package.json` — adds `@playwright/test`; `test-author` writes specs; location e.g. `web/e2e/`), how it gets the stack running (Playwright `webServer` for `ng serve` + API with `ConnectionStrings__Relay` from env and Docker `db` up and migrated — no committed secrets), and the README one-liner. Also say whether it should be excluded from `npm test` (separate `npm run e2e`).


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a0d8d8f462f462eab\" && grep -n \"^#\" PLAN.md | head -80 && wc -l PLAN.md","description":"List PLAN.md headings"}
```


<details><summary>tool result</summary>

```
1:# PLAN — DASH-247 "Is this normal for us?"
9:## 1. Interpretation of the ticket
23:## 2. What the seed data told us
42:## 3. Decisions (with rejected alternatives)
54:## 4. Assumptions & open questions (not sent to recruiter — working assumption stated)
65:## 5. Design
67:### 5.1 Database
76:### 5.2 API
104:### 5.3 Normality rules (the product logic — pure C#)
119:### 5.4 Frontend
130:## 6. Architecture & code rules
165:## 7. Test plan
197:## 8. Agent team & working model
215:## 9. Execution phases & parallelisation
234:## 10. Time budget & cut line
242:## 11. Deferred (deliberately)
248:## 12. AI log (minimal, no hooks)
255:## 13. Plan changes
258:### 2026-09-28 — Revised design from the four-agent debate (approved by the user)
278:#### §5.1 — index and de-duplication (replaces the index bullet)
284:#### §5.2 — API (replaces §5.2)
317:#### §5.3 — normality rules (replaces §5.3)
340:#### §5.4 — status copy and footnote (replaces the two §5.4 bullets on status and footnote)
365:#### §7 — golden values and Evaluator / Ranking / API edge cases (replace the corresponding parts of §7)
413:### 2026-09-28 — Phase 0 split by specialty; project-file ownership per stack (user decision)
424:### 2026-09-28 — Phase 0 decisions and promoted golden values (user decisions)
446:### 2026-09-28 — Input handling and UI copy decisions (user decisions, validated by the architect)
460:### 2026-09-28 — Contract decisions from the Phase 0 contract review (user decisions, validated by the architect) …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a0d8d8f462f462eab\" && sed -n 165,256p PLAN.md && sed -n 413,424p PLAN.md && sed -n 512,544p PLAN.md && cat web/package.json | head -30 && ls web README* 2>/dev/null; grep -n -i \"e2e\\|playwright\\|end-to-end\" PLAN.md README.md 2>/dev/null","description":"Read PLAN sections 7-12, ownership, recent changes"}
```


<details><summary>tool result</summary>

```
## 7. Test plan

| Layer | Project | Kind |
|---|---|---|
| 1 Business logic | `Relay.Core.Tests` | Unit, pure |
| 2 Service | `Relay.Core.Tests` | Unit, hand-written fakes of `IActivityQueries`/`IAccountQueries` |
| 3 Data | `Relay.Infrastructure.Tests` | Integration, Testcontainers, hand-built fixtures |
| 4 API + golden | `Relay.Api.Tests` | Integration against the real seed |
| 5 Frontend | `web` (Vitest) | `DashboardState` URL round-trip + normalisation; `LocationTable`/`AccountSummary` states |

**Edge cases that must have tests**
- Calendar: DST start week (Mar 8 2026) and end week (Nov 1 2026) in America/Chicago; America/Phoenix (no DST); UTC; event exactly at a week boundary (belongs to the new week);
  latest complete week when the anchor is Monday vs Sunday 23:59:59 local vs exactly Monday 00:00 local; non-Monday week rejected; invalid IANA id.
- Grid: site with zero events in `W` appears with 0; site silent for the whole baseline; weeks on/before first-activity week ineligible; site whose first event is after `W` excluded.
- Evaluator: < 4 eligible weeks; MAD = 0 (floor applies); median 0; even-count median; spike inside baseline; low clamped at 0; exactly on the band edge is `normal`.
- Ranking: insufficient last; ties by name; above and below ranked by magnitude together.
- SQL: exact duplicates counted once, near-duplicates not; boundary instant; type filter; other accounts' rows ignored; events outside windows ignored; no rows → empty.
- API: 404 unknown accou …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a0d8d8f462f462eab\" && sed -n 424,512p PLAN.md | grep -n -i \"14\\|site b\\|3–12\\|normalis\\|week=\\|Lower than\\|Usually\\|default\" ; cat web/proxy.conf.json; grep -rn \"e2e\\|vitest\\|include\" web/angular.json web/tsconfig.spec.json | head","description":"Check scenario facts and test config"}
```


<details><summary>tool result</summary>

```
16:| Account 14, week 2026-01-26, all (earliest week) | total 2, `insufficient_data`; only Sites B and D listed (1 each), both `insufficient_data` |
17:| Account 14, week 2026-02-02, all | total 27, `insufficient_data`; all four sites listed, all `insufficient_data` (0 eligible weeks) |
18:| Account 14, week 2026-03-02, all (mixed history) | total 40, median 25, range 16–36, `above`; Site D 16 vs 2–11 `above` (1st), Site B 9 vs 2–10 `normal` (2nd), Sites A and C `insufficient_data` (last, by name) |
19:| Account 14, week 2026-07-20, all | ranking order B, C, A, D |
20:| Account 14, week 2026-07-20, `appointment_set` (ties) | total 2, range 1–8, `normal`; Site A 0 vs 0–4 (dev −1.12) first; Sites B, C, D have deviation 0 → ordered by name B, C, D |
28:- **URL normalisation:** any invalid URL parameter is rewritten to its default (latest complete week, `all`, account 14) with `replaceUrl` — never snapped to the nearest Monday.
53:| De-duplicated events per account, all weeks | 1: 1221 · 2: 729 · 3: 477 · 4: 796 · 5: 884 · 6: 2637 · 7: 437 · 8: 260 · 9: 546 · 10: 342 · 11: 354 · 12: 1303 · 13: 205 · 14: 638 · 15: 499 · 16: 167 · 17: 323 · 18: 586 · 19: 210 · 20: 0 · total 12614 |
54:| Account 14 site first-activity weeks | Sites B and D: 2026-01-26; Sites A and C: 2026-02-02 |
55:| Account 14, week 2026-07-20, `appointment_set` | Site B 0, median 0, range 0–2, `normal` |
56:| Account 14, week 2026-07-20, `call_received` / `lead_created` | totals 16 (range 9–24, `normal`) / 8 (ran …[clipped in markdown; full text in raw/]
```
</details>


### 🤖 ASSISTANT — 2026-09-28T22:03:05.904Z

**Verdict: VALID, with conditions.** No public contracts change. It only touches files the frontend agent owns.

1. **Rule 4 (red, then green).** The specs can't honestly start red, because the app already works. Record this as a deviation. Instead, each spec proves it can fail: turn off one expected behaviour in a throwaway branch (for example the Site B status label, or the rewrite of a bad URL value), run it and see it go red, then restore. Record the red and green output in the handoff. A stopped API only proves the stack can't be reached, so it doesn't count as proof.
2. **Rule 5 (expectations come from PLAN.md).** "Site B, 2 vs Usually 3–12" is not in PLAN. For account 14 in week 2026-07-20, PLAN only gives the ranking order B, C, A, D. Either promote that value from the Python model into §13 first, or assert only the ranking and the status. Account 6's week of 2026-06-01, account 20's empty state and the default week all trace to §7. The `week=2026-07-21` rewrite traces to the URL-normalisation rule in §13.
3. **§7 layers and §9 phases.** This adds a sixth test layer and a Phase 4 after Phase 3. The reviewer then reviews the new specs, since its full pass is already done by then.
4. **§10 cut line.** This becomes the first cut, before the frontend component tests. It is not in the "never cut" list.
5. **Ownership.** `frontend` adds `@playwright/test`, the `e2e` script, `playwright.config.ts` and gitignore entries for `test-results/` and `playwright-report/`. `test-author` writes the specs in `web/e2e/*.e2e.ts`. They select by role and approved copy, and no test ids are added.
6. **Running the stack.** Playwright's `webServer` config starts both `dotnet run --project ../src/Relay.Api` and `ng serve`. Each waits for its URL and reuses a server that's already running. The API gets `ConnectionStrings__Relay` from your environment. The database must already be up (`docker compose up -d db`); the API migrates and seeds it on start. Nothing secret is committed.
7. **Keep it out of `npm test`.** Name the files `*.e2e.ts` and put them outside `src/`, so Vitest never picks them up. They run only through `npm run e2e`.

**PLAN §13 text to record:**

```
### 2026-09-28 — End-to-end smoke layer (user decision, validated by the architect)

**Reason.** One automated check that the real browser, Angular app, API and seeded SQL Server work together. Adds §7 layer 6 and a Phase 4 to §9.
- **Layer 6 — E2E smoke** (`web/e2e/*.e2e.ts`, Playwright, Chromium, 4–6 tests): default view (account 14, latest complete week 2026-07-20, ranking B, C, A, D);
  account 6 week 2026-06-01 (total 880, all 15 sites `above`); account 20 empty state; invalid `week=2026-07-21` rewritten to 2026-07-20 (replace);
  optionally a filter change undone by Back. Expectations come only from §7/§13 golden values and approved copy; selectors use roles and approved copy, no test ids.
- **Phase 4 (last):** runs only after the API and golden-value suite is green, the frontend agent's manual end-to-end check, product acceptance and the reviewer's full pass.
  `frontend` adds `@playwright/test`, `playwright.config.ts`, the `e2e` script and ignores `test-results/` and `playwright-report/`; `test-author` writes the specs; the reviewer reviews them.
- **Red-first deviation:** the app is already green, so each spec proves it can fail by fault injection on a throwaway branch (one targeted mutation per spec, red output recorded,
  mutation discarded). A stopped API is not accepted as proof.
- **Running:** `webServer` starts the API (`dotnet run --project ../src/Relay.Api`, `ConnectionStrings__Relay` from the environment) and `ng serve`, reusing servers already running;
  the DB is started with `docker compose up -d db`, and the API migrates and seeds it on start. No secrets committed. Excluded from `npm test`; runs via `npm run e2e`.
- **Cut line:** this layer is the first cut in §10, ahead of the frontend component tests.
- **README:** `docker compose up -d db && cd web && npx playwright install chromium && npm run e2e` (with `ConnectionStrings__Relay` set).
```

