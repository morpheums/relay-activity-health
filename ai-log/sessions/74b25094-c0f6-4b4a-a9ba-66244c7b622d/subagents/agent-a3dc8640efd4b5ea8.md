
### 🧑 USER — 2026-09-29T09:14:20.217Z

Cleanup review (not bug hunting), REUSE angle, of the session-3 web changes. The diff is at /private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/74b25094-c0f6-4b4a-a9ba-66244c7b622d/scratchpad/simplify.diff (non-test web code, db42240..main); your worktree holds main.

Flag new code that re-implements something the codebase or its installed libraries already have: Angular Material/CDK, date-fns, existing helpers in web/src/app (week.ts, health-copy.ts, the icon component, the testing helpers), or duplicated SCSS rules and literals instead of the tokens in styles.scss. Name the existing helper or token to use instead. For each finding, give the file, line, a one-line summary and the concrete cost.

Constraints for your suggestions: test files are frozen; public contracts (WeekPicker inputs/outputs, IconName, models, tokens) are architect-owned, so flag those separately; desktop only; the footer must stay inside DashboardPage. Only report findings worth fixing; skip nits. Never edit files.


**tool call — Bash**
```json
{"command":"wc -l /private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/74b25094-c0f6-4b4a-a9ba-66244c7b622d/scratchpad/simplify.diff; cat /private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/74b25094-c0f6-4b4a-a9ba-66244c7b622d/scratchpad/simplify.diff | head -1500"}
```


<details><summary>tool result</summary>

```
<persisted-output>
Output too large (51.5KB). Full output saved to: /Users/josemejia/.claude-alt/projects/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/74b25094-c0f6-4b4a-a9ba-66244c7b622d/tool-results/bnpz5ydy1.txt

Preview (first 2KB):
    1143 /private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/74b25094-c0f6-4b4a-a9ba-66244c7b622d/scratchpad/simplify.diff
diff --git a/web/angular.json b/web/angular.json
index b1044ce..7726546 100644
--- a/web/angular.json
+++ b/web/angular.json
@@ -19,7 +19,10 @@
             "browser": "src/main.ts",
             "tsConfig": "tsconfig.app.json",
             "styles": [
-              "src/styles.css"
+              "@fontsource/geist/400.css",
+              "@fontsource/geist/500.css",
+              "@fontsource/geist/600.css",
+              "src/styles.scss"
             ]
           },
           "configurations": {
@@ -32,7 +35,7 @@
                 },
                 {
                   "type": "anyComponentStyle",
-                  "maximumWarning": "4kB",
+                  "maximumWarning": "6kB",
                   "maximumError": "8kB"
                 }
               ],
diff --git a/web/package.json b/web/package.json
index da3f3b4..50c7d83 100644
--- a/web/package.json
+++ b/web/package.json
@@ -11,11 +11,17 @@
   "private": true,
   "packageManager": "npm@11.12.1",
   "dependencies": {
+    "@angular/cdk": "22.2.0",
     "@angular/com …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Read**
```json
{"file_path":"/Users/josemejia/.claude-alt/projects/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/74b25094-c0f6-4b4a-a9ba-66244c7b622d/tool-results/bnpz5ydy1.txt","offset":40}
```


<details><summary>tool result</summary>

```
40	+    "@angular/material": "22.2.0",
41	+    "@angular/material-date-fns-adapter": "22.2.0",
42	     "@angular/platform-browser": "^22.2.0",
43	     "@angular/router": "^22.2.0",
44	+    "@fontsource/geist": "^5.3.0",
45	+    "date-fns": "^4.4.0",
46	     "rxjs": "~7.8.0",
47	     "tslib": "^2.3.0"
48	   },
49	diff --git a/web/src/app/app.config.ts b/web/src/app/app.config.ts
50	index f225160..40e1464 100644
51	--- a/web/src/app/app.config.ts
52	+++ b/web/src/app/app.config.ts
53	@@ -1,15 +1,27 @@
54	 import { provideHttpClient } from '@angular/common/http';
55	 import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
56	+import { MAT_DATE_LOCALE, MatDateFormats } from '@angular/material/core';
57	+import { MAT_DATE_FNS_FORMATS, provideDateFnsAdapter } from '@angular/material-date-fns-adapter';
58	 import { provideRouter } from '@angular/router';
59	+import { enUS } from 'date-fns/locale';
60	 import { AccountsApi, HttpAccountsApi } from './core/api/accounts.api';
61	 import { ActivityHealthApi, HttpActivityHealthApi } from './core/api/activity-health.api';
62	 import { routes } from './app.routes';
63	 
64	+const dateFormatsWithFullMonthLabel: MatDateFormats = {
65	+  ...MAT_DATE_FNS_FORMATS,
66	+  display: { ...MAT_DATE_FNS_FORMATS.display, monthYearLabel: 'LLLL yyyy' },
67	+};
68	+
69	+const enUSWithMondayWeekStart = { ...enUS, options: { ...enUS.options, weekStartsOn: 1 as const } };
70	+
71	 export const appConfig: ApplicationConfig = {
72 …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd web/src/app/features/dashboard && cat week.ts; grep -rn \"sundayOfWeek\\|addWeeks\" --include=*.ts . | grep -v spec | head; grep -rn \"activityCount\" --include=*.ts .. | head"}
```


<details><summary>tool result</summary>

```
const ISO_DATE_PATTERN = /^\d{4}-\d{2}-\d{2}$/;
const MILLISECONDS_PER_DAY = 24 * 60 * 60 * 1000;
const DAYS_PER_WEEK = 7;
const MONDAY = 1;

interface CalendarDayParts {
  weekday: string;
  month: string;
  day: string;
  year: string;
}

function utcMidnightOf(isoDate: string): Date {
  return new Date(`${isoDate}T00:00:00Z`);
}

function isoDateOf(utcMidnight: Date): string {
  return utcMidnight.toISOString().slice(0, 10);
}

export function isIsoMonday(candidate: string | null): candidate is string {
  if (candidate === null || !ISO_DATE_PATTERN.test(candidate)) {
    return false;
  }
  const parsed = utcMidnightOf(candidate);
  return !Number.isNaN(parsed.getTime()) && isoDateOf(parsed) === candidate && parsed.getUTCDay() === MONDAY;
}

export function addWeeks(weekStart: string, weekCount: number): string {
  return isoDateOf(new Date(utcMidnightOf(weekStart).getTime() + weekCount * DAYS_PER_WEEK * MILLISECONDS_PER_DAY));
}

export function sundayOfWeek(weekStart: string): string {
  return isoDateOf(new Date(utcMidnightOf(weekStart).getTime() + (DAYS_PER_WEEK - 1) * MILLISECONDS_PER_DAY));
}

function calendarDayParts(instant: Date, timeZone: string): CalendarDayParts {
  const parts = new Intl.DateTimeFormat('en-US', { weekday: 'short', month: 'short', day: 'numeric', year: 'numeric', timeZone }).formatToParts(instant);
  const partValue = (type: Intl.DateTimeFormatPartTypes): string => parts.find((part) => part.type === type)?.value ?? '';
  return { weekday: part …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd web/src/app && grep -rn \"sundayOfWeek\\|addWeeks\\|activityCount\\|isMonday\" . | grep -v \"spec.ts\""}
```


<details><summary>tool result</summary>

```
features/dashboard/week.ts:29:export function addWeeks(weekStart: string, weekCount: number): string {
features/dashboard/week.ts:33:export function sundayOfWeek(weekStart: string): string {
features/dashboard/health-copy.ts:22:export function activityCount(count: number, eventType: EventType): string {
features/dashboard/dashboard.page.ts:8:import { formatCalendarDay, sundayOfWeek } from './week';
features/dashboard/dashboard.page.ts:196:    return week === null ? null : { start: week, end: sundayOfWeek(week) };
features/dashboard/components/dashboard-filters.ts:5:import { addWeeks } from '../week';
features/dashboard/components/dashboard-filters.ts:135:    this.weekSelected.emit(addWeeks(weekStart, weekCount));
features/dashboard/components/week-picker.ts:17:import { format, isMonday, isWithinInterval, parseISO } from 'date-fns';
features/dashboard/components/week-picker.ts:125:    day !== null && isMonday(day) && isWithinInterval(day, { start: this.earliestMonday(), end: this.latestCompleteMonday() });
features/dashboard/components/week-picker.ts:128:    view === 'month' && !isMonday(day) && isWithinInterval(day, { start: this.selectedMonday(), end: this.selectedSunday() })
```
</details>


### 🤖 ASSISTANT — 2026-09-29T09:15:06.040Z

I found six things worth fixing. I read the diff and `week.ts` but did not run the app or the tests. Paths are under `web/src/app/features/dashboard/` unless they start with `web/`.

**1. `components/week-picker.ts:125` – `dateFilter` re-checks the min/max range that `MatCalendar` already enforces.**
- `[minDate]` and `[maxDate]` already disable out-of-range days. `isSelectableMonday` repeats that with `isWithinInterval(day, earliestMonday, latestCompleteMonday)`.
- Use: keep only `isMonday(day)` in the filter.
- Cost: two `computed`s, `isWithinInterval`, and one more place to keep in sync if the bounds change.

**2. `components/week-picker.ts:128` and `web/src/styles.scss:1086-1100` – the selected-week highlight rebuilds Material's built-in range highlight.**
- `MatCalendar.selected` accepts a `DateRange<Date>`. Passing `new DateRange(monday, sunday)` gives the in-range, range-start and range-end classes and styling for free.
- Restyle it through `mat.datepicker-overrides`, for example the `calendar-date-in-range-state-*` tokens.
- What goes away:
  - `selectedWeekClass` and the `SELECTED_WEEK_CLASS` constant.
  - About 15 lines of `:has()` and `nth-child(7)` gradient SCSS.
  - The Sunday special-casing.
- Caveat: check that disabled Tue–Sun cells still render the range. If they don't, the current approach stays.

**3. `week.ts:24-35` – `addWeeks` and `sundayOfWeek` duplicate date-fns, which the same feature already imports.**
- `week-picker.ts` imports date-fns while `week.ts` hand-rolls UTC-millisecond math, so the feature now has two date idioms.
- Use `addWeeks` and `endOfISOWeek` (or `addDays(…, 6)`) from date-fns.
- `isIsoMonday` could also use `parseISO`, `isValid` and `isMonday`. It would still need a strict-format check.
- `week.ts` also has its own `addWeeks`, so both `addWeeks` functions will be in scope for the next person.
- Cost: the same weekday and week arithmetic is kept in two places.
- Leave `formatCalendarDay` alone, because it needs the timezone and date-fns-tz isn't installed.
- `week.ts` is not a public contract, but its tests are frozen. Check the specs against any new behaviour before swapping.

**4. `components/account-summary.ts:26-33` and `components/location-table.ts:29-35` – the status-to-icon mapping is written twice.**
- Both files switch `normal` to `within-range` and `insufficient_data` to `not-enough-history`. A new status or icon has to be edited in both.
- Use: one `statusIcon(status): IconName | null` next to `statusLabel` in `health-copy.ts`, then `@if (icon(); as name) { <app-icon [name]="name" /> }`.
- It only consumes `IconName`, so no contract change.

**5. Status badge SCSS is duplicated.**
- `account-summary.ts` (`.status-badge[data-status=above|below]`) and `location-table.ts` (`tr[data-status=…] .status-badge`) carry the same tint and ink pairs. They differ only in size (height, padding, font size).
- Use: a shared `.status-badge` rule in `styles.scss` keyed on `[data-status]`. Component-level rules would then set only the size.
- Cost: a colour change for a status has to be made twice, and it can drift.

**6. Repeated literals and control chrome that `styles.scss` doesn't tokenize.**
- **Card shell:** `background: var(--color-surface); border: 1px solid var(--color-line); border-radius: 12px` appears in `account-summary.ts` `.summary`, `location-table.ts` `:host`, `dashboard.page.ts` `.card`, and `week-picker-dialog` and `.fact-tiles li` (which differ in border colour).
  - Use: a `.card` class, or `--radius-card`, in `styles.scss`.
- **Control chrome:** 44px height, 8px radius, `--color-control-border`, and the disabled trio `--color-disabled-fill`, `--color-disabled-border` and `--color-disabled-ink`. It is written out separately for:
  - `select` and `.step` in `dashboard-filters.ts`;
  - `.trigger` in `week-picker.ts`;
  - `.latest-week` in `styles.scss`;
  - `.primary-button` in `dashboard.page.ts`.
  - The disabled trio alone is repeated at `.step:disabled`, `.trigger:disabled`, `.latest-week:disabled` and `.week-placeholder`.
  - Use: `--control-height: 44px`, `--radius-control: 8px` and `--radius-pill: 999px` tokens, plus one shared disabled rule.
- Cost: a redesign tweak touches five or more places. This also explains why the component-style budget was raised from 4kB to 6kB in `angular.json`.

**Flagged separately (architect-owned):**
- Nothing here needs a change to `WeekPicker` inputs or outputs, `IconName`, the models or the tokens. Findings 1, 2 and 6 are internal to `WeekPicker` and its styles.
- Finding 4 only reads `IconName`. It adds a helper in `health-copy.ts`, which is not a contract.

**Considered and skipped:**
- Native `<select>` with a custom chevron instead of `mat-select`. The tests likely rely on native change events.
- `activityCount` is now only used by tests. It is a thin wrapper over `activityNoun`.
- The custom overlay setup in `week-picker.ts` is already built on `CdkConnectedOverlay` and `cdkTrapFocus`, so it isn't a re-implementation.

