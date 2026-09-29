
### 🧑 USER — 2026-09-29T09:14:33.302Z

Cleanup review (not bug hunting), ALTITUDE angle, of the session-3 web changes. The diff is at /private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/74b25094-c0f6-4b4a-a9ba-66244c7b622d/scratchpad/simplify.diff (non-test web code, db42240..main); your worktree holds main.

Check that each change fixes the root cause at the right depth rather than patching a symptom. Examples to judge:
- the CSS lowercase/first-letter hack against Material upper-casing the month label;
- global styles.scss selectors that depend on Material DOM internals;
- the fixed pixel widths for stepper buttons to keep a 576px group;
- the placeholder week slot;
- the raised component style budget instead of moving shared footer/card styles to a global stylesheet;
- `_userSelection` for re-select.
Prefer the simpler, more general change over special cases, and name that change. For each finding, give the file, line, a one-line summary and the concrete cost.

Constraints: test files are frozen; contracts are architect-owned; desktop only; the footer stays in DashboardPage; user-approved visuals must not change. Only report findings worth fixing; skip nits. Never edit files.


**tool call — Read**
```json
{"file_path":"/private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/74b25094-c0f6-4b4a-a9ba-66244c7b622d/scratchpad/simplify.diff"}
```


<details><summary>tool result</summary>

```
1	diff --git a/web/angular.json b/web/angular.json
2	index b1044ce..7726546 100644
3	--- a/web/angular.json
4	+++ b/web/angular.json
5	@@ -19,7 +19,10 @@
6	             "browser": "src/main.ts",
7	             "tsConfig": "tsconfig.app.json",
8	             "styles": [
9	-              "src/styles.css"
10	+              "@fontsource/geist/400.css",
11	+              "@fontsource/geist/500.css",
12	+              "@fontsource/geist/600.css",
13	+              "src/styles.scss"
14	             ]
15	           },
16	           "configurations": {
17	@@ -32,7 +35,7 @@
18	                 },
19	                 {
20	                   "type": "anyComponentStyle",
21	-                  "maximumWarning": "4kB",
22	+                  "maximumWarning": "6kB",
23	                   "maximumError": "8kB"
24	                 }
25	               ],
26	diff --git a/web/package.json b/web/package.json
27	index da3f3b4..50c7d83 100644
28	--- a/web/package.json
29	+++ b/web/package.json
30	@@ -11,11 +11,17 @@
31	   "private": true,
32	   "packageManager": "npm@11.12.1",
33	   "dependencies": {
34	+    "@angular/cdk": "22.2.0",
35	     "@angular/common": "^22.2.0",
36	     "@angular/compiler": "^22.2.0",
37	     "@angular/core": "^22.2.0",
38	+    "@angular/forms": "^22.2.0",
39	+    "@angular/material": "22.2.0",
40	+    "@angular/material-date-fns-adapter": "22.2.0",
41	     "@angular/platform-browser": "^22.2.0",
42	     "@angular/router": "^22.2.0",
43	+    "@fontsource/geist": "^5.3.0",
4 …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd web 2>/dev/null; grep -rn \"toLocaleUpperCase\" node_modules/@angular/material/fesm2022/datepicker.mjs | head; grep -n \"headerComponent\" node_modules/@angular/material/fesm2022/datepicker.mjs | head -5; grep -rn \"week-picker-dialog\\|weekStartsOn\\|firstDayOfWeek\" src --include=*.ts -l | head"}
```


<details><summary>tool result</summary>

```
ugrep: warning: node_modules/@angular/material/fesm2022/datepicker.mjs: No such file or directory
ugrep: warning: node_modules/@angular/material/fesm2022/datepicker.mjs: No such file or directory
(eval):1: no matches found: --include=*.ts
```
</details>


### 🤖 ASSISTANT — 2026-09-29T09:15:26.532Z

## Findings

Most severe first. I could not check these against Material's source because `node_modules` is not installed in this worktree. The Material claims below rely on the diff's own comment and on my knowledge of the Material API. Line numbers refer to `simplify.diff`.

1. **`web/src/styles.scss` (about 1053–1135, diff lines 1053–1135): the picker's own template styles are global.**
   - `.helper`, `.footer`, `.latest-week` and `.week-picker-dialog` style markup that `week-picker.ts` itself owns. They sit in `styles.scss` only because the overlay renders outside the component's encapsulation.
   - The Material-internals selectors sit in the same block: `.mat-calendar-*`, `.mdc-button__label > span`, `tr:has(> .mat-calendar-body-label[colspan='7'])`, and the `nth-child(7)` gradients.
   - Fix: move the whole `.week-picker-dialog { … }` block and its keyframes into `WeekPicker`'s `styles`, and set `encapsulation: ViewEncapsulation.None` on that component. Leave the tokens in `styles.scss` as they are: `:root`, `mat.theme`, `theme-overrides` and `datepicker-overrides`.
   - Cost today: the picker's look is split across `week-picker.ts` and a root stylesheet, and the root stylesheet couples to Material DOM internals. Deleting the picker leaves about 90 lines of orphan global CSS. The `--week-picker-dialog-width` variable and `--popover-shadow` also become unnecessary as globals.

2. **`styles.scss` (1068–1070): the lowercase / `::first-letter` hack.**
   - It fights the wrong layer, and it is also fragile. It depends on the `.mdc-button__label > span` internals and on the label being a single word plus year. The `text-transform` trick uppercases only the first letter, so any format that puts the year or a second word first would render wrongly.
   - `app.config.ts` already builds a custom `dateFormatsWithFullMonthLabel` that is defeated afterwards by the header's `toLocaleUpperCase()`. The two workarounds fight each other.
   - Root fix: give `<mat-calendar>` its own `[headerComponent]`, a small header showing `format(activeDate, 'LLLL yyyy')` with the previous, next and period buttons. `MatCalendar` exposes `headerComponent` for this.
   - Once that header exists, drop `dateFormatsWithFullMonthLabel` and `provideDateFnsAdapter(...)`'s custom formats in `app.config.ts`, and drop the two CSS rules.
   - Cost: the current approach is two workarounds in two files, and one Material minor bump can break the label.

3. **`dashboard.page.ts` styles and `angular.json` (about 6.5 kB of styles; budget raised 4 kB → 6 kB).**
   - Raising `anyComponentStyle` warning to 6 kB hides the real problem: `DashboardPage` now owns the loading skeleton, the error card, the empty card, the header, the footer and the layout.
   - Extract the loading skeleton (about 1.7 kB of CSS plus its template) into an `app-loading-skeleton` component. Extract the error and empty states into their own component or components too.
   - `.card` (surface, 1px line, 12px radius) is repeated in `account-summary`, `location-table` and the page. Put it in one place, either a global `.card` in `styles.scss` or a shared surface component.
   - The footer stays in `DashboardPage` as required, and the page's styles fall back under 4 kB. Revert the `angular.json` budget change.
   - Cost: the budget is now loosened for every component in the app.

4. **`account-summary.ts` (127–137) and `location-table.ts` (456–466): the status-to-icon `@switch` and the badge CSS are duplicated.**
   - The status-to-icon `@switch` (`normal` gets `within-range`, `insufficient_data` gets `not-enough-history`) is copied in both places.
   - So are the tint and ink rules for `above`/`below` (`.status-badge[data-status=…]` in one, `tr[data-status=…] .status-badge` in the other).
   - Fix: add one `app-status-badge` component that takes `status` and `text` and owns the icon mapping and colours.
   - Cost: a status added later needs edits in two places, and both templates and stylesheets grow.

5. **`week-picker.ts` (588) and `dashboard-filters.ts` (`--week-step-previous-width`, `--week-step-next-width`, `--week-picker-trigger-width`): fixed pixel widths reproduce a 576px group.**
   - The three widths and the two `-1px` margins add up to 576 by construction (150 + 300 + 128 − 2). The group is a hand-tuned sum that breaks if button copy or font changes.
   - The general fix: the group is already `width: var(--week-field-width)` (576px). Give the step buttons `flex: none` with natural width, and give `app-week-picker` `flex: 1; min-width: 0` with `.trigger { width: 100% }`. The picker then absorbs the remainder.
   - That removes three CSS variables and leaves one magic number. Visuals are unchanged if the natural button widths match the current 150 and 128, which is worth confirming.

6. **`week-picker.ts` (588): `(_userSelection)` is used to detect re-selecting the current week.**
   - It is a underscore-prefixed, semi-private Material output. It is used only so that clicking the already-selected Monday closes the popover.
   - `choose()` already no-ops when the week is unchanged, so the only gain is closing the popover.
   - Simpler: use the public `(selectedChange)`. The popover then stays open on a same-week click, which is a normal outcome. Escape, an outside click and "Latest week" still close it.
   - Cost: it silently breaks on a Material bump. The exact-pinned `22.2.0` version only defers that.

7. **`dashboard-filters.ts` (233–236, 287–293): the placeholder week slot.**
   - `.week-field` already reserves 576px in every state, so the `@else` block does not fix layout shift. It only draws a decorative skeleton, using three spans with hard-coded 110, 220 and 90px widths and `nth-child` margins. This duplicates the `.skeleton` idiom in `dashboard.page.ts`.
   - If the visual is approved, keep it but reuse the shared skeleton class or component from item 3.
   - Cost: this is about 8 lines of one-off magic-number CSS.

8. **`week-picker.ts` (643–649): the date-filter and cell-class logic duplicates Material and date-fns.**
   - `[minDate]` and `[maxDate]` already restrict the range, so `isWithinInterval` inside `isSelectableMonday` is redundant. Keep only `isMonday`.
   - `selectedWeekClass` can be `!isMonday(day) && isSameWeek(day, selectedMonday(), { weekStartsOn: 1 })`. That removes the `selectedSunday` computed and `isWithinInterval`.
   - Cost: two computeds and duplicated bounds logic.

9. **`app.config.ts` (68): `enUSWithMondayWeekStart` is a spread-and-cast locale.**
   - It spreads `enUS.options` and needs `as const`. Use `{ provide: MAT_DATE_LOCALE, useValue: enGB }`, since `enGB` starts weeks on Monday.
   - Cost: a cast and an object clone that will drift from date-fns.

10. **`app.routes.ts` (91): lazy `loadComponent` for the only route.**
    - It does not reduce the Material or CDK cost, because the picker is imported by the same chunk. It looks like a budget workaround.
    - If bundle size is the goal, use `@defer` around the calendar overlay content instead. Otherwise revert this to the eager route.
    - Confidence is low. Skip it if it was requested separately.

## Judged fine

- The `Icon` component is a reasonable single abstraction.
- `formatCalendarDayPair` is a good de-dup of the week formatting.
- The `footnoteReport` computed is acceptable.

