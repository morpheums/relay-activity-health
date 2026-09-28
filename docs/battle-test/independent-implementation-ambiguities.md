# Ambiguities in PLAN.md §5.3 (independent Python implementation)

Implementation: `impl.py` -> `cells.csv` (9,044 data rows), `derived.json`. All §7 golden values reproduce exactly
(acct 6 Jun 1: 880 / median 66 / 35-97 / above, 15/15 sites above, top Site C 67 dev 36.95; acct 6 Jul 20 all: 87 / 72.5 / 24-121;
call_received 51 / 42 / 13-71; acct 12 Jul 20: 54 / 56 / 39-73, Site F 11 vs 1-10 dev 2.35 rank 1; acct 1 Site C Jul 6 = 4;
acct 8 Mar 2 insufficient (3), Mar 9 median 11 spread 3.3166 range 5-17; §5.2 example Site M count 7 dev 1.57 also matches).

## A. Real ambiguities (more than one reasonable reading)

1. **"First event" under a type filter (HIGH impact).** §5.3.1 and .2 say "site's first event" / "account's first event" without
   saying whether it is the first event *of the selected type* or of any type.
   *Picked:* any type (site existence is a property of the site, §4 "Does a site exist before its first event?"), used both for the
   site list (rule 1) and for eligibility (rule 2). *Alternative:* first event of the selected type. On this seed 81 of 264
   (series, type) combos have a different first week; the alternative changes 833 rows' eligible_weeks/stats and **111 statuses**
   (eligibility only; it would change even more if also applied to the site list). This is the single biggest divergence risk.
2. **Site list under a type filter.** Rule 1 doesn't mention type. *Picked:* the same site set for every type (a site with no
   events of that type in W still appears with 0). No site in the seed lacks any type entirely, so only the timing issue in #1 matters.
3. **`earliestWeek` is never defined** (only appears in the §5.2 example = 2026-01-26 for account 6, and as the stepper bound in §5.4).
   Readings: (a) local week containing the account's first event, (b) global first-event week, (c) first week with sufficient data,
   (d) the first week with any eligible baseline. *Picked:* (a); account 6's first event (2026-02-01 12:06Z, a Sunday) gives 01-26, consistent
   with the example but (b) gives the same value for account 6, so the example doesn't disambiguate. Per-account values: 01-26 for
   1,4,5,6,7,12,14,18; 02-02 for the rest; **account 20 = null** (no events; spec silent).
4. **Weeks before `earliestWeek`.** §5.2 only rejects weeks *after* latest complete. Whether earlier weeks are 400 or return zeros is
   unspecified. For the CSV I emit every week from 01-26 anyway (total row, count 0, insufficient; no site rows).
5. **Ordering of `insufficient_data` sites among themselves** (rule 8 says only "last"). *Picked:* location name ascending
   (applying the secondary key). Affects 32 (account, type, week) groups that mix evaluated and insufficient sites, plus all-insufficient groups.
6. **Ranking on rounded vs unrounded deviation.** Rule 7 says rounding is "for display only", so I rank on the full double; ties are then
   broken by name. 932 site rows share an |deviation| (at 6 dp) with a sibling; 269 of those are opposite-sign ties (e.g. +1.0 vs -1.0),
   ranked together by magnitude then name per §7 "above and below ranked by magnitude together". Ranking by the 2-dp value would
   create additional ties. I verified no case where float ordering contradicts name ordering among 6-dp ties.
7. **Name ordering collation** ("location name ascending"): ordinal vs culture. Irrelevant on seed ("Site A".."Site O"), used ordinal.
8. **Exact-duplicate definition vs NULLs.** "Every column equal" — whether NULL = NULL. *Picked:* NULLs compare equal (GROUP BY
   semantics). 12 pairs removed (12,626 -> 12,614); none is a triple.
9. **Which instant is "now"/data anchor**: max over raw rows (duplicates don't matter) = 2026-07-27T22:20:34Z.
10. **Latest complete week**: local week containing the anchor minus 7 days (i.e. the week whose local end <= anchor).
    "Latest complete local week" could also be read against the account's *own* last event (D1 explicitly rejects that). = 2026-07-20 for all 20
    accounts (in every account tz the anchor falls on local Monday 07-27). Default week = the same, per account.
11. **"Before the end of W"** (rule 1): strict `<` local next-Monday 00:00 converted to UTC; an event exactly at Monday 00:00 belongs to the next week (§7).
12. **Median of 8 when fewer than 8 eligible**: I take the median/MAD over only the eligible weeks (4-8 values); ineligible zero-filled weeks are dropped,
    not counted as zeros. Rule 2 says "zero-filled" then restricts to eligible, so zeros only survive for eligible weeks with no events.
13. **MAD definition**: median of |x - median| over the same eligible values (no scale besides the explicit 1.4826). Not stated but standard.
14. **Account-total series for an account with no events** (acct 20): first event undefined -> 0 eligible weeks -> insufficient; `locations` empty. Matches §5.2.
15. **Status boundary** uses exact double comparison `count > median + 2*spread`. 111 cells have count *exactly* on an edge (all from
    perfect-square medians, where sqrt is exact in IEEE, e.g. median 4 -> spread 2 -> edges 0/8); all are `normal` per §7. An implementation
    comparing against a rounded or decimal-converted spread (e.g. C# decimal of sqrt) could flip these. No near-integer (inexact) edges exist in the seed.
16. **Output precision**: rule 7 rounds deviation to 2 dp for display; median/spread/low/high rounding in the API is not specified. I emit 6 dp unrounded.

## B. Things in the spec I think are wrong, loose, or inconsistent

- §5.3.2 vs §4: consistent ("after the week containing the first event" == "weeks before and including first-activity week don't count"),
  but §5.3.1 includes a site in W's list as soon as its first event is in W, so in its first week the site appears with 0 eligible weeks — fine but worth a UI note.
- §5.3.6 claims `low/high` are "equivalent to the rule" — true only because counts are integers and low is clamped at 0; `low = 0` when
  median - 2*spread is negative means "below" is unreachable, which the displayed range conveys correctly. OK, but the "equivalence" breaks if anyone rounds spread first.
- §5.2 says `week` after latest complete -> 400 but is silent on weeks before `earliestWeek` and on `earliestWeek` for an empty account (see A3, A4).
- §2 "All sites' first activity is in week Jan 26 or Feb 2" — confirmed (16 / 53 of 69 sites). "Local vs UTC bucketing moves only 8 events" — confirmed (8).
- §2 says the account's week is "Jul 20-26" as the default for everyone because it's global-anchor based; with a per-account timezone east of UTC+1:40
  the anchor would already be Tuesday locally — still week 07-27 partial, so no inconsistency on this data, but the spec never states how
  the per-account latest complete week relates to the single global `dataAsOf` beyond "latest complete local week".
- `baseline.weeksUsed` in §5.2 is not defined; I assume = number of eligible weeks (my `eligible_weeks` column).
