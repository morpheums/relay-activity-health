
### 🧑 USER — 2026-09-28T20:00:06.960Z

Final delta review of `docs/acceptance-criteria.md` for DASH-247. Read-only. Branch `worktree-agent-a83c87ee8003d7e64` (latest 2006444); compare with the version you last reviewed: `git diff aaada44 2006444 -- docs/acceptance-criteria.md`. Worktree `/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a83c87ee8003d7e64`.

Your last review (aaada44) raised: (1) BL-14 non-representable midpoints; (2) DATA-29 contradicts consensus §4.5; (3) SEED values in automated criteria; (4) missing BL-31b normal-row ordering; (5) UI-41/42 order; (6) UI-04 unassertable clause; (7) README-08 not verbatim; plus the missing PLAN entry for the capitalised account method line. All decisions are now in PLAN §13 (latest entry "Contract decisions from the Phase 0 contract review", which also promotes the remaining seed values and adds nullable dataAsOf / exact "…Z" / strict week variants).

Verify each item is fixed, the new criteria (API-32 empty database with dataAsOf null + TimeProvider example 2026-09-28 → latest complete week 2026-09-21; UI-22; API-10 exact dataAsOf; API-46 variants; API-48 rounding 0.125/−0.125) match PLAN §13, and every GOLDEN-P value matches `analysis/goldens/promoted_goldens_out.md`. Check no new contradiction was introduced. Verdict APPROVED / CHANGES REQUIRED with ranked findings split into (a) objective and (b) needing a user decision. CLAUDE.md handoff format.


**tool call — Bash**
```json
{"command":"W=\"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a83c87ee8003d7e64\"; cd \"$W\"; git diff aaada44 2006444 -- docs/acceptance-criteria.md; git log --oneline -3"}
```


<details><summary>tool result</summary>

```
This agent is isolated in the worktree /Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-ac0b0c3d1bdd79f95, but this command changes directory to the shared checkout (/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a83c87ee8003d7e64) before running git. Refusing to run it — a worktree-isolated agent's git operations must target its own worktree. Run the equivalent from /Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-ac0b0c3d1bdd79f95 without the redirect.
```
</details>


**tool call — Bash**
```json
{"command":"W=\"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a83c87ee8003d7e64\"; cd \"$W\"; cat analysis/goldens/promoted_goldens_out.md; grep -n \"^## 13\" -A200 PLAN.md | tail -120"}
```


<details><summary>tool result</summary>

```
### account 6, week 2026-06-08, type all
| series | count | median | range | status | deviation (z) | current rule (low,high,status,dev) | baseline |
|---|---|---|---|---|---|---|---|
| TOTAL | 102 | 66.0 | 37–104 | normal | 1.88 | (32, 100, 'above', 2.11) | [55, 64, 86, 68, 78, 62, 53, 880] |
<!-- total centreT=16.294171 spreadT=2.092731 -->
| 1. Site C | 11 | 3.5 | 1–8 | above | 2.81 | (0, 7, 'above', 4.01) | [3, 5, 3, 6, 2, 3, 4, 67] |
| 2. Site J | 11 | 5.0 | 2–10 | above | 2.11 | (1, 9, 'above', 2.68) | [5, 10, 5, 5, 7, 3, 4, 56] |
| 3. Site K | 9 | 4.5 | 2–9 | normal | 1.71 | (1, 8, 'above', 2.02) | [3, 4, 6, 4, 6, 5, 3, 60] |
| 4. Site H | 7 | 3.5 | 1–8 | normal | 1.44 | (0, 7, 'normal', 1.57) | [1, 4, 2, 5, 5, 3, 3, 54] |
| 5. Site L | 10 | 5.0 | 1–12 | normal | 1.39 | (0, 10, 'normal', 1.69) | [5, 1, 11, 7, 4, 3, 5, 67] |
| 6. Site O | 9 | 5.0 | 1–12 | normal | 1.14 | (0, 10, 'normal', 1.35) | [7, 3, 7, 3, 4, 6, 3, 46] |
| 7. Site D | 4 | 5.5 | 2–11 | normal | -0.66 | (1, 10, 'normal', -0.64) | [4, 6, 12, 3, 4, 7, 5, 70] |
| 8. Site I | 5 | 3.5 | 1–9 | normal | 0.61 | (0, 7, 'normal', 0.67) | [2, 5, 3, 4, 5, 2, 1, 47] |
| 9. Site B | 6 | 4.0 | 1–12 | normal | 0.59 | (0, 9, 'normal', 0.67) | [4, 2, 4, 7, 6, 3, 2, 65] |
| 10. Site N | 5 | 4.0 | 1–9 | normal | 0.44 | (0, 8, 'normal', 0.45) | [1, 8, 3, 3, 6, 4, 4, 72] |
| 11. Site E | 8 | 6.5 | 2–16 | normal | 0.38 | (0, 13, 'normal', 0.4) | [3, 6, 9, 7, 9, 4, 4, 55] |
| 12. Site M | 3 | 3.5 | 1–8 | normal | -0.26 | (0,  …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"git log --oneline -3 2006444 2>&1 | head; git diff aaada44 2006444 -- docs/acceptance-criteria.md"}
```


<details><summary>tool result</summary>

```
<persisted-output>
Output too large (30.2KB). Full output saved to: /Users/josemejia/.claude-alt/projects/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/485c1256-5727-457d-8aec-65bc845ee171/tool-results/bbj7q21oi.txt

Preview (first 2KB):
2006444 Acceptance criteria: apply contract-review decisions and reviewer re-review
fe59491 Merge main into product acceptance-criteria branch
b0a2171 Record Phase 0 contract decisions and further promoted goldens
diff --git a/docs/acceptance-criteria.md b/docs/acceptance-criteria.md
index 4d099c2..f6dc1ff 100644
--- a/docs/acceptance-criteria.md
+++ b/docs/acceptance-criteria.md
@@ -6,10 +6,11 @@ User: a customer admin of one Relay account, on Monday morning, who has to act o
 
 ## How to read this document
 
-**Precedence.** `PLAN.md` §13 has four entries dated 2026-09-28. Three of them apply here:
+**Precedence.** `PLAN.md` §13 has five entries dated 2026-09-28. Four of them apply here:
 - **"Revised design from the four-agent debate (approved by the user)"** overrides §5.1 (index bullet), §5.2, §5.3, the §5.4 status labels and footnote, the §7 Evaluator/Ranking/API bullets, the §7 golden table, D5 and D7.
 - **"Phase 0 decisions and promoted golden values (user decisions)"** adds ten golden scenarios and fixes the API port at 5080.
 - **"Input handling and UI copy decisions (user decisions, validated by the architect)"** decides malformed input, URL normalisation, account switching, the empty and error st …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"git diff aaada44 2006444 -- docs/acceptance-criteria.md | sed -n 20,400p | cut -c1-900"}
```


<details><summary>tool result</summary>

```
 |---|---|---|
 | `GOLDEN` | A value from the "Revised design" §7 golden table or its hand derivations | Yes. Tests must use these values |
-| `GOLDEN-P` | A value from the promoted-goldens table in the "Phase 0 decisions" entry of §13 | Yes |
+| `GOLDEN-P` | A value from a promoted-goldens table in §13: the "Phase 0 decisions" entry or the "Contract decisions" entry | Yes |
 | `SPEC` | Follows directly from a PLAN rule, cited next to it | Yes |
-| `SEED` | A value from my query on the seed, using the statistician's R2\* model (Appendix A). It is **not** in PLAN | Product acceptance only. Tests must not copy it unless it is added to PLAN §13 |
+| `SEED` | A value from my query on the seed, using the statistician's R2\* model (Appendix A). It is **not** in PLAN | No. Appendix and manual acceptance only; no automated criterion currently uses it |
 | `PROPOSED` | Behaviour or copy that PLAN does not specify | Only after the coordinator or user approves it |
 
 **Defaults.** Default account **14** (Beacon Home Security, America/New_York, 4 sites). Default week **2026-07-20**. Default type `all`. Data as of `2026-07-27T22:20:34Z`.
@@ -46,7 +47,7 @@ Sources: PLAN §13 §5.4, D5, and §13 "Input handling and UI copy".
 | C-06a | `{noun}` by type, plural / singular (count = 1) | all: `inbound events` / `inbound event` · call_received: `calls` / `call` · lead_created: `leads` / `lead` · appointment_set: `appointments` / `appointment` | SPEC §13 "Input handling and UI copy" |
 | C-06b | A …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"git show 2006444:PLAN.md | grep -n \"Contract decisions from the Phase 0\" -A60 | cut -c1-1200"}
```


<details><summary>tool result</summary>

```
460:### 2026-09-28 — Contract decisions from the Phase 0 contract review (user decisions, validated by the architect)
461-
462-**Reason.** Raised by the reviewer on the architect's contracts; decided by the user; validated by the `architect`.
463-- **Core stays package-free:** `AddRelayCore` lives in `Relay.Api/Composition`; Core receives `NormalityOptions` as plain data (still bound and `ValidateOnStart`-validated).
464-- **Wire names owned by Core:** one mapping for `ActivityType` (`all|call_received|lead_created|appointment_set`) and `HealthStatus` (`above|below|normal|insufficient_data`),
465-  used by the API mapper, the API `type` validation (pattern derived from it) and Infrastructure SQL.
466-- **Malformed `week` → 400** via a `yyyy-MM-dd` validation attribute on a `string? Week` (the architect's choice, delegated by the user): same validation-problem shape as `type`;
467-  `07/20/2026` and `2026-7-20` are rejected too.
468-- **Instants stay `DateTime` (UTC)** in Core and the API response, serialised as `…Z` exactly as §13 §5.2; an API golden test asserts the exact `dataAsOf` string. (`DateTimeOffset` was rejected: it serialises as `+00:00`.)
469-- **Empty database (no events at all):** 200 empty state; `dataAsOf` is **null** (the UI hides "Data as of"); the latest complete week falls back to the current clock via an injected
470-  `TimeProvider`. D1 still holds whenever data exists. Contract change: `dataAsOf` nullable in C#, TypeScript and §5.2 for this case only.
4 …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"f=docs/acceptance-criteria.md; grep -n \"BL-14\\|DATA-29\\|SEED\\|coordinator adding\\|A\\.[0-9]\" $f | cut -c1-260; echo ---; grep -n \"README-08\" $f | cut -c1-2500; echo ---; grep -n \"Known limits\" -A12 docs/design-consensus.md | cut -c1-400; echo; grep -n \"dataAsOf\" $f | cut -c1-220"}
```


<details><summary>tool result</summary>

```
ugrep: warning: docs/acceptance-criteria.md: No such file or directory
---
ugrep: warning: docs/acceptance-criteria.md: No such file or directory
---
78:**Known limits (README, carried verbatim from the statistician):**
79-- A location that usually gets ≤ 2 a week can never show "lower".
80-- A drop to 0 is caught about 90 % of the time at 4+ a week, and about 65 % at 3.
81-- With a 4-week baseline, false flags rise by about 1 point per side.
82-- Design flag rate: about 4 % of site-weeks; 13 % of account-weeks show at least one flagged location (`product_monday_view_out.md`).
83-- A halving at a single site is usually not caught in one week. Of 99 real halvings at median ≥ 6, 34 were flagged and 65 were not (`reviewer_default_week_out.md`).
84-- A spike stays in the baseline for 7 weeks and widens ranges by about 20–30 % (true of every rule).
85-- Per-type filters at site level are thin.
86-
87-## 2. Presentation — DECIDED
88-
89-**API statuses** are unchanged: `above | below | normal | insufficient_data`.
90-

ugrep: warning: docs/acceptance-criteria.md: No such file or directory
```
</details>


**tool call — Bash**
```json
{"command":"git show 2006444:docs/acceptance-criteria.md > /private/tmp/claude-501/ac.md; f=/private/tmp/claude-501/ac.md; grep -n \"BL-14\\|DATA-29\\|SEED\\|coordinator adding\\|A\\.[0-9]\" $f | cut -c1-240; echo ---; grep -n \"README-08\" $f | cut -c1-2600; echo ---; grep -n \"dataAsOf\\|Data as of\" $f | cut -c1-200"}
```


<details><summary>tool result</summary>

```
This agent is isolated in the worktree /Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-ac0b0c3d1bdd79f95, but this command names git in a form too complex to verify that it stays inside the worktree. Refusing to run it — a worktree-isolated agent's git operations must target its own worktree. Split it into plain, separate commands and run them from /Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-ac0b0c3d1bdd79f95.
```
</details>


**tool call — Bash**
```json
{"command":"git show 2006444:docs/acceptance-criteria.md > /private/tmp/claude-501/ac.md"}
```


<details><summary>tool result</summary>

```
(Bash completed with no output)
```
</details>


**tool call — Bash**
```json
{"command":"f=/private/tmp/claude-501/ac.md; grep -n \"BL-14\\|DATA-29\\|SEED\\|coordinator adding\\|A\\.[0-9]\" $f | cut -c1-240; echo ---; grep -n \"README-08\" $f | cut -c1-2600; echo ---; grep -n \"dataAsOf\\|Data as of\" $f | cut -c1-200"}
```


<details><summary>tool result</summary>

```
24:| `SEED` | A value from my query on the seed, using the statistician's R2\* model (Appendix A). It is **not** in PLAN | No. Appendix and manual acceptance only; no automated criterion currently uses it |
186:| API-01 | `GET $API/api/accounts` | 200, 20 items `{ id, name, timezone }`, including `{ "id": 14, "name": "Beacon Home Security", "timezone": "America/New_York" }` and `{ "id": 20, "name": "Quiet Harbor Spa", "timezone": "America/Los_
337:Most of this is now covered by the promoted goldens (`GOLDEN-P`), which were independently recomputed in `analysis/goldens/promoted_goldens_out.md`. Every value below is now promoted to PLAN §13 (the "Phase 0 decisions" and "Contract de
342:**A.1 Site first-activity weeks** (account 14): `{'Site A': '2026-02-02', 'Site B': '2026-01-26', 'Site C': '2026-02-02', 'Site D': '2026-01-26'}`; account 8: `{'Site A': '2026-02-02'}`.
344:**A.2 Accounts**: `(14, 'Beacon Home Security', 'America/New_York')`, `(20, 'Quiet Harbor Spa', 'America/Los_Angeles')`, 20 rows.
346:**A.3 De-duplication** (SQLite over the seed):
355:**A.4 Account 14, 2026-07-20**
365:**A.5 Account 6, 2026-06-08** (baseline ends with the 880 week)
370:**A.6 Account 6, 2026-07-20**: order Site M, O, I, A, E, J, G, N, L, B, H, F, K, D, C (all normal).
372:**A.7 Account 8, 2026-07-20**: `TOTAL 7 | 10.0 | 5–17 | normal -1.01`; `Site A` identical.
374:**A.8 Account 14, early weeks**
382:**A.9 Zero-activity locations** (type all, whole seed): account 6 Site G 2026-04-13 (below, 2 …[clipped in markdown; full text in raw/]
```
</details>


### 🤖 ASSISTANT — 2026-09-28T20:00:54.026Z

## Verdict: APPROVED

Every item from the aaada44 review is fixed. Every GOLDEN-P value matches `analysis/goldens/promoted_goldens_out.md` and PLAN §13. I found no new contradiction, only two small clarity points.

## Summary
I diffed `aaada44..2006444` for `docs/acceptance-criteria.md`, read the file at 2006444, and checked it against PLAN §13 and `promoted_goldens_out.md`. I also checked the README-08 quotes against `docs/design-consensus.md` §1. I did not run any tests, and none apply to this document.

## Changed (in the reviewed diff)
`docs/acceptance-criteria.md` only. I read it via `git show 2006444:…` because the file is not in this review worktree.

## Verification

**Previous findings**
1. **BL-14 non-representable midpoints — fixed.** BL-14 is removed and replaced by API-48, which uses 0.125 and −0.125. Both are exactly representable in binary and expected to give 0.13 and −0.13 with `AwayFromZero`. This matches PLAN §13 §5.2/§5.3.8. No dangling BL-14 references remain.
2. **DATA-29 vs consensus §4.5 — fixed.** DATA-29 is removed and dropped from the coverage table's "Type filter" row. Case-sensitivity is covered only by API-45.
3. **SEED values in automated criteria — fixed.** The legend now says no automated criterion uses `SEED`.
   - DATA-02 per-account counts, DATA-10, the Site B appointment_set range, and the 16 and 8 call and lead totals are now GOLDEN-P.
   - API-14's Site M `locations[0]` is now GOLDEN-P.
   - Only Appendix A (kept as manual evidence) still says `SEED`.
4. **BL-31b — added.** Account 14, 2026-07-20: Site B (`below`) first, then C (0.98), A (0.61), D (|0.19|), which matches the GOLDEN-P order B, C, A, D.
5. **UI-41/42 order — fixed.** UI-41 now comes before UI-42.
6. **UI-04 unassertable clause — fixed.** "No deviation or median number appears" is removed.
7. **README-08 — fixed.** The seven bullets are quoted exactly. They are character-for-character the same as consensus §1 lines 79–85.
8. **Capitalised account method line — fixed.** C-08 now cites "Contract decisions", and the PLAN §13 entry exists ("Account method line is capitalised").

**Precedence note:** PLAN §13 has five entries dated 2026-09-28, and the document correctly says four apply. The "Phase 0 split by specialty" entry does not.

**New criteria against PLAN §13**
- **API-32:** empty database gives 200 with `dataAsOf` null, and `latestCompleteWeek` comes from an injected `TimeProvider`. This matches PLAN §13. The example is correct: 2026-09-28 is a Monday and its week is still in progress, so the latest complete week is 2026-09-21.
- **UI-22:** matches PLAN's "the UI hides 'Data as of'".
- **API-10:** exact string `"2026-07-27T22:20:34Z"`, matching the "serialised as `…Z`, golden test asserts the exact string" decision.
- **API-46:** adds `07/20/2026` and `2026-7-20`, and the "same validation-problem shape as `type`" requirement. All match PLAN.
- **API-48:** matches PLAN §13 §5.2/§5.3.8 (see finding 1 above).

## Hand-verified golden values
- **DATA-02:** 1221+729+477+796+884+2637+437+260+546+342+354+1303+205+638+499+167+323+586+210+0 = 12,614. This matches both the output file and PLAN §13.
- **Account 6, 2026-06-08:** 102, range 37–104, `normal`; Site C 11 vs 1–8 `above`; Site J 11 vs 2–10 `above`. OK.
- **Account 6, 2026-06-29:** 69, 41–111; Site G 0 vs 2–9, −3.19, `below`. OK.
- **Account 14, 2026-03-02:** 40, 16–36 `above`; Site D 16 vs 2–11; Site B 9 vs 2–10; Sites A and C `insufficient_data`. OK.
- **Account 14, 2026-07-20, `appointment_set`:** total 2, 1–8; Site A 0 vs 0–4, −1.12; Sites B, C, D have deviation 0; Site B range 0–2. OK.
- **Other values checked, all OK:**
  - Account 18, 2026-03-23: 18, 15–33; Site C 0 vs 1–9, −2.90.
  - Account 8, 2026-07-20: 7, 5–17.
  - Account 14, 2026-01-26 and 2026-02-02.
  - Per-type totals 16 (9–24) and 8 (2–12).
  - Site M first on account 6, 2026-07-20 (7, 1–9, 1.30).
  - DATA-10 first-activity weeks: Sites B and D in 2026-01-26, Sites A and C in 2026-02-02.
- **`weeksUsed` 3 for Sites A and C at 2026-03-02:** not stated in the output file. It follows from their first activity in week 2026-02-02, which leaves eligible weeks 02-09, 02-16 and 02-23, so 3. OK.

## Findings

**(a) Objective (minor, non-blocking)**
1. `docs/acceptance-criteria.md` API-01b (line 192) lists `dataAsOf` as a plain field. It should say "string, or null only for an empty database (API-32)", so a test author does not treat null as a violation of the contract.
2. API-32 (line 220) says "2026-09-28 in America/New_York" without an instant. Any instant on that local Monday gives 2026-09-21, but naming one (e.g. 2026-09-28T12:00:00-04:00) removes the ambiguity. It also does not state that `earliestWeek` equals `latestCompleteWeek` (2026-09-21), though "as in API-30" implies it.

**(b) Needing a user decision**
None.

## Deviations
None.

## Uncertain
- Whether the "Data as of" footnote shows in the seeded empty state for account 20, where `dataAsOf` is non-null. UI-20 and C-13 do not say. It is not a contradiction, but the answer should be fixed before UI tests are written.

## Next
Proceed to the red test suites. The two minor edits above can be folded into the next touch of the document.

