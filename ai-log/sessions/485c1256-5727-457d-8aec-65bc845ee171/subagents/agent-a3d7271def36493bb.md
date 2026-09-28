
### 🧑 USER — 2026-09-28T20:30:27.412Z

The DASH-247 contracts were simplified (branch `worktree-agent-a111a8b2c782a44df`, commit 267b1c6 — read with `git show`/`git diff main...worktree-agent-a111a8b2c782a44df`). The JSON is unchanged, but type names changed: `AccountTotal` and `BaselineAssessment` are gone (now `SeriesHealth(Count, Baseline, Status, Deviation)`, `Baseline(WeeksUsed, Median, Low, High)`, `LocationHealth : SeriesHealth`), `ActivityHealthQuery` dropped (`GetAsync(accountId, week, eventType, ct)`), `WeeklySeries(SelectedWeekCount, EligibleWeekCounts)` (no `SeriesWeek`/`IsEligible`), Core `ActivityTypeNames`/`HealthStatusNames` removed (enums serialised by a snake_case JSON converter), API DTOs removed (Core records serialised directly; rounding at the API boundary).

In this worktree, update only `docs/acceptance-criteria.md` so no criterion names a removed type or member (search for AccountTotal, BaselineAssessment, ActivityHealthQuery, SeriesWeek, IsEligible, HealthStatusNames, ActivityTypeNames in Core, *Response DTO names). Keep behaviour and values unchanged. Also add one API criterion: JSON numbers are compared numerically (e.g. `deviation` 1.30 in PLAN is serialised as `1.3`), and `locations[]` property order is location, count, baseline, status, deviation. Cite PLAN §13 entry "Contract simplification (/simplify)" (the coordinator is adding it). Commit with the Co-Authored-By trailer; never touch the main worktree. Reply with commit id, worktree path and branch.


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a3d7271def36493bb\" && git branch --show-current && git log --oneline -3 && git show --stat 267b1c6 | head -60 && grep -nE \"AccountTotal|BaselineAssessment|ActivityHealthQuery|SeriesWeek|IsEligible|HealthStatusNames|ActivityTypeNames|Response\\b|[A-Za-z]+Response|Dto\" docs/acceptance-criteria.md","description":"Inspect branch, commit, and matches"}
```


<details><summary>tool result</summary>

```
worktree-agent-a3d7271def36493bb
036d4e7 Merge worktree-agent-a73b2ea8c833f1f05 (calendar simplification) into main
8fbd559 Merge worktree-agent-ac9381cc42ed979cb (calendar simplification) into main
282d926 Merge governance/calendar (calendar simplification) into main
commit 267b1c655afefe3c8ac0cbfb0d62ab60fb067870
Author: Jose Mejia <<redacted>>
Date:   Mon Sep 28 21:29:19 2026 +0100

    Simplify Phase 0 contracts: serialise Core records, snake_case enum converter
    
    Replace Core wire-name maps with JsonStringEnumConverter(SnakeCaseLower) in the API
    and a strict ordinal type-name validator derived from the enum. Reshape Core records to
    the section 13 5.2 JSON (WeekRange, Baseline, SeriesHealth/LocationHealth, DataAsOf,
    Timezone) and delete the six response DTOs and three mappers. WeeklySeries carries the
    selected count and eligible counts only; ActivityHealthQuery dropped; endpoints merged.
    
    Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>

 .../Composition/ApiServiceCollectionExtensions.cs  |  6 +++++
 src/Relay.Api/Endpoints/AccountEndpoints.cs        | 19 ---------------
 src/Relay.Api/Endpoints/ActivityHealthEndpoints.cs | 20 ----------------
 src/Relay.Api/Endpoints/RelayEndpoints.cs          | 27 ++++++++++++++++++----
 src/Relay.Api/Http/AccountListItemExtensions.cs    |  9 --------
 src/Relay.Api/Http/AccountResponse.cs              |  3 ---
 .../Http/ActivityHealthReportExtensions.cs         |  2 +-
 src/Relay.Api/Http/ActivityH …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a3d7271def36493bb\" && wc -l docs/acceptance-criteria.md && grep -nE \"AccountTotal|BaselineAssessment|ActivityHealthQuery|SeriesWeek|IsEligible|HealthStatusNames|ActivityTypeNames|Response|Dto|DTO|WeeklySeries|SeriesHealth|LocationHealth|GetAsync|Relay\\.Core|mapper|Mapper\" docs/acceptance-criteria.md","description":"Search criteria doc for type names"}
```


<details><summary>tool result</summary>

```
     391 docs/acceptance-criteria.md
81:## 1. Business logic slice (`Relay.Core`, unit tests with no DB and no mocks)
83:How to verify: run `dotnet test tests/Relay.Core.Tests` and read the test sources. Each criterion below needs a test with these exact inputs and expected values. A matching test name is not enough.
240:| API-48 | API-boundary mapper unit test with constructed deviations 0.125 and −0.125 (exactly representable binary midpoints) | Emits 0.13 and −0.13 (2 dp, `MidpointRounding.AwayFromZero`). Core keeps full precision; rounding happens only in the mapper. No seed value sits on a midpoint (consensus §3), so constructed values are required | SPEC §13 §5.2, §5.3.8 |
```
</details>


**tool call — Read**
```json
{"file_path":"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a3d7271def36493bb/docs/acceptance-criteria.md"}
```


<details><summary>tool result</summary>

```
1	# DASH-247 — Acceptance criteria
2	
3	Owner: `product`. Written in Phase 0, before any product code.
4	User: a customer admin of one Relay account, on Monday morning, who has to act on what they see. Every criterion here serves one of two questions:
5	**Q1** "Is this number normal for us?" or **Q2** "Which of my locations needs attention?"
6	
7	## How to read this document
8	
9	**Precedence.** `PLAN.md` §13 has several entries dated 2026-09-28. These apply here:
10	- **"Revised design from the four-agent debate (approved by the user)"** overrides §5.1 (index bullet), §5.2, §5.3, the §5.4 status labels and footnote, the §7 Evaluator/Ranking/API bullets, the §7 golden table, D5 and D7.
11	- **"Phase 0 decisions and promoted golden values (user decisions)"** adds ten golden scenarios and fixes the API port at 5080.
12	- **"Input handling and UI copy decisions (user decisions, validated by the architect)"** decides malformed input, URL normalisation, account switching, the empty and error states, and the additional copy. It is cited below as §13 "Input handling and UI copy".
13	- **"Contract decisions from the Phase 0 contract review (user decisions, validated by the architect)"** covers the empty database (`dataAsOf` null), the exact `dataAsOf` string, more malformed-week variants, the capitalised account method line, and more promoted goldens. It is cited below as §13 "Contract decisions".
14	- **"Last Phase 0 clarifications (user decisions, validated by the architect)"** cov …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a3d7271def36493bb\" && git diff main...worktree-agent-a111a8b2c782a44df -- src/Relay.Core src/Relay.Api/Http/ActivityHealthReportExtensions.cs src/Relay.Api/Http/ActivityHealthResultExtensions.cs src/Relay.Api/Http/ActivityTypeNames.cs src/Relay.Api/Composition | head -250; grep -n \"simplify\\|Contract simplification\" PLAN.md | head","description":"Show Core and API contract diff"}
```


<details><summary>tool result</summary>

```
diff --git a/src/Relay.Api/Composition/ApiServiceCollectionExtensions.cs b/src/Relay.Api/Composition/ApiServiceCollectionExtensions.cs
index e5ea04d..5c7959b 100644
--- a/src/Relay.Api/Composition/ApiServiceCollectionExtensions.cs
+++ b/src/Relay.Api/Composition/ApiServiceCollectionExtensions.cs
@@ -1,3 +1,6 @@
+using System.Text.Json;
+using System.Text.Json.Serialization;
+
 namespace Relay.Api.Composition;
 
 public static class ApiServiceCollectionExtensions
@@ -6,6 +9,9 @@ public static class ApiServiceCollectionExtensions
     {
         services.AddProblemDetails();
         services.AddValidation();
+        services.ConfigureHttpJsonOptions(options =>
+            options.SerializerOptions.Converters.Add(
+                new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower, allowIntegerValues: false)));
         return services;
     }
 }
diff --git a/src/Relay.Api/Http/ActivityHealthReportExtensions.cs b/src/Relay.Api/Http/ActivityHealthReportExtensions.cs
index b63ec77..21c1965 100644
--- a/src/Relay.Api/Http/ActivityHealthReportExtensions.cs
+++ b/src/Relay.Api/Http/ActivityHealthReportExtensions.cs
@@ -4,6 +4,6 @@ namespace Relay.Api.Http;
 
 public static class ActivityHealthReportExtensions
 {
-    public static ActivityHealthResponse ToResponse(this ActivityHealthReport report) =>
+    public static ActivityHealthReport WithDisplayDeviations(this ActivityHealthReport report) =>
         throw new NotImplementedException();
 }
diff --git a/src/Relay.Api …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a3d7271def36493bb\" && git diff main...worktree-agent-a111a8b2c782a44df -- src/Relay.Core/Normality/WeeklySeries.cs src/Relay.Api/Endpoints/RelayEndpoints.cs; git show worktree-agent-a111a8b2c782a44df:src/Relay.Core/ActivityHealth/ActivityHealthResult.cs 2>/dev/null || git ls-tree -r --name-only worktree-agent-a111a8b2c782a44df src/Relay.Core | grep -i result; grep -rn \"NotAWeekStart\\|InvalidWeek\" $(git ls-tree -r --name-only worktree-agent-a111a8b2c782a44df src/Relay.Core | sed 's/^/x/;s/^x//') 2>/dev/null | head; grep -n \"Contract simplification\\|/simplify\" PLAN.md","description":"Check remaining contract details"}
```


<details><summary>tool result</summary>

```
This agent is isolated in the worktree /Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a3d7271def36493bb, but this command names git in a form too complex to verify that it stays inside the worktree. Refusing to run it — a worktree-isolated agent's git operations must target its own worktree. Split it into plain, separate commands and run them from /Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a3d7271def36493bb.
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a3d7271def36493bb\" && git diff main...worktree-agent-a111a8b2c782a44df -- src/Relay.Core/Normality/WeeklySeries.cs src/Relay.Api/Endpoints/RelayEndpoints.cs src/Relay.Core/ActivityHealth/ActivityHealthResult.cs","description":"Diff WeeklySeries and endpoints"}
```


<details><summary>tool result</summary>

```
diff --git a/src/Relay.Api/Endpoints/RelayEndpoints.cs b/src/Relay.Api/Endpoints/RelayEndpoints.cs
index 2fc6ba8..f369c34 100644
--- a/src/Relay.Api/Endpoints/RelayEndpoints.cs
+++ b/src/Relay.Api/Endpoints/RelayEndpoints.cs
@@ -1,9 +1,28 @@
+using Microsoft.AspNetCore.Http.HttpResults;
+using Relay.Api.Http;
+using Relay.Core.Accounts;
+using Relay.Core.ActivityHealth;
+using Relay.Core.Queries;
+
 namespace Relay.Api.Endpoints;
 
 public static class RelayEndpoints
 {
-    public static IEndpointRouteBuilder MapRelayEndpoints(this IEndpointRouteBuilder routes) =>
-        routes
-            .MapAccountEndpoints()
-            .MapActivityHealthEndpoints();
+    public static IEndpointRouteBuilder MapRelayEndpoints(this IEndpointRouteBuilder routes)
+    {
+        routes.MapGet("/api/accounts", ListAccounts);
+        routes.MapGet("/api/accounts/{accountId:int}/activity-health", GetActivityHealth);
+        return routes;
+    }
+
+    private static Task<Ok<IReadOnlyList<AccountListItem>>> ListAccounts(
+        IAccountService accountService,
+        CancellationToken cancellationToken) =>
+        throw new NotImplementedException();
+
+    private static Task<Results<Ok<ActivityHealthReport>, ProblemHttpResult>> GetActivityHealth(
+        [AsParameters] ActivityHealthRequest request,
+        IActivityHealthService activityHealthService,
+        CancellationToken cancellationToken) =>
+        throw new NotImplementedException();
 }
diff --git a/src/Relay.Core/Norm …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a3d7271def36493bb\" && git grep -n \"NotAWeekStart\\|InvalidWeek\\|AccountNotFound\" worktree-agent-a111a8b2c782a44df -- src/Relay.Core | head","description":"Check result error names still exist"}
```


<details><summary>tool result</summary>

```
worktree-agent-a111a8b2c782a44df:src/Relay.Core/ActivityHealth/ActivityHealthResult.cs:11:    public sealed record AccountNotFound : ActivityHealthResult;
worktree-agent-a111a8b2c782a44df:src/Relay.Core/ActivityHealth/ActivityHealthResult.cs:13:    public sealed record InvalidWeek(InvalidWeekReason Reason) : ActivityHealthResult;
worktree-agent-a111a8b2c782a44df:src/Relay.Core/ActivityHealth/InvalidWeekReason.cs:3:public enum InvalidWeekReason
worktree-agent-a111a8b2c782a44df:src/Relay.Core/ActivityHealth/InvalidWeekReason.cs:5:    NotAWeekStart,
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a3d7271def36493bb\" && grep -n '\"median\"\\|\"deviation\"\\|\"location\"' PLAN.md | head -20","description":"Check PLAN JSON example number formats"}
```


<details><summary>tool result</summary>

```
97:  "summary": { "count": 87, "baseline": { "median": 72.5, "low": 24, "high": 121, "weeksUsed": 8 }, "status": "normal", "deviation": 0.59 },
98:  "locations": [ { "location": "Site M", "count": 7, "baseline": { … }, "status": "normal", "deviation": 1.57 } ]
306:  "summary": { "count": 87, "baseline": { "weeksUsed": 8, "median": 72.5, "low": 30, "high": 134 }, "status": "normal", "deviation": 0.53 },
307:  "locations": [ { "location": "Site M", "count": 7, "baseline": { "weeksUsed": 8, "median": 3.5, "low": 1, "high": 9 }, "status": "normal", "deviation": 1.30 } ]
```
</details>


### 🤖 ASSISTANT — 2026-09-28T20:31:04.231Z

Only API-48 still refers to a removed item (the deleted mapper). I'll update it, add the precedence entry, and add the new API criterion.


**tool call — Edit**
```json
{"replace_all":false,"file_path":"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a3d7271def36493bb/docs/acceptance-criteria.md","old_string":"It is cited below as §13 \"Calendar contract simplified\".\n","new_string":"It is cited below as §13 \"Calendar contract simplified\".\n- **\"Contract simplification (/simplify)\"** keeps the §13 §5.2 JSON unchanged but serialises the Core report records directly (no separate API response types) with a snake_case enum converter, and rounds `deviation` at the API boundary. It is cited below as §13 \"Contract simplification\".\n"}
```


<details><summary>tool result</summary>

```
The file /Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a3d7271def36493bb/docs/acceptance-criteria.md has been updated successfully. (file state is current in your context — no need to Read it back)
```
</details>


**tool call — Edit**
```json
{"replace_all":false,"file_path":"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a3d7271def36493bb/docs/acceptance-criteria.md","old_string":"| API-01d | `…/accounts/14/activity-health?week=2026-07-20` with **no** `type` | Same body as `type=all`; `eventType` `all` | SPEC §13 §5.2 (default `all`) |","new_string":"| API-01d | `…/accounts/14/activity-health?week=2026-07-20` with **no** `type` | Same body as `type=all`; `eventType` `all` | SPEC §13 §5.2 (default `all`) |\n| API-01e | `…/accounts/6/activity-health?week=2026-07-20` (the PLAN §13 §5.2 example) | JSON numbers are compared by value, not by text: PLAN's `\"deviation\": 1.30` for Site M passes when the body has `1.3`, and `\"median\": 72.5` passes as `72.5`. Each `locations[]` object has its properties in exactly this order: `location`, `count`, `baseline`, `status`, `deviation` | SPEC §13 §5.2; SPEC §13 \"Contract simplification\" |"}
```


<details><summary>tool result</summary>

```
The file /Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a3d7271def36493bb/docs/acceptance-criteria.md has been updated successfully. (file state is current in your context — no need to Read it back)
```
</details>


**tool call — Edit**
```json
{"replace_all":false,"file_path":"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a3d7271def36493bb/docs/acceptance-criteria.md","old_string":"| API-48 | API-boundary mapper unit test with constructed deviations 0.125 and −0.125 (exactly representable binary midpoints) | Emits 0.13 and −0.13 (2 dp, `MidpointRounding.AwayFromZero`). Core keeps full precision; rounding happens only in the mapper. No seed value sits on a midpoint (consensus §3), so constructed values are required | SPEC §13 §5.2, §5.3.8 |","new_string":"| API-48 | Unit test of the API-boundary rounding step (`Relay.Api`) with constructed deviations 0.125 and −0.125 (exactly representable binary midpoints) on the summary and on a location | Emits 0.13 and −0.13 (2 dp, `MidpointRounding.AwayFromZero`). Core keeps full precision; rounding happens only at the API boundary, never in `Relay.Core`. No seed value sits on a midpoint (consensus §3), so constructed values are required | SPEC §13 §5.2, §5.3.8; SPEC §13 \"Contract simplification\" |"}
```


<details><summary>tool result</summary>

```
The file /Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a3d7271def36493bb/docs/acceptance-criteria.md has been updated successfully. (file state is current in your context — no need to Read it back)
```
</details>


**tool call — Bash**
```json
{"command":"cd \"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a3d7271def36493bb\" && grep -nE \"AccountTotal|BaselineAssessment|ActivityHealthQuery|SeriesWeek|IsEligible|HealthStatusNames|ActivityTypeNames|Response|mapper|ToName\" docs/acceptance-criteria.md; git add docs/acceptance-criteria.md && git commit -q -m \"Acceptance criteria: align with simplified contracts (API-01e, API-48)\n\nCo-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>\" && git log --oneline -1 && git branch --show-current && git status --short","description":"Verify no stale names and commit"}
```


<details><summary>tool result</summary>

```
625aeb2 Acceptance criteria: align with simplified contracts (API-01e, API-48)
worktree-agent-a3d7271def36493bb
```
</details>


### 🤖 ASSISTANT — 2026-09-28T20:31:29.624Z

## Summary
I updated `docs/acceptance-criteria.md` for the simplified contracts and committed it. Only one criterion named something that no longer exists: API-48 referred to the "mapper", and the mappers were deleted. I also added the new API criterion you asked for (API-01e) and a precedence note. No behaviour or golden values changed.

- **Commit:** `625aeb2`
- **Worktree:** `/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a3d7271def36493bb`
- **Branch:** `worktree-agent-a3d7271def36493bb`

## Changed
`/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a3d7271def36493bb/docs/acceptance-criteria.md`
- **Precedence list:** added a bullet for PLAN §13 "Contract simplification (/simplify)". It says the JSON is unchanged, Core records are serialised directly with a snake_case enum converter, and `deviation` is rounded at the API boundary.
- **API-01e (new):** for `…/accounts/6/activity-health?week=2026-07-20`, JSON numbers are compared by value. PLAN's `"deviation": 1.30` for Site M passes as `1.3`, and `"median": 72.5` passes as `72.5`. Each `locations[]` object has its properties in exactly this order: `location`, `count`, `baseline`, `status`, `deviation`. The values come from the PLAN §13 §5.2 example and API-14.
- **API-48:** now says "unit test of the API-boundary rounding step (`Relay.Api`)" instead of "mapper". It now covers both the summary and a location, and says rounding never happens in `Relay.Core`. It is also cited against §13 "Contract simplification". The values (0.125 → 0.13, −0.125 → −0.13) are the same.

## Verification
- A search of the file for `AccountTotal|BaselineAssessment|ActivityHealthQuery|SeriesWeek|IsEligible|HealthStatusNames|ActivityTypeNames|Response|mapper|ToName` finds nothing.
- The names the criteria still use all exist on branch `worktree-agent-a111a8b2c782a44df`: `AccountNotFound`, `InvalidWeek` and `NotAWeekStart` (from `git grep`).
- `git log --oneline -1` shows `625aeb2 Acceptance criteria: align with simplified contracts (API-01e, API-48)`, and `git status` is clean. The main worktree was not touched.

## Deviations
None. API-01e and the precedence bullet cite a PLAN §13 entry that is **not in PLAN.md yet**. `grep "Contract simplification" PLAN.md` found nothing. You said you're adding it, and the citation depends on it landing with that exact title.

## Uncertain
- **The property order in API-01e may fail as the code stands.** `LocationHealth` inherits from `SeriesHealth`, so it declares only `Location` itself; the other four properties come from the base record. I'm fairly sure, though I haven't run it, that System.Text.Json writes base-class properties first. That would give `count, baseline, status, deviation, location`. If so, the architect/backend will need `[JsonPropertyOrder]` or something similar. That is a contract change, so it's their call, not mine.
- I didn't add a property-order rule for `summary`, because you asked only about `locations[]`.

## Next
- Backend tests for API-01e should parse the JSON and compare numbers by value, and check the property order of `locations[]` objects separately.
- API-48 should be tested against the API's rounding extension (currently `WithDisplayDeviations`), not against Core.

