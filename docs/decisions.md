# Design decisions and trade-offs

[← README](../README.md) · [Running](running.md) · [Testing](testing.md) · [API](api.md) · [Interpretation](interpretation.md) · [Decisions](decisions.md) · [Architecture](architecture.md) · [Dashboard](dashboard.md) · [Deferred](deferred.md)

What was decided and rejected (PLAN §3 as revised by §13), with the numbers behind the band rule. Deferred work and next steps: [deferred.md](deferred.md).

## Design decisions and trade-offs

Main decisions from PLAN §3, as revised by §13. Rationale and evidence: [`docs/design-consensus.md`](design-consensus.md). Every number below comes from `analysis/` or `docs/battle-test/`.

| # | Decision | Rejected | Why |
|---|---|---|---|
| D1 | "Now" = latest event in the whole dataset (one global anchor) | System clock; a fixed config date; a separate anchor per account | The clock shows nothing on static data. A per-account anchor would make an account that stopped sending data look normal |
| D2 | Normal = the location's own history | Sibling comparison (a site's share of the account); % change against the mean | Sibling share is confounded when the whole account moves: in the spike week all 15 sites keep their share, so it says "all normal". It is also useless for single-site accounts. The mean is poisoned by the spike: 171 vs a median of 72.5 for account 6 in the week of Jul 20. % change cries wolf on counts of 3–6 |
| D3 | One method at two levels: an account summary row plus locations sorted by how unusual they are | A second statistic for the sibling view | One set of edge cases. It still separates "the whole account moved" from "one site moved" |
| D4 | Metric = inbound event count, all types by default, with a type filter | A column per type; outcome rates | Totals have enough volume. Per-type counts per site are mostly noise. Rates are deferred |
| D5 | "Viewing as" account switcher (impersonation for the demo, not auth). The default is **account 14** | Account 12 (the original default); a hard-coded account | Under the final rule, account 14's Site B (2 vs usually 3–12) is the **only** flagged series in the whole seed for the default week, which shows a location going quiet. Account 12 flags nothing that week. The switcher also lets an evaluator open account 6 (the spike), account 20 (empty) and the single-site accounts |
| D6 | SQL only counts (de-duplicate, bucket into given UTC windows, group). Week math, zero-fill, statistics and ranking are in pure C# | Everything in SQL; LINQ | Product rules stay unit-testable without a database. LINQ would not help, because InMemory and SQLite have different semantics from SQL Server |
| D7 | Band rule **R2\*** ([interpretation](interpretation.md#how-normal-is-decided)) with k = 2, floor 1.0, and at least 4 eligible weeks | See the table below | See the table below |

### How the band rule was chosen (D7, revised)

The rule was simulated on the seed before any code was written. The simulation used every site-week with a full 8-week baseline and excluded the spike week.

| Candidate | Share of site-weeks flagged | Drop to 0 caught (all / calls) | Outcome |
|---|---|---|---|
| 2nd-lowest to 2nd-highest baseline week | **34.6 %** | — | Rejected: flags a third of all sites every week |
| Min–max of the baseline | 15.1 % | — | Rejected: too noisy |
| Median ± 3 × spread | 0.3 % | — | Rejected: misses too much |
| Median ± 2 × spread, √median floor (first plan) | 4.8 % | 78 % / **37 %** | Rejected **after** it was accepted. A full sweep showed that a location falling to **zero** was never flagged for leads or appointments, and was flagged for calls only 37 % of the time |
| **R2\*: Anscombe scale, k = 2, floor 1.0** | **4.3 %** (≈ 4 %) | **98 % / 96 %** | **Chosen.** 0 contradictions between status and range in 253,149 checks. Account 6's spike week still flags 15/15 sites, and in the week of 2026-07-20, with the spike still in the baseline, all 15 are within range |

Other options rejected in the debate: k = 1.75 (false "below" doubles to 5.5 %); k = 2.5 (drop-to-0 detection for calls falls to 70 %); Freeman–Tukey (7 % false "below" at median 2); exact Poisson/negative-binomial tails (drop-to-0 for calls only 56 %); EARS (based on mean and SD, and flags "above" only); Farrington/Noufaily (fitted models, out of scope); a fifth "too few to judge" status (costs contract, UI and test work; the footnote covers it).

**The trade-off accepted:** a location that usually gets 2 or fewer events a week can never show "Lower than usual". A week with 0 is ordinary at that level: P(0 | Poisson 2) = 13.5 %. The dashboard says so rather than raise false alarms.

### Later decisions (PLAN §13), with the reason for each

| Decision | Reason |
|---|---|
| Neutral copy: "Higher than usual" / "Lower than usual" / "Within usual range". No median, z or "±" on screen | 13 % of account-weeks show at least one flagged location, so the labels must not sound alarming. The admin needs a range, not statistics |
| `baseline` is always returned, with `weeksUsed`; `minimumEligibleWeeks` is a top-level field | The UI needs "N of 4 weeks" even when there is no range |
| A week before `earliestWeek` or after `latestCompleteWeek` returns 400. For an account with no events, `earliestWeek` equals `latestCompleteWeek` | The rule is the same on both sides, and the week stepper needs no special case for null |
| A site's existence and eligibility use its first event of **any** type. The type filter changes counts only | Using "first event of this type" changes 110 statuses on the seed, 99 of them from normal to insufficient, with no statistical gain |
| Exact duplicates are removed only with `DISTINCT`/`GROUP BY` over every non-id column. NULL, `''` and `0` count as equal | An `=`-based self-join misses 4 of the 12 pairs because of NULL columns. Treating NULL/''/0 as equal was a user decision; the total stays 12,614 on the seed |
| Covering index `(account_id, occurred_at) INCLUDE (location, event_type, duration_seconds, outcome)` | Without the extra columns the planned index went unused. With them the weekly query is one index seek per window (a 200 ms scan became about 7 ms of seeks) |
| Input precedence: a malformed `week` or `type` returns 400; then an unknown account returns 404; then a non-Monday week; then out of range. `type` is case-sensitive. A non-numeric account id returns 404 | Errors are predictable and testable, and each rule has exactly one place |
| An invalid URL parameter is reset to its default, with the URL rewritten using `replaceUrl` (never snapped to the nearest Monday). Switching account keeps the week and type, and falls back to the latest week if the new account can't show it. User actions add browser history entries | A reload or a shared link always reproduces the same view, and Back undoes a filter change |
| An empty database (no events at all) returns 200 with `dataAsOf: null`, and the clock is the fallback anchor | The one case where D1 has nothing to anchor on |
| No separate DTO layer. Core's output records are shaped like the JSON; casing, enum names and rounding are configured once in the API | Otherwise every contract change would be made in four places |
| API on port 5080. Connection string never in `appsettings*.json` | macOS holds port 5000. No committed secrets |
| In Development the API reads the repo-root `.env` with the DotNetEnv library; real environment variables win; Production never reads `.env`. The EF design-time factory was removed in favour of `--startup-project src/Relay.Api` | The earlier step that loaded `.env` into the shell worked only in bash/zsh. One library call makes setup identical on Windows, macOS and Linux, and was preferred over a hand-written `.env` reader |
| Time zones whose DST change falls at local midnight are out of scope | No seed zone is affected, because the US zones switch at 02:00. An untested branch for them disagreed with the rest of the calendar, so it was removed rather than half-supported |
| Process: every agent works in its own git worktree; the red test suite is committed before the implementation; a reviewer on a different model after each layer | See [`AI_LOG.md`](../AI_LOG.md) |
| UI redesign (session 3): header and footer, the footnote lines moved into the footer, Geist type, colour only for status direction, desktop only, no pagination. Behaviour, copy and the API are unchanged. Approved mockups: [docs/design/](design/README.md) | The user asked for a proper week picker, a header and footer, and a minimal modern look. Designing first on a canvas let the user approve the look before any Angular change |
| Week picker: Angular Material's calendar in an overlay, only Mondays from `earliestWeek` to `latestCompleteWeek` selectable; the ◀/▶ stepper stays; a "Latest week" button in the picker footer | A maintained library over a hand-built calendar. The bounds come from the API, so the admin can't pick a week that would return 400. Rejected: ng-bootstrap's datepicker and a CDK listbox of weeks |
| Footer option B: the three method facts as tiles, "Keep in mind" for the limits, "Data as of" beside the heading | Facts and caveats read differently: the tiles say how the numbers are made, "Keep in mind" says what they can't show. Rejected: grouped columns (option A) and ruled notes (option C) |

---

## Deferred work and next steps

What was deferred and what comes with another day now live in one place: [deferred.md](deferred.md).
