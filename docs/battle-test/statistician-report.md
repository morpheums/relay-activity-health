# Statistician report — normality rule (verbatim final handoff)

The agent's harness blocked it from writing a report file; this is its final message, unedited. Scripts: `analysis/statistician/` (re-run from the repo; `golden.py` reproduces `statistician-outputs/golden_out.md` exactly). Raw outputs: `statistician-outputs/`.

## Summary
The current rule needs replacing. Its band is symmetric, but small counts are skewed, so it misses drops at small counts and flags too many rises. I recommend one rule, **R2\***: a robust z-score on the Anscombe scale, T(x) = 2√(x+3/8), with k = 2 and a spread floor of 1. The displayed range is converted back to counts and the status is read from that range, so the two can't contradict each other.
All the claims in your brief reproduced exactly. The current rule's PLAN §7 golden values also reproduced, which confirms the harness is sound. One small bug in `alt.py`: `floor(lo)+1` and `ceil(hi)-1` should be `ceil` and `floor`. It made no difference on this data.
**Candidates tested:** current rule (control); current + zero override; Anscombe; Freeman–Tukey; Poisson/negative-binomial exact tails. EARS was rejected because it is mean/SD-based and only flags "above". Farrington/Noufaily were rejected as fitted models, which are out of scope.
## Comparison (site level, real weeks with a full 8-week baseline, spike week excluded)
| Rule | False above / below | Drop→0 (all / calls) | +100% (all) | Low edge = 0 | Contradictions (full sweep) |
|---|---|---|---|---|---|
| Current | 3.8 / 1.0% | 78 / 37% | 78% | 22% | 0 |
| Current + zero | 3.8 / 1.1% | 100 / 100% | 78% | 22% | **1,426** |
| **R2\*** | **1.4 / 2.9%** | **98 / 96%** | 48% | 2.4% | **0 of 253,149** |
| Freeman–Tukey | 1.4 / 2.9% | 99.5 / 97% | 48% | 0.5% | 0 |
| Poisson/NB | 1.7 / 1.2% | 89 / 56% | 57% | 11% | 0 |
- **Account level (all types):** R2\* gives 1.6 / 3.2% false flags, 100% drop→0, 73% for a halving, 97% for a doubling.
- **Synthetic Poisson data:** R2\* is balanced at about 2 / 3% above / below. The current rule is lopsided at 3–8% / 0–2%.
- **Freeman–Tukey is rejected:** it gives 7% false "below" flags at a median of 2.
- **Doubling detection is lower under R2\* because it's correct:** the current rule's upper edge sits below the Poisson 2.5% point, so it "catches" rises that aren't unusual.
- **Spike week:** every rule flags account 6 above in all types. For all types, calls and leads that's 15 of 15 sites; for appointments, 13–14.
- **After the spike:** it stays in the baseline for 7 weeks and widens the range by about 20–30%. This affects every rule.
## Thresholds
At k = 2, R2\* flags 4.3% of site-weeks in total, the same budget D7 already accepted (4.8%).
- **k = 1.75:** false "below" doubles to 5.5%.
- **k = 2.5:** drop→0 for calls falls to 70%.
- **Poisson/NB:** needs α ≈ 5% to match R2\*'s drop detection, which costs more false flags.
## Recommended formula (C# `double`)
1. `median` = raw median of the eligible baseline counts. This is still the value the API shows.
2. `centre = 2*Math.Sqrt(median+0.375)`.
3. `madT` = median of |2√(cᵢ+0.375) − centre| over the baseline counts.
4. `spread = Math.Max(1.4826*madT, 1.0)`.
5. `lowT = centre − 2*spread`, `highT = centre + 2*spread`.
6. `low = lowT <= 1.224744871391589 ? 0 : ceil((lowT/2)*(lowT/2) − 0.375)`. **The guard is mandatory:** squaring a negative `lowT` gives a false positive edge.
7. `high = floor((highT/2)*(highT/2) − 0.375)`.
8. Status comes from the integers: `x < low` → below, `x > high` → above, otherwise normal.
9. `deviation = (2√(x+0.375) − centre)/spread`.
- **Why no contradiction:** status depends only on (x, low, high). T is increasing, so this matches |z| > 2 exactly; I found 0 disagreements.
- **Median 0:** range 0–2, the same as today.
- **Code changes:** add `SpreadFloor = 1.0`; `deviation` now means a z-score on the transformed scale.
## Short baselines (your first request)
Truncating real baselines to 4 weeks raises false flags by about 1 point per side (site/all: 2.4 / 3.5% against 1.4 / 2.9% at 8 weeks). Drop detection holds at 97%. Keep the 4-week minimum and the floor of 1.
## Ranking (your second request)
On a series with a steady median of 8:
| Week | Raw deviation (current) | R2\* z | How rare |
|---|---|---|---|
| Drop to 0 | −2.83 | −4.56 | 1 in 3,000 |
| Doubling to 16 | +2.83 | +2.31 | 1 in 120 |
On the raw scale the two tie even though the drop is 25 times rarer, so drops rank too low. On the z scale above and below are comparable to a first approximation. Keep ranking by |z| and don't put every "below" first. Break ties at equal |z| with below before above, then location name.
## Golden values under R2\*
| Scenario | Result | Was |
|---|---|---|
| Account 6, 2026-06-01, all types | 880, range 39–101, above, dev 22.37; all 15 sites above; Site C top (dev 12.74) | 35–97; Site C 36.95 |
| Account 6, 2026-07-20, all types | 87, range 30–134, normal; all 15 sites normal | 24–121 |
| Account 6, 2026-07-20, calls | 51, range 17–79, normal | 13–71 |
| Account 12, 2026-07-20, all types | 54, range 40–74, normal; **Site F 11 vs 2–11 → normal (1.90)**, still ranked first | 39–73; Site F above |
| Account 8, 2026-03-02 | insufficient data | same |
| Account 8, 2026-03-09 | range **6–18**, normal | 5–17 |
By hand for account 8 on 03-09: the baseline is 11, 11, 11, 8, so the median is 11 and centre = 2√11.375 = 6.745369. Three of the four gaps are 0, so the MAD is 0 and the spread takes the floor of 1. That gives low = ⌈2.3726845² − 0.375⌉ = ⌈5.2546⌉ = 6 and high = ⌊4.3726845² − 0.375⌋ = ⌊18.745⌋ = 18.
## Deviations and decisions for you
- **§5.3 steps 5–7 and the §7 golden values change.** These need a §13 entry.
- **D5's default no longer has a flagged site.** Site F's 11 against a median of 5.5 has a 2.53% chance under normal variation. Account 12's R2\* flags fall in other weeks: 03-02 total, 03-09 Site G, 05-04 Site A, 06-01 Site C, 06-22 Site A. Choosing a new default is a product decision.
## Uncertain
- The seed data is not bursty (variance ≈ mean). On bursty synthetic data, false flags rise to about 5–8% per side.
- One spike in the baseline still widens the range for 8 weeks.
## Limits (plain language, for the README)
- A location that usually has 2 or fewer events a week can never show "below". A drop to zero is caught about 90% of the time at 4+ a week and about 65% at 3.
- Per-type filters at location level are too thin to judge.
- A halving at a single location is rarely detectable in one week.
## Next
The test-author needs the new golden values and a test for the `lowT` guard: median 1 with spread 3 must give low = 0.
Files are in `battle/stats/` under the scratchpad:
- `rules.py`
- `sim.py` → `sim_out.md`
- `recheck.py` → `recheck_out.md`
- `montecarlo.py` → `mc_out.md`
- `explore.py` → `explore_out.md`
- `golden.py` → `golden_out.md`
- `acct12.py` → `acct12_out.md`
Sources: Montgomery (c-chart); Wheeler (XmR); [Ryan & Schwertman 1997](https://www.tandfonline.com/doi/abs/10.1080/00224065.1997.11979728); Garwood 1936; Laney 2002; [Hutwagner 2003 EARS](https://www.researchgate.net/publication/10720736_The_Bioterrorism_Preparedness_and_Response_Early_Aberration_Reporting_System_EARS); [Hutwagner 2005](https://wwwnc.cdc.gov/eid/article/11/2/04-0587_article); [Salmon et al.](https://arxiv.org/pdf/1411.1292); Farrington 1996; [Noufaily 2013](https://onlinelibrary.wiley.com/doi/10.1002/sim.5595); Iglewicz & Hoaglin 1993; Rousseeuw & Croux 1993; Anscombe 1948; Freeman & Tukey 1950.
