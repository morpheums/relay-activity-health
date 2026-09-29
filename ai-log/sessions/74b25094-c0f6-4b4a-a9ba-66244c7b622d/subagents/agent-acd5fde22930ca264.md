
### 🧑 USER — 2026-09-29T09:14:24.301Z

Cleanup review (not bug hunting), SIMPLIFICATION angle, of the session-3 web changes. The diff is at /private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/74b25094-c0f6-4b4a-a9ba-66244c7b622d/scratchpad/simplify.diff (non-test web code, db42240..main); your worktree holds main.

Flag unnecessary complexity the diff adds: redundant or derivable state or signals, copy-paste with slight variation (templates, SCSS blocks), deep nesting, dead code left behind (e.g. health-copy `activityCount` if unused outside frozen tests; note tests are frozen), over-specific CSS. Name the simpler form that does the same job. For each finding, give the file, line, a one-line summary and the concrete cost.

Constraints: test files are frozen; public contracts (WeekPicker inputs/outputs, IconName, models) are architect-owned, so flag those separately; desktop only; the footer stays in DashboardPage; the UI never re-sorts or recomputes. Only report findings worth fixing; skip nits. Never edit files.


**tool call — Bash**
```json
{"command":"wc -l /private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/74b25094-c0f6-4b4a-a9ba-66244c7b622d/scratchpad/simplify.diff; grep -n '^diff' /private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/74b25094-c0f6-4b4a-a9ba-66244c7b622d/scratchpad/simplify.diff"}
```


<details><summary>tool result</summary>

```
    1143 /private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/74b25094-c0f6-4b4a-a9ba-66244c7b622d/scratchpad/simplify.diff
1:diff --git a/web/angular.json b/web/angular.json
26:diff --git a/web/package.json b/web/package.json
48:diff --git a/web/src/app/app.config.ts b/web/src/app/app.config.ts
80:diff --git a/web/src/app/app.routes.ts b/web/src/app/app.routes.ts
93:diff --git a/web/src/app/features/dashboard/components/account-summary.ts b/web/src/app/features/dashboard/components/account-summary.ts
174:diff --git a/web/src/app/features/dashboard/components/dashboard-filters.ts b/web/src/app/features/dashboard/components/dashboard-filters.ts
309:diff --git a/web/src/app/features/dashboard/components/icon.ts b/web/src/app/features/dashboard/components/icon.ts
412:diff --git a/web/src/app/features/dashboard/components/location-table.ts b/web/src/app/features/dashboard/components/location-table.ts
514:diff --git a/web/src/app/features/dashboard/components/week-picker.ts b/web/src/app/features/dashboard/components/week-picker.ts
693:diff --git a/web/src/app/features/dashboard/dashboard.page.ts b/web/src/app/features/dashboard/dashboard.page.ts
904:diff --git a/web/src/app/features/dashboard/health-copy.ts b/web/src/app/features/dashboard/health-copy.ts
924:diff --git a/web/src/app/features/dashboard/week.ts b/web/src/app/features/dashboard/week.ts
952:diff --git a/web/src/styles.css b/web/src/styles.css
955:diff --git a/web/src …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Read**
```json
{"file_path":"/private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/74b25094-c0f6-4b4a-a9ba-66244c7b622d/scratchpad/simplify.diff","offset":48,"limit":1100}
```


<details><summary>tool result</summary>

```
48	diff --git a/web/src/app/app.config.ts b/web/src/app/app.config.ts
49	index f225160..40e1464 100644
50	--- a/web/src/app/app.config.ts
51	+++ b/web/src/app/app.config.ts
52	@@ -1,15 +1,27 @@
53	 import { provideHttpClient } from '@angular/common/http';
54	 import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
55	+import { MAT_DATE_LOCALE, MatDateFormats } from '@angular/material/core';
56	+import { MAT_DATE_FNS_FORMATS, provideDateFnsAdapter } from '@angular/material-date-fns-adapter';
57	 import { provideRouter } from '@angular/router';
58	+import { enUS } from 'date-fns/locale';
59	 import { AccountsApi, HttpAccountsApi } from './core/api/accounts.api';
60	 import { ActivityHealthApi, HttpActivityHealthApi } from './core/api/activity-health.api';
61	 import { routes } from './app.routes';
62	 
63	+const dateFormatsWithFullMonthLabel: MatDateFormats = {
64	+  ...MAT_DATE_FNS_FORMATS,
65	+  display: { ...MAT_DATE_FNS_FORMATS.display, monthYearLabel: 'LLLL yyyy' },
66	+};
67	+
68	+const enUSWithMondayWeekStart = { ...enUS, options: { ...enUS.options, weekStartsOn: 1 as const } };
69	+
70	 export const appConfig: ApplicationConfig = {
71	   providers: [
72	     provideBrowserGlobalErrorListeners(),
73	     provideRouter(routes),
74	     provideHttpClient(),
75	+    provideDateFnsAdapter(dateFormatsWithFullMonthLabel),
76	+    { provide: MAT_DATE_LOCALE, useValue: enUSWithMondayWeekStart },
77	     { provide: ActivityHealthApi, useClass: HttpA …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd web/src && grep -rn \"activityCount\" . | grep -v \"health-copy.ts:\"; for i in calendar chevron-down chevron-up within-range not-enough-history alert retry empty-inbox loading history copies info clock; do echo \"$i: $(grep -rn \"name=\\\"$i\\\"\\|'$i'\" app --include=*.ts | grep -v 'spec\\|icon.ts' | wc -l)\"; done; grep -n \"footnoteReport\\|state.report\\|state.error\" app/features/dashboard/dashboard.page.ts | head -30; grep -rn \"ACTIVITY_NOUNS\" app | head -3; grep -rn \"color-ground\\|week-step\\|account-field\" app styles.scss | head"}
```


<details><summary>tool result</summary>

```
app/features/dashboard/health-copy.spec.ts:3:import { activityCount, statusLabel, usualRange } from './health-copy';
app/features/dashboard/health-copy.spec.ts:15:  ])('activityCount($count, $eventType) reads "$expected"', ({ count, eventType, expected }) => {
app/features/dashboard/health-copy.spec.ts:16:    expect(activityCount(count, eventType)).toBe(expected);
(eval):1: no matches found: --include=*.ts
calendar:        0
(eval):1: no matches found: --include=*.ts
chevron-down:        0
(eval):1: no matches found: --include=*.ts
chevron-up:        0
(eval):1: no matches found: --include=*.ts
within-range:        0
(eval):1: no matches found: --include=*.ts
not-enough-history:        0
(eval):1: no matches found: --include=*.ts
alert:        0
(eval):1: no matches found: --include=*.ts
retry:        0
(eval):1: no matches found: --include=*.ts
empty-inbox:        0
(eval):1: no matches found: --include=*.ts
loading:        0
(eval):1: no matches found: --include=*.ts
history:        0
(eval):1: no matches found: --include=*.ts
copies:        0
(eval):1: no matches found: --include=*.ts
info:        0
(eval):1: no matches found: --include=*.ts
clock:        0
37:      @if (state.error()) {
45:      } @else if (state.report(); as report) {
75:    <footer class="page-footer" [class.with-footnotes]="footnoteReport()">
76:      @if (footnoteReport(); as report) {
186:    source: this.state.report,
191:    const report = this.state.report();
206:  protected readonly footnoteRepor …[clipped in markdown; full text in raw/]
```
</details>


### 🤖 ASSISTANT — 2026-09-29T09:15:07.606Z

## Verdict: CHANGES REQUIRED (cleanup only, no correctness findings)

Findings are ordered by cost. I did not run the tests or the build. I grepped for usages and read the diff.

- **[Medium] `web/src/app/features/dashboard/health-copy.ts:~17-20` (diff lines 919-921).** `activityCount` is dead in product code. `AccountSummary` now calls `activityNoun`, and the only remaining caller is `health-copy.spec.ts`, which is frozen. Keeping it means two exports for one concept, and `activityCount` exists only to keep a frozen spec compiling. The simpler form is to keep only `activityNoun`. That needs the spec to change, so it is a product call. If the spec stays frozen, leave the function but don't extend it. The split is not needed, because the summary could render `{{ count }} {{ noun }}` from `activityCount`, as it did before. The new `<span class="figure">` needs the number separately, so `activityNoun` does earn its place.

- **[Medium] The status-icon `@switch` is copy-pasted across three places.**
  - Locations: `account-summary.ts` (diff 128-135), `location-table.ts` (diff 456-466), and the `Icon` usage in both.
  - Both switches map `normal` to `within-range` and `insufficient_data` to `not-enough-history`.
  - Cost: the status-to-icon mapping is duplicated in two templates, and the table repeats the whole `<span class="status-text">…</span>` wrapper per case. A new status would need edits in both files.
  - Simpler form: put `statusIcon: IconName | null` on the row (in the table) and on a `computed` (in the summary), next to `statusLabel` in `health-copy.ts`. The template becomes one `@if (icon(); as name) { <app-icon [name]="name" /> }`.

- **[Medium] `dashboard.page.ts` lines ~35-95 (diff 747-763): the skeleton block is over-built.**
  - Cost: about 14 lines of SCSS. It includes four `nth-child` width rules (diff 858-861) and hard-coded widths (260, 380, 200, 150, 140px) that duplicate the real table's layout.
  - `skeletonRows = [1, 2, 3, 4]` (diff 897) is a component field that exists only to drive an `@for`. `track skeletonRow` on a constant array is noise.
  - The skeleton is also `aria-hidden` next to a separate `role="status"` line, so it is decoration-only.
  - Simpler form: a single `.skeleton` block class with a few rows in the template, or reuse the real `LocationTable` with placeholder rows. If the design requires the current look, at least drop `skeletonRows` for `@for (_ of [1,2,3,4])`, or a literal repeated row.

- **[Medium] The footer's `footnoteReport` is a derived signal that duplicates the page's own branching.**
  - Location: diff 899 and 767-768. It is `computed(() => error() ? undefined : report())`.
  - The main `@if (state.error()) … @else if (state.report(); as report)` chain (diff 725-735) already has the same condition, and the footer re-derives it.
  - It is used twice in the template (`[class.with-footnotes]` and `@if`), plus `dataAsOfLabel` reads `state.report()` without the error guard. Two nearly identical "report unless errored" reads can drift.
  - Simpler form: make `dataAsOfLabel` and the footer read the same source. The footer's `@if (footnoteReport(); as report)` could also drive a `.with-footnotes` class through `@if`/`@else` blocks, or the `padding-top` distinction could go away.

- **[Medium] `.with-footnotes` is a state class that exists only to vary padding.**
  - Location: diff 865, 889, and the `[class.with-footnotes]` binding.
  - Cost: two override rules (`.with-footnotes { padding-top }` and `.with-footnotes .footer-base { margin-top; padding-top; border-top }`) mean the `footer-base` is styled two ways depending on a parent class.
  - Simpler form: wrap the footnotes in their own `<section class="footnotes">` inside the footer and give it its own margin and border. `.page-footer` then has one padding rule.

- **[Low-Medium] Single-use CSS variables in `styles.scss:29-35` (diff 989-995).**
  - The variables are `--account-field-width`, `--week-field-width`, `--type-field-width`, `--week-picker-trigger-width`, `--week-step-previous-width`, `--week-step-next-width` and `--week-picker-dialog-width`.
  - Each is defined in the global stylesheet and consumed exactly once, in a different file, so a component's layout is split across two files. The width tokens also imply the layout has to be consistent (576px = 150 + 300 + 128 − 2px overlap), but nothing enforces it.
  - Simpler form: inline the numbers in the component that uses them. `--week-field-width` could be dropped so the field is `flex: none` and sized by its contents. The week-picker dialog width could also live in the picker if the overlay's `panelClass` were used. Keep only the tokens that are reused: the colours and `--popover-shadow`.

- **[Low-Medium] Colour tokens duplicate values or exist for one consumer.**
  - Examples: `--color-disabled-chevron` (used once, `week-picker.ts`), `--color-danger-tint` and `--color-danger` (once each), and the `--color-info-*` tokens (three tiles only).
  - Also the `above-*`/`below-*` four-token families, where each level has `-tint`, `-ink`, `-row`, `-border`, and `-border` is used only for `above`.
  - Cost: the tokens are noise for a reader tracing colour. Fold `--color-disabled-chevron` into `--color-disabled-ink` or `--color-control-border`. The stylesheet defines `--color-line-soft`, `--color-line`, `--color-fill-muted` and `--color-disabled-fill` as four near-identical warm greys (#e3e1dc, #eeece7, #eceae5, #f1f0ec). Collapse to two.

- **[Low-Medium] `styles.scss` datepicker overrides are over-specific and fragile.**
  - `.mat-calendar-period-button .mdc-button__label > span` (diff 1069-1070) targets Material's internal MDC DOM to undo the library's upper-casing with `lowercase` plus `::first-letter`. This will break on a Material upgrade.
  - `tr:has(> .mat-calendar-body-label[colspan='7'])` (diff 1071) hides an internal row.
  - `.mat-calendar-body-cell-container:has(...):nth-child(7)` and three `linear-gradient` rules (diff 1085-1099) hand-draw the week range bar.
  - Simpler form: a second `MatDateFormats.display.monthYearLabel` passthrough does not fix the upper-casing, but `dateFormatsWithFullMonthLabel` in `app.config.ts` is then a workaround for a workaround. If the copy is accepted as upper-case, this whole block goes, along with the `dateFormatsWithFullMonthLabel` config (diff 63-66). The selected-week highlight (`selectedWeekClass`, `selectedSunday`, and the seven `:has`/`nth-child` rules) is about 25 lines for a visual band. A simple highlighted row would be much cheaper.

- **[Low-Medium] `DashboardFilters` `weekStepper` is a pass-through.**
  - Location: diff 297-308. `weekStepper` now returns `{ week, earliestWeek, latestCompleteWeek, canGoToPreviousWeek, canGoToNextWeek }`, so the two bounds are copied from inputs into the object only to be re-read in the template.
  - Cost: extra shape churn. The template also uses `stepper.week.start` twice, and `stepWeek` already takes a string.
  - Simpler form: keep the bounds as inputs and read them directly, so the computed holds only the two `can…` booleans.

- **[Low-Medium] `WeekPicker` has redundant derived signals.**
  - Location: diff 633-641. `isDisabled` and `isLatestWeekSelected` are each used once. `selectedSunday` is used only inside `selectedWeekClass`.
  - Cost: nine computeds for one component. `triggerName` (`${weekLabel}, choose week`) restates the visible label, so the label plus `aria-haspopup` already convey it. The `aria-label` overrides the visible text, which has to keep matching.
  - `closeOnOutsideClick` (diff 667-671) manually re-implements the trigger check, which `cdkConnectedOverlay` does not do for the origin click, so it does earn its place. The `close()` early-return guard exists only because `(detach)` calls `close()` after the picker already closed it.
  - Simpler form: inline `isDisabled` and `isLatestWeekSelected`, and derive `selectedWeekClass` from `week()` directly.

- **[Low] `formatCalendarDayPair` in `week.ts` (diff 932-937).**
  - It is fine as an extraction, but `formatSelectableWeeks` (diff 947) is only a label string with `Weeks from … to …`. Consider whether the helper copy is worth an exported function versus a picker `computed`. This is minor.

## Public contracts to flag separately (architect-owned)

- `IconName` (`icon.ts:3-16`) has 13 members. From my grep I could not confirm every icon is used outside `icon.ts` (my usage loop failed on an unmatched glob, so treat it as unchecked). Verify that `calendar`, `chevron-up`/`chevron-down`, `retry`, `loading`, `history`, `copies`, `info`, `clock` and `alert` are all used, and drop any that are not. `empty-inbox` is used twice for two unrelated meanings (the empty account and the "inbound events" fact), which is a semantic overload.
- `Icon` has `strokeWidth` and `size` inputs. Only `[strokeWidth]="2"` is passed in three places, so make 2 the default or drop the input.
- `WeekPicker` inputs and outputs are `week`, `earliestWeek`, `latestCompleteWeek`, `weekSelected`. `week` (a `WeekRange`) and the two bounds as strings are inconsistent: the `week` end is only used for the highlight. If the highlight goes, `week` could be `weekStart: string`.

## Clean areas
- Routing lazy load, `app.config.ts` date adapter, and `Monday` locale are minimal for their purpose. No client-side sorting or statistics were added. `LocationTable.isFlagged` is a display flag derived from server status, not a recomputation. It could instead be `tr[data-status='above'], tr[data-status='below']` in CSS, which would drop the row field. That is optional.

