
### 🧑 USER — 2026-09-28T21:35:19.296Z

Review the frontend's Phase 2 Angular implementation for DASH-247 before merge. Read-only. Branch `worktree-agent-a1a385f2ce4281726` (commit 78ee3ff); diff `git diff aca0fe5 78ee3ff -- web/src/app`. Coordinator verified: `npm test` 179/179 green, `npm run build` no warnings, no spec/testing/package/config file changed.

Check against `CLAUDE.md`, `.claude/agents/frontend.md`, PLAN §5.4 and all §13 entries, `docs/acceptance-criteria.md` §0.1/§4:
- `dashboard-state.ts` (focus the agent asked for): `fetchReport`, `isShownByServerResolution`, `rewriteInvalidParams`, `rewriteUrlToShownReport`, the 404→account 14 and 400→no-week retries, two `effect`s writing the URL with replaceUrl — correct, no loops, no redundant requests (the agent probed request counts), no stale-closure/leak issues, simplest form that satisfies the specs? Is anything over-engineered?
- The server decides every number/status/order; no client statistics or re-sorting; URL is the only filter state; user actions push history, normalisation replaces.
- Presentational components: inputs/outputs only, OnPush, no injected services; copy verbatim from §0.1 (`health-copy.ts`); accessibility (labels, th scope, symbol+text).
- `week.ts` date math in UTC/plain dates; `Intl` formatting; timezone for "Data as of".
- The agent's flagged choices: fixed C-20 error copy instead of ProblemDetails.title (§13 precedence); `DashboardState` provided in `DashboardPage.providers`; filters hidden until the first report; `<h1>Activity health</h1>` not in the copy table; empty "Usual range" cell for insufficient rows; "8 full weeks" from `report.baselineWeeks`. Judge each: fine, objective fix, or user decision.
- No `any`, no comment blocks, no dead code.
Verdict APPROVED / CHANGES REQUIRED; findings split (a) objective / (b) user decision. CLAUDE.md handoff format, concise.


**tool call — Bash**
```json
{"command":"git diff aca0fe5 78ee3ff --stat -- web/src/app; git diff aca0fe5 78ee3ff -- web/src/app | wc -l; ls .claude/agents docs"}
```


<details><summary>tool result</summary>

```
 web/src/app/core/api/accounts.api.ts               |   2 +-
 web/src/app/core/api/activity-health.api.ts        |   6 +-
 .../dashboard/components/account-summary.ts        |  28 +++-
 .../dashboard/components/dashboard-filters.ts      |  62 +++++++-
 .../dashboard/components/location-table.ts         |  47 +++++-
 web/src/app/features/dashboard/dashboard-state.ts  | 166 +++++++++++++++++++--
 web/src/app/features/dashboard/dashboard.page.ts   |  96 +++++++++++-
 web/src/app/features/dashboard/health-copy.ts      |  38 +++++
 web/src/app/features/dashboard/week.ts             |  61 ++++++++
 9 files changed, 477 insertions(+), 29 deletions(-)
     624
.claude/agents:
architect.md
backend.md
database.md
frontend.md
product.md
reviewer.md
statistician.md
test-author.md

docs:
acceptance-criteria.md
battle-test
design-consensus.md
handoff
```
</details>


**tool call — Bash**
```json
{"command":"git diff aca0fe5 78ee3ff -- web/src/app"}
```


<details><summary>tool result</summary>

```
diff --git a/web/src/app/core/api/accounts.api.ts b/web/src/app/core/api/accounts.api.ts
index 8759395..4c99a9d 100644
--- a/web/src/app/core/api/accounts.api.ts
+++ b/web/src/app/core/api/accounts.api.ts
@@ -12,6 +12,6 @@ export class HttpAccountsApi extends AccountsApi {
   private readonly http = inject(HttpClient);
 
   listAccounts(): Observable<Account[]> {
-    throw new Error('Not implemented');
+    return this.http.get<Account[]>('/api/accounts');
   }
 }
diff --git a/web/src/app/core/api/activity-health.api.ts b/web/src/app/core/api/activity-health.api.ts
index 64e1219..609e181 100644
--- a/web/src/app/core/api/activity-health.api.ts
+++ b/web/src/app/core/api/activity-health.api.ts
@@ -1,4 +1,4 @@
-import { HttpClient } from '@angular/common/http';
+import { HttpClient, HttpParams } from '@angular/common/http';
 import { Injectable, inject } from '@angular/core';
 import { Observable } from 'rxjs';
 import { ActivityHealthReport, EventType } from '../models';
@@ -18,6 +18,8 @@ export class HttpActivityHealthApi extends ActivityHealthApi {
   private readonly http = inject(HttpClient);
 
   getActivityHealth(request: ActivityHealthRequest): Observable<ActivityHealthReport> {
-    throw new Error('Not implemented');
+    const weekParams = request.week === null ? new HttpParams() : new HttpParams().set('week', request.week);
+    const params = weekParams.set('type', request.eventType);
+    return this.http.get<ActivityHealthReport>(`/api/accounts/${request.account …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cat .claude/agents/frontend.md; grep -n \"§13\\|^## 13\" -n PLAN.md | head; "}
```


<details><summary>tool result</summary>

```
---
name: frontend
description: Angular engineer. Use in Phase 0 to scaffold the Angular shell (CLI app, Vitest, dev proxy, dashboard route) and afterwards for any package.json/angular.json change; in Phase 2 to implement the dashboard (URL-backed DashboardState, HTTP API services, DashboardPage, DashboardFilters, AccountSummary, LocationTable) against the red Vitest suite using fake APIs, and in Phase 3 to wire it to the real API through the dev proxy and verify it end to end.
tools: Read, Grep, Glob, Write, Edit, Bash
model: opus
---
# Role
You build the one screen the admin opens on Monday morning. It must be **correct, clear and reload-proof** — function over form.
The server decides every number, status and the ranking order; the UI never recomputes or re-sorts business data.

# Read before any task
`CLAUDE.md`, `PLAN.md` §5.2 (JSON contract) and §5.4 (frontend design), the copy table from the product agent (`docs/acceptance-criteria.md`), the architect's contracts in `web/src/app/core`, and the red specs.

# Phase 0 — Angular shell (you own `package.json` and `angular.json` from now on)
Structure only; no contracts, no behaviour (the architect adds TS models and API tokens afterwards).
- `web/` via the current stable Angular CLI: standalone, routing, CSS, strict mode, no SSR, Vitest as the test runner.
- `proxy.conf.json` mapping `/api` → the API's dev URL (`http://localhost:5000` unless `src/Relay.Api` says otherwise); `npm start` uses it.
- Single route `dashboard` (d …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"sed -n 150,254p PLAN.md | grep -n \"5.4\" ; grep -n \"5.4\\|frontend\\|Frontend\\|404\\|C-20\\|timezone\\|Data as of\\|dataAsOf\\|8 full\\|insufficient\" PLAN.md | sed -n 1,80p"}
```


<details><summary>tool result</summary>

```
61:| Week start day | Monday (ISO), in the account's IANA timezone |
77:`GET /api/accounts` → `[{ id, name, timezone }]` (includes account 20).
85:| `accountId` | Unknown → 404 |
90:  "account": { "id": 6, "name": "Metro Collision Centers", "timezone": "America/New_York" },
93:  "dataAsOf": "2026-07-27T22:20:34Z",
101:`status ∈ above | below | normal | insufficient_data`. `baseline` is `null` when insufficient. `locations` is returned sorted (§5.3).
102:Empty account → 200, `summary.count = 0`, `insufficient_data`, `locations: []`.
109:3. Fewer than **4** eligible weeks → `insufficient_data` (count still shown).
115:8. Ranking: `insufficient_data` last; otherwise |deviation| descending, then location name ascending.
119:### 5.4 Frontend
173:| 5 Frontend | `web` (Vitest) | `DashboardState` URL round-trip + normalisation; `LocationTable`/`AccountSummary` states |
180:- Ranking: insufficient last; ties by name; above and below ranked by magnitude together.
182:- API: 404 unknown account; 400 non-Monday / future week / bad type; default week; empty account 200.
192:| Account 8, week 2026-03-02 | `insufficient_data` (3 eligible weeks) |
208:| `frontend` | Opus 5.5 | Angular state, services, components | Code |
225:Phase 2  parallel     backend → A green  ‖  database → B green  ‖  frontend → C green (against fake API)
227:Phase 3  sequential   test-author: API + golden red suite → backend: endpoints green → frontend wired to real API
237:1. Frontend component tests (keep the `Dashb …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"grep -n \"^## \\|^### \" docs/acceptance-criteria.md | head -40"}
```


<details><summary>tool result</summary>

```
7:## How to read this document
36:## 0. User-facing copy (verbatim; frontend copies these exactly)
38:### 0.1 Approved copy
76:### 0.2 Proposed copy
82:## 1. Business logic slice (`Relay.Core`, unit tests with no DB and no mocks)
86:### 1.1 Normality evaluator (§13 §5.3 steps 3–8)
104:### 1.2 Grid, eligibility and site list (§13 §5.3 steps 1–2; §7 Grid cases)
115:### 1.3 Ranking (§13 §5.3 step 9)
125:### 1.4 Calendar and service
146:## 2. Data slice
148:### 2.1 SQL queries (`Relay.Infrastructure.Tests`: SQL Server via Testcontainers, `InitialCreate` only, hand-built fixtures)
170:### 2.2 Seed load and starter files (`Relay.Api.Tests` against the real seed, plus the repo)
182:## 3. API slice (`Relay.Api`, integration + golden against the real seed)
186:### 3.1 Accounts
192:### 3.2 Activity health: happy paths (Q1 + Q2)
219:### 3.3 Empty account
228:### 3.4 Invalid input (all errors are `ProblemDetails`, `Content-Type: application/problem+json`)
246:## 4. Dashboard slice (`web/`, Angular)
251:### 4.1 Default view: Q1 and Q2 answered at a glance
263:### 4.2 Scenarios
282:### 4.3 URL state, reload and invalid params (§5.4, still in force)
305:## 5. README slice (`README.md`, Phase 3)
323:## 6. Coverage of the required scenarios
344:## Appendix A — Seed evidence
```
</details>


**tool call — Bash**
```json
{"command":"sed -n 36,80p docs/acceptance-criteria.md; sed -n 246,304p docs/acceptance-criteria.md; sed -n 119,135p PLAN.md; sed -n 338,366p PLAN.md; sed -n 445,530p PLAN.md"}
```


<details><summary>tool result</summary>

```
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
| C-07 | Emp …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd web && ls src/app src/app/features/dashboard src/app/core/models; cat src/app/app.routes.ts src/app/app.config.ts; grep -rn \"DashboardState\" src --include=*.spec.ts | grep -n \"provide\" | head; grep -rn \"toHaveBeenCalledTimes\\|callCount\\|requests\\b\" src/app/features/dashboard/*.spec.ts | head -20; grep -rnE \"\\bany\\b|/\\*|^\\s*//\" src/app --include=*.ts --exclude=*.spec.ts | head; grep -n \"baselineWeeks\" -r src/app/core"}
```


<details><summary>tool result</summary>

```
src/app:
app.config.ts
app.routes.ts
app.spec.ts
app.ts
core
features

src/app/core/models:
account.ts
activity-health-report.ts
baseline.ts
event-type.ts
health-status.ts
index.ts
location-health.ts
problem-details.ts
series-health.ts
week-range.ts

src/app/features/dashboard:
components
dashboard-state.spec.ts
dashboard-state.ts
dashboard.page.spec.ts
dashboard.page.ts
import { Routes } from '@angular/router';
import { DashboardPage } from './features/dashboard/dashboard.page';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
  { path: 'dashboard', component: DashboardPage },
];
import { provideHttpClient } from '@angular/common/http';
import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { AccountsApi, HttpAccountsApi } from './core/api/accounts.api';
import { ActivityHealthApi, HttpActivityHealthApi } from './core/api/activity-health.api';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(),
    { provide: ActivityHealthApi, useClass: HttpActivityHealthApi },
    { provide: AccountsApi, useClass: HttpAccountsApi },
  ],
};
(eval):1: no matches found: --include=*.spec.ts
src/app/features/dashboard/dashboard.page.spec.ts:432:      expect(activityHealthApi.requests).toHaveLength(2);
src/app/features/dashb …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd web && grep -rnE \"\\bany\\b|/\\*|^\\s*//\" src/app/features src/app/core | grep -v spec | head; sed -n 1,80p src/app/features/dashboard/dashboard-state.spec.ts; grep -n \"DashboardState\\|providers\" src/app/features/dashboard/dashboard.page.spec.ts | head -20; sed -n 415,440p src/app/features/dashboard/dashboard.page.spec.ts"}
```


<details><summary>tool result</summary>

```
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { AccountsApi } from '../../core/api/accounts.api';
import { ActivityHealthApi } from '../../core/api/activity-health.api';
import { EVENT_TYPES } from '../../core/models';
import {
  beaconDefaultWeekReport,
  beaconLeadsDefaultWeekReport,
  lakesideDefaultWeekReport,
  METRO_COLLISION_CENTERS,
  genericReport,
  metroSpikeWeekReport,
  quietHarborEmptyReport,
  seedAccounts,
} from '../../../testing/activity-health-fixtures';
import { FakeAccountsApi, FakeActivityHealthApi, networkFailure, serverError } from '../../../testing/fake-apis';
import { RecordedNavigation, currentPath, currentQueryParams, queryParamsOf, recordNavigations, settle } from '../../../testing/router-testing';
import { DEFAULT_ACCOUNT_ID, DEFAULT_EVENT_TYPE, DashboardState } from './dashboard-state';

@Component({
  selector: 'app-dashboard-route-stub',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: '',
})
class DashboardRouteStub {}

interface StateUnderTest {
  state: DashboardState;
  activityHealthApi: FakeActivityHealthApi;
  accountsApi: FakeAccountsApi;
  navigations: RecordedNavigation[];
  harness: RouterTestingHarness;
}

const DEFAULT_URL = '/dashboard?account=14&week=2026-07-20&type=all';
const DEFAULT_QUERY_PARAMS = { account: '14', we …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd web && grep -n \"toHaveLength(1)\\|toHaveLength(3)\" src/app/features/dashboard/*.spec.ts | head; git status --short | head; git log --oneline -1"}
```


<details><summary>tool result</summary>

```
src/app/features/dashboard/dashboard.page.spec.ts:256:      expect(rows).toHaveLength(1);
src/app/features/dashboard/dashboard-state.spec.ts:263:      expect(navigationsAfterOpening(navigations)).toHaveLength(1);
src/app/features/dashboard/dashboard-state.spec.ts:273:      expect(navigationsAfterOpening(navigations)).toHaveLength(1);
src/app/features/dashboard/dashboard-state.spec.ts:283:      expect(navigationsAfterOpening(navigations)).toHaveLength(1);
46fad89 Record DST-at-midnight out-of-scope ruling in PLAN §13
```
</details>


### 🤖 ASSISTANT — 2026-09-28T21:36:48.678Z

## Verdict: APPROVED

I read the diff against the spec text only. My worktree HEAD is 46fad89, not 78ee3ff, so I did not run the tests or the build. I rely on the coordinator's 179/179 green and clean build. I found no objective blockers.

## Findings

**(a) Objective**

- [Minor] `dashboard-state.ts` (`reportRequest` `equal`, `fetchReport`, `serverResolution`) — No spec pins this logic. A default `/dashboard` load requests `week=null`, resolves, then rewrites the URL to `week=2026-07-20`. Without the custom `equal`, that rewrite would trigger a duplicate request.
  - I traced the logic and it is correct. `requested === previous` holds because `fetchReport(params, params)` passes the very object the computed retained, and `tap` only fires for a request that was not superseded.
  - No spec asserts the request count. `grep toHaveLength` finds only the "Try again" count (`dashboard.page.spec.ts:432`) and the navigation counts. The reference-equality coupling could break silently in a refactor.
  - Fix: route a test to test-author. Default load, a Tuesday week, a 400 week and a 404 account should each make exactly one successful request (a failed attempt plus its fallback counts as one). Specs are frozen for me, so I only report this.
- [Minor] `dashboard-state.ts:96` — `accountsApi.listAccounts()` failure is swallowed to `[]`. The "Viewing as" select then renders empty with no error and no retry. Fix: leave as is, or surface it.
- [Nit] `dashboard-filters.ts` — `Number.isInteger(selectedAccountId)` in `onAccountChange` is a guard that cannot trip, because option values are ids from `accounts()`. Remove it.
- [Nit] `dashboard-state.ts` `isEventType` and `dashboard-filters.ts` `EVENT_TYPES.find` are two hand-written narrowings of the same list. Consolidate if convenient.
- [Nit] `dashboard.page.ts` — while a new report loads, the filters keep the previous report's `earliestWeek` and `latestCompleteWeek`. The stepper bounds can be briefly stale after an account switch. This is transient and harmless.

**(b) User decisions**

1. **Filters hidden until the first report loads** (`@if (lastLoadedReport())`). The bounds inputs are required, so this is defensible. The cost: if the first request fails, for example `?account=6` with a 500, the user sees only the C-20 message and "Try again" and cannot switch account or type. UI-40 only says "filters stay in the URL", so this is not a violation. Choose one:
   - accept it;
   - show the account and type selects without the stepper.
2. **`<h1>Activity health</h1>`** is not in the §0.1 copy table. It is harmless, does not hit the UI-04 forbidden words, and gives the page a heading. It is still a new user-facing string, so ask the user to approve it or remove it.
3. **Empty "Usual range" cell for insufficient rows.** This matches C-05 and UI-16 ("no range"). Accept as is, or ask for a placeholder such as "—". The dash is not in the copy table.

**Judged fine**

- **Fixed C-20 copy instead of `ProblemDetails.title`.** This is correct. PLAN §13 and C-20 supersede `frontend.md`. `state.error()` still exposes a `ProblemDetails`, but only its truthiness is used, and the `problemDetailsOf` cast is harmless.
- **`DashboardState` in `DashboardPage.providers`.** This is correct. It is `@Injectable()` with no `providedIn`, and it scopes the state per route activation. `app.config.ts` does not provide it, so the page must. It also cleans up with the component.
- **"8 full weeks" from `report.baselineWeeks`.** This is fine. It avoids a magic number and follows the same pattern as `minimumEligibleWeeks` for the "4". The wire value is 8, so the output matches C-08 and C-09.

## Verification of the focus areas

No hand-recomputed golden values apply. The frontend only prints server values, and I traced no arithmetic.

- **No effect loops.** Effect 1 (`rewriteInvalidParams`) fires only when the parsed params differ from the raw ones. After the rewrite they match, so it stops.
- **Effect 2 (`rewriteUrlToShownReport`) stops in the same way.** It is guarded by `status() === 'resolved'`. Resource state resets synchronously when the params change, so it cannot revert a user click to the old shown report. Once the URL matches the shown request it stops.
- **404 → account 14, then 400 → `week: null`, bounded chain.**
  - The 404 fallback is guarded by `accountId !== 14`, and the 400 fallback by `week !== null`. The chain has at most two hops.
  - This covers UI-32, UI-35, UI-36 and UI-39b.
  - The type is pre-validated client-side, so a 400 for a bad type cannot occur.
- **Navigation history.** User actions call `writeQueryParams(..., false)`, which pushes history. Both normalisations use `replaceUrl: true`. `week: null` with `merge` drops the param, so `week=` is never emitted (UI-38b).
- **No stale-closure or leak problems.**
  - The `rxResource` unsubscribes superseded streams, so `serverResolution` is written only by the live request.
  - The `toSignal` calls are made in the injection context.
  - `router.navigate` runs inside an effect with no signal writes in it.
- **`week.ts` date math.** Everything uses UTC dates, with a round-trip check for validity. `addWeeks` adds 7 days in UTC milliseconds, so DST cannot shift it. The string comparisons on ISO dates for the stepper bounds are valid.
- **"Data as of" formatting.**
  - `formatCalendarDay` renders the instant in `account.timezone` via `Intl`, giving "Mon Jul 27, 2026".
  - A `null` `dataAsOf` hides the line (UI-22).
  - An invalid timezone string would throw a `RangeError`. The API only emits valid IANA ids.
- **Server-decided numbers and order.** There are no client statistics and no re-sorting. `LocationTable` maps `locations()` in payload order and prints `low` and `high` as given. `isEmptyAccount` implements the specified trigger, `locations.length === 0 && baseline.weeksUsed === 0`.

## Clean areas

- Presentational components: inputs and outputs only, OnPush, no injected services.
- Copy: C-01 to C-06, C-08 to C-22 checked verbatim. Nouns handle singular and plural, and the en dash is used.
- Accessibility: labels wrap the selects, `th scope="col"` and `scope="row"` are present, symbol plus text is always shown, and the week label has `aria-live`.
- API services: `HttpClient` calls only. The `week` param is omitted when null, and the request type is always sent.
- No `any`, no comment blocks, no dead code.
- Scope: the diff touches only `web/src/app` implementation files. No specs, contracts or config changed.

## Uncertain

- I could not run the tests. The worktree is not on 78ee3ff, so this is a static review.
- I did not check request-count behaviour empirically. My trace of the reference-equality `equal` says it is correct, but see the untested-behaviour finding above.

## Next

- Merge as is, or after the user answers b1 to b3.
- Route the request-count spec to test-author before Phase 3.

Files reviewed:
- `web/src/app/features/dashboard/dashboard-state.ts`
- `web/src/app/features/dashboard/dashboard.page.ts`
- `web/src/app/features/dashboard/week.ts`
- `web/src/app/features/dashboard/health-copy.ts`
- `web/src/app/features/dashboard/components/*.ts`
- `web/src/app/core/api/*.api.ts`

