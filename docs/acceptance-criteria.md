# DASH-247 — Acceptance criteria

Owner: `product`. Written in Phase 0, before any product code.
User: a customer admin of one Relay account, on Monday morning, who has to act on what they see. Every criterion here serves one of two questions:
**Q1** "Is this number normal for us?" or **Q2** "Which of my locations needs attention?"

## How to read this document

**Precedence.** `PLAN.md` §13 (2026-09-28 entry) overrides §5.1 (index bullet), §5.2, §5.3, the §5.4 status labels and footnote, the §7 Evaluator/Ranking/API bullets, the §7 golden table, D5 and D7.
Everything else in PLAN.md still applies, including the §5.4 URL-state rules and the §7 Calendar, Grid and SQL edge cases. The rationale is in `docs/design-consensus.md`.

**Tags on every criterion**

| Tag | Meaning | May a test assert it? |
|---|---|---|
| `GOLDEN` | Value from the PLAN §13 §7 golden table or the hand derivations | Yes. Tests must use these values |
| `SPEC` | Follows directly from a PLAN rule, cited next to it | Yes |
| `SEED` | Value from my query on the seed, using the statistician's R2\* model (Appendix A). It is **not** in PLAN | Product acceptance only. Test authors must not copy it into tests unless the coordinator adds it to PLAN §13 |
| `PROPOSED` | Behaviour or copy that PLAN does not specify | Only after the coordinator or user approves it |

**Defaults.** Default account **14** (Beacon Home Security, America/New_York, 4 sites). Default week **2026-07-20**. Default type `all`. Data as of `2026-07-27T22:20:34Z`.
**API base.** `$API` = `http://localhost:5xxx`, meaning whatever port `dotnet run --project src/Relay.Api` prints.
**Verdicts** in slice reviews: PASS, FAIL (observed vs expected), or NOT TESTABLE YET.

---

## 0. User-facing copy (verbatim; frontend copies these exactly)

### 0.1 Approved copy (PLAN §13 §5.4 = consensus §2)

| Id | Where | Exact string | Source |
|---|---|---|---|
| C-01 | Status `above` | `▲ Higher than usual` | §13 §5.4 |
| C-02 | Status `below` | `▼ Lower than usual` | §13 §5.4 |
| C-03 | Status `normal` | `Within usual range` | §13 §5.4 |
| C-04 | Status `insufficient_data` | `Not enough history yet (N of 4 weeks needed)`. N = `baseline.weeksUsed`; 4 = `minimumEligibleWeeks` from the response | §13 §5.4 |
| C-05 | Location row range | `Usually X–Y a week`. X–Y = API `low`–`high`, with an en dash (–) | §13 §5.4 |
| C-06 | Account summary line | `{count} inbound events · usually X–Y a week` (e.g. `26 inbound events · usually 18–38 a week`) | §13 §5.4 |
| C-07 | Empty account | `No activity recorded for this account yet.` | §13 §5.4 |
| C-08 | Method line, account summary | `compared with the last 8 full weeks for this account` | consensus §9, product sign-off |
| C-09 | Method line, locations (footnote) | `compared with the last 8 full weeks at this location` | §13 §5.4 |
| C-10 | Footnote | `inbound events, not unique customers` | §13 §5.4 |
| C-11 | Footnote | `exact duplicates counted once` | §13 §5.4 |
| C-12 | Footnote | `Locations that usually get 2 or fewer events a week can't show 'lower than usual'` | §13 §5.4 |
| C-13 | Footnote | `Data as of Mon Jul 27, 2026`, with `dataAsOf` rendered in the account's timezone | §13 §5.4 |
| C-14 | Extra footnote line, only when type ≠ `all` | `Per-type counts at a single location are small; only large changes show up.` | §13 §5.4 |
| C-15 | Account switcher label | `Viewing as` | PLAN D5 |

Rules on these strings (§13 §5.4):
- The symbol and the text always appear together. Colour is never the only signal.
- Never shown on screen: deviation, z, σ, `±`, the median, "typical ~N".
- The word `Normal` never appears on its own.
- The UI never recomputes X–Y. It prints the API's `low`/`high`.

### 0.2 Proposed copy (not in PLAN; needs coordinator approval before the frontend uses it)

| Id | Where | Proposed string |
|---|---|---|
| P-01 | Type select label / options | `Activity type`: `All activity` (all), `Calls` (call_received), `Leads` (lead_created), `Appointments` (appointment_set) |
| P-02 | Week label | `Mon Jul 20 – Sun Jul 26, 2026` (from `week.start`/`week.end`) |
| P-03 | Week stepper buttons | `◀ Previous week`, `Next week ▶` (accessible names match the visible text) |
| P-04 | Location table headers | `Location`, `This week`, `Usual range`, `Status` |
| P-05 | Summary line, insufficient | `{count} inbound events`, with no "usually" part, followed by C-04 |
| P-06 | Singular count | `1 inbound event` |
| P-07 | Load error (network or 5xx) | `We couldn't load this week's activity. Try again.` plus a `Try again` button |
| P-08 | Loading | `Loading…` |
| P-09 | Summary heading | `{account name} — all locations` |

---

## 1. Business logic slice (`Relay.Core`, unit tests with no DB and no mocks)

How to verify: run `dotnet test tests/Relay.Core.Tests` and read the test sources. Each criterion below needs a test with these exact inputs and expected values. A matching test name is not enough.

### 1.1 Normality evaluator (§13 §5.3 steps 3–8)

| Id | Given (eligible baseline → count) | Then | Tag |
|---|---|---|---|
| BL-01 | [11,11,11,8] → 11 (account 8, 2026-03-09) | median 11, madT 0, spread 1.0 (floor), low 6, high 18, `normal` | GOLDEN |
| BL-02 | [11,11,11,8] → 6 | `normal` (6 equals `low`) | GOLDEN (§13 §7 edge rows) |
| BL-03 | [11,11,11,8] → 5 | `below` | GOLDEN |
| BL-04 | [2,4,6,20] → 14 | median 5, centre 4.636809 (T of the raw median), spread 1.488613, range 1–14, `normal`, z 1.98 | GOLDEN |
| BL-05 | [2,4,6,20] → 15 / → 0 | `above` / `below` | GOLDEN |
| BL-06 | [0,0,0,0] → 0 / → 3 | range 0–2; `normal` / `above` | GOLDEN |
| BL-07 | [2,2,2,2] → 0 | range 0–6, `normal`. A median of 2 cannot be `below` | GOLDEN |
| BL-08 | [3,3,3,3] → 0 | range 1–7, `below`, z −2.45 | GOLDEN |
| BL-09 | [0,1,5,9] (lowT −1.927794 < 0) | range **0–21**. Without the guard, low would be 1 | GOLDEN (low guard) |
| BL-10 | [1,1,1,1] (lowT 0.345208, between 0 and T(0)) | range 0–4 | GOLDEN (low guard) |
| BL-11 | Any baseline, count = `high` | `normal` (edges are inclusive) | SPEC §13 §5.3.7 |
| BL-12 | 3 eligible weeks (e.g. account 8, 2026-03-02) | `insufficient_data`; weeksUsed 3; median, low, high, deviation all null; count still present | GOLDEN / SPEC §5.3.3 |
| BL-13 | 8 eligible weeks including an 880-sized spike ([53,880,102,59,76,69,79,50] → 87) | median 72.5, range 30–134, `normal`, dev 0.53. The spike does not flag the following weeks | GOLDEN (account 6, 2026-07-20) |
| BL-14 | Any evaluated series | `deviation` has full precision internally. It is rounded to 2 dp, away from zero, only at the API boundary | SPEC §13 §5.3.8 |

### 1.2 Grid, eligibility and site list (§13 §5.3 steps 1–2; §7 Grid cases)

| Id | Given / When | Then | Tag |
|---|---|---|---|
| BL-20 | A site with no events in W that existed before W | Appears with count 0 and is evaluated normally | SPEC §7 Grid |
| BL-21 | A site with no events for the whole baseline | Baseline zero-filled with 0s (not dropped) | SPEC §7 Grid |
| BL-22 | Baseline weeks on or before the week of the series' first event | Not eligible. `weeksUsed` counts only eligible weeks | SPEC §13 §5.3.2 |
| BL-23 | A site whose first event is on or after the end of W | Not in the site list for W | SPEC §13 §5.3.1 |
| BL-24 | Type filter ≠ `all` | Same site list and eligibility as `all`, based on the first event of **any** type. Only counts change | SPEC §13 §5.3.1–2 |
| BL-25 | Account total | First event = MIN over its sites. Median over eligible weeks only (ineligible weeks are dropped, not counted as zeros) | SPEC §13 §5.3.2/4 |

### 1.3 Ranking (§13 §5.3 step 9)

| Id | Given | Then | Tag |
|---|---|---|---|
| BL-30 | Mixed statuses | Order: flagged (`above`/`below`) first, then `normal`, with `insufficient_data` last | SPEC |
| BL-31 | Flagged rows | Sorted by \|deviation\| descending using the unrounded value; above and below are ranked together | SPEC |
| BL-32 | Equal \|deviation\| | `below` before `above`, then location name ascending (ordinal) | SPEC |
| BL-33 | Several `insufficient_data` rows | Sorted among themselves by name (ordinal) | SPEC |

### 1.4 Calendar and service

| Id | Given / When | Then | Tag |
|---|---|---|---|
| BL-40 | Anchor 2026-07-27T22:20:34Z, any of the seed timezones (New_York, Chicago, Denver, Phoenix, Los_Angeles, UTC) | Latest complete week = 2026-07-20 | GOLDEN / §13 consensus §3 |
| BL-41 | §7 Calendar list (DST weeks 2026-03-08 and 2026-11-01 in America/Chicago, Phoenix, UTC, event exactly at the boundary, anchor on Monday 00:00 local vs Sunday 23:59:59) | Each case has a test; a boundary instant belongs to the new week | SPEC §7 Calendar (still in force) |
| BL-42 | Service, no week given | Uses latestCompleteWeek | SPEC §13 §5.2 |
| BL-43 | Service, week not a Monday / after latestCompleteWeek / before earliestWeek | `InvalidWeek` | SPEC §13 §5.2 |
| BL-44 | Service, unknown account | `AccountNotFound` | SPEC §5.2 |
| BL-45 | Service, account with no events | Found. count 0, `insufficient_data`, weeksUsed 0, earliestWeek = latestCompleteWeek, no locations | SPEC §13 §5.2 |

---

## 2. Data slice (`Relay.Infrastructure`, SQL Server via Testcontainers, plus the dev DB)

How to verify: run `dotnet test tests/Relay.Infrastructure.Tests`. After `docker compose up -d db` and a Development API start (which applies migrations), run the `sqlcmd` checks against the dev DB.

| Id | Given / When | Then | Tag |
|---|---|---|---|
| DATA-01 | Migrations applied to an empty DB | `accounts` has 20 rows; `activity_events` has **12,626** raw rows (seed loaded verbatim, duplicates kept) | SPEC §2, §5.1 |
| DATA-02 | Weekly de-duplicated count over all rows | **12,614** distinct events (12 exact-duplicate pairs removed). 4 of the 12 duplicate pairs carry NULLs; a `=`-based de-dup would wrongly keep 12,618 | SPEC §2; §13 §5.1; consensus §4.2; Appendix A.3 |
| DATA-03 | Account 1, Site C, local week 2026-07-06 (America/Chicago), all | Count **4** (raw rows 5; ids 11266/11267 are one event) | GOLDEN |
| DATA-04 | Two events 1–60 s apart that differ in any column | Both counted (near-duplicates are not merged) | SPEC §2, §7 SQL |
| DATA-05 | An event exactly at a window's UTC start | Counted in that window, not in the previous one | SPEC §7 SQL |
| DATA-06 | `type = call_received` | Only that type is counted. `all` is sent to SQL as null (no type predicate) | SPEC §7 SQL; consensus §4.5 |
| DATA-07 | Rows of other accounts, or outside every window | Ignored | SPEC §7 SQL |
| DATA-08 | Account 20 (no rows) | Empty site list, empty counts, no exception | SPEC §7 SQL |
| DATA-09 | Anchor query | Returns `2026-07-27T22:20:34Z` with `DateTimeKind.Utc` | SPEC §2; §13 §5.1 |
| DATA-10 | Sites query for account 14 | Unbounded (whole account). Sites A, B, C, D with first events in local weeks 2026-02-02, 2026-01-26, 2026-02-02, 2026-01-26 | SPEC consensus §3; SEED Appendix A.1 |
| DATA-11 | Schema | Index `IX_activity_events_account_occurred` on `(account_id, occurred_at)` INCLUDE `(location, event_type, duration_seconds, outcome)`; no unique constraint | SPEC §13 §5.1 |

---

## 3. API slice (`Relay.Api`, integration + golden against the real seed)

How to verify: run `dotnet test tests/Relay.Api.Tests`, then `curl` the running API. Each check below is a literal request.

### 3.1 Accounts

| Id | When | Then | Tag |
|---|---|---|---|
| API-01 | `GET $API/api/accounts` | 200, 20 items `{ id, name, timezone }`, including `{ "id": 14, "name": "Beacon Home Security", "timezone": "America/New_York" }` and `{ "id": 20, "name": "Quiet Harbor Spa", "timezone": "America/Los_Angeles" }` | SPEC §13 §5.2; Appendix A.2 |

### 3.2 Activity health: happy paths (Q1 + Q2)

| Id | Given / When | Then | Tag |
|---|---|---|---|
| API-10 | Default: `GET $API/api/accounts/14/activity-health` | 200. `week.start` `2026-07-20`, `week.end` `2026-07-26`, `eventType` `all`, `dataAsOf` `2026-07-27T22:20:34Z`, `latestCompleteWeek` `2026-07-20`, `earliestWeek` `2026-01-26`, `baselineWeeks` 8, `minimumEligibleWeeks` 4. Summary: count 26, median 27, low 18, high 38, `normal`. `locations[0]` = Site B, count 2, low 3, high 12, `below`, deviation −2.16. Sites C, A, D all `normal` | GOLDEN |
| API-11 | Same as API-10 | Location order is exactly Site B, Site C, Site A, Site D | SEED (A.4); consistent with GOLDEN |
| API-12 | Spike week: `…/accounts/6/activity-health?week=2026-06-01` | Summary 880, median 66, low 39, high 101, `above`, dev 22.37. All 15 locations `above`. `locations[0]` = Site C, 67, median 3, 1–7, dev 12.74 | GOLDEN |
| API-13 | Week after the spike: `…/accounts/6/activity-health?week=2026-06-08` | Summary 102, 37–104, `normal`. `locations[0]` Site C 11 vs 1–8 `above`, `locations[1]` Site J 11 vs 2–10 `above`, and the other 13 `normal`. The spike stays in the baseline but does not hide real rises | SEED (A.5) |
| API-14 | Spike still in the baseline: `…/accounts/6/activity-health?week=2026-07-20` | Summary 87, median 72.5 (unrounded), 30–134, `normal`, dev 0.53. All 15 locations `normal`. Site M: 7, median 3.5, 1–9, dev 1.30, and it is `locations[0]` | GOLDEN (+ order: SEED A.6) |
| API-15 | Type filter: `…/accounts/6/activity-health?week=2026-07-20&type=call_received` | `eventType` `call_received`. Summary 51, median 42, 17–79, `normal`, dev 0.54. Still 15 locations | GOLDEN; SPEC §13 §5.3.1 |
| API-16 | Single-site account: `…/accounts/8/activity-health` | Exactly 1 location (Site A). Summary and location both show 7, median 10, 5–17, `normal`. `earliestWeek` `2026-02-02` | SEED (A.7); earliestWeek GOLDEN |
| API-17 | Single site, floor case: `…/accounts/8/activity-health?week=2026-03-09` | Summary 11, median 11, 6–18, `normal` | GOLDEN |
| API-18 | Insufficient history: `…/accounts/8/activity-health?week=2026-03-02` | Summary `insufficient_data`, `baseline` = `{ "weeksUsed": 3, "median": null, "low": null, "high": null }`, `deviation` null, count 8 present | GOLDEN (count 8 from `golden_out.md`) |
| API-19 | Early February, no eligible weeks: `…/accounts/14/activity-health?week=2026-02-02` | 200. Summary count 27, `insufficient_data`, weeksUsed 0. All 4 locations `insufficient_data`, ordered by name A, B, C, D | SPEC; values SEED (A.8) |
| API-20 | Mixed eligibility: `…/accounts/14/activity-health?week=2026-03-02` | Summary 40 vs 16–36 `above`. Order: Site D (16 vs 2–11 `above`), Site B (9 vs 2–10 `normal`), then Site A and Site C, both `insufficient_data` with weeksUsed 3 | SEED (A.8) |
| API-21 | Earliest week, sites not yet active: `…/accounts/14/activity-health?week=2026-01-26` | 200. Only Site B and Site D are listed (A and C first appear in the week of 2026-02-02). All rows `insufficient_data`, weeksUsed 0 | SPEC §13 §5.3.1; values SEED (A.8) |
| API-22 | Location with zero activity (flagged): `…/accounts/6/activity-health?week=2026-06-29` | Site G present with count 0, 2–9, `below`, and it is `locations[0]` | SEED (A.9) |
| API-23 | Zero activity under a type filter: `…/accounts/14/activity-health?type=appointment_set` | All 4 sites listed. Site A 0 vs 0–4 and Site B 0 vs 0–2 are both `normal` (median ≤ 2, so they cannot be `below`) | SEED (A.4); SPEC §13 §5.3 known limit |
| API-24 | Exact-tie ordering: same request as API-23 | Sites B, C, D all have deviation exactly 0, so they are ordered by name: `locations` = A, B, C, D | SEED (A.4); SPEC §13 §5.3.9 |
| API-25 | Partial eligibility on a total: `…/accounts/18/activity-health?week=2026-03-23` | Summary `baseline.weeksUsed` 7, 18 vs 15–33 `normal`. Site C 0 vs 1–9 `below`, and it is `locations[0]` | SEED (A.9) |
| API-26 | Every account, default week | `week.start` 2026-07-20. `earliestWeek` is 2026-01-26 for accounts 1, 4, 5, 6, 7, 12, 14, 18; 2026-02-02 for 2, 3, 8–11, 13, 15–17, 19; 2026-07-20 for 20 | GOLDEN |
| API-27 | Account 12, default week | Summary 54, 40–74, `normal`. Site F 11 vs 2–11 `normal` (dev 1.90), `locations[0]`. No location is flagged | GOLDEN |

### 3.3 Empty account

| Id | When | Then | Tag |
|---|---|---|---|
| API-30 | `GET …/accounts/20/activity-health` | 200. `summary.count` 0, `insufficient_data`, `baseline.weeksUsed` 0 with null median/low/high, `deviation` null, `locations` `[]`, `earliestWeek` = `latestCompleteWeek` = `2026-07-20` | GOLDEN |
| API-31 | `GET …/accounts/20/activity-health?week=2026-03-02` | 400 ProblemDetails (before earliestWeek) | GOLDEN |

### 3.4 Invalid input (all errors are `ProblemDetails`, `Content-Type: application/problem+json`)

| Id | Request | Expected | Tag |
|---|---|---|---|
| API-40 | `…/accounts/999/activity-health` | 404 | SPEC §13 §5.2 |
| API-41 | `…/accounts/14/activity-health?week=2026-07-21` (a Tuesday) | 400 | SPEC |
| API-42 | `…/accounts/14/activity-health?week=2026-07-27` (a Monday, but the current partial week) | 400 | SPEC |
| API-43 | `…/accounts/14/activity-health?week=2026-01-19` (before earliestWeek) | 400 | SPEC / GOLDEN |
| API-44 | `…/accounts/8/activity-health?week=2026-01-26` (valid for 14, before 8's earliestWeek) | 400; the same week is 200 for account 14 (API-21) | SPEC |
| API-45 | `type=ALL`, `type=Call_Received`, `type=calls` | 400 each (case-sensitive) | GOLDEN |
| API-46 | `week=2026-13-01`, `week=20260720`, `week=abc` | 400 each | PROPOSED (PLAN says "YYYY-MM-DD" but not what a malformed value returns; 400 is the natural reading) |

---

## 4. Dashboard slice (`web/`, Angular)

How to verify: run `cd web && npm test` (Vitest output), then open the app with the API running. Each "open" below is a literal URL.
Copy ids (C-xx, P-xx) refer to §0.

### 4.1 Default view: Q1 and Q2 answered at a glance

| Id | Given / When | Then | Tag |
|---|---|---|---|
| UI-01 | Open `/dashboard` with no params | URL becomes `/dashboard?account=14&week=2026-07-20&type=all`. `Viewing as` shows Beacon Home Security | SPEC §5.4 URL rules; §13 D5 |
| UI-02 | Same | Summary reads `26 inbound events · usually 18–38 a week` and `Within usual range` | GOLDEN; C-03, C-06 |
| UI-03 | Same | The first table row is Site B: `2`, `Usually 3–12 a week`, `▼ Lower than usual`. The rows below it (C, A, D) each show `Within usual range` | GOLDEN; order SEED |
| UI-04 | Same | Nowhere on the page: a median, a deviation, "z", "σ", "±", "typical", or the word `Normal` on its own | SPEC §13 §5.4 |
| UI-05 | Same | Status is readable with colours removed (symbol + text), e.g. by checking the DOM text or a greyscale screenshot | SPEC §13 §5.4 |
| UI-06 | Same | Footnote contains C-09, C-10, C-11, C-12 and `Data as of Mon Jul 27, 2026`. The summary carries C-08. C-14 is absent | SPEC §13 §5.4 |
| UI-07 | Same | `Next week ▶` is disabled (2026-07-20 = latestCompleteWeek); `◀ Previous week` is enabled | SPEC §5.4 stepper bounds |

### 4.2 Scenarios

| Id | Open | Then | Tag |
|---|---|---|---|
| UI-10 | Spike week `?account=6&week=2026-06-01&type=all` | Summary `880 inbound events · usually 39–101 a week`, `▲ Higher than usual`. All 15 rows `▲ Higher than usual`. First row Site C, `67`, `Usually 1–7 a week` | GOLDEN |
| UI-11 | Week after the spike `?account=6&week=2026-06-08&type=all` | Summary `Within usual range` (102, usually 37–104). Site C then Site J at the top, both `▲ Higher than usual` | SEED |
| UI-12 | Spike in the baseline `?account=6&week=2026-07-20&type=all` | Summary `87 inbound events · usually 30–134 a week`, `Within usual range`. All 15 rows `Within usual range` | GOLDEN |
| UI-13 | Type filter: from UI-12, choose Calls | URL `type=call_received`. Summary `51 inbound events · usually 17–79 a week`. Footnote now also shows C-14 | GOLDEN; SPEC §13 §5.4 |
| UI-14 | Single-site `?account=8` | One row, Site A: `7`, `Usually 5–17 a week`, `Within usual range`. The summary shows the same figures | SEED |
| UI-15 | Insufficient history `?account=8&week=2026-03-02&type=all` | Summary shows `8 inbound events` (no "usually") and `Not enough history yet (3 of 4 weeks needed)` | GOLDEN; C-04; P-05 |
| UI-16 | Earliest weeks `?account=14&week=2026-02-02&type=all` | Every row shows its count and `Not enough history yet (0 of 4 weeks needed)`, with no range | SPEC; SEED |
| UI-17 | Mixed `?account=14&week=2026-03-02&type=all` | Site D `▲ Higher than usual` first. Sites A and C last, each with `Not enough history yet (3 of 4 weeks needed)` | SEED |
| UI-18 | Zero-activity location `?account=6&week=2026-06-29&type=all` | First row Site G, `0`, `Usually 2–9 a week`, `▼ Lower than usual` | SEED |
| UI-19 | Zero, small median `?account=14&week=2026-07-20&type=appointment_set` | Sites A and B show `0` and `Within usual range`; Site B reads `Usually 0–2 a week`. Footnote C-12 explains why neither can be lower than usual | SEED; SPEC known limit |
| UI-20 | Empty account `?account=20` | Shows `No activity recorded for this account yet.` Both week buttons disabled (earliestWeek = latestCompleteWeek). No table rows, no error | GOLDEN; C-07 |
| UI-21 | Stepper at the lower bound `?account=14&week=2026-01-26&type=all` | `◀ Previous week` disabled. Only Site B and Site D listed | SPEC; SEED |

### 4.3 URL state, reload and invalid params (§5.4, still in force)

| Id | Given / When | Then | Tag |
|---|---|---|---|
| UI-30 | Set account 6, week 2026-06-01, type Calls using the controls, then reload | Same account, week and type selected; same numbers shown; URL `?account=6&week=2026-06-01&type=call_received` | SPEC §5.4 |
| UI-31 | Click `◀ Previous week` from the default | URL `week=2026-07-13`; `Next week ▶` enabled. Reload keeps 2026-07-13 | SPEC §5.4 |
| UI-32 | Open `?account=999` | Rewritten to `account=14`; default view | SPEC §5.4 ("invalid → defaults") |
| UI-33 | Open `?account=14&week=2026-07-21` | Rewritten to `week=2026-07-20` | SPEC §5.4 |
| UI-34 | Open `?account=14&week=2026-07-27` (current partial week) | Rewritten to `week=2026-07-20` | SPEC §5.4 + §13 §5.2 |
| UI-35 | Open `?account=14&week=2025-12-29` (before earliestWeek) | Rewritten to `week=2026-07-20` | SPEC §5.4 + §13 §5.2 |
| UI-36 | Open `?account=20&week=2026-03-02` | Rewritten to `week=2026-07-20`; empty state shown, not an error | GOLDEN (API 400) + SPEC §5.4 |
| UI-37 | Open `?type=ALL` or `?type=foo` | Rewritten to `type=all` | SPEC §5.4 + §13 §5.2 |
| UI-38 | Any rewrite in UI-32…37 | Uses replace, not a new history entry, so Back does not return to the invalid URL | PROPOSED |
| UI-39 | Switch `Viewing as` from 14 (at week 2026-01-26) to 8 | Week falls back to 2026-07-20 because 2026-01-26 is before account 8's earliestWeek. Type is kept | PROPOSED (PLAN does not say what happens to week/type on account switch) |
| UI-40 | API unreachable or 5xx | P-07 shown with a retry; filters stay in the URL | PROPOSED |

---

## 5. Coverage of the required scenarios

| Scenario | Criteria |
|---|---|
| Default week | BL-40, BL-42, API-10, API-26, UI-01…07 |
| Empty account (20) | BL-45, DATA-08, API-30, API-31, UI-20, UI-36 |
| Single-site account | API-16, API-17, UI-14 |
| Spike week | API-12, UI-10 |
| Week after the spike (baseline contains it) | BL-13, API-13, API-14, UI-11, UI-12 |
| Insufficient history (early February) | BL-12, API-18…21, UI-15…17, UI-21 |
| Type filter | BL-24, DATA-06, API-15, API-23, UI-13, UI-19 |
| Reload preserves every filter | UI-30, UI-31 |
| Invalid URL params | UI-32…UI-37 (API side: API-40…46) |
| Location with zero activity in the week | BL-20, API-22, API-23, API-25, UI-18, UI-19 |
| Duplicates | DATA-02, DATA-03 |
| Ranking and ties | BL-30…33, API-11, API-24 |

---

## Appendix A — Seed evidence behind `SEED` values

These come from the statistician's R2\* model (`analysis/statistician/golden.py`, which executes `analysis/reference_model.py` on `schema.sql` + `seed.sql` in SQLite), queried by product on 2026-09-28 with a scratch script. Nothing in the repo was changed.
Command: `python3 <scratch>/pq.py analysis/statistician` and `pq2.py` (both call `show(account, week, type)` / `evaluate(...)` from `golden.py`).

**A.1 Site first-activity weeks** (account 14): `{'Site A': '2026-02-02', 'Site B': '2026-01-26', 'Site C': '2026-02-02', 'Site D': '2026-01-26'}`; account 8: `{'Site A': '2026-02-02'}`.

**A.2 Accounts**: `(14, 'Beacon Home Security', 'America/New_York')`, `(20, 'Quiet Harbor Spa', 'America/Los_Angeles')`, 20 rows.

**A.3 De-duplication** (SQLite over the seed):
```
raw 12626 · distinct over non-id columns 12614 · duplicate groups 12, of which with a NULL column 4
account 1 Site C, 2026-07-06 week: ids 11266 and 11267 identical (call_received, 2026-07-07 20:26:04, 497, voicemail)
```

**A.4 Account 14, 2026-07-20**
```
all:              TOTAL 26 | 27.0 | 18–38 | normal
                  1. Site B 2 | 6.5 | 3–12 | below -2.16   2. Site C 9 | 2–12 | normal 0.98
                  3. Site A 9 | 2–16 | normal 0.61     4. Site D 6 | 3–12 | normal -0.19
call_received:    TOTAL 16 | 15.5 | 9–24 | normal
appointment_set:  TOTAL 2 | 3.5 | 1–8 | normal
                  1. Site A 0 | 1.0 | 0–4 | normal -1.12   2. Site B 0 | 0.0 | 0–2 | normal 0.0
                  3. Site C 1 | 1.0 | 0–6 | normal 0.0     4. Site D 1 | 1.0 | 0–4 | normal 0.0
```
**A.5 Account 6, 2026-06-08** (baseline ends with the 880 week)
```
TOTAL 102 | 66.0 | 37–104 | normal 1.88
1. Site C 11 | 3.5 | 1–8 | above 2.81   2. Site J 11 | 5.0 | 2–10 | above 2.11   3.–15. normal
```
**A.6 Account 6, 2026-07-20**: order Site M, O, I, A, E, J, G, N, L, B, H, F, K, D, C (all normal).

**A.7 Account 8, 2026-07-20**: `TOTAL 7 | 10.0 | 5–17 | normal -1.01`; `Site A` identical.

**A.8 Account 14, early weeks**
```
2026-01-26: TOTAL 2 insufficient; Site B 1, Site D 1 (insufficient)
2026-02-02: TOTAL 27 insufficient; Site A 9, B 5, C 7, D 6 (all insufficient)
2026-03-02: TOTAL 40 | 25.0 | 16–36 | above 2.63
            1. Site D 16 | 5.5 | 2–11 | above 3.25   2. Site B 9 | 5.0 | 2–10 | normal 1.49
            3. Site A 7 insufficient   4. Site C 8 insufficient
```
**A.9 Zero-activity locations** (type all, whole seed): account 6 Site G 2026-04-13 (below, 2–12), account 6 Site G 2026-06-29 (below, 2–9, ranked first), account 6 Site N 2026-02-23 (insufficient), account 18 Site C 2026-03-23 (below, 1–9; the account 18 total uses 7 eligible weeks: `[26, 24, 29, 23, 17, 19, 22]`).

**A.10 `dataAsOf` in each seed timezone**: 2026-07-27T22:20:34Z is Mon Jul 27 in America/New_York (18:20), Chicago (17:20), Denver and Phoenix (16:20 / 15:20), Los_Angeles (15:20) and UTC (22:20). So C-13 reads `Data as of Mon Jul 27, 2026` for every account.
