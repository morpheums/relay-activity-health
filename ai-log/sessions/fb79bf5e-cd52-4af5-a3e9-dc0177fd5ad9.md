
### 🧑 USER — 2026-09-28T21:34:02.448Z

Review this change for security vulnerabilities.

Changed files (you may Read these and any other file in the repo):
  - src/Relay.Core/ActivityHealth/ActivityHealthService.cs
  - src/Relay.Core/Calendar/WeekCalendar.cs
  - src/Relay.Core/Calendar/WeekLength.cs
  - src/Relay.Core/Normality/BaselineEvaluator.cs
  - src/Relay.Core/Normality/WeeklyGridBuilder.cs
  - src/Relay.Core/Queries/SiteFirstActivityExtensions.cs

Unified diff (only + lines are new):

=== DIFF: src/Relay.Core/ActivityHealth/ActivityHealthService.cs ===
@@ -1,6 +1,7 @@
 using Relay.Core.Calendar;
 using Relay.Core.Normality;
 using Relay.Core.Queries;
+using static Relay.Core.Calendar.WeekLength;
 
 namespace Relay.Core.ActivityHealth;
 
@@ -14,8 +15,6 @@ public sealed class ActivityHealthService(
     NormalityOptions normalityOptions,
     TimeProvider timeProvider) : IActivityHealthService
 {
-    private const int DaysPerWeek = 7;
-
     public async Task<ActivityHealthResult> GetAsync(
         int accountId,
         DateOnly? week,
@@ -75,12 +74,11 @@ public sealed class ActivityHealthService(
 
     private DateOnly EarliestWeekFor(IReadOnlyList<SiteFirstActivity> siteFirstActivities, DateOnly latestCompleteWeek, string timeZoneId)
     {
-        if (siteFirstActivities.Count == 0)
+        if (siteFirstActivities.AccountFirstActivityUtc() is not { } accountFirstActivityUtc)
         {
             return latestCompleteWeek;
         }
 
-        var accountFirstActivityUtc = siteFirstActivities.Min(site => site.FirstActivityUtc);
         var firstActivityWeek = weekCalendar.WeekContaining(accountFirstActivityUtc, timeZoneId);
         return firstActivityWeek < latestCompleteWeek ? firstActivityWeek : latestCompleteWeek;
     }


=== DIFF: src/Relay.Core/Calendar/WeekCalendar.cs ===
@@ -1,9 +1,9 @@
+using static Relay.Core.Calendar.WeekLength;
+
 namespace Relay.Core.Calendar;
 
 public sealed class WeekCalendar : IWeekCalendar
 {
-    private const int DaysPerWeek = 7;
-
     public WeekWindow Window(DateOnly weekStart, string timeZoneId)
     {
         if (weekStart.DayOfWeek != DayOfWeek.Monday)
@@ -35,16 +35,6 @@ public sealed class WeekCalendar : IWeekCalendar
         return localDay.AddDays(-daysSinceMonday);
     }
 
-    private static DateTime LocalMidnightToUtc(DateOnly localDay, TimeZoneInfo timeZone)
-    {
-        var localMidnight = localDay.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
-        if (!timeZone.IsInvalidTime(localMidnight))
-        {
-            return TimeZoneInfo.ConvertTimeToUtc(localMidnight, timeZone);
-        }
-
-        // Midnight skipped by a DST jump: the day starts at the jump, which is midnight read with the previous day's offset.
-        var offsetBeforeJump = timeZone.GetUtcOffset(localMidnight.AddDays(-1));
-        return DateTime.SpecifyKind(localMidnight - offsetBeforeJump, DateTimeKind.Utc);
-    }
+    private static DateTime LocalMidnightToUtc(DateOnly localDay, TimeZoneInfo timeZone) =>
+        TimeZoneInfo.ConvertTimeToUtc(localDay.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified), timeZone);
 }


=== DIFF: src/Relay.Core/Calendar/WeekLength.cs ===
@@ -0,0 +1,6 @@
+namespace Relay.Core.Calendar;
+
+internal static class WeekLength
+{
+    public const int DaysPerWeek = 7;
+}


=== DIFF: src/Relay.Core/Normality/BaselineEvaluator.cs ===
@@ -34,8 +34,11 @@ public sealed class BaselineEvaluator(NormalityOptions normalityOptions) : IBase
 
     private static double AnscombeTransform(double count) => AnscombeScale * Math.Sqrt(count + AnscombeOffset);
 
-    private static double InverseAnscombeTransform(double transformed) =>
-        Math.Pow(transformed / AnscombeScale, 2) - AnscombeOffset;
+    private static double InverseAnscombeTransform(double transformed)
+    {
+        var root = transformed / AnscombeScale;
+        return root * root - AnscombeOffset;
+    }
 
     private static double MedianOf(IEnumerable<double> values)
     {


=== DIFF: src/Relay.Core/Normality/WeeklyGridBuilder.cs ===
@@ -41,12 +41,11 @@ public sealed class WeeklyGridBuilder : IWeeklyGridBuilder
             .ToDictionary(group => group.Key, group => group.Sum(weeklySiteCount => weeklySiteCount.Count));
 
         var selectedWeekCount = accountCountsByWeek.GetValueOrDefault(selectedWeek.WeekStart);
-        if (siteFirstActivities.Count == 0)
+        if (siteFirstActivities.AccountFirstActivityUtc() is not { } accountFirstActivityUtc)
         {
             return new WeeklySeries(selectedWeekCount, []);
         }
 
-        var accountFirstActivityUtc = siteFirstActivities.Min(site => site.FirstActivityUtc);
         var eligibleWeekCounts = EligibleWindows(baselineWindows, accountFirstActivityUtc)
             .Select(window => accountCountsByWeek.GetValueOrDefault(window.WeekStart))
             .ToList();


=== DIFF: src/Relay.Core/Queries/SiteFirstActivityExtensions.cs ===
@@ -0,0 +1,7 @@
+namespace Relay.Core.Queries;
+
+internal static class SiteFirstActivityExtensions
+{
+    public static DateTime? AccountFirstActivityUtc(this IReadOnlyList<SiteFirstActivity> siteFirstActivities) =>
+        siteFirstActivities.Count == 0 ? null : siteFirstActivities.Min(site => site.FirstActivityUtc);
+}


Investigate per the method in your instructions, then return the findings list.


**tool call — Read**
```json
{"file_path":"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a440e778df2f76d2c/src/Relay.Core/ActivityHealth/ActivityHealthService.cs"}
```


<details><summary>tool result</summary>

```
1	using Relay.Core.Calendar;
2	using Relay.Core.Normality;
3	using Relay.Core.Queries;
4	using static Relay.Core.Calendar.WeekLength;
5	
6	namespace Relay.Core.ActivityHealth;
7	
8	public sealed class ActivityHealthService(
9	    IAccountQueries accountQueries,
10	    IActivityQueries activityQueries,
11	    IWeekCalendar weekCalendar,
12	    IWeeklyGridBuilder weeklyGridBuilder,
13	    IBaselineEvaluator baselineEvaluator,
14	    ILocationRanker locationRanker,
15	    NormalityOptions normalityOptions,
16	    TimeProvider timeProvider) : IActivityHealthService
17	{
18	    public async Task<ActivityHealthResult> GetAsync(
19	        int accountId,
20	        DateOnly? week,
21	        ActivityType eventType,
22	        CancellationToken cancellationToken)
23	    {
24	        var account = await accountQueries.FindAsync(accountId, cancellationToken);
25	        if (account is null)
26	        {
27	            return new ActivityHealthResult.AccountNotFound();
28	        }
29	
30	        if (week is { DayOfWeek: not DayOfWeek.Monday })
31	        {
32	            return new ActivityHealthResult.InvalidWeek(InvalidWeekReason.NotAWeekStart);
33	        }
34	
35	        var dataAnchorUtc = await activityQueries.FindDataAnchorAsync(cancellationToken);
36	        var latestCompleteWeek = weekCalendar.LatestCompleteWeek(dataAnchorUtc ?? timeProvider.GetUtcNow().UtcDateTime, account.Timezone);
37	        var siteFirstActivities = await activityQueries.ListSiteFirstActivitiesAsync(acco …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Read**
```json
{"file_path":"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a440e778df2f76d2c/src/Relay.Core/Calendar/WeekCalendar.cs"}
```


<details><summary>tool result</summary>

```
1	using static Relay.Core.Calendar.WeekLength;
2	
3	namespace Relay.Core.Calendar;
4	
5	public sealed class WeekCalendar : IWeekCalendar
6	{
7	    public WeekWindow Window(DateOnly weekStart, string timeZoneId)
8	    {
9	        if (weekStart.DayOfWeek != DayOfWeek.Monday)
10	        {
11	            throw new ArgumentException($"Week start {weekStart:yyyy-MM-dd} is not a Monday.", nameof(weekStart));
12	        }
13	
14	        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
15	        return new WeekWindow(
16	            weekStart,
17	            LocalMidnightToUtc(weekStart, timeZone),
18	            LocalMidnightToUtc(weekStart.AddDays(DaysPerWeek), timeZone));
19	    }
20	
21	    public DateOnly WeekContaining(DateTime instantUtc, string timeZoneId)
22	    {
23	        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
24	        var utcInstant = DateTime.SpecifyKind(instantUtc, DateTimeKind.Utc);
25	        var localDay = DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(utcInstant, timeZone));
26	        return MondayOnOrBefore(localDay);
27	    }
28	
29	    public DateOnly LatestCompleteWeek(DateTime dataAnchorUtc, string timeZoneId) =>
30	        WeekContaining(dataAnchorUtc, timeZoneId).AddDays(-DaysPerWeek);
31	
32	    private static DateOnly MondayOnOrBefore(DateOnly localDay)
33	    {
34	        var daysSinceMonday = ((int)localDay.DayOfWeek - (int)DayOfWeek.Monday + DaysPerWeek) % DaysPerWeek;
35	        return localDay.AddDays( …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Read**
```json
{"file_path":"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a440e778df2f76d2c/src/Relay.Core/Calendar/WeekLength.cs"}
```


<details><summary>tool result</summary>

```
1	namespace Relay.Core.Calendar;
2	
3	internal static class WeekLength
4	{
5	    public const int DaysPerWeek = 7;
6	}
7	
```
</details>


**tool call — Read**
```json
{"file_path":"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a440e778df2f76d2c/src/Relay.Core/Normality/BaselineEvaluator.cs"}
```


<details><summary>tool result</summary>

```
1	namespace Relay.Core.Normality;
2	
3	public sealed class BaselineEvaluator(NormalityOptions normalityOptions) : IBaselineEvaluator
4	{
5	    // Scales a MAD so it is comparable with a standard deviation.
6	    private const double MadConsistencyConstant = 1.4826;
7	    private const double AnscombeOffset = 0.375;
8	    private const double AnscombeScale = 2.0;
9	
10	    private static readonly double TransformedZero = AnscombeTransform(0);
11	
12	    public SeriesHealth Evaluate(IReadOnlyList<int> eligibleWeekCounts, int selectedWeekCount)
13	    {
14	        if (eligibleWeekCounts.Count < normalityOptions.MinimumEligibleWeeks)
15	        {
16	            return new SeriesHealth(
17	                selectedWeekCount,
18	                new Baseline(eligibleWeekCounts.Count, null, null, null),
19	                HealthStatus.InsufficientData,
20	                null);
21	        }
22	
23	        var median = MedianOf(eligibleWeekCounts.Select(count => (double)count));
24	        var centre = AnscombeTransform(median);
25	        var spread = SpreadOf(eligibleWeekCounts, centre);
26	        var (low, high) = RangeFor(centre, spread);
27	
28	        return new SeriesHealth(
29	            selectedWeekCount,
30	            new Baseline(eligibleWeekCounts.Count, median, low, high),
31	            StatusFor(selectedWeekCount, low, high),
32	            DeviationOf(selectedWeekCount, centre, spread));
33	    }
34	
35	    private static double AnscombeTransform(double count) => Ans …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Read**
```json
{"file_path":"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a440e778df2f76d2c/src/Relay.Core/Normality/WeeklyGridBuilder.cs"}
```


<details><summary>tool result</summary>

```
1	using Relay.Core.Calendar;
2	using Relay.Core.Queries;
3	
4	namespace Relay.Core.Normality;
5	
6	public sealed class WeeklyGridBuilder : IWeeklyGridBuilder
7	{
8	    public IReadOnlyList<LocationSeries> BuildLocationSeries(
9	        WeekWindow selectedWeek,
10	        IReadOnlyList<WeekWindow> baselineWindows,
11	        IReadOnlyList<SiteFirstActivity> siteFirstActivities,
12	        IReadOnlyList<WeeklySiteCount> weeklySiteCounts)
13	    {
14	        var countsBySiteAndWeek = weeklySiteCounts
15	            .GroupBy(weeklySiteCount => (weeklySiteCount.Location, weeklySiteCount.WeekStart))
16	            .ToDictionary(group => group.Key, group => group.Sum(weeklySiteCount => weeklySiteCount.Count));
17	
18	        int CountFor(string location, DateOnly weekStart) =>
19	            countsBySiteAndWeek.GetValueOrDefault((location, weekStart));
20	
21	        return siteFirstActivities
22	            .Where(site => site.FirstActivityUtc < selectedWeek.EndUtc)
23	            .Select(site => new LocationSeries(
24	                site.Location,
25	                new WeeklySeries(
26	                    CountFor(site.Location, selectedWeek.WeekStart),
27	                    EligibleWindows(baselineWindows, site.FirstActivityUtc)
28	                        .Select(window => CountFor(site.Location, window.WeekStart))
29	                        .ToList())))
30	            .ToList();
31	    }
32	
33	    public WeeklySeries BuildAccountSeries(
34	        WeekWindow selectedWeek,
35 …[clipped in markdown; full text in raw/]
```
</details>


**tool call — Read**
```json
{"file_path":"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a440e778df2f76d2c/src/Relay.Core/Queries/SiteFirstActivityExtensions.cs"}
```


<details><summary>tool result</summary>

```
1	namespace Relay.Core.Queries;
2	
3	internal static class SiteFirstActivityExtensions
4	{
5	    public static DateTime? AccountFirstActivityUtc(this IReadOnlyList<SiteFirstActivity> siteFirstActivities) =>
6	        siteFirstActivities.Count == 0 ? null : siteFirstActivities.Min(site => site.FirstActivityUtc);
7	}
8	
```
</details>


### 🤖 ASSISTANT — 2026-09-28T21:34:43.995Z

Based on my analysis of all six changed files:

1. **ActivityHealthService.cs** - Extracts `DaysPerWeek` to shared const and moves null-check logic into an extension method. Behavior preserved.
2. **WeekCalendar.cs** - Uses the new shared const; also removes DST-invalid-time handling in `LocalMidnightToUtc`. This is a correctness regression (will throw on DST spring-forward for zones that skip midnight), but not a security vulnerability — no attacker-influenced sink, no auth bypass, no volumetric DoS lever the diff introduces beyond a per-account timezone crash.
3. **WeekLength.cs** - New constant-only file.
4. **BaselineEvaluator.cs** - Replaces `Math.Pow(x,2)` with `x*x`. Mathematically identical.
5. **WeeklyGridBuilder.cs** - Same extension-method refactor as (1). Behavior preserved.
6. **SiteFirstActivityExtensions.cs** - New helper wrapping the pre-existing `Count==0 → null / else Min(...)` pattern.

No changes touch entry points, sinks, auth/tenant boundaries, logging of sensitive data, deserialization, allowlists, IaC, CI trust, or parsers. There is no new attacker-influenceable data flow, no new sink, and no removed security gate. The DST regression in `WeekCalendar` is a correctness bug worth flagging in a normal code review, but it does not meet the security-review bar (no attacker-controllable trigger via the request surface; timezone is per-account config).


**tool call — StructuredOutput**
```json
{"findings":[]}
```


<details><summary>tool result</summary>

```
Structured output provided successfully
```
</details>

