# Battle-testing PLAN.md against the seed (before any product code)

PLAN.md draft at commit `7ca4f8a` was checked by independent agents before implementation. Scripts: `analysis/`.

| Check | Agent | Result | File |
|---|---|---|---|
| Can the spec be implemented unambiguously? | general-purpose (Opus), saw only PLAN.md + seed | **9,044 / 9,044 cells identical** to the reference model; 9 ambiguities listed | `independent-implementation-ambiguities.md` |
| Does it hold on real SQL Server? | general-purpose (Opus), throwaway container | Seed loads as one batch in ~1 s; **5,851 / 5,851 counts identical**; planned index unused (fix: INCLUDE `duration_seconds`, `outcome`); naive `NOT EXISTS` dedup misses 4 of 12 NULL-column duplicates; collation is case/trailing-space insensitive; OPENJSON drops offsets | `sqlserver-findings.md` |
| Does the plan meet the brief? | reviewer (Sonnet) | All golden values reproduced; 7 findings (drop-to-zero blind spot, `weeksUsed`, `earliestWeek`, unowned README sections, DB time risk, ranking symmetry, 4-week calibration) | `plan-review.md` |
| Full sweep of the band rule | coordinator | Lower band edge is 0 for 73.5 % of site-weeks → drops to zero never flagged for leads/appointments, 37 % for calls | `analysis/rule_comparison.py` |
| What do analytics products do? | general-purpose (Opus), web research | Expected value + band is the norm; weekly grain uses simple robust statistics; no product documents count-specific bands; recommends ±3 / ≥6 weeks (on synthetic data) | `industry-survey.md` |
| Which normality rule? | statistician | see `statistician-report.md` | `statistician-report.md` |

Outcome: the design is settled in `docs/design-consensus.md` by a direct agent debate, then applied to PLAN.md §13 after user approval.
