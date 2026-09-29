
### 🧑 USER — 2026-09-29T02:35:46.548Z

The user decided two things for the DASH-247 week picker (2026-09-29):
- Keep the Material calendar (option A). Days from neighbouring months stay blank; it's a known Material limitation.
- Add a "Latest week" button in the picker popover footer, under the C-30 range line.

Behaviour: clicking it closes the picker, returns focus to the trigger and selects latestCompleteWeek, which writes ?week= to the URL as a new history entry, the same as choosing a week. It is natively disabled, but still visible, when the selected week already is latestCompleteWeek. It never selects the in-progress week (PLAN D1).

The designer's copy proposal is L-01: button "Latest week", with its accessible name matching the visible text (alternatives: "Go to latest week", "Latest complete week").

Read CLAUDE.md, the newest PLAN.md §13 entry and docs/acceptance-criteria.md (C-24…C-32, UI-48/UI-49). Then:
1. Finalise the label as C-33, in the existing voice.
2. Add ONE lean UI-50 check: button present in the open picker; for account 14 at week 2026-07-13 it selects 2026-07-20 (new history entry, dialog closes, focus back on the trigger); disabled at 2026-07-20.
3. Note under the picker entries that adjacent-month days stay blank, by user decision (option A).
Only edit docs/acceptance-criteria.md. Commit; the message ends with `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`. Handoff report in the CLAUDE.md format.


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a3e0c0ecff9ea5359\" && git log --oneline -3 && grep -n \"^## \\|^### \" PLAN.md | tail -20 && grep -n \"C-2[4-9]\\|C-3[0-9]\\|UI-4[6-9]\\|UI-5\" docs/acceptance-criteria.md"}
```


<details><summary>tool result</summary>

```
1c94af1 Merge UI/UX redesign: header, footer, Material week picker, Geist, light-red higher
378b7fe Fix reviewer findings on the DASH-247 redesign
5f855e4 Add red test: re-selecting the current week closes the week picker
215:## 9. Execution phases & parallelisation
234:## 10. Time budget & cut line
242:## 11. Deferred (deliberately)
248:## 12. AI log (minimal, no hooks)
255:## 13. Plan changes
258:### 2026-09-28 — Revised design from the four-agent debate (approved by the user)
413:### 2026-09-28 — Phase 0 split by specialty; project-file ownership per stack (user decision)
424:### 2026-09-28 — Phase 0 decisions and promoted golden values (user decisions)
446:### 2026-09-28 — Input handling and UI copy decisions (user decisions, validated by the architect)
460:### 2026-09-28 — Contract decisions from the Phase 0 contract review (user decisions, validated by the architect)
482:### 2026-09-28 — Last Phase 0 clarifications (user decisions, validated by the architect)
488:### 2026-09-28 — Calendar contract simplified (user decision at contract review, proposed by the architect)
497:### 2026-09-28 — Contract simplification (/simplify) (user decisions; the DTO question decided by the architect at the user's request)
512:### 2026-09-28 — Phase 1 red-suite decisions (user decisions, validated by the architect)
529:### 2026-09-28 — DST transitions at local midnight are out of scope (architect ruling, delegated by the user)
536:### 2026-09-28 — Phase 2 review decisions (user decisions …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a3e0c0ecff9ea5359\" && sed -n '584,700p' PLAN.md && sed -n '1,30p;70,100p;325,340p;360,380p' docs/acceptance-criteria.md"}
```


<details><summary>tool result</summary>

```
### 2026-09-29 — UI/UX redesign (user decisions, validated by the architect)

**Reason.** Session 3 redesigned the dashboard's look through the `designer` agent, and the user approved the mockups. Behaviour, approved copy and the API are unchanged. Adds to §5.4.
- **Reference:** the approved mockups and design spec (`Spec`: tokens, type scale, states, picker anatomy) are the visual reference for `frontend`. A header and a footer are added.
  The footnote lines move from under the table into a full-width footer, which stays in `DashboardPage`'s template (outside `<main>`). The page keeps exactly one `<h1>` (C-23).
- **Week picker:** Angular Material `MatCalendar` in a CDK connected overlay, opened by our own trigger that shows C-17. Only Mondays from `earliestWeek` to `latestCompleteWeek`
  are selectable, and a selection emits the existing `weekSelected`. Adapter: `provideDateFnsAdapter()` with the date-fns `enUS` locale and `weekStartsOn: 1`. Packages: `@angular/material`,
  `@angular/cdk`, `@angular/material-date-fns-adapter`, `date-fns`. The ◀/▶ stepper and the native selects stay. Before a report loads, a same-width placeholder holds the week slot, so UI-44 still holds.
- **Contract:** no API, TS-model or abstract-token change. One new presentational component, `WeekPicker` (`week`, `earliestWeek`, `latestCompleteWeek` → `weekSelected`),
  rendered by `DashboardFilters`. The inputs and outputs of `DashboardFilters` are unchanged. The picker's disabled state is derived (`e …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a3e0c0ecff9ea5359\" && python3 - <<'EOF'\np='docs/acceptance-criteria.md'\ns=open(p).read()\ndef rep(old,new):\n    global s\n    assert s.count(old)==1, old[:60]\n    s=s.replace(old,new)\nrep(\"\"\"(each marked \"amended by Redesign 2026-09-29\").\n\"\"\",\"\"\"(each marked \"amended by Redesign 2026-09-29\").\n- **Week picker decisions** (user decisions 2026-09-29, after the redesign landed): keep the Material calendar (option A), and add a \"Latest week\" button to the picker footer. PLAN.md has no §13 entry for them yet, so they are cited below as \"Picker decisions 2026-09-29\". C-33 and UI-50 come from them.\n\"\"\")\nrep(\"\"\"| C-32 | Location table caption | `Locations — most unusual first` | T-01, accepted |\n\"\"\",\"\"\"| C-32 | Location table caption | `Locations — most unusual first` | T-01, accepted |\n| C-33 | Picker footer button, under C-30. Its accessible name is the visible text, with nothing added | `Latest week` | L-01, accepted (Picker decisions 2026-09-29). Rejected: `Go to latest week` (longer, and every other control is a short noun phrase), `Latest complete week` (C-30 already shows the range ending at that week) |\n\"\"\")\nrep(\"\"\"Focus is visible at every step | Redesign 2026-09-29; C-27, C-28 |\n\"\"\",\"\"\"Focus is visible at every step | Redesign 2026-09-29; C-27, C-28 |\n| UI-50 | Account 14, `week=2026-07-13`. Open the …[clipped in markdown; full text in raw/]
```


<details><summary>tool result</summary>

```
6988602 Add C-33 Latest week copy, UI-50 check and option A note for the week picker
 docs/acceptance-criteria.md | 7 ++++++-
 1 file changed, 6 insertions(+), 1 deletion(-)
```
</details>


### 🤖 ASSISTANT — 2026-09-29T02:36:18.869Z

## Summary
I added C-33, UI-50 and the option A note to the acceptance criteria and committed them as `6988602`. That is the only file I changed. PLAN.md has no §13 entry for these two week-picker decisions yet, so I cite them as "Picker decisions 2026-09-29" and explain that name in the "How to read" list, the same way "Redesign 2026-09-29" is handled.

## Changed
- `/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a3e0c0ecff9ea5359/docs/acceptance-criteria.md`
  - **"How to read" list:** one new bullet naming the source of C-33 and UI-50.
  - **C-33 (copy table §0.1b):** the button reads `Latest week` and sits under C-30. Its accessible name is exactly the visible text. The table also gives the reasons for rejecting the two alternatives:
    - `Go to latest week` is longer, and every other control is a short noun phrase.
    - `Latest complete week` repeats C-30, which already shows the range ending at that week.
  - **UI-50 (§4.4), one lean check:**
    - Account 14 at `week=2026-07-13`: the button is visible in the open picker.
    - Clicking it closes the dialog, puts focus back on the trigger and sets `week=2026-07-20` as a new history entry (Back returns to 2026-07-13).
    - At 2026-07-20 the button is still visible but natively disabled.
    - It never selects Mon Jul 27, the week in progress (D1).
  - **Note under UI-50:** days from neighbouring months stay blank. This is a known Material limitation kept by user decision (option A), so it is not a defect and no test should assert it.
  - **Traceability row:** UI-46…UI-49 becomes UI-46…UI-50.

## Verification
```
git commit ... && git log --oneline -1
6988602 Add C-33 Latest week copy, UI-50 check and option A note for the week picker
 docs/acceptance-criteria.md | 7 ++++++-
 1 file changed, 6 insertions(+), 1 deletion(-)
```
Before editing, a check confirmed each replaced piece of text appeared exactly once. This is a document-only change, so there is nothing to test yet. UI-50 is not testable until the button is built.

## Deviations
None. The values in UI-50 are already in the document: `latestCompleteWeek` 2026-07-20 and Mon Jul 27 as the partial week both come from UI-48.

## Uncertain
- **Missing PLAN.md entry:** the newest §13 entry is "UI/UX redesign" and says nothing about the Latest week button or option A. The architect should add a §13 entry so these criteria have a proper PLAN.md source. That entry would also cover the older gap where "Redesign 2026-09-29" is cited the same way.
- **Explaining the disabled state:** I did not add any wording that explains why the button is disabled. The selected week, shown in the trigger label and the C-30 line, makes it clear enough. If you want an explanation, that is a new copy decision.

## Next
The frontend agent should copy C-33 exactly and write a red test for UI-50 before building the button. The architect should add the missing §13 entry to PLAN.md.

