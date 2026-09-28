
### 🧑 USER — 2026-09-28T20:13:12.351Z

/simplify pass on the DASH-247 Phase 0 contracts — angle: **ALTITUDE**. Read-only; edit nothing. You are improving quality, not hunting correctness bugs.

Scope: the unified diff at `/private/tmp/claude-501/-Users-josemejia-Documents-Software-Development-Qualitara-relay-activity-health/485c1256-5727-457d-8aec-65bc845ee171/scratchpad/contracts.diff` (= `git diff 8284184 main -- src web/src`, 69 files). The code is contracts + stubs. Constraints: `CLAUDE.md` and `PLAN.md` §13 are binding.

Angle: check each contract decision fixes things at the right depth rather than patching symptoms — e.g. the `IsoDateAttribute` + `MinLength` + regex stack for query validation, the `ActivityTypeNames`/`HealthStatusNames` mapping vs JSON enum converters, `DateTime` Kind=Utc handling spread across layers vs one boundary, `AddRelayCore` living in Api with options exposed as plain data, `TimeProvider` only for the empty-DB case, the `ActivityHealthResult` hierarchy + `ToHttpResult` mapping, the TS `DashboardState` surface. Flag special cases layered on shared infrastructure and name the simpler, more general mechanism. If the better fix contradicts a PLAN §13 decision, say so explicitly (it would need the user).

Return findings as a list: `file:line` — one-line summary — concrete cost — the deeper/simpler mechanism. Say "none" if clean. Keep it short.


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


**tool call — Bash**
```json
{"command":"grep -n \"^## 13\" -A80 PLAN.md | head -150"}
```


<details><summary>tool result</summary>

```
255:## 13. Plan changes
256-_(append-only, dated, with reason)_
257-
258-### 2026-09-28 — Revised design from the four-agent debate (approved by the user)
259-
260-**Reason.** Battle-testing showed the original band (median ± 2·spread, √median floor) never flags a location dropping to zero for leads/appointments
261-and only 37 % of the time for calls (AI_LOG 8). `statistician`, `product`, `architect` and `reviewer` debated the fix directly and all signed
262-`docs/design-consensus.md` AGREE with no dissent; every number in it is backed by a script in `analysis/` whose output was re-run and reproduced
263-byte-for-byte by the coordinator. The user approved the design as written.
264-
265-**This entry supersedes**, for implementation purposes: §5.1 index/de-dup bullet, §5.2 in full, §5.3 in full, the §5.4 status labels and footnote,
266-the §7 Evaluator / Ranking / API edge-case bullets and golden-values table, D5's default account and D7's numbers. Everything not listed stays in force,
267-including the §7 Calendar, Grid and SQL edge cases. Rationale, rejected options and evidence per item: `docs/design-consensus.md` §1–§4.
268-
269-**Decision changes in short**
270-- D2/D7: the normality rule is R2\* — robust z on the Anscombe scale `T(x) = 2√(x + 3/8)`, k = 2, spread floor 1.0, minimum 4 eligible weeks,
271-  status read from the back-transformed integer range. Flags 4.3 % of site-weeks (was 4.8 %); drop-to-0 caught 98 % all / 96 % calls (was 78 / 37 %);
272-  account 6's s …[clipped in markdown; full text in raw/]
```
</details>


### 🤖 ASSISTANT — 2026-09-28T20:13:36.239Z

Findings (ALTITUDE), most valuable first:

1. `ActivityHealthRequest.cs:8-10`, `IsoDateAttribute.cs`, `ActivityTypeNames.cs:768` — Query validity is encoded three ways (`IsoDate`, `MinLength(1)`, a regex assembled from name constants) and again at the boundary (`ToQuery`, `TryParse`). Cost: the four type names live in the constants, the regex, `ToName`/`TryParse`, and the TS `EVENT_TYPES`. `MinLength(1)` is redundant because the regex already rejects `""`. Adding a type means edits in about five places. Simpler: bind `type` as `ActivityType?` and `week` as `DateOnly?`, and put one custom `IParsable`/`TryParse` on `ActivityType` (or a single `[JsonStringEnumConverter]` naming policy `snake_case_lower`). Then `AddValidation` only needs a "Monday" check. The `IsoDate` attribute goes away, because `DateOnly.TryParse` on the query string is already invariant ISO. Caveat: default binding failures return 400 without the `errors` dictionary shape, so confirm the ProblemDetails format PLAN §5.2 needs. If so, keep one `[AllowedValues]` on the string instead of the regex plus `MinLength`.

2. `HealthStatusNames.cs`, `ActivityTypeNames.cs`, and `string Status` / `string EventType` in `*Response.cs` — Hand-written name mapping with `NotImplemented` stubs, and the API DTOs downgrade enums to `string`. Cost: `ToName` and `TryParse` need switch tests that add no business value. The enum-to-wire mapping sits in Core, which should not know wire format. Nothing enforces exhaustiveness when an enum member is added. Simpler: keep the enums typed in the DTOs and configure one `JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower)` in `ConfigureHttpJsonOptions`. The mapping then lives at the serialization boundary, and `InsufficientData` becomes `insufficient_data` for free. `WeekStart` and `LatestCompleteWeek` already rely on the same JSON options for `DateOnly`. This does not contradict §13. It contradicts only the implicit "strings in DTOs" choice, so it needs no user decision.

3. `IWeekCalendar` / `SiteFirstActivity.FirstActivityUtc` / `DataAsOfUtc` / `WeekWindow` — `DateTime` with the "must be Kind=Utc" invariant is spread across five contracts. §13 §5.1 requires marking instants read back as `DateTimeKind.Utc`, so every consumer trusts a convention the type system cannot express. Cost: a Kind bug produces silent off-by-offset weeks. Simpler: use `DateTimeOffset` (or `Instant`) in Core and the query records. The SQL layer converts once at read (`SpecifyKind` in one row-mapper). `WeekWindow` then serialises the UTC `Z` windows for the SQL query directly, with no per-consumer Kind assertions. This is compatible with §13, which only requires that the instants be UTC.

4. `CoreServiceCollectionExtensions.cs` in `Relay.Api` — `AddRelayCore` lives in the composition root but only registers Core types. `NormalityOptions` carries `[Range]` data annotations, so Core references `System.ComponentModel.DataAnnotations`. The cross-field check (`MinimumEligibleWeeks <= BaselineWeeks`) sits in Api, not next to the options. Cost: any host or test that reuses Core must reproduce the registration and the validation. Simpler: put the cross-field rule in `NormalityOptions` (`IValidatableObject`, or an `IValidateOptions<NormalityOptions>` in Core). `AddRelayCore` stays in Api only if CLAUDE.md's "Core references nothing else" would otherwise be violated. That is a legitimate reason, since it needs `Microsoft.Extensions.*`. Keep it there, but register the singleton `NormalityOptions` projection once inside the options validator. This is minor.

5. `ActivityHealthResult.cs` + `ActivityHealthResultExtensions.ToHttpResult` — Three outcomes (`Found`, `AccountNotFound`, `InvalidWeek(reason)`) are mapped to `Results<Ok, ProblemHttpResult>`. This is sound. The soft spot is `InvalidWeekReason`, which is a Core enum that only exists to pick the message text and detail. Cost: adding a rule means editing the enum, the service, the mapper, and the tests. Note also that `NotAWeekStart` can be decided from the request alone. Its home is the request validator, not the service, which pushes a rule into Core that could sit with `IsoDate`. Simpler: drop `NotAWeekStart` from Core and validate "Monday" in `AddValidation`. Keep only the two range reasons, which need account data. Alternatively collapse them into a single `WeekOutOfRange(earliest, latest)` carrying the bounds, so the ProblemDetails detail can name the valid range. Check whether §13 names `NotAWeekStart` as a service-level result before deciding.

6. `TimeProvider` in `ActivityHealthService` — Used only for the empty-DB case, when there is no data anchor. Cost: one constructor dependency exists solely for a degenerate branch. It forces a fake clock into tests that otherwise need none. Simpler: `FindDataAnchorAsync` returning null means "no events", and the service returns the empty-account result with `LatestCompleteWeek = earliestWeek = the calendar week of clock`. Only that branch needs the time. Keep `TimeProvider`, since it is the standard abstraction and is cheap. Flag as fine, not a change.

7. `ActivityHealthReport` / `ActivityHealthResponse` / `AccountTotal` / `LocationHealth` — Two parallel hierarchies with near-identical shapes: `AccountTotal` and `LocationHealth` both hold `Count + BaselineAssessment`, and the response has `SummaryResponse` and `LocationHealthResponse` with the same fields. Cost: every field addition needs four edits plus two mappers. Simpler: one `SeriesHealth(Count, BaselineAssessment)`, with `LocationHealth(Location, SeriesHealth)`, and one `SeriesHealthResponse` flattened by JSON. The wire shape is unchanged. This is a small win, and the current split is defensible.

8. `web/.../dashboard-state.ts` — The `DashboardState` surface has three parallel signals (`accountId`, `week`, `eventType`) plus three `selectX` setters plus `reload()`. CLAUDE.md says the URL is the only filter state. Cost: `selectX` methods that each write a query param duplicate the same navigate call, and `reload()` exists only to re-trigger a fetch that a URL change already triggers. `DEFAULT_ACCOUNT_ID = 14` is a magic number duplicated from §13/D5 and should be exposed by a single config, not two places. Simpler: one `filters` signal derived from `ActivatedRoute.queryParamMap`, and a single `updateFilters(patch)` that calls `router.navigate({queryParams, queryParamsHandling:'merge'})`. Drop `reload()` unless a retry button truly needs it, in which case tie it to `error`. Also the `computed(notImplemented)` idiom for stubs is fine for a Phase 0 red state.

9. `web/.../activity-health.api.ts:5-8` — `ActivityHealthRequest` is re-declared in TS with the same name as the C# record, but with `eventType` and `week: string | null`, versus C# `type` and `week`. Cost: the wire param name (`type`) differs from the TS field name (`eventType`) and the mapping will be hidden in `HttpActivityHealthApi`. This is a mild vocabulary split, so pick one name across layers (`eventType` on the wire would need a §5.2 change and the user).

Overall: none blocking. Items 1-3 are the ones where a more general mechanism (native enum and DateOnly binding, JSON naming policy, `DateTimeOffset`) removes whole categories of bugs and stub tests.

