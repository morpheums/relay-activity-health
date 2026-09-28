# DASH-247 design consensus — statistician · product · architect · reviewer

Scribe: `architect`. Debated directly between the four agents via `SendMessage` (Round 0 openings, Round 1 resolutions).
Source material: `PLAN.md` §1–§7/§11, `docs/battle-test/*`, `../Requirements.md`.
Evidence scripts: `analysis/statistician/*` (outputs in `docs/battle-test/statistician-outputs/`) and `analysis/debate/*.py` (outputs `*_out.md` next to each script).
All scripts were re-run on the seed during the debate.

## Open / dissenting items

**None.** All four agents signed off AGREE (§9), with no dissent. Non-blocking notes from sign-off have been applied in place.

Positions changed during the debate:
- **Architect, ranking key.** Moved from "|deviation| alone" to the statistician's explicit *flagged-before-normal* key. The seed shows the extra key never changes the order: `architect_contract_checks_out.md` found 0 groups where a normal row out-ranks a flagged one. It is kept as a guard against float noise at one line of cost.
- **Empty-account `earliestWeek` (resolved after crossed messages).** In R1 the architect moved to `null` while product moved to non-null. Final value: **non-null, equal to `latestCompleteWeek`** for an account with no events. Reviewer and product prefer it because the TS type and the stepper need no null branch and the 400 rule is symmetric. The statistician has no objection (0 eligible weeks means insufficient either way). The empty-state copy keys off `locations == [] && summary.baseline.weeksUsed == 0`, not off `earliestWeek`.
- **Statistician, centre difference.** Retracted the claim that T(median) and median(T) differ by < 0.01. The measured maximum is 0.289, p99 0.086, and 22 % of cells differ by more than 0.01; status differs in 1 of 7,136 cells (`statistician_centre_check_out.md`). Decision unchanged (centre = T(raw median)), but the spec states it explicitly and a golden case pins it.
- **Architect, low guard.** Accepted the statistician's `lowT ≤ T(0)` form as the spec text. The architect's shorter `lowT ≤ 0 ? 0 : max(0, ⌈…⌉)` is proven equivalent (0 differences in 7,136 cells).
- **Reviewer, row-level `eligibleWeeks`.** Dropped in favour of the always-present `baseline.weeksUsed`; the two are equivalent.
- **Product, `weeksRequired` per row.** Replaced by a single top-level `minimumEligibleWeeks`.
- **Product, statistician, D5.** Everyone moved from default account 12 to 14.

---

## 1. Normality rule — DECIDED: R2\* (robust z on the Anscombe scale)

**Decision.** Replace PLAN §5.3 steps 5–7 with R2\*.
- Transform: T(x) = 2·√(x + 3/8).
- Band: k = 2 around T(raw median).
- Spread: floor 1.0; the MAD is taken on the transformed scale.
- Minimum eligible weeks: 4.
- The integer range is back-transformed, and **status is read from that integer range**.
- There is no special state for very small medians. Their limit is documented instead.

Constants: `NormalityOptions { BaselineWeeks = 8, MinimumEligibleWeeks = 4, BandWidth = 2.0, SpreadFloor = 1.0 }` (plain data, bound with `ValidateOnStart`).

**Rationale (site level, real weeks with a full 8-week baseline, spike week excluded).** Sources: `statistician-report.md`, `statistician-outputs/sim_out.md`, `mc_out.md`, `recheck_out.md`.

| Rule | False above / below | Drop→0 caught (all / calls) | Low edge = 0 | Status/range contradictions |
|---|---|---|---|---|
| Current PLAN rule | 3.8 / 1.0 % | 78 / 37 % | 22 % | 0 |
| **R2\*** | **1.4 / 2.9 %** | **98 / 96 %** | **2.4 %** | **0 of 253,149** |

**Supporting figures:**
- **Account level:** false flags 1.6 / 3.2 %. Drop to 0 is caught 100 % of the time, a halving 73 %, a doubling 97 %.
- **Account 6 spike week (2026-06-01):** 15/15 sites flag `above`. In 2026-07-20, with the spike inside the baseline, all 15 sites are `normal`.
- **Threshold k = 2:** 4.3 % of site-weeks are flagged, inside the 4.8 % budget PLAN D7 already accepted. At k = 1.75 false "below" doubles to 5.5 %. At k = 2.5, drop-to-0 detection for calls falls to 70 %.
- **Floor 1.0:** this is the Poisson SD on the Anscombe scale, the transformed analogue of the old √median floor.
- **Minimum 4 weeks:** truncating real baselines to 4 weeks raises false flags by about 1 point per side (2.4 / 3.5 %). Drop detection holds at 97 % (`recheck_out.md`).
- **Centre = T(raw median), deliberately.** The displayed median and the band centre are the same number. The alternative, the median of T values, is not interchangeable: it differs by up to 0.289 on the T scale and changes 1 status in 7,136 cells (`statistician_centre_check_out.md`). The [2,4,6,20] golden case in §8 fails under the wrong reading.
- **Why status ≡ |z| > 2 exactly:** for an integer x, x < ⌈y⌉ ⇔ x < y, and T is strictly increasing. So x < low ⇔ T(x) < lowT ⇔ z < −2, and likewise on the high side. Only floating-point rounding at an exact edge could split them.
- **No contradiction on the seed:** status disagrees with |z| > 2 in 0 of 7,136 evaluated cells, and no cell has |z| within 1e-9 of 2 (`statistician_evidence_out.md` §1, `architect_contract_checks_out.md`).

**Small medians (hand-checkable, `statistician_evidence_out.md` §2):**

| Baseline | Count | Median | Range | Status | z |
|---|---|---|---|---|---|
| [0,0,0,0] | 0 | 0 | 0–2 | normal | 0.00 |
| [0,0,0,0] | 3 | 0 | 0–2 | above | 2.45 |
| [1,1,1,1] | 0 | 1 | 0–4 | normal | −1.12 |
| [2,2,2,2] | 0 | 2 | 0–6 | normal | −1.86 |
| [3,3,3,3] | 0 | 3 | 1–7 | **below** | −2.45 |
| [4,4,4,4] | 0 | 4 | 1–9 | below | −2.96 |
| [11,11,11,8] | 6 | 11 | 6–18 | normal (edge) | −1.70 |
| [11,11,11,8] | 5 | 11 | 6–18 | below | −2.11 |
| [3,3,4,4] | 2 | 3.5 | 1–8 | normal | −0.85 |

A series with a usual median of 2 or less cannot show "below". This is honest: P(0 | Poisson 2) = 13.5 %. It is stated in the UI footnote and the README.

**Rejected:**
- Current rule: its symmetric band on a skewed variable misses drops.
- Current rule + zero override: 1,426 status/range contradictions.
- Freeman–Tukey: 7 % false "below" at median 2.
- Poisson/NB exact tails: drop-to-0 for calls only 56 %.
- ±3 and a ≥6-week minimum: survey recommendation drawn from synthetic Poisson data; on the seed it costs drop detection.
- EARS: mean/SD based, flags above only.
- Farrington/Noufaily: fitted models, so out of scope.
- A fifth "too few to judge" status: adds contract, UI and test cost; the footnote covers it.

**Known limits (README, carried verbatim from the statistician):**
- A location that usually gets ≤ 2 a week can never show "lower".
- A drop to 0 is caught about 90 % of the time at 4+ a week, and about 65 % at 3.
- With a 4-week baseline, false flags rise by about 1 point per side.
- Design flag rate: about 4 % of site-weeks; 13 % of account-weeks show at least one flagged location (`product_monday_view_out.md`).
- A halving at a single site is usually not caught in one week. Of 99 real halvings at median ≥ 6, 34 were flagged and 65 were not (`reviewer_default_week_out.md`).
- A spike stays in the baseline for 7 weeks and widens ranges by about 20–30 % (true of every rule).
- Per-type filters at site level are thin.

## 2. Presentation — DECIDED

**API statuses** are unchanged: `above | below | normal | insufficient_data`.

**UI copy** (product-owned) replaces the §5.4 labels:

| Status | Label |
|---|---|
| above | ▲ Higher than usual |
| below | ▼ Lower than usual |
| normal | Within usual range |
| insufficient_data | Not enough history yet (N of 4 weeks needed) |

Symbol and text are always shown together, never colour alone.

- **Range line:** "Usually X–Y a week", where X–Y is the API's `low`–`high`, never recomputed in the UI. Account row: "54 inbound events · usually 40–74 a week".
- **On screen:** no deviation, z, σ, "±" or median.
- **Severity tier:** none; stays deferred (§11).
- **Insufficient data:** the count is shown, with no range, and the label above. N is `baseline.weeksUsed` and 4 is `minimumEligibleWeeks`.
- **Account with no events:** "No activity recorded for this account yet." Shown when `locations == [] && summary.baseline.weeksUsed == 0`. The week stepper is disabled because `earliestWeek == latestCompleteWeek`.
- **Median:** not shown on screen (no "typical ~N"); "Usually X–Y" answers the question.
- The word "Normal" never appears alone on screen; "Within usual range" is used in the table, the summary and the footnote.
- **Footnote** (plain English):
  - method: "compared with the last 8 full weeks at this location";
  - "inbound events, not unique customers";
  - "exact duplicates counted once";
  - "Locations that usually get 2 or fewer events a week can't show 'lower than usual'";
  - "Data as of Mon Jul 27, 2026", with `dataAsOf` rendered in the account timezone.
- **Type ≠ all:** one extra line, "Per-type counts at a single location are small; only large changes show up."

**Ranking order:**
1. `insufficient_data` last, ordered among themselves by location name (ordinal).
2. Flagged (`above`/`below`) before `normal`.
3. |deviation| descending, using the **unrounded** double.
4. At equal |deviation|, `below` before `above`.
5. Location name ascending, ordinal.

Why not "below first": on the z scale a drop to 0 at median 8 scores −4.56, while a doubling scores +2.31 (`statistician-report.md`). Drops therefore already rank by rarity. Ties at equal |z| (to 1e-12 within a group) occur when (median, madT, count) coincide: 1,161 seed rows are in tie groups, and none mixes above with below (`statistician_evidence_out.md` §6). Tests must therefore exercise the name tie-break.

**D5 default account: 12 → 14 (Beacon Home Security, 4 sites).** Under R2\*, account 12 flags nothing in 2026-07-20: Site F 11 vs 2–11 is `normal`, z = 1.90, P(X≥11 | Poisson 5.5) = 2.5 %. In the default week exactly one series in the whole seed is flagged: account 14 Site B, 2 vs usually 3–12, `below`, z = −2.16. That is the "location going quiet" story (`product_default_account_out.md`, `reviewer_default_week_out.md`). Accounts 6 and 20 stay reachable via "Viewing as". Context: 46 of 341 account-weeks (13 %) show at least one flagged site, and account 6 shows one in 7 of 17 weeks (`product_monday_view_out.md`). This is the reason for the neutral wording.

## 3. Contract gaps — DECIDED

| Gap | Decision |
|---|---|
| `baseline` when insufficient | `baseline` is **always an object** `{ weeksUsed, median, low, high }`. `weeksUsed` = number of eligible weeks (0–8; 0 when none). `median`/`low`/`high` are `null` when insufficient. Series `deviation` is `null` when insufficient |
| "N of 4" in the UI | New top-level `minimumEligibleWeeks: 4` next to `baselineWeeks: 8`. No per-row `weeksRequired` |
| `earliestWeek` | Local Monday of the week containing the account's first event (**any type**). **Never null**: for an account with no events it equals `latestCompleteWeek`. Seed values: 2026-01-26 for accounts 1, 4, 5, 6, 7, 12, 14, 18; 2026-02-02 for 2, 3, 8–11, 13, 15–17, 19; 2026-07-20 for account 20 (`architect_contract_checks_out.md`) |
| Week before `earliestWeek` | **400** for every account, symmetric with "after latest complete week". For account 20 only 2026-07-20 is valid |
| Site existence under a type filter | Site list and eligibility use the site's first event of **any type**. The type filter changes counts only, so a site with no events of that type shows 0. The of-type alternative changes 110 statuses on the seed, 99 of them normal → insufficient, with no statistical gain (`statistician_evidence_out.md` §5) |
| Account first event | MIN over the sites' first events. The sites query is **unbounded** (`GROUP BY location` over the whole account), and Core filters to `firstActivity < end of W` (rule 1 stays in Core). The anchor query returns only the global `MAX(occurred_at)` |
| Latest complete week | Per account: the local week containing the global anchor, minus 7 days. It is 2026-07-20 for all 20 accounts |
| Median of the baseline | Over **eligible** weeks only (ineligible weeks are dropped, not zeros); an even count gives the mean of the middle two |
| Rounding | `deviation` rounded to 2 dp with `MidpointRounding.AwayFromZero`, **at the API boundary only**. On the seed, 0 values sit on a midpoint. `median` is returned unrounded (always x or x.5); `low`/`high` are ints |
| Band edges | Status is an integer comparison (`count < low`, `count > high`), so the float-edge ambiguity A15 disappears. A count equal to `low` or `high` is `normal` |
| Name ordering | Ordinal (`StringComparer.Ordinal`) |
| Empty account | 200, `summary.count = 0`, `insufficient_data`, `baseline.weeksUsed = 0`, `earliestWeek = latestCompleteWeek`, `locations: []` |
| Starter file location | `schema.sql` and `seed.sql` are currently at the repo root, not in `db/` as PLAN §5.1/§6 and CLAUDE.md say. The Phase 0 architect task moves them (`git mv`, content untouched); until then the plan's `db/` paths are the target, not the current state |
| Deviation meaning | z on the Anscombe scale (not raw count units) |

## 4. Data layer — DECIDED: adopt every SQL Server finding

1. **Covering index:** `IX_activity_events_account_occurred (account_id, occurred_at) INCLUDE (location, event_type, duration_seconds, outcome)`. Without the two extra columns the weekly query does a clustered scan with 2,641 OPENJSON executions. With them it does 26 index seeks in about 7 ms (`sqlserver-findings.md` §5).
2. **Deduplication:** only via `SELECT DISTINCT` / `GROUP BY` over **all non-id columns** (NULL = NULL). `=`-based self-joins or `NOT EXISTS` are forbidden: they miss 4 of 12 duplicates that carry NULLs, keeping 12,618 rows instead of 12,614.
3. **Windows:** passed as OPENJSON with **UTC `Z`** instants (`DateTime.Kind = Utc`). OPENJSON silently drops offsets.
4. **Returned instants:** `DateTime.SpecifyKind(…, Utc)` in Infrastructure before they reach Core.
5. **Event type:** validated in the API against the exact lower-case set (`all | call_received | lead_created | appointment_set`, case-sensitive). `all` is passed to SQL as `null`, because the collation is case- and trailing-space-insensitive.
6. **Column types:** explicit `varchar(n)`.
7. **SQL shape:** results materialised with `ToListAsync()` only; raw SQL has no trailing `;` and no `ORDER BY`.
8. **Rejected:** TVP (needs a user-defined type and a migration, for no gain at this size); scalar range plus C# bucketing (moves counting out of SQL, against CLAUDE.md rule 7).

The weekly counts query text is the one recommended in `sqlserver-findings.md` §3(c).

---

## 5. Replacement text — PLAN §5.1 (index line only)

> **No unique constraint** (it would reject the duplicate rows). Index `IX_activity_events_account_occurred` on
> `(account_id, occurred_at) INCLUDE (location, event_type, duration_seconds, outcome)` — covers the de-duplication so the weekly query seeks.
> Exact duplicates are removed only by `DISTINCT`/`GROUP BY` over every non-id column (never `=` on nullable columns). Windows are sent as
> UTC (`Z`) JSON; instants read back are marked `DateTimeKind.Utc`. Columns are explicit `varchar(n)`.

## 6. Replacement text — PLAN §5.2

> `GET /api/accounts` → `[{ id, name, timezone }]` (includes account 20).
>
> `GET /api/accounts/{accountId}/activity-health?week=YYYY-MM-DD&type=all`
>
> | Param | Rule |
> |---|---|
> | `week` | Optional local Monday. Default = latest complete week. Not a Monday → 400. After `latestCompleteWeek` → 400. Before `earliestWeek` → 400 |
> | `type` | Exactly `all` (default) \| `call_received` \| `lead_created` \| `appointment_set`, case-sensitive; else 400 |
> | `accountId` | Unknown → 404 |
>
> Errors are `ProblemDetails`. Response (account 6, 2026-07-20, all):
> ```json
> {
>   "account": { "id": 6, "name": "Metro Collision Centers", "timezone": "America/New_York" },
>   "eventType": "all",
>   "week": { "start": "2026-07-20", "end": "2026-07-26" },
>   "dataAsOf": "2026-07-27T22:20:34Z",
>   "latestCompleteWeek": "2026-07-20",
>   "earliestWeek": "2026-01-26",
>   "baselineWeeks": 8,
>   "minimumEligibleWeeks": 4,
>   "summary": { "count": 87, "baseline": { "weeksUsed": 8, "median": 72.5, "low": 30, "high": 134 }, "status": "normal", "deviation": 0.53 },
>   "locations": [ { "location": "Site M", "count": 7, "baseline": { "weeksUsed": 8, "median": 3.5, "low": 1, "high": 9 }, "status": "normal", "deviation": 1.30 } ]
> }
> ```
> - `status ∈ above | below | normal | insufficient_data`.
> - `baseline` is always present. When `insufficient_data`, `baseline = { weeksUsed: 0–3, median: null, low: null, high: null }` and `deviation: null`.
> - `earliestWeek` = local Monday of the week containing the account's first event (any type); for an account with no events it equals `latestCompleteWeek` (never null).
> - `deviation` is a z-score on the Anscombe scale, rounded to 2 dp (away from zero). `median` is unrounded (x or x.5). `low`/`high` are integers.
> - `locations` is returned sorted (§5.3 step 9).
> - Empty account → 200, `summary.count = 0`, `insufficient_data`, `baseline.weeksUsed = 0`, `earliestWeek = latestCompleteWeek`, `locations: []`.

## 7. Replacement text — PLAN §5.3

> ### 5.3 Normality rules (the product logic — pure C#)
> Notation: T(x) = 2·√(x + 0.375) (Anscombe transform; makes small counts roughly equal-variance). T(0) = 2·√0.375 = 1.224744871391589.
>
> For the account total and for each site, for selected week `W`:
> 1. **Sites** = distinct locations whose first event (any type) is before the end of `W` (local next-Monday 00:00 in UTC, exclusive). The type filter never changes the site list.
> 2. **Baseline weeks** = the 8 local weeks before `W`, **zero-filled**. A week is *eligible* only if it starts **after** the week containing the series'
>    first event of any type (site → site's first event; account total → account's first event = MIN over its sites). `weeksUsed` = number of eligible weeks.
> 3. Fewer than **4** eligible weeks → `insufficient_data`: count shown; median, low, high and deviation are null.
> 4. `median` = median of the eligible weeks' counts only (mean of the middle two when even). This is the displayed median.
> 5. `centre = T(median)` — T of the raw median, **not** the median of the transformed values (they differ for even counts); `madT` = median of |T(cᵢ) − centre| over the eligible counts; `spread = max(1.4826 × madT, 1.0)`
>    (1.4826 makes the MAD comparable to a standard deviation; 1.0 is the Poisson SD on this scale).
> 6. `lowT = centre − 2·spread`, `highT = centre + 2·spread`.
>    `low = lowT ≤ T(0) ? 0 : ⌈(lowT/2)² − 0.375⌉` (the guard is mandatory: squaring a negative `lowT` would create a false lower edge);
>    `high = ⌊(highT/2)² − 0.375⌋`.
> 7. Status from the integers only: `count < low` → `below`; `count > high` → `above`; otherwise `normal` (a count equal to `low` or `high` is `normal`).
>    The displayed range "usually low–high" therefore can never contradict the status.
> 8. `deviation = (T(count) − centre) / spread`, full precision internally; rounded to 2 dp (away from zero) only in the API response.
> 9. Ranking of locations: `insufficient_data` last (among themselves by name); then flagged (`above`/`below`) before `normal`; then |deviation|
>    descending on the unrounded value; then `below` before `above`; then location name ascending (ordinal).
>
> Constants live in `NormalityOptions { BaselineWeeks = 8, MinimumEligibleWeeks = 4, BandWidth = 2.0, SpreadFloor = 1.0 }`.
> Known limit: a series whose usual median is ≤ 2 can never be `below` (a drop to 0 is within normal variation there).

## 8. Replacement text — PLAN §7 golden values and new edge cases

Scope: this replaces only the §7 **golden values** table and the **Evaluator / Ranking / API** edge-case bullets. The Calendar (DST, Phoenix, UTC, boundary instant,
latest-complete-week), Grid and SQL edge cases in PLAN §7 stay in force unchanged. The §13 entry must say so.

Sources:
- 6/12/8/1 rows: `analysis/statistician/golden.py` → `docs/battle-test/statistician-outputs/golden_out.md`.
- Account 14 row: `analysis/debate/statistician_evidence.py` → `statistician_evidence_out.md` §3, cross-checked by `product_default_account_out.md` and `reviewer_default_week_out.md`.
- Unit edge cases: `statistician_evidence_out.md` §2.
- Even-count centre case: `analysis/debate/statistician_even_golden.py` → `statistician_even_golden_out.md`; `statistician_centre_check_out.md`.
- `earliestWeek`: `architect_contract_checks_out.md`.

> **Golden values (R2\*, from the independent Python model)**
> | Scenario | Expected |
> |---|---|
> | Account 6, week 2026-06-01, all | total 880, median 66, range 39–101, `above`, dev 22.37; **all 15 sites `above`**; top = Site C (67, median 3, range 1–7, dev 12.74) |
> | Account 6, week 2026-07-20, all | total 87, median 72.5, range 30–134, `normal`, dev 0.53 (baseline contains the 880 week); all 15 sites `normal`; Site M 7, median 3.5, range 1–9, dev 1.30 |
> | Account 6, week 2026-07-20, `call_received` | total 51, median 42, range 17–79, `normal`, dev 0.54 |
> | Account 12, week 2026-07-20, all | total 54, median 56, range 40–74, `normal`; Site F 11 vs 2–11 → `normal` (dev 1.90), ranked first |
> | Account 14, week 2026-07-20, all (default account) | total 26, median 27, range 18–38, `normal`; **Site B 2 vs 3–12 → `below` (dev −2.16), ranked first**; Sites C, A, D `normal` |
> | Account 1, week 2026-07-06, Site C | 4 (raw rows 5 — one exact duplicate) |
> | Account 8, week 2026-03-02 | `insufficient_data`, `weeksUsed` 3 |
> | Account 8, week 2026-03-09 | baseline 11,11,11,8 → median 11, madT 0, spread 1.0 (floor), range 6–18, `normal` |
> | Account 20 | default week → 200 empty state, `earliestWeek` = `latestCompleteWeek` = 2026-07-20; `week=2026-03-02` → 400 |
> | Default week (any account) | 2026-07-20 |
> | `earliestWeek` | 2026-01-26 for accounts 1, 4, 5, 6, 7, 12, 14, 18; 2026-02-02 for 2, 3, 8–11, 13, 15–17, 19; 2026-07-20 for 20 |
>
> Hand derivation, account 8 on 2026-03-09: median 11; centre = 2√11.375 = 6.745369; three of the four |T(cᵢ) − centre| are 0, so madT = 0 and spread = 1.0;
> lowT = 4.745369 > T(0), so low = ⌈2.3726845² − 0.375⌉ = ⌈5.2546⌉ = 6; highT = 8.745369, so high = ⌊4.3726845² − 0.375⌋ = ⌊18.745⌋ = 18.
>
> Even-count centre case (pins centre = T(raw median)), baseline [2,4,6,20]: median 5, centre 4.636809, madT 1.004056 (gaps 1.554602, 0.453509, 0.412943,
> 4.390926 → middle two average), spread 1.488613, lowT 1.659583 > T(0) → low ⌈0.3136⌉ = 1, highT 7.614035 → high ⌊14.118⌋ = 14.
> Count 14 → `normal` (z 1.98); 15 → `above`; 0 → `below`. (Under the wrong centre = median of T the range is 1–13 and 14 reads `above`.)
>
> **Evaluator edge cases (unit, no DB)**
> - Fewer than 4 eligible weeks.
> - madT = 0, so the floor applies.
> - Even-count median.
> - Spike inside the baseline.
> - Count exactly `low` or `high` → `normal`.
> - Rows from `statistician_evidence_out.md` §2:
>   - [0,0,0,0]→0 gives 0–2 normal.
>   - [0,0,0,0]→3 gives above.
>   - [2,2,2,2]→0 gives 0–6 normal.
>   - [3,3,3,3]→0 gives 1–7 **below** (z −2.45).
>   - [11,11,11,8]→6 gives normal (edge).
>   - [11,11,11,8]→5 gives below.
> - **Low guard:** a case with lowT < 0 (e.g. median 1 with spread 3 → low 0), one with lowT in (0, T(0)] → low 0, and one just above T(0) ([2,4,6,20], lowT 1.6596 → low 1).
>   Concrete discriminating baselines (`analysis/debate/statistician_guard_case.py` → `_out.md`): lowT < 0: [0,1,5,9] → median 3, lowT −1.927794, range **0–21**
>   (without the guard the false edge gives low 1 — prefer this test); 0 < lowT ≤ T(0): [1,1,1,1] → lowT 0.345208, range 0–4; just above T(0): [2,4,6,20] → low 1, or [3,3,3,3] → 1–7.
>   With the T(0) guard the ceiling argument is always > 0 (lowT > T(0) ⇒ (lowT/2)² − 0.375 > 0), so low ≥ 1 in that branch; a `Math.Max(0, …)` is defensive only.
>
> **Ranking:** insufficient last (by name); flagged before normal; above and below by |deviation| together; equal |deviation| → below before above, then name.
>
> **API:**
> - 400 for a week before `earliestWeek`.
> - 400 for `type=ALL` / `Call_Received` (case-sensitive).
> - Account 20: default week → 200 empty with `earliestWeek` 2026-07-20; `week=2026-03-02` → 400.

D5 in §3 changes: default account **14** (Beacon Home Security, 4 sites, Site B lower than usual in the default week).

## 9. Sign-off

| Agent | Reply |
|---|---|
| statistician | AGREE (verified every statistical number against its outputs; supplied concrete guard baselines, now in §8; tie wording tightened in §2) |
| product | AGREE (non-blocking note: verbatim copy strings live in product's copy table `docs/acceptance-criteria.md`, which must stay consistent with §2; the method line reads "at this location" for sites and "for this account" for the summary row) |
| reviewer | AGREE (independently re-derived acct 6 07-20 total, acct 8 03-09, [2,4,6,20], small-median rows; cross-checked acct 12 and acct 6 calls. Nits applied: guard note reworded as defensive; §8 scope note added) |
| architect | AGREE (scribe) |

## Handoff (architect)
## Summary        Agreed on R2*, neutral UI copy, the always-present baseline object, earliestWeek (non-null; equals latestCompleteWeek when there are no events), the covering index and the rest of the SQL findings, and default account 14. PLAN §5.1/§5.2/§5.3/§7 replacement text is in this document.
## Changed        docs/design-consensus.md; analysis/debate/architect_contract_checks.py (+ _out.md)
## Verification   python3 analysis/debate/architect_contract_checks.py → 7,136 evaluated cells; low-guard form differences 0; status vs |z|>2 disagreements 0; normal-outranks-flagged groups 0; rounding midpoints 0
## Deviations     None from the process. Content deviates from PLAN §5.2–§5.4, §7 and D5 by design; this needs a §13 entry after user approval
## Uncertain      Behaviour on bursty data (seed variance ≈ mean; on synthetic bursty data false flags rise to about 5–8 % per side)
## Next           Architect Phase 0 contracts must include: BaselineAssessment with WeeksUsed plus nullable Median/Low/High/Deviation; NormalityOptions.SpreadFloor; non-null EarliestWeek (= LatestCompleteWeek when no events); MinimumEligibleWeeks in the response DTO; an unbounded sites query (no beforeUtc)
