# Interpretation: what "normal" means

[← README](../README.md) · [Running](running.md) · [Testing](testing.md) · [API](api.md) · [Interpretation](interpretation.md) · [Decisions](decisions.md) · [Architecture](architecture.md)

How the ticket was read, the rule that decides "normal", the assumptions behind it, how messy data is handled, and what the dashboard cannot detect.

## Interpretation of the ticket

### For the admin

For each location, and for the account as a whole, the dashboard shows **last week's inbound events** next to what that location **usually** gets. "Usually" comes from that location's own last 8 full weeks.

| What the admin sees | Meaning |
|---|---|
| **▲ Higher than usual** | More than this location's usual range |
| **▼ Lower than usual** | Less than this location's usual range |
| **Within usual range** | Inside the range |
| **Not enough history yet (N of 4 weeks needed)** | Fewer than 4 full weeks of history. The count is shown, but no range and no judgement |

Every row has a line such as "Usually 3–12 a week". Locations that need attention are listed first. Status is always a symbol plus words, never colour alone.
The default view shows an example: at Beacon Home Security, **Site B had 2 events against a usual 3–12**. It is marked "▼ Lower than usual" and listed first, while the account as a whole (26, usually 18–38) is within its usual range.

"Last week" means the **last complete week** (Monday to Sunday in the account's time zone). The week in progress is never shown, because on a Monday morning it would always look like a collapse.

### How "normal" is decided

This is the precise rule (PLAN §13 §5.3, "R2\*").

- **Window.** For a selected week W, the baseline is the 8 local weeks before W, zero-filled. A baseline week counts only if it starts after the week of that series' first event (a site's own first event; for the account, its earliest event). With fewer than 4 such weeks the status is `insufficient_data`.
- **Scale.** Counts are compared on the Anscombe scale T(x) = 2·√(x + 3/8). This transform makes small counts roughly equally variable.
- **Band.** The centre is T(median of the eligible weeks). The spread is max(1.4826 × MAD on the T scale, 1.0), where 1.0 is the Poisson noise floor. The band is centre ± 2·spread, converted back to a whole-number range `low–high`.
- **Status comes from the integer range only.** `count < low` gives `below`, `count > high` gives `above`, and anything else, including exactly `low` or `high`, gives `normal`. The "Usually low–high" line therefore can never contradict the label.
- **Ranking.** Flagged rows come before normal ones. Within that, rows are ordered by how far they are from usual (|z| on the T scale, unrounded), then `below` before `above` on a tie, then location name. `insufficient_data` rows come last.
- **Metric.** Inbound activity events (calls received, leads created, appointments set). The default is all types, and a filter narrows it to one type.

**Not built:** alerting, forecasting and ML (out of scope per product); cross-account benchmarks, which serve a different persona (the account manager); outcome rates (deferred, see [decisions](decisions.md#deliberately-deferred)).

---

## Key assumptions

None of these went to the recruiter. Each is a working assumption stated in PLAN §4, with the seed evidence behind it.

| Question | Working assumption | Evidence in the seed |
|---|---|---|
| What is "now"? | The latest event in the dataset. The default week is the last complete local week before it: **2026-07-20** for every account | Data ends **Mon 2026-07-27 22:20:34 UTC**, while the system clock is in September 2026. Measured from the clock, every account would look silent |
| Does "this week" mean the partial week? | No. Only complete weeks are compared | The final week holds one day of data. Weekends are about 25 % of a weekday |
| Week start | Monday (ISO), in the account's IANA time zone | Each account has an IANA `timezone`. Local-week and UTC-week bucketing differ by only 8 events, but local time is what the admin means |
| Is account 6's 880-event week real (a storm, a campaign) or a bad import? | Unknown. It is shown as "Higher than usual" and never excluded. The median keeps it from distorting later weeks | Week of Jun 1: 880 events (805 on Jun 3 alone) against about 70 a week, spread across all 15 sites, and the fields look plausible |
| Are exact duplicates real repeated events? | No. They are ingestion duplicates and are counted once | 12 pairs with adjacent ids and every column equal, down to the second |
| Does a site exist before its first event? | No. Weeks up to and including a site's first-activity week don't count toward its baseline | Every site's first activity is in the week of Jan 26 or Feb 2, so this only matters for early weeks |
| Is "All" a count of customers? | No. It counts inbound **events**; a call, a lead and an appointment can be the same person. The UI says "inbound events, not unique customers" | There is no customer id in `activity_events` |

---

## Data handling

| Messy part | What the app does |
|---|---|
| **Exact duplicates** | 12 pairs (adjacent ids, every column equal) are counted once at query time. The raw rows are kept, so the data is used as-is: 12,626 rows give 12,614 events. The page footer says "Exact duplicates counted once" |
| **Near-duplicates** | 27 near-duplicates within 60 seconds look like ordinary traffic and are **kept** |
| **Account 6 spike** | 880 events in the week of Jun 1 (805 on Jun 3) against about 70 a week. That week shows ▲ Higher than usual at all 15 sites. It is never removed. Because the baseline uses the median, the week of Jul 20 still reads 87 vs usually 30–134, within range. A mean baseline would have been 171 |
| **Empty account (20)** | Zero events is a valid state, not an error. The API returns 200 with count 0, and the page shows "No activity recorded for this account yet." with the whole week control (both stepper buttons and the week picker) disabled |
| **Partial weeks** | Only complete weeks are shown. The data ends on a Monday, so the week of Jul 27 is never compared |
| **Time zones** | Weeks run Monday 00:00 to Monday 00:00 in the account's IANA time zone and are converted to UTC windows (DST-aware). The events are stored in UTC |
| **Silent weeks** | A week with no events at a site counts as 0. It is not skipped. A site with nothing in the selected week is still listed, with 0 |
| **Early history** | A baseline week counts only once the site has started sending data. With fewer than 4 such weeks the page says "Not enough history yet (N of 4 weeks needed)" instead of guessing. Example: account 8 in the week of Mar 2 |
| **NULL outcomes and call durations** | About 400 NULL outcomes and 313 NULL durations. They don't affect counts. They are one reason outcome rates are deferred |
| **Small counts** | Per-site weekly medians are 3–6, so ranges are wide by necessity. See the limits below |

---

## Known limits

What the dashboard can and cannot detect. These bullets are quoted verbatim from the statistician (`docs/design-consensus.md` §1):

- A location that usually gets ≤ 2 a week can never show "lower".
- A drop to 0 is caught about 90 % of the time at 4+ a week, and about 65 % at 3.
- With a 4-week baseline, false flags rise by about 1 point per side.
- Design flag rate: about 4 % of site-weeks; 13 % of account-weeks show at least one flagged location (`product_monday_view_out.md`).
- A halving at a single site is usually not caught in one week. Of 99 real halvings at median ≥ 6, 34 were flagged and 65 were not (`reviewer_default_week_out.md`).
- A spike stays in the baseline for 7 weeks and widens ranges by about 20–30 % (true of every rule).
- Per-type filters at site level are thin.

Also:
- **By design, about 4 % of location-weeks are flagged** (4.3 % measured) even when nothing has changed. That is the cost of catching 96–98 % of drops to zero.
- **The seed is not bursty.** Its week-to-week variance is about equal to its mean, which is what the rule was calibrated on. On synthetic bursty data, false flags rise to about 5–8 % per side.
- **Time zones whose DST change falls at local midnight** (Monday 00:00 skipped or repeated) are out of scope. No seed zone is affected.
- **"All activity" can hide a single-type change.** Combined totals have wider ranges, so a change in one type at one location may only show when that type is selected. A per-type hint in "All activity" is listed under [another day](decisions.md#with-another-day).
