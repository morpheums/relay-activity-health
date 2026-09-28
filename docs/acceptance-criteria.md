# DASH-247 — Acceptance criteria

Owner: `product`. Written in Phase 0, before any product code.
User: a customer admin of one Relay account, on Monday morning, who has to act on what they see. Every criterion here serves one of two questions:
**Q1** "Is this number normal for us?" or **Q2** "Which of my locations needs attention?"

## How to read this document

**Precedence.** `PLAN.md` §13 has five entries dated 2026-09-28. Four of them apply here:
- **"Revised design from the four-agent debate (approved by the user)"** overrides §5.1 (index bullet), §5.2, §5.3, the §5.4 status labels and footnote, the §7 Evaluator/Ranking/API bullets, the §7 golden table, D5 and D7.
- **"Phase 0 decisions and promoted golden values (user decisions)"** adds ten golden scenarios and fixes the API port at 5080.
- **"Input handling and UI copy decisions (user decisions, validated by the architect)"** decides malformed input, URL normalisation, account switching, the empty and error states, and the additional copy. It is cited below as §13 "Input handling and UI copy".
- **"Contract decisions from the Phase 0 contract review (user decisions, validated by the architect)"** covers the empty database (`dataAsOf` null), the exact `dataAsOf` string, more malformed-week variants, the capitalised account method line, and more promoted goldens. It is cited below as §13 "Contract decisions".

Everything else in PLAN.md still applies, including the §5.4 URL-state rules and the §7 Calendar, Grid and SQL edge cases. The rationale is in `docs/design-consensus.md`.

**Tags on every criterion**

| Tag | Meaning | May a test assert it? |
|---|---|---|
| `GOLDEN` | A value from the "Revised design" §7 golden table or its hand derivations | Yes. Tests must use these values |
| `GOLDEN-P` | A value from a promoted-goldens table in §13: the "Phase 0 decisions" entry or the "Contract decisions" entry | Yes |
| `SPEC` | Follows directly from a PLAN rule, cited next to it | Yes |
| `SEED` | A value from my query on the seed, using the statistician's R2\* model (Appendix A). It is **not** in PLAN | No. Appendix and manual acceptance only; no automated criterion currently uses it |
| `PROPOSED` | Behaviour or copy that PLAN does not specify | Only after the coordinator or user approves it |

**Defaults.** Default account **14** (Beacon Home Security, America/New_York, 4 sites). Default week **2026-07-20**. Default type `all`. Data as of `2026-07-27T22:20:34Z`.
**API base.** `$API` = `http://localhost:5080`, a fixed port (PLAN §13, Phase 0 decisions). The Angular dev proxy targets it.
**Verdicts** in slice reviews: PASS, FAIL (observed vs expected), or NOT TESTABLE YET.

---

## 0. User-facing copy (verbatim; frontend copies these exactly)

### 0.1 Approved copy

Sources: PLAN §13 §5.4, D5, and §13 "Input handling and UI copy".

| Id | Where | Exact string | Source |
|---|---|---|---|
| C-01 | Status `above` | `▲ Higher than usual` | §13 §5.4 |
| C-02 | Status `below` | `▼ Lower than usual` | §13 §5.4 |
| C-03 | Status `normal` | `Within usual range` | §13 §5.4 |
| C-04 | Status `insufficient_data` | `Not enough history yet (N of 4 weeks needed)`. N = `baseline.weeksUsed`; 4 = `minimumEligibleWeeks` from the response. N = 0 renders `(0 of 4 weeks needed)` | §13 §5.4; SPEC §13 "Input handling and UI copy" (0 case kept) |
| C-05 | Location row range | `Usually X–Y a week`. X–Y = API `low`–`high`, with an en dash (–) | §13 §5.4 |
| C-06 | Account summary line, sufficient history | `{count} {noun} · usually X–Y a week`, e.g. `26 inbound events · usually 18–38 a week`, `51 calls · usually 17–79 a week` | §13 §5.4; SPEC §13 "Input handling and UI copy" (noun) |
| C-06a | `{noun}` by type, plural / singular (count = 1) | all: `inbound events` / `inbound event` · call_received: `calls` / `call` · lead_created: `leads` / `lead` · appointment_set: `appointments` / `appointment` | SPEC §13 "Input handling and UI copy" |
| C-06b | Account summary line, `insufficient_data` | `{count} {noun}`, with no "usually" part, followed by C-04 (e.g. `8 inbound events` + `Not enough history yet (3 of 4 weeks needed)`) | SPEC §13 "Input handling and UI copy" |
| C-07 | Empty account | `No activity recorded for this account yet.` It replaces both the summary and the table. Filters stay visible, and both week stepper buttons are disabled | §13 §5.4; SPEC §13 "Input handling and UI copy" (placement) |
| C-08 | Method line, account summary | `Compared with the last 8 full weeks for this account` | consensus §9; SPEC §13 "Input handling and UI copy" (wording); SPEC §13 "Contract decisions" (capital first letter) |
| C-09 | Method line, locations (footnote) | `Compared with the last 8 full weeks at this location` | §13 §5.4 |
| C-10 | Footnote | `Inbound events, not unique customers` | §13 §5.4 |
| C-11 | Footnote | `Exact duplicates counted once` | §13 §5.4 |
| C-12 | Footnote | `Locations that usually get 2 or fewer events a week can't show 'lower than usual'` | §13 §5.4 |
| C-13 | Footnote | `Data as of Mon Jul 27, 2026`, with `dataAsOf` rendered in the account's timezone | §13 §5.4 |
| C-14 | Extra footnote line, only when type ≠ `all` | `Per-type counts at a single location are small; only large changes show up.` | §13 §5.4 |
| C-15 | Account switcher label | `Viewing as` | PLAN D5 |
| C-16 | Type select label and options | `Activity type`: `All activity` (all), `Calls` (call_received), `Leads` (lead_created), `Appointments` (appointment_set) | SPEC §13 "Input handling and UI copy" |
| C-17 | Week label | `Mon Jul 20 – Sun Jul 26, 2026` (from `week.start`/`week.end`) | SPEC §13 "Input handling and UI copy" |
| C-18 | Week stepper buttons | `◀ Previous week`, `Next week ▶` (accessible names match the visible text) | SPEC §13 "Input handling and UI copy" |
| C-19 | Location table headers | `Location`, `Events`, `Usual range`, `Status` | SPEC §13 "Input handling and UI copy" |
| C-20 | Load error (network or 5xx) | `We couldn't load this week's activity. Try again.` plus a `Try again` button that calls `DashboardState.reload()` | SPEC §13 "Input handling and UI copy" |
| C-21 | Loading | `Loading…` | SPEC §13 "Input handling and UI copy" |
| C-22 | Summary heading | `{account name} — all locations` | SPEC §13 "Input handling and UI copy" |

Rules on these strings (§13 §5.4 and §13 "Input handling and UI copy"):
- The symbol and the text always appear together. Colour is never the only signal.
- Never shown on screen: deviation, z, σ, `±`, the median, "typical ~N".
- The word `Normal` never appears on its own.
- The UI never recomputes X–Y. It prints the API's `low`/`high`.
- Footnote and method lines are rendered with the first letter capitalised. The wording is otherwise exactly PLAN §13 §5.4 (§13 "Input handling and UI copy"). The strings above are already capitalised.

### 0.2 Proposed copy

None open. P-01…P-11 were decided in §13 "Input handling and UI copy" and are now C-06a/b, C-07 (placement), C-08 and C-16…C-22.

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
| BL-11 | [11,11,11,8] → 18 / → 19 | 18 = `high`, so `normal` / 19 is `above` (edges are inclusive) | SPEC §13 §5.3.7, range from GOLDEN BL-01 |
| BL-12 | 3 eligible weeks (e.g. account 8, 2026-03-02) | `insufficient_data`; weeksUsed 3; median, low, high, deviation all null; count still present | GOLDEN / SPEC §5.3.3 |
| BL-13 | Account 6, week 2026-07-20: 8 eligible weeks including the 880 spike ([53,880,102,59,76,69,79,50] → 87) | Total: median 72.5, range 30–134, `normal`, dev 0.53. All 15 sites `normal`. Scope is this week only. Flags in the week right after the spike are expected and correct (API-13, 2026-06-08: Sites C and J `above`) | GOLDEN (account 6, 2026-07-20) |

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
| BL-31b | `normal` rows | Sorted by \|deviation\| descending after all flagged rows. Account 14, 2026-07-20: Site B (`below`) first, then the normal rows C (0.98), A (0.61), D (−0.19, \|0.19\|) | SPEC §13 §5.3.9; GOLDEN-P (PLAN §13 "Phase 0 decisions": order B, C, A, D) |
| BL-32 | Equal \|deviation\| | `below` before `above`, then location name ascending (ordinal) | SPEC |
| BL-33 | Several `insufficient_data` rows | Sorted among themselves by name (ordinal) | SPEC |

### 1.4 Calendar and service

| Id | Given / When | Then | Tag |
|---|---|---|---|
| BL-40 | Anchor 2026-07-27T22:20:34Z, any of the seed timezones (New_York, Chicago, Denver, Phoenix, Los_Angeles, UTC) | Latest complete week = 2026-07-20 | GOLDEN |
| BL-41a | America/Chicago, week 2026-03-02 (contains DST start, Sun 2026-03-08) | UTC window [2026-03-02T06:00Z, 2026-03-09T05:00Z), **167 h** | SPEC §7 Calendar |
| BL-41b | America/Chicago, week 2026-10-26 (contains DST end, Sun 2026-11-01) | UTC window [2026-10-26T05:00Z, 2026-11-02T06:00Z), **169 h** | SPEC §7 Calendar |
| BL-41c | America/Phoenix, week 2026-03-02 | [2026-03-02T07:00Z, 2026-03-09T07:00Z), 168 h (no DST) | SPEC §7 Calendar |
| BL-41d | UTC, week 2026-03-02 | [2026-03-02T00:00Z, 2026-03-09T00:00Z), 168 h | SPEC §7 Calendar |
| BL-41e | America/Chicago, instant 2026-03-09T05:00:00Z (local Mon 00:00) | Belongs to week 2026-03-09, not 2026-03-02 | SPEC §7 Calendar |
| BL-41f | Latest complete week, America/New_York: anchor Mon 2026-07-27 00:00 local (04:00Z) / Sun 2026-07-26 23:59:59 local (2026-07-27T03:59:59Z) / Mon 2026-07-27 18:20:34 local (the seed anchor) | 2026-07-20 / 2026-07-13 / 2026-07-20 | SPEC §7 Calendar; consensus §3 ("week containing the anchor, minus 7 days") |
| BL-41g | Invalid IANA id (e.g. `Mars/Olympus`) | Fails with an error; no silent fallback to UTC | SPEC §7 Calendar |
| BL-41h | Is-week-start for 2026-07-21 (Tue) / 2026-07-20 (Mon) | false / true | SPEC §7 Calendar |
| BL-42 | Service, no week given | Uses latestCompleteWeek | SPEC §13 §5.2 |
| BL-43 | Service, week not a Monday / after latestCompleteWeek / before earliestWeek | `InvalidWeek` | SPEC §13 §5.2 |
| BL-44 | Service, unknown account | `AccountNotFound` | SPEC §5.2 |
| BL-45 | Service, account with no events | Found. count 0, `insufficient_data`, weeksUsed 0, earliestWeek = latestCompleteWeek, no locations | SPEC §13 §5.2 |

---

## 2. Data slice

### 2.1 SQL queries (`Relay.Infrastructure.Tests`: SQL Server via Testcontainers, `InitialCreate` only, hand-built fixtures)

Per PLAN §5.1 these tests never load the seed. Every row below is a fixture the test inserts itself.
How to verify: run `dotnet test tests/Relay.Infrastructure.Tests` and read each fixture.

| Id | Given (fixture) / When | Then | Tag |
|---|---|---|---|
| DATA-20 | Two rows identical in every non-id column, both with `duration_seconds` NULL | Counted once | SPEC §13 §5.1 (no `=` on nullable columns); consensus §4.2 |
| DATA-21 | Two identical rows, both with `outcome` NULL | Counted once | SPEC §13 §5.1 |
| DATA-22 | Two identical rows, both with `duration_seconds` **and** `outcome` NULL | Counted once | SPEC §13 §5.1 |
| DATA-23 | Two rows with the same instant and fields that differ only in `location` | Not merged: one count at each location | SPEC §2 (exact duplicates only) |
| DATA-24 | Two rows that differ only in `event_type` | Not merged: counted once under each type, and twice under `all` | SPEC §2 |
| DATA-25 | Two rows 1–60 s apart, otherwise equal | Both counted (near-duplicates are not merged) | SPEC §2, §7 SQL |
| DATA-26 | An event exactly at a window's UTC start | Counted in that window | SPEC §7 SQL |
| DATA-27 | An event exactly at a window's UTC end (e.g. Chicago 2026-03-09T05:00:00Z, the end of week 2026-03-02) | Not counted in that window; counted in the next window (2026-03-09) when that window is requested | SPEC §7 SQL (windows are `[start, end)`) |
| DATA-28 | A fixture with one `call_received`, one `lead_created` and one `appointment_set` in a window | Type all → 3. `call_received` → 1. `lead_created` → 1. `appointment_set` → 1 | SPEC §7 SQL |
| DATA-30 | Rows for another account, and rows outside every requested window | Ignored | SPEC §7 SQL |
| DATA-31 | Account with no rows | Empty site list, empty counts, no exception | SPEC §7 SQL |
| DATA-32 | Anchor query on a fixture whose latest row is 2026-07-27T22:20:34 | Returns that instant with `DateTimeKind.Utc` | SPEC §13 §5.1 |
| DATA-33 | Sites query, fixture with a site whose first event is later than any window passed elsewhere | The site is still returned with its first-event instant; the query is unbounded, and filtering by W happens in Core | SPEC consensus §3 |
| DATA-34 | Schema after `InitialCreate` | Index `IX_activity_events_account_occurred` on `(account_id, occurred_at)` INCLUDE `(location, event_type, duration_seconds, outcome)`; no unique constraint | SPEC §13 §5.1 |

### 2.2 Seed load and starter files (`Relay.Api.Tests` against the real seed, plus the repo)

| Id | Given / When | Then | Tag |
|---|---|---|---|
| DATA-01 | All migrations applied (`InitialCreate` + `LoadSeedData`) | `accounts` has 20 rows; `activity_events` has **12,626** raw rows (duplicates kept) | SPEC §2, §5.1 |
| DATA-02 | De-duplicated weekly counts for every account (1–20), with windows covering every local week from 2026-01-26 through 2026-07-27 inclusive (the partial week too) | The counts sum to **12,614** in total, so nothing is lost or double-counted at the week edges. Per account: 1: 1,221 · 2: 729 · 3: 477 · 4: 796 · 5: 884 · 6: 2,637 · 7: 437 · 8: 260 · 9: 546 · 10: 342 · 11: 354 · 12: 1,303 · 13: 205 · 14: 638 · 15: 499 · 16: 167 · 17: 323 · 18: 586 · 19: 210 · 20: 0 | GOLDEN-P (PLAN §13 "Contract decisions" promoted table); total also SPEC §2 / consensus §4.2 |
| DATA-03 | Account 1, Site C, local week 2026-07-06 (America/Chicago), all | Count **4** (raw rows 5; ids 11266/11267 are one event) | GOLDEN |
| DATA-10 | Account 14 sites | Sites B and D first active in local week 2026-01-26; Sites A and C in 2026-02-02 | GOLDEN-P (PLAN §13 "Contract decisions" promoted table) |
| DATA-40 | `db/schema.sql`, `db/seed.sql` after the Phase 0 `git mv` | Content unchanged. SHA-256 must equal the originals committed at the repo root in `4898e69`: `schema.sql` `348912f4fd6dade1728058a4f666780c60b94578f4276135583f502616e51d3d`, `seed.sql` `40e60ee81d999eb32057b4437bc84e9ec197265d4e58c13c0bfbad150e6eaea2`. `git log --follow` shows a rename | SPEC §13 (starter files), CLAUDE.md boundaries |

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
| API-01b | `GET …/accounts/14/activity-health` (JSON names) | Top level exactly: `account{id,name,timezone}`, `eventType`, `week{start,end}`, `dataAsOf` (a string, or null only for an empty database, API-32), `latestCompleteWeek`, `earliestWeek`, `baselineWeeks`, `minimumEligibleWeeks`, `summary`, `locations`. Summary and each location: `count`, `baseline{weeksUsed,median,low,high}`, `status`, `deviation`; locations also `location`. camelCase as in the PLAN §13 §5.2 example | SPEC §13 §5.2 |
| API-01c | Every response in §3 | `status` is exactly one of `above`, `below`, `normal`, `insufficient_data` (lower case, underscore) | SPEC §13 §5.2 |
| API-01d | `…/accounts/14/activity-health?week=2026-07-20` with **no** `type` | Same body as `type=all`; `eventType` `all` | SPEC §13 §5.2 (default `all`) |
| API-10 | Default: `GET $API/api/accounts/14/activity-health` | 200. `week.start` `2026-07-20`, `week.end` `2026-07-26`, `eventType` `all`, `dataAsOf` exactly the string `"2026-07-27T22:20:34Z"` (a `Z` suffix, not `+00:00`, and no fractional seconds), `latestCompleteWeek` `2026-07-20`, `earliestWeek` `2026-01-26`, `baselineWeeks` 8, `minimumEligibleWeeks` 4. Summary: count 26, median 27, low 18, high 38, `normal`. `locations[0]` = Site B, count 2, low 3, high 12, `below`, deviation −2.16. Sites C, A, D all `normal` | GOLDEN; SPEC §13 "Contract decisions" (exact `dataAsOf` string) |
| API-11 | Same as API-10 | Location order is exactly Site B, Site C, Site A, Site D | GOLDEN-P (PLAN §13 "Phase 0 decisions" promoted table), account 14 07-20 ordering |
| API-12 | Spike week: `…/accounts/6/activity-health?week=2026-06-01` | Summary 880, median 66, low 39, high 101, `above`, dev 22.37. All 15 locations `above`. `locations[0]` = Site C, 67, median 3, 1–7, dev 12.74 | GOLDEN |
| API-13 | Week after the spike: `…/accounts/6/activity-health?week=2026-06-08` | Summary 102, 37–104, `normal`. `locations[0]` Site C 11 vs 1–8 `above`, `locations[1]` Site J 11 vs 2–10 `above`, and the other 13 `normal`. The spike stays in the baseline but does not hide real rises | GOLDEN-P (PLAN §13 "Phase 0 decisions" promoted table), account 6 06-08 |
| API-14 | Spike still in the baseline: `…/accounts/6/activity-health?week=2026-07-20` | Summary 87, median 72.5 (unrounded), 30–134, `normal`, dev 0.53. All 15 locations `normal`. Site M: 7, median 3.5, 1–9, dev 1.30, and it is `locations[0]` | GOLDEN; GOLDEN-P (PLAN §13 "Contract decisions" promoted table) (Site M is `locations[0]`) |
| API-15 | Type filter: `…/accounts/6/activity-health?week=2026-07-20&type=call_received` | `eventType` `call_received`. Summary 51, median 42, 17–79, `normal`, dev 0.54. Still 15 locations | GOLDEN; SPEC §13 §5.3.1 |
| API-16 | Single-site account: `…/accounts/8/activity-health` | Exactly 1 location (Site A). Summary and location both show 7, median 10, 5–17, `normal`. `earliestWeek` `2026-02-02` | GOLDEN-P (PLAN §13 "Phase 0 decisions" promoted table), account 8 07-20; earliestWeek GOLDEN |
| API-17 | Single site, floor case: `…/accounts/8/activity-health?week=2026-03-09` | Summary 11, median 11, 6–18, `normal` | GOLDEN |
| API-18 | Insufficient history: `…/accounts/8/activity-health?week=2026-03-02` | Summary `insufficient_data`, `baseline` = `{ "weeksUsed": 3, "median": null, "low": null, "high": null }`, `deviation` null. Count **8** is present | GOLDEN (status, weeksUsed 3); GOLDEN-P (PLAN §13 "Phase 0 decisions" promoted table) (count 8); SPEC §13 §5.2 (null fields) |
| API-19 | Early February, no eligible weeks: `…/accounts/14/activity-health?week=2026-02-02` | 200. Summary count 27, `insufficient_data`, weeksUsed 0. All 4 locations `insufficient_data`, ordered by name A, B, C, D | GOLDEN-P (PLAN §13 "Phase 0 decisions" promoted table), account 14 02-02 (0 eligible weeks); SPEC §13 §5.3.9 (insufficient rows by name) |
| API-20 | Mixed eligibility: `…/accounts/14/activity-health?week=2026-03-02` | Summary 40 vs 16–36 `above`. Order: Site D (16 vs 2–11 `above`), Site B (9 vs 2–10 `normal`), then Site A and Site C, both `insufficient_data` with weeksUsed 3 | GOLDEN-P (PLAN §13 "Phase 0 decisions" promoted table), account 14 03-02 (weeksUsed 3 per site: SPEC §13 §5.3.2) |
| API-21 | Earliest week, sites not yet active: `…/accounts/14/activity-health?week=2026-01-26` | 200. Only Site B and Site D are listed (A and C first appear in the week of 2026-02-02). All rows `insufficient_data`, weeksUsed 0 | GOLDEN-P (PLAN §13 "Phase 0 decisions" promoted table), account 14 01-26; SPEC §13 §5.3.1 |
| API-22 | Location with zero activity (flagged): `…/accounts/6/activity-health?week=2026-06-29` | Site G present with count 0, 2–9, `below`, and it is `locations[0]` | GOLDEN-P (PLAN §13 "Phase 0 decisions" promoted table), account 6 06-29 |
| API-23 | Zero activity under a type filter: `…/accounts/14/activity-health?type=appointment_set` | Total 2, 1–8, `normal`. All 4 sites listed. Site A 0 vs 0–4 (dev −1.12) is `normal`. Site B 0 vs 0–2 is `normal` too (median ≤ 2, so it cannot be `below`) | GOLDEN-P (PLAN §13 "Phase 0 decisions": total, Site A) and GOLDEN-P (PLAN §13 "Contract decisions" promoted table) (Site B 0, median 0, 0–2); SPEC §13 §5.3 known limit |
| API-24 | Exact-tie ordering: same request as API-23 | Sites B, C, D all have deviation exactly 0, so they are ordered by name: `locations` = A, B, C, D | GOLDEN-P (PLAN §13 "Phase 0 decisions" promoted table), account 14 appointment_set ties |
| API-25 | Partial eligibility on a total: `…/accounts/18/activity-health?week=2026-03-23` | Summary `baseline.weeksUsed` 7, 18 vs 15–33 `normal`. Site C 0 vs 1–9 `below`, and it is `locations[0]` | GOLDEN-P (PLAN §13 "Phase 0 decisions" promoted table), account 18 03-23 |
| API-26 | Every account, default week | `week.start` 2026-07-20. `earliestWeek` is 2026-01-26 for accounts 1, 4, 5, 6, 7, 12, 14, 18; 2026-02-02 for 2, 3, 8–11, 13, 15–17, 19; 2026-07-20 for 20 | GOLDEN |
| API-27 | Account 12, default week | Summary 54, 40–74, `normal`. Site F 11 vs 2–11 `normal` (dev 1.90), `locations[0]`. No location is flagged | GOLDEN |

### 3.3 Empty account

| Id | When | Then | Tag |
|---|---|---|---|
| API-30 | `GET …/accounts/20/activity-health` | 200. `summary.count` 0, `insufficient_data`, `baseline.weeksUsed` 0 with null median/low/high, `deviation` null, `locations` `[]`, `earliestWeek` = `latestCompleteWeek` = `2026-07-20` | GOLDEN |
| API-31 | `GET …/accounts/20/activity-health?week=2026-03-02` | 400 ProblemDetails (before earliestWeek) | GOLDEN |
| API-32 | Empty **database** (no events at all; `activity_events` empty), `GET …/accounts/14/activity-health` | 200 empty state as in API-30, with `dataAsOf` **null**. `latestCompleteWeek` comes from the current clock via the injected `TimeProvider`. With the clock pinned at `2026-09-28T12:00:00-04:00` (Mon, America/New_York), `latestCompleteWeek` = `earliestWeek` = `2026-09-21` | SPEC §13 "Contract decisions" (empty database); consensus §3 (latest complete week rule) |

### 3.4 Invalid input (all errors are `ProblemDetails`, `Content-Type: application/problem+json`)

| Id | Request | Expected | Tag |
|---|---|---|---|
| API-40 | `…/accounts/999/activity-health` | 404 | SPEC §13 §5.2 |
| API-40b | `…/accounts/abc/activity-health` | 404 (route constraint `{accountId:int}`) | SPEC §13 "Input handling and UI copy" |
| API-41 | `…/accounts/14/activity-health?week=2026-07-21` (a Tuesday) | 400 | SPEC |
| API-42 | `…/accounts/14/activity-health?week=2026-07-27` (a Monday, but the current partial week) | 400 | SPEC |
| API-43 | `…/accounts/14/activity-health?week=2026-01-19` (before earliestWeek) | 400 | SPEC / GOLDEN |
| API-44 | `…/accounts/8/activity-health?week=2026-01-26` (valid for 14, before 8's earliestWeek) | 400; the same week is 200 for account 14 (API-21) | SPEC |
| API-45 | `type=ALL`, `type=Call_Received`, `type=calls` | 400 each (case-sensitive) | GOLDEN |
| API-46 | `week=2026-13-01`, `week=20260720`, `week=abc`, `week=07/20/2026`, `week=2026-7-20` | 400 ProblemDetails each, with the same validation-problem shape as a bad `type` | SPEC §13 "Input handling and UI copy"; SPEC §13 "Contract decisions" (extra variants, shape) |
| API-47 | Unhandled exception in the pipeline (test: a service fake that throws) | 500, `Content-Type: application/problem+json`, no exception message and no stack trace in the body | SPEC §13 §5.2 ("Errors are ProblemDetails") |
| API-48 | API-boundary mapper unit test with constructed deviations 0.125 and −0.125 (exactly representable binary midpoints) | Emits 0.13 and −0.13 (2 dp, `MidpointRounding.AwayFromZero`). Core keeps full precision; rounding happens only in the mapper. No seed value sits on a midpoint (consensus §3), so constructed values are required | SPEC §13 §5.2, §5.3.8 |

---

## 4. Dashboard slice (`web/`, Angular)

How to verify: run `cd web && npm test` (Vitest output), then open the app with the API running. Each "open" below is a literal URL.
Copy ids (C-xx, P-xx) refer to §0.

### 4.1 Default view: Q1 and Q2 answered at a glance

| Id | Given / When | Then | Tag |
|---|---|---|---|
| UI-01 | Open `/dashboard` with no params | URL becomes `/dashboard?account=14&week=2026-07-20&type=all`. `Viewing as` shows Beacon Home Security | SPEC §5.4 URL rules; §13 D5 |
| UI-02 | Same | Summary reads `26 inbound events · usually 18–38 a week` and `Within usual range` | GOLDEN; C-03, C-06 |
| UI-03 | Same | The first table row is Site B: `2`, `Usually 3–12 a week`, `▼ Lower than usual`. The rows below it (C, A, D) each show `Within usual range` | GOLDEN; GOLDEN-P (PLAN §13 "Phase 0 decisions" promoted table) (order B, C, A, D) |
| UI-04 | Same, and every scenario in §4.2 | The rendered page text matches none of: `\bz\b`, `σ`, `±`, `\bmedian\b` (case-insensitive), `\btypical\b` (case-insensitive), `\bdeviation\b` (case-insensitive), and a standalone `\bNormal\b` (capital N, whole word, so `Within usual range` passes). | SPEC §13 §5.4 |
| UI-05 | Same | Status is readable with colours removed (symbol + text), e.g. by checking the DOM text or a greyscale screenshot | SPEC §13 §5.4 |
| UI-06 | Same | Footnote contains C-09, C-10, C-11, C-12 and `Data as of Mon Jul 27, 2026`, each starting with a capital letter. The summary carries C-08. C-14 is absent | SPEC §13 §5.4; SPEC §13 "Input handling and UI copy" (capitalisation, C-08) |
| UI-07 | Same | `Next week ▶` is disabled (2026-07-20 = latestCompleteWeek); `◀ Previous week` is enabled | SPEC §5.4 stepper bounds |

### 4.2 Scenarios

| Id | Open | Then | Tag |
|---|---|---|---|
| UI-10 | Spike week `?account=6&week=2026-06-01&type=all` | Summary `880 inbound events · usually 39–101 a week`, `▲ Higher than usual`. All 15 rows `▲ Higher than usual`. First row Site C, `67`, `Usually 1–7 a week` | GOLDEN |
| UI-11 | Week after the spike `?account=6&week=2026-06-08&type=all` | Summary `Within usual range` (102, usually 37–104). Site C then Site J at the top, both `▲ Higher than usual` | GOLDEN-P (PLAN §13 "Phase 0 decisions" promoted table), account 6 06-08 |
| UI-12 | Spike in the baseline `?account=6&week=2026-07-20&type=all` | Summary `87 inbound events · usually 30–134 a week`, `Within usual range`. All 15 rows `Within usual range` | GOLDEN |
| UI-13 | Type filter: from UI-12, choose `Calls` | URL `type=call_received`. Summary `51 calls · usually 17–79 a week` (no "inbound events"). Footnote now also shows C-14 | GOLDEN (51, 17–79); SPEC §13 §5.4 (C-14); SPEC §13 "Input handling and UI copy" (noun) |
| UI-14 | Single-site `?account=8` | One row, Site A: `7`, `Usually 5–17 a week`, `Within usual range`. The summary shows the same figures | GOLDEN-P (PLAN §13 "Phase 0 decisions" promoted table), account 8 07-20 |
| UI-15 | Insufficient history `?account=8&week=2026-03-02&type=all` | Summary reads `8 inbound events` with no "usually" range, and `Not enough history yet (3 of 4 weeks needed)` | GOLDEN (status, 3 weeks); GOLDEN-P (PLAN §13 "Phase 0 decisions" promoted table) (count 8); SPEC §13 "Input handling and UI copy" (C-06b) |
| UI-16 | Earliest weeks `?account=14&week=2026-02-02&type=all` | Every row shows its count and `Not enough history yet (0 of 4 weeks needed)`, with no range | GOLDEN-P (PLAN §13 "Phase 0 decisions" promoted table), account 14 02-02; SPEC §13 "Input handling and UI copy" (C-04 with N = 0 kept) |
| UI-17 | Mixed `?account=14&week=2026-03-02&type=all` | Site D `▲ Higher than usual` first. Sites A and C last, each with `Not enough history yet (3 of 4 weeks needed)` | GOLDEN-P (PLAN §13 "Phase 0 decisions" promoted table), account 14 03-02 |
| UI-18 | Zero-activity location `?account=6&week=2026-06-29&type=all` | First row Site G, `0`, `Usually 2–9 a week`, `▼ Lower than usual` | GOLDEN-P (PLAN §13 "Phase 0 decisions" promoted table), account 6 06-29 |
| UI-19 | Zero, small median `?account=14&week=2026-07-20&type=appointment_set` | Sites A and B show `0` and `Within usual range`; Site B reads `Usually 0–2 a week`. Footnote C-12 explains why neither can be lower than usual | GOLDEN-P (PLAN §13 "Phase 0 decisions": Site A) and GOLDEN-P (PLAN §13 "Contract decisions" promoted table) (Site B 0–2); SPEC known limit |
| UI-20 | Empty account `?account=20` | Shows `No activity recorded for this account yet.` in place of **both** the summary and the table: no `0 inbound events`, no table. The trigger is `locations == [] && summary.baseline.weeksUsed == 0`, not `earliestWeek`. Filters stay visible and usable; both week buttons are disabled (earliestWeek = latestCompleteWeek). No error banner | GOLDEN; SPEC §13 §5.4 (trigger); SPEC §13 "Input handling and UI copy" (replaces summary and table, filters stay) |
| UI-21 | Stepper at the lower bound `?account=14&week=2026-01-26&type=all` | `◀ Previous week` disabled. Only Site B and Site D listed | SPEC; GOLDEN-P (PLAN §13 "Phase 0 decisions" promoted table), account 14 01-26 |
| UI-22 | Component test: `DashboardState`/page given an empty-state response with `dataAsOf: null` (empty database) | C-07 is shown, and no `Data as of` line is rendered anywhere (not "Data as of null" or "Invalid Date") | SPEC §13 "Contract decisions" (empty database) |

### 4.3 URL state, reload and invalid params (§5.4, still in force)

| Id | Given / When | Then | Tag |
|---|---|---|---|
| UI-30 | Set account 6, week 2026-06-01, type Calls using the controls, then reload | Same account, week and type selected; same numbers shown; URL `?account=6&week=2026-06-01&type=call_received` | SPEC §5.4 |
| UI-31 | Click `◀ Previous week` from the default | URL `week=2026-07-13`; `Next week ▶` enabled. Reload keeps 2026-07-13 | SPEC §5.4 |
| UI-32 | Open `?account=999` | Rewritten to `account=14`; default view | SPEC §5.4 ("invalid → defaults") |
| UI-33 | Open `?account=14&week=2026-07-21` (a Tuesday) | Rewritten to the default `week=2026-07-20` (the latest complete week), **not** the nearest Monday. Here they coincide, so also check UI-33b | SPEC §5.4; SPEC §13 "Input handling and UI copy" (defaults, not nearest Monday) |
| UI-33b | Open `?account=14&week=2026-03-04` (a Wednesday) | Rewritten to `week=2026-07-20`, not `2026-03-02` | SPEC §13 "Input handling and UI copy" |
| UI-34 | Open `?account=14&week=2026-07-27` (current partial week) | Rewritten to `week=2026-07-20` | SPEC §5.4 + §13 §5.2 |
| UI-35 | Open `?account=14&week=2025-12-29` (before earliestWeek) | The UI recovers from the API's 400: the URL is replaced with `week=2026-07-20`, the default week's data is shown, and no error banner appears | SPEC §5.4 + §13 §5.2 |
| UI-36 | Open `?account=20&week=2026-03-02` | Rewritten to `week=2026-07-20`; empty state shown, not an error | GOLDEN (API 400) + SPEC §5.4 |
| UI-37 | Open `?type=ALL` or `?type=foo` | Rewritten to `type=all` | SPEC §5.4 + §13 §5.2 |
| UI-38 | Any rewrite in UI-32…37 | Uses replace (`replaceUrl`), not a new history entry, so Back does not return to the invalid URL | SPEC §5.4 ("URL rewritten") |
| UI-39 | At `?account=14&week=2026-03-02&type=call_received`, switch `Viewing as` to 6 | Week and type are kept: `?account=6&week=2026-03-02&type=call_received` | SPEC §13 "Input handling and UI copy" |
| UI-39b | At `?account=14&week=2026-01-26&type=call_received`, switch `Viewing as` to 8 | The kept week 2026-01-26 is before account 8's earliestWeek (2026-02-02). The UI gets the API's 400 for that week, then rewrites the URL (replaceUrl) to `?account=8&week=2026-07-20&type=call_received`. Type is kept, and no error banner appears | SPEC §13 "Input handling and UI copy" (account switch) |
| UI-40 | API unreachable or 5xx | C-20 shown; filters stay in the URL. `Try again` calls `DashboardState.reload()` and, once the API is back, shows the data without changing the URL | SPEC §13 "Input handling and UI copy" |
| UI-41 | Component test: `LocationTable` given a fixture of 4 rows in an order that is neither alphabetical nor by \|deviation\| (e.g. Site C normal 0.1, Site A below −2.5, Site D normal −1.0, Site B above 3.0), with `low`/`high` that no client formula would reproduce (e.g. 7–8) | Rows render in exactly the payload order C, A, D, B, and each shows `Usually 7–8 a week` as given. No client re-sorting or recomputation | SPEC §13 §5.2 ("locations returned sorted"), §5.4 ("never recomputed in the UI") |
| UI-42 | Each type for account 14, 2026-07-20 | Summary nouns: all → `26 inbound events`, Calls → `16 calls`, Leads → `8 leads`, Appointments → `2 appointments`. Singular (`1 call` etc.) is covered by a component test with count 1 | SPEC §13 "Input handling and UI copy" (C-06a); counts 16 and 8 GOLDEN-P (PLAN §13 "Contract decisions" promoted table); 2 GOLDEN-P (PLAN §13 "Phase 0 decisions") |

---

## 5. README slice (`README.md`, Phase 3)

How to verify: read `README.md` in the merged repo and follow the run steps on a clean clone. The brief (`../Requirements.md`, "What to submit" §2) requires the first seven items.

| Id | README must contain | Then | Tag |
|---|---|---|---|
| README-01 | How to run locally: DB (`docker compose up -d db`), env vars / connection string override, API and web commands | A reader on a clean clone gets the dashboard at the documented URL by following only these steps | Brief; PLAN §5.1 |
| README-02 | A one-line note on how to run the tests (backend and web) | The commands run as written | Brief |
| README-03 | Interpretation of the ticket | States Q1 and Q2 in admin language first, then the precise rule: 8 prior full weeks, R2\* band, status from the integer range | Brief; PLAN §1, §13 §5.3 |
| README-04 | Key assumptions | Each PLAN §4 assumption, with its seed evidence | Brief; PLAN §4 |
| README-05 | Design decisions and trade-offs | D1–D7 as revised by §13, including rejected options and the simulated numbers: R2\* flags ≈ 4 % of site-weeks (4.3 %), drop to 0 caught 98 % all / 96 % calls (was 78 / 37 %) | Brief; PLAN §3, §13 |
| README-06 | Deliberately deferred | PLAN §11 items, each with one line of why | Brief; PLAN §11 |
| README-07 | With another day | Prioritised list, one line of why each | Brief |
| README-08 | Known limits | The seven bullets of consensus §1 "Known limits (README, carried verbatim from the statistician)", quoted exactly:<br>"A location that usually gets ≤ 2 a week can never show "lower"."<br>"A drop to 0 is caught about 90 % of the time at 4+ a week, and about 65 % at 3."<br>"With a 4-week baseline, false flags rise by about 1 point per side."<br>"Design flag rate: about 4 % of site-weeks; 13 % of account-weeks show at least one flagged location (`product_monday_view_out.md`)."<br>"A halving at a single site is usually not caught in one week. Of 99 real halvings at median ≥ 6, 34 were flagged and 65 were not (`reviewer_default_week_out.md`)."<br>"A spike stays in the baseline for 7 weeks and widens ranges by about 20–30 % (true of every rule)."<br>"Per-type filters at site level are thin." | SPEC §13 ("README must carry the known limits in `docs/design-consensus.md` §1 verbatim and state the ≈ 4 % design flag rate") |
| README-09 | Data handling | Duplicates (12 exact pairs counted once; near-duplicates kept), the account 6 spike, the empty account, partial weeks, timezones, small-count limits | Brief ("handle the unglamorous parts"); PLAN §2 |

---

## 6. Coverage of the required scenarios

| Scenario | Criteria |
|---|---|
| Default week | BL-40, BL-42, API-10, API-26, UI-01…07 |
| Empty account (20) | BL-45, DATA-31, API-30, API-31, UI-20, UI-36 (empty database: API-32, UI-22) |
| Single-site account | API-16, API-17, UI-14 |
| Spike week | API-12, UI-10 |
| Week after the spike (baseline contains it) | BL-13, API-13, API-14, UI-11, UI-12 |
| Insufficient history (early February) | BL-12, API-18…21, UI-15…17, UI-21 |
| Type filter | BL-24, DATA-28, API-01d, API-15, API-23, UI-13, UI-19, UI-42 |
| Reload preserves every filter | UI-30, UI-31, UI-39 |
| Invalid URL params | UI-32…UI-37, UI-33b, UI-39b (API side: API-40…46, API-40b) |
| Location with zero activity in the week | BL-20, BL-21, API-22, API-23, API-25, UI-18, UI-19 |
| Duplicates | DATA-20…24, DATA-02, DATA-03 |
| Ranking and ties | BL-30…33, BL-31b, API-11, API-24, UI-41 |
| Starter files unchanged | DATA-40 |
| README | README-01…09 |

---

## Appendix A — Seed evidence

Most of this is now covered by the promoted goldens (`GOLDEN-P`), which were independently recomputed in `analysis/goldens/promoted_goldens_out.md`. Every value below is now promoted to PLAN §13 (the "Phase 0 decisions" and "Contract decisions" tables). No automated criterion is tagged `SEED`. This appendix is kept as the original product evidence and for manual acceptance.

These come from the statistician's R2\* model (`analysis/statistician/golden.py`, which executes `analysis/reference_model.py` on `schema.sql` + `seed.sql` in SQLite), queried by product on 2026-09-28 with a scratch script. Nothing in the repo was changed.
Command: `python3 <scratch>/pq.py analysis/statistician` and `pq2.py` (both call `show(account, week, type)` / `evaluate(...)` from `golden.py`).

**A.1 Site first-activity weeks** (account 14): `{'Site A': '2026-02-02', 'Site B': '2026-01-26', 'Site C': '2026-02-02', 'Site D': '2026-01-26'}`; account 8: `{'Site A': '2026-02-02'}`.

**A.2 Accounts**: `(14, 'Beacon Home Security', 'America/New_York')`, `(20, 'Quiet Harbor Spa', 'America/Los_Angeles')`, 20 rows.

**A.3 De-duplication** (SQLite over the seed):
```
raw 12626 · distinct over non-id columns 12614 · duplicate groups 12, of which with a NULL column 4
raw per account:      1:1223 2:730 3:477 4:797 5:884 6:2641 7:437 8:261 9:546 10:342 11:354 12:1304 13:205 14:638 15:499 16:167 17:323 18:588 19:210
distinct per account: 1:1221 2:729 3:477 4:796 5:884 6:2637 7:437 8:260 9:546 10:342 11:354 12:1303 13:205 14:638 15:499 16:167 17:323 18:586 19:210 (20: none)
sha256 schema.sql 348912f4…e51d3d · seed.sql 40e60ee8…eaea2 (repo root, commit 4898e69)
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
