# Deferred work and next steps

[← README](../README.md) · [Running](running.md) · [Testing](testing.md) · [API](api.md) · [Interpretation](interpretation.md) · [Decisions](decisions.md) · [Architecture](architecture.md) · [Dashboard](dashboard.md) · [Deferred](deferred.md)

What was left out on purpose (PLAN §11, the design debate and later user decisions), and what comes next with another day. This is the single source for deferred work; [decisions.md](decisions.md) links here.

## Deliberately deferred

| Item | Why it was deferred |
|---|---|
| Outcome rates (missed-call rate, lead conversion, no-shows) | These are more actionable, but the small-number problem is worse for rates, and about 400 NULL outcomes and "missed" calls that have durations need product decisions first |
| Playwright E2E smoke tests (PLAN §13 layer 6) | Deferred by user decision (2026-09-29). The unmerged branch `e2e-scaffold` holds the tooling (`@playwright/test`, `playwright.config.ts`, an `e2e` script) and a placeholder spec. Unlike PLAN §13, its config starts only `ng serve`, so the API must already be running (`scripts/dev.sh`). There is no `npm run e2e` on `main` |
| Days from adjacent months in the week picker | They stay blank because Angular Material's calendar cannot show them (angular/components #26768, #29549, both open). User decision: keep the Material calendar rather than switch library |
| Mobile layout and pagination | Desktop only by the redesign's scope decision. Accounts have at most 15 locations, so one page holds them all |
| A second severity tier (e.g. "very unusual") | It adds a threshold to calibrate and explain. With neutral labels and ranking, the worst row is already on top |
| A sibling-share statistic | Confounded when the whole account moves, and meaningless for single-site accounts ([D2](decisions.md#design-decisions-and-trade-offs)) |
| A near-duplicate policy | 27 near-duplicates within 60 seconds look like real traffic. Merging them would be a guess |
| Spike root-cause notes | The data can't say whether Jun 3 was a storm or a bad import. That is for a human to annotate |
| Trend sparklines | Useful context, but "Usually X–Y" already answers the Monday question |
| Auth | Out of scope per the brief. "Viewing as" is impersonation for the demo |
| Caching | Not needed at seed scale (the weekly query takes milliseconds) |
| A "too few to judge" status | Covered by the footnote, at no cost to the contract or tests |

---

## With another day

In priority order.

1. **Add the E2E smoke layer (PLAN §13 layer 6), starting from the `e2e-scaffold` branch.** It is the only test that proves the browser, API and seeded DB work together; the current tests stop at each boundary. The specs target the final UI, so they come after the redesign.
2. **Outcome rates, starting with the missed-call rate.** "Calls are normal but we missed half of them" is the most actionable thing the data holds.
3. **Recalibrate on bursty data.** The ≈ 4 % flag rate holds only for Poisson-like data. Real customers with campaigns would see 5–8 % false flags per side; a negative-binomial check or a per-account k would address that.
4. **Make the global data-anchor query cheap.** `MAX(occurred_at)` scans the index on every request. That is fine for 12k rows but not for production, so it should be cached or indexed.
5. **Show an error when the account list fails to load.** Today "Viewing as" is just left empty (an accepted reviewer note).
6. **Add a per-type hint in "All activity".** Combined totals have wider ranges, so a change in one type can stay hidden until the admin picks that type.
7. **Add a second severity tier.** Once there is real usage data to calibrate it on.
8. **Add a trend sparkline per location.** It helps tell a one-off week from a slide.
9. **Support time zones with a DST change at midnight.** Needed before onboarding customers outside the US zones in the seed.
10. **Trim the `DashboardPage` styles back under 4 kB.** The component style budget was raised to 6 kB for the footer ([AI_LOG.md](../AI_LOG.md) entry 56); housekeeping, no visible change.
