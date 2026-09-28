
### 🧑 USER — 2026-09-28T20:13:06.846Z

/simplify pass on the DASH-247 Phase 0 contracts — angle: **EFFICIENCY**. Read-only; edit nothing. You are improving quality, not hunting correctness bugs.

Scope: the unified diff at `/private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/485c1256-5727-457d-8aec-65bc845ee171/scratchpad/contracts.diff` (= `git diff 8284184 main -- src web/src`, 69 files). The code is contracts + stubs; judge the work the contracts will FORCE on implementations. Constraints: `CLAUDE.md` and `PLAN.md` §13 are binding (e.g. SQL counts only; statistics in Core).

Angle: flag wasted work the contract shapes introduce — query interfaces that force extra round trips (e.g. separate anchor / sites / counts / account calls that could be fewer), repeated I/O per request, sequential independent calls, blocking work at startup (options validation, DI), DI lifetimes that rebuild things per request needlessly, long-lived objects capturing closures/large scopes (e.g. DI factory lambdas, Angular signals/effects capturing big state). Name the cheaper alternative and whether it changes a PLAN-pinned boundary.

Return findings as a list: `file:line` — one-line summary — concrete cost — cheaper alternative. Say "none" if clean. Keep it short.


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


### 🤖 ASSISTANT — 2026-09-28T20:13:21.323Z

Findings (efficiency angle):

- `src/Relay.Core/Queries/IActivityQueries.cs:802-810` — three separate activity calls (anchor, first activities, weekly counts), plus `IAccountQueries.FindAsync`. That is 4 round trips per request, and the `ActivityHealthService` will probably await them one after another. The `ListSiteFirstActivitiesAsync` scan is a full-account `MIN(occurred_at)` group-by over history, repeated on every request.
  - Cheaper: the anchor and account lookup don't depend on each other, and the first-activities query doesn't depend on the anchor. Start `FindAsync`, `FindDataAnchorAsync` and `ListSiteFirstActivitiesAsync` together, then run the counts after the anchor resolves.
  - A single `IActivityQueries` method that returns anchor, first activities and counts as one result set is the bigger saving, but it needs the baseline windows up front, which come from the anchor.
  - The contract can stay as it is if the service runs the independent calls concurrently.
  - Caution: EF's `DbContext` is not thread-safe, and both query classes share one scoped context. Parallel calls would need a `DbContext` factory (`AddDbContextFactory`) or separate connections. So this is a contract-level choice.
  - No PLAN-pinned boundary changes, because SQL still only counts.

- `src/Relay.Core/Queries/IActivityQueries.cs:802` and `src/Relay.Core/Queries/IAccountQueries.cs:788` — the anchor (`MAX` over the whole activity table) and the account row are re-queried per request even though they change rarely.
  - Cost: a table-wide aggregate on every dashboard load, and the account list is re-read on every page load.
  - Cheaper: cache the anchor briefly (a decorator, or `IMemoryCache` inside `SqlActivityQueries`). Or derive the anchor from the same statement as the counts (`MAX` over the query's date range), which removes one round trip.
  - Caching the account list is safe. Caching the anchor is only safe if the data is static, which the seed suggests. Check that against PLAN §5.1 before doing it.

- `src/Relay.Core/Queries/IActivityQueries.cs:806-810` and `src/Relay.Core/Normality/IWeeklyGridBuilder.cs:613-623` — `WeeklySiteCount` is passed to `BuildLocationSeries` and again to `BuildAccountSeries`, so the implementation is likely to group and zero-fill the same rows twice.
  - Cost: two passes over roughly 9 weeks × N sites, which is small. The bigger cost is that the account series is derivable from the location series.
  - Cheaper: build `BuildAccountSeries` from `IReadOnlyList<LocationSeries>` by summing eligible sites. That avoids re-deriving eligibility and reuses the first-activity lookup. The catch is that the account total must exclude ineligible site-weeks exactly as PLAN §5.3 says, so this is a Core-internal change, not a PLAN boundary change.

- `src/Relay.Api/Composition/CoreServiceCollectionExtensions.cs:43` — the singleton factory lambda `serviceProvider => ...GetRequiredService<IOptions<NormalityOptions>>().Value` captures nothing large.
  - Cost: it resolves once, since the singleton is cached. `ValidateOnStart` runs once at startup. This is negligible, so leave it as it is.

- `src/Relay.Api/Composition/CoreServiceCollectionExtensions.cs:50-51` — `AccountService` and `ActivityHealthService` are Scoped, but their only scoped dependencies are the query classes. This is correct, because the queries hold the `DbContext`. Note that the singleton calendar, grid builder, evaluator and ranker stay singleton (good).

- `web/src/app/features/dashboard/dashboard-state.ts:1146-1153` — `accounts` and `report` are separate signals. If they are backed by two independent `rxResource`s or `effect`s, the account list refetches whenever `report` reloads.
  - Cheaper: load the account list once at `DashboardState` construction and never key it to the URL params. Key `report` only to `{accountId, week, eventType}`, and use `rxResource` so a changed param cancels the in-flight request. Add no `effect` that captures the whole state. `reload()` should re-trigger only the report resource.
  - The account name is already returned in the report (`account` in the response), so the list only feeds the selector.

- `web/src/app/app.config.ts:938-939` — `useClass` with `@Injectable()` and no `providedIn` is fine and lazy. No cost.

Everything else, including the API endpoint, the response records, the models and the options validation, is none.

