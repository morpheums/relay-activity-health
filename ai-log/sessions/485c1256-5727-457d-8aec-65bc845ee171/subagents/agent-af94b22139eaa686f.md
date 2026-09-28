
### 🧑 USER — 2026-09-28T21:46:27.530Z

DASH-247: validate three user decisions (VALID / CONFLICT / CONTRACT IMPACT) and apply any contract impact in your scope. Base your worktree on branch `worktree-agent-a1a385f2ce4281726` (the frontend's Phase 2 implementation, commit 78ee3ff): `git merge worktree-agent-a1a385f2ce4281726` first. Contracts only — no behaviour, no specs; you may adjust component input/output signatures and must keep `npm run build` compiling (if the implementation must change to compile, make the minimal type-level adjustment and say exactly what frontend must still implement).

1. **When no report has loaded yet (e.g. the very first load fails), "Viewing as" and "Activity type" must still render**; only the week stepper waits for a report. Today `DashboardFilters` has required inputs `week`, `earliestWeek`, `latestCompleteWeek`, so `DashboardPage` hides the whole filter bar until the first report. Choose the simplest contract: e.g. make the week-related inputs optional (`WeekRange | null`, bounds `string | null`) with the stepper hidden when null, or split the stepper into its own component. Keep the surface minimal (the user dislikes over-engineering).
2. Keep the page heading `<h1>Activity health</h1>` (product adds it to the copy table) — expect no contract impact.
3. Rows with insufficient history keep an empty "Usual range" cell — expect no contract impact.
Rebuild (`cd web && npm run build` + `npm test` — report which specs, if any, break because of the signature change, since test-author will update them), commit with the Co-Authored-By trailer; never touch the main worktree. Reply with the validation table, the new `DashboardFilters` signature, and worktree/branch/commit.


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-af94b22139eaa686f\" && git status --short && git log --oneline -1 && git merge worktree-agent-a1a385f2ce4281726 2>&1 | tail -3 && git log --oneline -1 && ls web/src/app/features/dashboard -R","description":"Merge frontend branch and list dashboard files"}
```


<details><summary>tool result</summary>

```
Exit code 1
46fad89 Record DST-at-midnight out-of-scope ruling in PLAN §13
 9 files changed, 477 insertions(+), 29 deletions(-)
 create mode 100644 web/src/app/features/dashboard/health-copy.ts
 create mode 100644 web/src/app/features/dashboard/week.ts
33b6039 Merge branch 'worktree-agent-a1a385f2ce4281726' into worktree-agent-af94b22139eaa686f
ls: -R: No such file or directory
web/src/app/features/dashboard:
components
dashboard-state.spec.ts
dashboard-state.ts
dashboard.page.spec.ts
dashboard.page.ts
health-copy.ts
week.ts
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-af94b22139eaa686f/web/src/app/features/dashboard\" && ls components/*; cat components/dashboard-filters/*.ts 2>/dev/null || cat components/*filters*; cat dashboard.page.ts","description":"Read filters component and page"}
```


<details><summary>tool result</summary>

```
components/account-summary.spec.ts
components/account-summary.ts
components/dashboard-filters.spec.ts
components/dashboard-filters.ts
components/location-table.spec.ts
components/location-table.ts
(eval):1: no matches found: components/dashboard-filters/*.ts
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Account, EventType } from '../../../core/models';
import { LATEST_COMPLETE_WEEK, seedAccounts, sundayOf } from '../../../../testing/activity-health-fixtures';
import { chooseOption, collapsedText, getButton, getSelect, isDisabled, optionTexts, selectedOptionText } from '../../../../testing/dom-queries';
import { DashboardFilters } from './dashboard-filters';

const PREVIOUS_WEEK = '◀ Previous week';
const NEXT_WEEK = 'Next week ▶';

interface FiltersInputs {
  accounts: readonly Account[];
  accountId: number;
  weekStart: string;
  earliestWeek: string;
  latestCompleteWeek: string;
  eventType: EventType;
}

interface FiltersUnderTest {
  root: HTMLElement;
  fixture: ComponentFixture<DashboardFilters>;
  selectedAccountIds: number[];
  selectedWeeks: string[];
  selectedEventTypes: EventType[];
}

const BEACON_DEFAULT_INPUTS: FiltersInputs = {
  accounts: seedAccounts(),
  accountId: 14,
  weekStart: '2026-07-20',
  earliestWeek: '2026-01-26',
  latestCompleteWeek: LATEST_COMPLETE_WEEK,
  eventType: 'all',
};

async function renderFilters(overrides: Partial<FiltersInputs> = {}): Promise<FiltersUnderTest> {
  const inputs = { ...BEACON_DEFAULT_I …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-af94b22139eaa686f\" && grep -n -i \"usual range\\|<h1>\\|heading\\|Viewing as\\|stepper\\|first load\\|copy table\" PLAN.md | head -40; grep -n \"usual\\|Usual\" web/src/app/features/dashboard/components/location-table.ts | head","description":"Check PLAN for decisions 2 and 3"}
```


<details><summary>tool result</summary>

```
50:| D5 | Account switcher labelled "Viewing as" (impersonation for demo, not auth). Default account **12** (Redline Tire & Service, 7 sites, one flagged in the default week) | Hardcode one account; cross-account overview | Lets an evaluator see account 6, 20 and single-site accounts in the app, not only in tests |
124:- Components: `DashboardPage` (container), `DashboardFilters` (Viewing-as select, ◀ week ▶ stepper bounded by `earliestWeek`/`latestCompleteWeek`, type select),
239:3. Week stepper → plain date input.
345:| normal | Within usual range |
354:- **Account with no events:** "No activity recorded for this account yet." Shown when `locations == [] && summary.baseline.weeksUsed == 0`. The week stepper is disabled because `earliestWeek == latestCompleteWeek`.
356:- The word "Normal" never appears alone on screen; "Within usual range" is used in the table, the summary and the footnote.
453:- **Empty account:** "No activity recorded for this account yet." replaces both the summary and the table; filters stay visible; the week stepper is disabled.
455:- **Additional copy:** type select "Activity type" — "All activity" / "Calls" / "Leads" / "Appointments"; week label "Mon Jul 20 – Sun Jul 26, 2026"; stepper "◀ Previous week" / "Next week ▶";
456:  table headers "Location" / "Events" / "Usual range" / "Status"; summary heading "{account name} — all locations"; account method line "compared with the last 8 full weeks for this account".
527:  latestCompleteWeek, eventType → a …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-af94b22139eaa686f\" && sed -n 440,460p PLAN.md; sed -n 520,530p PLAN.md; grep -n -i \"insufficient\" PLAN.md | head -20","description":"Read PLAN copy and component sections"}
```


<details><summary>tool result</summary>

```
| Account 14, week 2026-02-02, all | total 27, `insufficient_data`; all four sites listed, all `insufficient_data` (0 eligible weeks) |
| Account 14, week 2026-03-02, all (mixed history) | total 40, median 25, range 16–36, `above`; Site D 16 vs 2–11 `above` (1st), Site B 9 vs 2–10 `normal` (2nd), Sites A and C `insufficient_data` (last, by name) |
| Account 14, week 2026-07-20, all | ranking order B, C, A, D |
| Account 14, week 2026-07-20, `appointment_set` (ties) | total 2, range 1–8, `normal`; Site A 0 vs 0–4 (dev −1.12) first; Sites B, C, D have deviation 0 → ordered by name B, C, D |
| Account 18, week 2026-03-23, all (7 eligible weeks) | total 18, median 23, range 15–33, `normal`; Site C 0 vs 1–9 `below` (dev −2.90), ranked first |

### 2026-09-28 — Input handling and UI copy decisions (user decisions, validated by the architect)

**Reason.** Open items from the acceptance-criteria review (`docs/acceptance-criteria.md` §0.2 at the time). Each was decided by the user and validated by the
`architect` against the contracts (no conflicts). Supersedes the §5.4 status/footnote copy only where stated; the approved strings live verbatim in `docs/acceptance-criteria.md` §0.1.
- **API input:** malformed `week` (not `yyyy-MM-dd`, e.g. `2026-13-01`, `20260720`, `abc`) → 400 `ProblemDetails`; non-numeric `accountId` → 404 (route constraint `{accountId:int}`).
- **URL normalisation:** any invalid URL parameter is rewritten to its default (latest complete week, `all`, account 14) with …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-af94b22139eaa686f/web/src/app/features/dashboard\" && python3 - <<'EOF'\np='components/dashboard-filters.ts'\ns=open(p).read()\ns=s.replace('''    <div class=\"week-stepper\" role=\"group\" aria-label=\"Week\">\n      <button type=\"button\" [disabled]=\"!canGoToPreviousWeek()\" (click)=\"stepWeek(-1)\">◀ Previous week</button>\n      <span class=\"week-label\" aria-live=\"polite\">{{ weekLabel() }}</span>\n      <button type=\"button\" [disabled]=\"!canGoToNextWeek()\" (click)=\"stepWeek(1)\">Next week ▶</button>\n    </div>\n''','''    @if (weekStepper(); as stepper) {\n      <div class=\"week-stepper\" role=\"group\" aria-label=\"Week\">\n        <button type=\"button\" [disabled]=\"!stepper.canGoToPreviousWeek\" (click)=\"stepWeek(stepper.weekStart, -1)\">◀ Previous week</button>\n        <span class=\"week-label\" aria-live=\"polite\">{{ stepper.weekLabel }}</span>\n        <button type=\"button\" [disabled]=\"!stepper.canGoToNextWeek\" (click)=\"stepWeek(stepper.weekStart, 1)\">Next week ▶</button>\n      </div>\n    }\n''')\ns=s.replace('''  readonly week = input.required<WeekRange>();\n  readonly earliestWeek = input.required<string>();\n  readonly latestCompleteWeek = input.required<string>();''','''  readonly week = input<WeekRange | null>(null);\n  readonly earliestWeek = input<string | null>(null);\n  readonly latestCompleteWeek = input<string | …[clipped in markdown; full text in raw/]
```


<details><summary>tool result</summary>

```
 .../dashboard/components/dashboard-filters.ts      | 39 ++++++++++++++--------
 web/src/app/features/dashboard/dashboard.page.ts   | 24 ++++++-------
 2 files changed, 37 insertions(+), 26 deletions(-)
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-af94b22139eaa686f/web/src/app/features/dashboard\" && python3 - <<'EOF'\np='dashboard.page.ts'\ns=open(p).read()\ns=s.replace('[week]=\"lastLoadedReport() ? (selectedWeekRange() ?? lastLoadedReport()!.week) : null\"','[week]=\"stepperWeek()\"')\ns=s.replace('''  protected readonly dataAsOfLabel''','''  protected readonly stepperWeek = computed<WeekRange | null>(() => {\n    const boundsReport = this.lastLoadedReport();\n    return boundsReport === undefined ? null : (this.selectedWeekRange() ?? boundsReport.week);\n  });\n\n  protected readonly dataAsOfLabel''')\nopen(p,'w').write(s)\nEOF\ncd ../../../.. && ls node_modules >/dev/null 2>&1 || npm ci --silent; npm run build 2>&1 | tail -8; npm test 2>&1 | tail -40","description":"Refine page binding, build and test","timeout":600000}
```


<details><summary>tool result</summary>

```
styles-5INURTSO.css | styles        |   0 bytes |                 0 bytes

                    | Initial total | 251.50 kB |                68.80 kB

Application bundle generation complete. [3.458 seconds] - 2026-09-28T21:47:13.113Z

Output location: /Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-af94b22139eaa686f/web/dist/relay-activity-health-web


> relay-activity-health-web@0.0.0 test
> ng test

❯ Building...
✔ Building...
Application bundle generation complete. [1.621 seconds] - 2026-09-28T21:47:15.720Z


[1m[30m[46m RUN [49m[39m[22m [36mv5.0.2 [39m[90m/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-af94b22139eaa686f/web[39m


[2m Test Files [22m [1m[32m8 passed[39m[22m[90m (8)[39m
[2m      Tests [22m [1m[32m179 passed[39m[22m[90m (179)[39m
[2m   Start at [22m 22:47:17
[2m   Duration [22m 4.13s[2m (environment 59%, tests 21%, setup 13%, transform 5%, import 2%, worker 1%)[22m
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-af94b22139eaa686f\" && (cd web && npm run build 2>&1 | grep -i -c warn); git add web/src/app/features/dashboard/components/dashboard-filters.ts web/src/app/features/dashboard/dashboard.page.ts && git commit -q -m \"Make DashboardFilters week inputs optional so account and type selects render before the first report\n\nCo-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>\" && git log --oneline -1 && git branch --show-current","description":"Check warnings and commit"}
```


<details><summary>tool result</summary>

```
0
0f51cfe Make DashboardFilters week inputs optional so account and type selects render before the first report
worktree-agent-af94b22139eaa686f
```
</details>


### 🤖 ASSISTANT — 2026-09-28T21:47:41.510Z

## Summary
I checked the three user decisions against PLAN.md and the contracts. Only decision 1 changes a contract. I applied it by making the week inputs of `DashboardFilters` optional (null by default). The stepper is hidden until they are set, and "Viewing as" and "Activity type" now always render. The build passes with 0 warnings and all 179 tests pass, so no spec needs updating.

## Validation
| # | Decision | Verdict | Notes |
|---|---|---|---|
| 1 | Viewing as / Activity type render with no report loaded; only the stepper waits | **VALID + CONTRACT IMPACT** | Fits PLAN §13 ("filters stay visible"; after a failed first load the error and "Try again" show). It changes the `DashboardFilters` inputs from required to optional; the input and output names stay the same as the §13 component contract. |
| 2 | Keep `<h1>Activity health</h1>` | VALID, no contract impact | Already in `DashboardPage`. Product needs to add the string to the copy table. |
| 3 | Rows with insufficient history keep an empty "Usual range" cell | VALID, no contract impact | Matches §5.4 ("count is shown, with no range"). `LocationTable` already shows the cell only when there is a range. |

## New `DashboardFilters` signature
```ts
export class DashboardFilters {
  readonly accounts = input.required<readonly Account[]>();
  readonly accountId = input.required<number>();
  readonly week = input<WeekRange | null>(null);
  readonly earliestWeek = input<string | null>(null);
  readonly latestCompleteWeek = input<string | null>(null);
  readonly eventType = input.required<EventType>();

  readonly accountSelected = output<number>();
  readonly weekSelected = output<string>();
  readonly eventTypeSelected = output<EventType>();
}
```
The stepper renders only when `week`, `earliestWeek` and `latestCompleteWeek` are all non-null.

## Changed
- `/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-af94b22139eaa686f/web/src/app/features/dashboard/components/dashboard-filters.ts`: the three inputs are now optional. The old stepper computeds (`weekLabel`, `canGoToPreviousWeek`, `canGoToNextWeek`) are replaced by one `weekStepper` computed that returns null when any of the three is missing. The stepper is wrapped in `@if (weekStepper(); as stepper)`, and `stepWeek` now takes `(weekStart, weekCount)`.
- `/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-af94b22139eaa686f/web/src/app/features/dashboard/dashboard.page.ts`: `<app-dashboard-filters>` is no longer inside `@if (lastLoadedReport())`. A new `stepperWeek` computed passes null before any report has loaded, and the bounds are bound as `lastLoadedReport()?.earliestWeek ?? null` (same for `latestCompleteWeek`).

## Verification
- `git merge worktree-agent-a1a385f2ce4281726`: clean merge, 33b6039.
- `cd web && npm run build`: "Application bundle generation complete", 0 lines matching "warn".
- `cd web && npm test`: `Test Files 8 passed (8)`, `Tests 179 passed (179)`.
- `dotnet build` was not run because no C# changed.

## Deviations
- To keep the build compiling I made two small changes beyond the signatures: the `@if` around the stepper and the page no longer hiding the filter bar. That is the whole of decision 1 at the template level, so frontend has nothing left to implement for it.

## Uncertain
- **Test gap:** no spec fails, because every existing test passes all three week inputs. Test-author should add:
  - a `DashboardFilters` case with no week inputs set: both selects render and no stepper buttons appear;
  - a `DashboardPage` case where the first load fails: both selects are visible alongside the error.
- Before the first `/api/accounts` response, `accounts` is `[]`, so "Viewing as" renders empty for a moment. That seems acceptable, but check it against the copy review.

## Next
- Frontend has nothing left to implement for decision 1.
- Product adds `Activity health` to the copy table.
- Coordinator: consider adding one line to PLAN §13 noting that `week`, `earliestWeek` and `latestCompleteWeek` are optional in the `DashboardFilters` contract and the stepper is hidden until they are set.

## Worktree / branch / commit
- Worktree: `/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-af94b22139eaa686f`
- Branch: `worktree-agent-af94b22139eaa686f`
- Commit: `0f51cfe` (on top of merge `33b6039`, which includes the frontend's 78ee3ff). The main worktree was not touched.

