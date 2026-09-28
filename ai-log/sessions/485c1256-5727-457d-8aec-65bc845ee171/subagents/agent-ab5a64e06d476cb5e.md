
### 🧑 USER — 2026-09-28T20:12:56.560Z

/simplify pass on the DASH-247 Phase 0 contracts — angle: **REUSE**. Read-only; edit nothing. You are improving quality, not hunting correctness bugs.

Scope: the unified diff at `/private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/485c1256-5727-457d-8aec-65bc845ee171/scratchpad/contracts.diff` (= `git diff 8284184 main -- src web/src`, 69 files). The code is contracts + `NotImplementedException` stubs only (interfaces, records, DTOs, DI registration, TS models, abstract API tokens, DashboardState surface). Constraints: `CLAUDE.md` and `PLAN.md` §13 (all entries dated 2026-09-28) are binding — don't propose anything that contradicts them.

Angle: flag new code that re-implements something the codebase or the platform (.NET 10 BCL / ASP.NET Core / Angular) already provides — e.g. hand-rolled types where a framework type exists, duplicated constants/mappings, parallel DTOs that could be one type, duplicated validation. Grep adjacent files. Name the existing helper/type to use instead.

Return findings as a list: `file:line` — one-line summary — concrete cost (what is duplicated / harder to maintain) — the simpler reuse. Say "none" if clean. Keep it short.


**tool call — Read**
```json
{"file_path":"/private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/485c1256-5727-457d-8aec-65bc845ee171/scratchpad/contracts.diff"}
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


### 🤖 ASSISTANT — 2026-09-28T20:13:22.095Z

Findings (REUSE angle). I checked against the diff only, not the rest of the repo.

1. `src/Relay.Core/Normality/HealthStatusNames.cs:1-12` and `src/Relay.Core/Queries/ActivityTypeNames.cs:1-19` — two hand-rolled enum-to-snake-case tables, each with `const` strings, `ToName` and (for activity type) `TryParse`.
   - Cost: every enum member is spelled three times: the enum, the const, and the `ToName` switch. `ActivityTypeNames.ExactMatchPattern` (line 8) repeats the names a fourth time as a regex. `LocationHealthResponse.Status` and `SummaryResponse.Status` are `string` only because of this mapping.
   - Reuse: .NET 8+ ships `JsonNamingPolicy.SnakeCaseLower`. Register `new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower)` in the API's `ConfigureHttpJsonOptions`, then type the response `Status` as `HealthStatus` and drop `HealthStatusNames` entirely.
   - The `type` query parameter needs one parse routine, because minimal APIs bind enums with plain `Enum.TryParse`, which doesn't accept snake case. Keep only `TryParse`, or only the regex, not both, since they validate the same thing twice.

2. `src/Relay.Api/Http/SummaryResponse.cs:3` and `LocationHealthResponse.cs:3-8` — the two DTOs share `Count`, `Baseline`, `Status`, `Deviation`. The Core side has the same overlap (`AccountTotal(Count, Baseline)` vs `LocationHealth(Location, Count, Baseline)`), as does the TS side (`account-health-summary.ts` vs `location-health.ts`, identical apart from `location`).
   - Cost: a change to those four fields has to be made in about six places.
   - Reuse: let the location type wrap or extend the summary. In TS use `interface LocationHealth extends AccountHealthSummary { location: string }`. In C# make `LocationHealthResponse(string Location, SummaryResponse Summary)`, or reuse `LocationHealth` for the account total with a sentinel name. Only do this if PLAN §5.2 leaves the JSON shape open. If the JSON must stay flat, do it in TS and Core only.

3. `web/src/app/core/models/event-type.ts:1-3` — `EventType` and `EVENT_TYPES` list the same four literals twice.
   - Cost: adding a type means editing both, and the compiler won't catch a mismatch.
   - Reuse: `export const EVENT_TYPES = ['all', 'call_received', 'lead_created', 'appointment_set'] as const; export type EventType = (typeof EVENT_TYPES)[number];`

4. All `Relay.Core` and `Relay.Infrastructure` service classes (`AccountService.cs:7`, `ActivityHealthService.cs:434-441`, `BaselineEvaluator.cs:7`, `SqlAccountQueries.cs:8`, `SqlActivityQueries.cs:8`) — each primary-constructor parameter is copied into a `private readonly _field`.
   - Cost: about 20 redundant lines and a second name for every dependency. C# 12 primary-constructor parameters are already captured and readonly by convention.
   - Reuse: use the parameters directly, for example `accountQueries.ListAsync(...)`.

5. `src/Relay.Api/Http/ActivityHealthRequest.cs:9` with `IsoDateAttribute.cs` — a custom `ValidationAttribute` plus `ToQuery` re-parsing a `string? Week` into `DateOnly?`.
   - Cost: the date is parsed twice, once in the validator and once in `ToQuery`.
   - Reuse: bind `DateOnly? Week` directly, since minimal APIs bind `DateOnly` natively. If PLAN requires rejecting non-ISO formats, keep the attribute but say so, and note that it only validates and never removes the second parse. This one is low confidence.

6. `web/src/app/features/dashboard/dashboard-state.ts:1150-1153` — hand-rolled `report`, `isLoading` and `error` signals.
   - Cost: the loading, error and stale-value states need manual wiring and reload logic.
   - Reuse: Angular's `rxResource({ params: ..., stream: ... })` already provides `value()`, `isLoading()`, `error()` and `reload()`. It works with the abstract `ActivityHealthApi` token and `Observable` return, so nothing in CLAUDE.md is contradicted. Also, `ProblemDetails` in `problem-details.ts` is only needed if error bodies are read from `HttpErrorResponse.error`. Fine, but note it.

Nothing to flag on DI registration, options validation (`ValidateDataAnnotations` and `ValidateOnStart` are already the built-in mechanism), `ProblemDetails` and `IExceptionHandler` usage, or `TimeProvider.System`.

