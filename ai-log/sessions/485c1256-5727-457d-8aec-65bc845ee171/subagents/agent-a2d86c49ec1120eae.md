
### 🧑 USER — 2026-09-28T19:59:39.991Z

Re-review the architect's Phase 0 contracts for DASH-247 after fixes. Read-only: edit nothing. Branch `worktree-agent-a9154856706d28a3b` (latest 24d51b2), worktree `/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a9154856706d28a3b`. Diff against main: `git diff main...worktree-agent-a9154856706d28a3b`. You may build/test in a scratch copy.

Your previous review raised A1–A6 (objective) and B1–B6 (decisions). Decisions are now recorded in PLAN §13 entry "Contract decisions from the Phase 0 contract review" (plus earlier §13 entries). Verify:
1. A1 empty `type=` → 400; A2 `InvalidWeekReason` enum; A3 calendar failure contract (`TimeZoneNotFoundException`, `NotAWeekStartException`); A4 no namespace cycles in Core (check every `using` across Calendar/Queries/Normality/ActivityHealth/Accounts); A5 `PrecedingWeeks` rename; A6 `HealthStatus` zero value + `AccountTotal` rename — each fixed.
2. The recorded decisions are reflected exactly: AddRelayCore in Api with plain `NormalityOptions`; Core-owned wire-name mapping (`ActivityTypeNames`, `HealthStatusNames`) used by the API validation pattern; strict `yyyy-MM-dd` week validation via attribute (prove `?week=abc` → 400 problem+json if you can run it); `DateTime` instants; nullable `dataAsOf` only (C#, TS); `TimeProvider` injected into `ActivityHealthService`.
3. Flag: the architect implemented `IsoDateAttribute.IsValid` (one TryParseExact) — judge whether that is acceptable contract-level validation or should move to backend.
4. Anything new: logic leaking into contracts, comment blocks, project files touched, JSON shape drift from §13 §5.2 (except nullable dataAsOf).

Verdict APPROVED / CHANGES REQUIRED; findings ranked, separated into (a) objective and (b) needing a user decision. CLAUDE.md handoff format.


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a9154856706d28a3b\" && git log --oneline -5 && git diff main...HEAD --stat && grep -n \"Contract decisions from the Phase 0\" -A60 PLAN.md | head -120"}
```


<details><summary>tool result</summary>

```
This agent is isolated in the worktree /Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a2d86c49ec1120eae, but this command changes directory to the shared checkout (/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a9154856706d28a3b) before running git. Refusing to run it — a worktree-isolated agent's git operations must target its own worktree. Run the equivalent from /Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a2d86c49ec1120eae without the redirect.
```
</details>


**tool call — Bash**
```json
{"command":"git log --oneline -6 worktree-agent-a9154856706d28a3b && git diff main...worktree-agent-a9154856706d28a3b --stat && grep -n \"Contract decisions from the Phase 0\" -A50 PLAN.md | head -100"}
```


<details><summary>tool result</summary>

```
24d51b2 Make dataAsOf nullable for the empty-database case
22eaf16 Merge branch 'main' into worktree-agent-a9154856706d28a3b
b0a2171 Record Phase 0 contract decisions and further promoted goldens
d5212e0 Apply user decisions on wire names, clock fallback and week validation
3289ea2 Apply reviewer contract fixes A1-A6
4b03ebe Merge branch 'main' into worktree-agent-a9154856706d28a3b
 .../Composition/ApiServiceCollectionExtensions.cs  | 11 +++++++
 .../Composition/CoreServiceCollectionExtensions.cs | 31 ++++++++++++++++++
 src/Relay.Api/Endpoints/AccountEndpoints.cs        | 19 +++++++++++
 src/Relay.Api/Endpoints/ActivityHealthEndpoints.cs | 20 ++++++++++++
 src/Relay.Api/Endpoints/RelayEndpoints.cs          |  9 ++++++
 src/Relay.Api/Http/AccountListItemExtensions.cs    |  9 ++++++
 src/Relay.Api/Http/AccountResponse.cs              |  3 ++
 .../Http/ActivityHealthReportExtensions.cs         |  9 ++++++
 src/Relay.Api/Http/ActivityHealthRequest.cs        | 10 ++++++
 .../Http/ActivityHealthRequestExtensions.cs        |  9 ++++++
 src/Relay.Api/Http/ActivityHealthResponse.cs       | 13 ++++++++
 .../Http/ActivityHealthResultExtensions.cs         | 10 ++++++
 src/Relay.Api/Http/BaselineResponse.cs             |  3 ++
 src/Relay.Api/Http/IsoDateAttribute.cs             | 20 ++++++++++++
 src/Relay.Api/Http/LocationHealthResponse.cs       |  8 +++++
 src/Relay.Api/Http/SummaryResponse.cs              |  3 ++
 src/Relay.Api/Http/WeekRangeResponse.cs            |  3 ++
 src/Relay.A …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"W=/Users/josemejia/Documents/Software\\ Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a9154856706d28a3b; cd \"$W\"; for f in $(git diff main...HEAD --name-only | grep -E '\\.cs$'); do echo \"=== $f\"; cat \"$f\"; done"}
```


<details><summary>tool result</summary>

```
This agent is isolated in the worktree /Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a2d86c49ec1120eae, but this command is too complex to verify that it stays inside the worktree. Refusing to run it — a worktree-isolated agent's git operations must target its own worktree. Split it into plain, separate commands and run them from /Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a2d86c49ec1120eae.
```
</details>


### 🤖 ASSISTANT — 2026-09-28T19:59:47.253Z

I'll use git show on the branch for content.


**tool call — Bash**
```json
{"command":"B=worktree-agent-a9154856706d28a3b; for f in $(git diff main...$B --name-only | grep -E '^src/.*\\.cs$'); do echo \"=== $f\"; git show $B:$f; done"}
```


<details><summary>tool result</summary>

```
This agent is isolated in the worktree /Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a2d86c49ec1120eae, but this command names git in a form too complex to verify that it stays inside the worktree. Refusing to run it — a worktree-isolated agent's git operations must target its own worktree. Split it into plain, separate commands and run them from /Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a2d86c49ec1120eae.
```
</details>


**tool call — Bash**
```json
{"command":"git diff main...worktree-agent-a9154856706d28a3b -- src"}
```


<details><summary>tool result</summary>

```
<persisted-output>
Output too large (32.9KB). Full output saved to: /Users/josemejia/.claude-alt/projects/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/485c1256-5727-457d-8aec-65bc845ee171/tool-results/bqkm5hftj.txt

Preview (first 2KB):
diff --git a/src/Relay.Api/Composition/ApiServiceCollectionExtensions.cs b/src/Relay.Api/Composition/ApiServiceCollectionExtensions.cs
new file mode 100644
index 0000000..e5ea04d
--- /dev/null
+++ b/src/Relay.Api/Composition/ApiServiceCollectionExtensions.cs
@@ -0,0 +1,11 @@
+namespace Relay.Api.Composition;
+
+public static class ApiServiceCollectionExtensions
+{
+    public static IServiceCollection AddRelayApi(this IServiceCollection services)
+    {
+        services.AddProblemDetails();
+        services.AddValidation();
+        return services;
+    }
+}
diff --git a/src/Relay.Api/Composition/CoreServiceCollectionExtensions.cs b/src/Relay.Api/Composition/CoreServiceCollectionExtensions.cs
new file mode 100644
index 0000000..d5ec98f
--- /dev/null
+++ b/src/Relay.Api/Composition/CoreServiceCollectionExtensions.cs
@@ -0,0 +1,31 @@
+using Microsoft.Extensions.Options;
+using Relay.Core.Accounts;
+using Relay.Core.ActivityHealth;
+using Relay.Core.Calendar;
+using Relay.Core.Normality;
+
+namespace Relay.Api.Composition;
+
+public static class CoreServiceCollectionExtensions
+{
+    public static IServiceCollection AddRelayCore(this IServiceCollection services, IConfiguration configuration)
+    {
+         …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Read**
```json
{"file_path":"/Users/josemejia/.claude-alt/projects/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/485c1256-5727-457d-8aec-65bc845ee171/tool-results/bqkm5hftj.txt"}
```


<details><summary>tool result</summary>

```
1	diff --git a/src/Relay.Api/Composition/ApiServiceCollectionExtensions.cs b/src/Relay.Api/Composition/ApiServiceCollectionExtensions.cs
2	new file mode 100644
3	index 0000000..e5ea04d
4	--- /dev/null
5	+++ b/src/Relay.Api/Composition/ApiServiceCollectionExtensions.cs
6	@@ -0,0 +1,11 @@
7	+namespace Relay.Api.Composition;
8	+
9	+public static class ApiServiceCollectionExtensions
10	+{
11	+    public static IServiceCollection AddRelayApi(this IServiceCollection services)
12	+    {
13	+        services.AddProblemDetails();
14	+        services.AddValidation();
15	+        return services;
16	+    }
17	+}
18	diff --git a/src/Relay.Api/Composition/CoreServiceCollectionExtensions.cs b/src/Relay.Api/Composition/CoreServiceCollectionExtensions.cs
19	new file mode 100644
20	index 0000000..d5ec98f
21	--- /dev/null
22	+++ b/src/Relay.Api/Composition/CoreServiceCollectionExtensions.cs
23	@@ -0,0 +1,31 @@
24	+using Microsoft.Extensions.Options;
25	+using Relay.Core.Accounts;
26	+using Relay.Core.ActivityHealth;
27	+using Relay.Core.Calendar;
28	+using Relay.Core.Normality;
29	+
30	+namespace Relay.Api.Composition;
31	+
32	+public static class CoreServiceCollectionExtensions
33	+{
34	+    public static IServiceCollection AddRelayCore(this IServiceCollection services, IConfiguration configuration)
35	+    {
36	+        services.AddOptions<NormalityOptions>()
37	+            .Bind(configuration.GetSection(NormalityOptions.SectionName))
38	+            .ValidateDataAnnotations()
39	+         …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"git diff main...worktree-agent-a9154856706d28a3b -- web | grep '^[+-]' | grep -v '^+++\\|^---'; git diff main...worktree-agent-a9154856706d28a3b --name-only | grep -Ei 'csproj|sln|props|package|angular|appsettings'; ls; ls src/Relay.Api; cat src/Relay.Api/appsettings*.json"}
```


<details><summary>tool result</summary>

```
+import { provideHttpClient } from '@angular/common/http';
+import { AccountsApi, HttpAccountsApi } from './core/api/accounts.api';
+import { ActivityHealthApi, HttpActivityHealthApi } from './core/api/activity-health.api';
-    provideRouter(routes)
-  ]
+    provideRouter(routes),
+    provideHttpClient(),
+    { provide: ActivityHealthApi, useClass: HttpActivityHealthApi },
+    { provide: AccountsApi, useClass: HttpAccountsApi },
+  ],
+import { HttpClient } from '@angular/common/http';
+import { Injectable, inject } from '@angular/core';
+import { Observable } from 'rxjs';
+import { Account } from '../models';
+
+export abstract class AccountsApi {
+  abstract listAccounts(): Observable<Account[]>;
+}
+
+@Injectable()
+export class HttpAccountsApi extends AccountsApi {
+  private readonly http = inject(HttpClient);
+
+  listAccounts(): Observable<Account[]> {
+    throw new Error('Not implemented');
+  }
+}
+import { HttpClient } from '@angular/common/http';
+import { Injectable, inject } from '@angular/core';
+import { Observable } from 'rxjs';
+import { ActivityHealthReport, EventType } from '../models';
+
+export interface ActivityHealthRequest {
+  accountId: number;
+  week: string | null;
+  eventType: EventType;
+}
+
+export abstract class ActivityHealthApi {
+  abstract getActivityHealth(request: ActivityHealthRequest): Observable<ActivityHealthReport>;
+}
+
+@Injectable()
+export class HttpActivityHealthApi extends ActivityHealthApi {
+  private readonly http =  …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"git diff main...worktree-agent-a9154856706d28a3b -- PLAN.md | head; grep -n \"dataAsOf\\|invalid_week\\|instantUtc\\|TimeZoneNotFound\\|NotAWeekStart\\|Wire\\|wire\" PLAN.md | head -40"}
```


<details><summary>tool result</summary>

```
93:  "dataAsOf": "2026-07-27T22:20:34Z",
159:1. SOLID, no tight coupling. Every class with behaviour depends on interfaces and is wired via DI. Plain data is exempt.
227:Phase 3  sequential   test-author: API + golden red suite → backend: endpoints green → frontend wired to real API
301:  "dataAsOf": "2026-07-27T22:20:34Z",
362:  - "Data as of Mon Jul 27, 2026", with `dataAsOf` rendered in the account timezone.
464:- **Wire names owned by Core:** one mapping for `ActivityType` (`all|call_received|lead_created|appointment_set`) and `HealthStatus` (`above|below|normal|insufficient_data`),
468:- **Instants stay `DateTime` (UTC)** in Core and the API response, serialised as `…Z` exactly as §13 §5.2; an API golden test asserts the exact `dataAsOf` string. (`DateTimeOffset` was rejected: it serialises as `+00:00`.)
469:- **Empty database (no events at all):** 200 empty state; `dataAsOf` is **null** (the UI hides "Data as of"); the latest complete week falls back to the current clock via an injected
470:  `TimeProvider`. D1 still holds whenever data exists. Contract change: `dataAsOf` nullable in C#, TypeScript and §5.2 for this case only.
```
</details>


**tool call — Bash**
```json
{"command":"S=/private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/485c1256-5727-457d-8aec-65bc845ee171/scratchpad; rm -rf $S/wt; mkdir -p $S/wt; git archive worktree-agent-a9154856706d28a3b | tar -x -C $S/wt; cd $S/wt && dotnet build Relay.sln 2>&1 | tail -15","timeout":400000}
```


<details><summary>tool result</summary>

```
  Restored /private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/485c1256-5727-457d-8aec-65bc845ee171/scratchpad/wt/src/Relay.Api/Relay.Api.csproj (in 261 ms).
  Restored /private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/485c1256-5727-457d-8aec-65bc845ee171/scratchpad/wt/tests/Relay.Api.Tests/Relay.Api.Tests.csproj (in 266 ms).
  Restored /private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/485c1256-5727-457d-8aec-65bc845ee171/scratchpad/wt/src/Relay.Infrastructure/Relay.Infrastructure.csproj (in 287 ms).
  Relay.Core -> /private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/485c1256-5727-457d-8aec-65bc845ee171/scratchpad/wt/src/Relay.Core/bin/Debug/net10.0/Relay.Core.dll
  Relay.Infrastructure -> /private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/485c1256-5727-457d-8aec-65bc845ee171/scratchpad/wt/src/Relay.Infrastructure/bin/Debug/net10.0/Relay.Infrastructure.dll
  Relay.Core.Tests -> /private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/485c1256-5727-457d-8aec-65bc845ee171/scratchpad/wt/tests/Relay.Core.Tests/bin/Debug/net10.0/Relay.Core.Tests.dll
  Relay.Infrastructure.Tests -> /private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activit …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd /private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/485c1256-5727-457d-8aec-65bc845ee171/scratchpad/wt && (ConnectionStrings__Relay=\"Server=localhost,1;Database=x;User Id=sa;Password=x;TrustServerCertificate=true\" ASPNETCORE_URLS=http://localhost:5187 dotnet run --no-build --project src/Relay.Api > api.log 2>&1 &) ; sleep 6; for q in \"week=abc\" \"type=\" \"type=bogus\" \"type=all&week=2026-7-20\" \"week=\" \"type=ALL\" \"week=2026-07-20&type=all\" \"\"; do echo \"--- ?$q\"; curl -s -i \"http://localhost:5187/api/accounts/14/activity-health?$q\" | sed -n '1p;/content-type/Ip;$p' | cut -c1-400; done","timeout":60000}
```


<details><summary>tool result</summary>

```
This agent is isolated in the worktree /Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a2d86c49ec1120eae, but this command runs dotnet with the text Server=localhost,1;Database=x;User… inside a construct too complex to verify, so what it runs cannot be shown not to be git. Refusing to run it — a worktree-isolated agent's git operations must target its own worktree. Split it into plain, separate commands and run them from /Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a2d86c49ec1120eae.
```
</details>


**tool call — Bash**
```json
{"command":"export ConnectionStrings__Relay=\"Server=localhost,1;Database=x;User Id=sa;Password=x;TrustServerCertificate=true\"; export ASPNETCORE_URLS=http://localhost:5187; dotnet run --no-build --project /private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/485c1256-5727-457d-8aec-65bc845ee171/scratchpad/wt/src/Relay.Api > /private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/485c1256-5727-457d-8aec-65bc845ee171/scratchpad/api.log 2>&1 &\nsleep 7; echo started","run_in_background":true}
```


<details><summary>tool result</summary>

```
Command running in background with ID: b64b7osgf. Output is being written to: /private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/485c1256-5727-457d-8aec-65bc845ee171/tasks/b64b7osgf.output. You will be notified when it completes. To check interim output, use Read on that file path.
```
</details>


**tool call — Bash**
```json
{"command":"sleep 5; for q in \"week=abc\" \"type=\" \"type=bogus\" \"type=all&week=2026-7-20\" \"week=\" \"type=ALL\" \"week=2026-07-20&type=all\" \"week=2026-02-30\"; do echo \"--- ?$q\"; curl -s -i \"http://localhost:5187/api/accounts/14/activity-health?$q\" | grep -iE '^HTTP|content-type|^\\{' | cut -c1-350; done"}
```


<details><summary>tool result</summary>

```
--- ?week=abc
--- ?type=
--- ?type=bogus
--- ?type=all&week=2026-7-20
--- ?week=
--- ?type=ALL
--- ?week=2026-07-20&type=all
--- ?week=2026-02-30
```
</details>


**tool call — Bash**
```json
{"command":"cat /private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/485c1256-5727-457d-8aec-65bc845ee171/scratchpad/api.log | head -20"}
```


<details><summary>tool result</summary>

```
Using launch settings from /private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/485c1256-5727-457d-8aec-65bc845ee171/scratchpad/wt/src/Relay.Api/Properties/launchSettings.json...
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5080
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Development
info: Microsoft.Hosting.Lifetime[0]
      Content root path: /private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/485c1256-5727-457d-8aec-65bc845ee171/scratchpad/wt/src/Relay.Api
```
</details>


### 🤖 ASSISTANT — 2026-09-28T20:00:25.033Z

Launch settings override the port (5080). Retry there.


**tool call — Bash**
```json
{"command":"for q in \"week=abc\" \"type=\" \"type=bogus\" \"type=all&week=2026-7-20\" \"week=\" \"type=ALL\" \"week=2026-07-20&type=all\" \"week=2026-02-30\"; do echo \"--- ?$q\"; curl -s -i -m 20 \"http://localhost:5080/api/accounts/14/activity-health?$q\" | grep -iE '^HTTP|content-type|^\\{' | cut -c1-350; done","timeout":200000}
```


<details><summary>tool result</summary>

```
--- ?week=abc
HTTP/1.1 400 Bad Request
Content-Type: application/problem+json
{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"Week":["The field Week must be a date in yyyy-MM-dd format."]},"traceId":"00-63572c490c2d8d8e06b2e55e26b048ae-33c0251d86e125da-00"}
--- ?type=
HTTP/1.1 400 Bad Request
Content-Type: application/problem+json
{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"Type":["The field Type must be a string or array type with a minimum length of '1'."]},"traceId":"00-867b307c51239ebff619c7e82a31c2d3-e59e39101fd3d413-00"}
--- ?type=bogus
HTTP/1.1 400 Bad Request
Content-Type: application/problem+json
{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"Type":["The field Type must match the regular expression '^(all|call_received|lead_created|appointment_set)$'."]},"traceId":"00-3d01beeaa05d3ea39fedfc49565e73e3-b29ebf80d1045e2b-00"}
--- ?type=all&week=2026-7-20
HTTP/1.1 400 Bad Request
Content-Type: application/problem+json
{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"Week":["The field Week must be a date in yyyy-MM-dd format."]},"traceId":"00-9a0cd281818c5d0747e9f3c54120d33f-43eee2468ec8ced1-00"}
--- ?week=
HTTP/1.1 400 …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"pkill -f \"Relay.Api\" ; grep -n \"errors\\|400\\|problem\" PLAN.md | head -30"}
```


<details><summary>tool result</summary>

```
39:| ~400 NULL outcomes, 313 NULL call durations, missed calls with durations | Irrelevant to counts; noted, and a reason rates are deferred |
83:| `week` | Optional local Monday. Default = latest complete week. Not a Monday → 400. After latest complete week → 400 |
84:| `type` | `all` (default) \| `call_received` \| `lead_created` \| `appointment_set`; else 400 |
182:- API: 404 unknown account; 400 non-Monday / future week / bad type; default week; empty account 200.
243:- Outcome rates (missed-call rate, lead conversion, no-show) — more actionable, but worse small-number problem and NULL-outcome decisions. First "another day" item.
291:| `week` | Optional local Monday. Default = latest complete week. Not a Monday → 400. After `latestCompleteWeek` → 400. Before `earliestWeek` → 400 |
292:| `type` | Exactly `all` (default) \| `call_received` \| `lead_created` \| `appointment_set`, case-sensitive; else 400 |
377:| Account 20 | default week → 200 empty state, `earliestWeek` = `latestCompleteWeek` = 2026-07-20; `week=2026-03-02` → 400 |
409:- 400 for a week before `earliestWeek`.
410:- 400 for `type=ALL` / `Call_Received` (case-sensitive).
411:- Account 20: default week → 200 empty with `earliestWeek` 2026-07-20; `week=2026-03-02` → 400.
450:- **API input:** malformed `week` (not `yyyy-MM-dd`, e.g. `2026-13-01`, `20260720`, `abc`) → 400 `ProblemDetails`; non-numeric `accountId` → 404 (route constraint `{accountId:int}`).
452:- **Account switch:** week and type are kept; if the A …[clipped in markdown; full text in raw/]
```
</details>


### 🤖 ASSISTANT — 2026-09-28T20:00:51.197Z

## Summary
Re-reviewed branch `worktree-agent-a9154856706d28a3b` (24d51b2) against main. A1–A6 are fixed and the §13 decisions are reflected. I built it and ran the API in a scratch copy. There are no blockers, but two objective items should be fixed or written down before the red test suite is built on these contracts.

## Changed
Nothing. Read-only. The scratch copy is in the scratchpad `wt/` folder.

## Verification
- `git archive` of the branch into scratch, then `dotnet build Relay.sln`: Build succeeded, 0 warnings, 0 errors.
- I ran the API with a dummy connection string and curled `GET /api/accounts/14/activity-health`. The launch settings override the port to 5080.

| Query | Result |
|---|---|
| `week=abc` | 400 `application/problem+json`, `errors.Week: "must be a date in yyyy-MM-dd format"` |
| `type=` | 400, `errors.Type: minimum length of '1'` |
| `type=bogus` | 400, regex message |
| `type=ALL` | 400, regex message |
| `week=2026-7-20` | 400 |
| `week=2026-02-30` | 400 |
| `week=` | 400 |
| `week=2026-07-20&type=all` | 500 problem+json, no exception details (the handler is still a stub) |

- `git diff --name-only` filtered for csproj/sln/props/package/angular/appsettings: nothing touched.
- `git diff` for any comment lines (`//`, `/*`, `///`): none.

## Fix verification (1)
- **A1:** OK. `[MinLength(1), RegularExpression(ExactMatchPattern)]` gives 400 for `type=`, proven by the run. `RegularExpression` alone would have passed an empty string.
- **A2:** OK. `InvalidWeek(InvalidWeekReason)` with `NotAWeekStart`, `AfterLatestCompleteWeek` and `BeforeEarliestWeek`.
- **A3:** OK as a contract, with one gap.
  - `NotAWeekStartException : ArgumentException` exists with a `LocalDate` property.
  - `TimeZoneNotFoundException` is the BCL type.
  - The failure contract is not written in any test or in PLAN, so it is discoverable only by reading the exception type. Comments are forbidden, so the red suite must pin it.
- **A4:** OK. The `using` graph is acyclic:
  - Calendar depends on nothing.
  - Queries depends on Calendar (`WeekWindow`).
  - Normality depends on Calendar and Queries.
  - Accounts depends on Queries.
  - ActivityHealth depends on Accounts-free Queries, Normality and Calendar.
  - Nothing points back.
- **A5:** OK. `WeeklySeries(SelectedWeekCount, PrecedingWeeks)`.
- **A6:** OK. `HealthStatus.InsufficientData` is the zero value. `AccountTotal` replaces the old name.

## Decisions reflected (2)
- `AddRelayCore` lives in `Relay.Api/Composition`, with plain `NormalityOptions` registered as a singleton from `IOptions`. Core takes no package references, and validation runs on start. OK.
- `ActivityTypeNames` and `HealthStatusNames` are in Core. The `type` regex is built from the constants, and `ToResponse` is stubbed against them. OK.
- Strict `yyyy-MM-dd` week validation is proven above.
- Instants are `DateTime`.
- `dataAsOf` is nullable in C# (`DateTime? DataAsOfUtc`, `DateTime? DataAsOf`) and TypeScript (`string | null`).
- `TimeProvider` is injected into `ActivityHealthService`, and `TimeProvider.System` is registered.

## Findings

### (a) Objective
1. **[Major] `dataAsOf` "Z" is not guaranteed by the contract.**
   - Evidence: `WeekWindow`, `SiteFirstActivity` and `ActivityHealthResponse.DataAsOf` are plain `DateTime`, and `FindDataAnchorAsync` returns `DateTime?`.
   - Failing input: a value read via `SqlQuery<DateTime>` has `Kind=Unspecified`, and System.Text.Json then emits `2026-07-27T22:20:34` with no `Z`. That breaks §5.2.
   - Fix: the red suite must include an API golden test asserting the exact `"…Z"` string. That test is already promised in §13. The implementer must apply `DateTime.SpecifyKind(…, Utc)` once, at the Infrastructure boundary or in `ToResponse`.
   - Not a contract-shape defect, but it is a known trap.
2. **[Minor] PLAN §5.2 (around line 301) is not updated for the nullable `dataAsOf`.** The §13 entry says it is "nullable in §5.2", but the example and field table are unchanged. Update the PLAN (governance), or the spec and code will disagree.
3. **[Minor] The calendar failure contract is not pinned anywhere.**
   - `IWeekCalendar` promises `TimeZoneNotFoundException` and `NotAWeekStartException`, but only the exception class exists.
   - The test suite must assert both. Also assert that the service maps `NotAWeekStart` before calling the calendar with a non-Monday.
4. **[Nit] Validation error keys are PascalCase (`Week`, `Type`) while the wire names are camelCase.** This is a `ProblemDetails.errors` key concern only, and the frontend's `ProblemDetails.errors` is a free-form record, so it is harmless. Pin the actual behaviour in the API tests, not an assumed `week`.

### (b) Needs a user decision
1. **Judgement on `IsoDateAttribute.IsValid`: acceptable, keep it in the contract layer.**
   - It is a single `TryParseExact` on a syntactic format. It has no calendar knowledge; Monday, future and earliest-week checks remain in Core (`InvalidWeekReason`). It is what gives the proven 400 through `AddValidation`.
   - Moving it to backend would just re-create the same attribute.
   - Ask for two things: (i) the backend `ToQuery` must reuse `IsoDateAttribute.Format` rather than a second literal, and (ii) a unit test on the attribute in the red suite (null, `abc`, `2026-7-20`, `2026-02-30`, `20260720`).
2. **`?week=` (present but empty) returns 400.** `?type=` is deliberately 400 (A1), but the PLAN has no rule for an empty `week`. The Angular URL layer might emit `week=` for "latest". Decide whether empty `week` means default or 400. Currently it is 400, which is consistent with `type=`.

## Verdict: APPROVED
The objective items (a1–a3) are for the test-author and implementer, not a redo of the contracts. Item a2 is a PLAN edit for the coordinator. The decisions in (b) do not block the red suite, but (b2) should be settled before the API tests are written.

## Next
- The test-author should include the exact `dataAsOf` "Z" golden, the empty and malformed `week` / `type` cases, and the two calendar exception assertions.
- The coordinator should update PLAN §5.2 for the nullable `dataAsOf`.

