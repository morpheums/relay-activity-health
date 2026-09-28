## 1. Full sweep: status from integer range vs |z|>2, every (account, series, type, week) with >=4 eligible weeks
evaluated cells 7136; status/z disagreements 0; cells with |z| within 1e-9 of 2: 0; statuses {'insufficient_data': 2016, 'normal': 6803, 'below': 163, 'above': 170}

## 2. Hand-checkable edge cases (baseline -> count: median, low-high, status, z)
[0, 0, 0, 0] -> 0: median 0.0, 0-2, normal, z=0.00
[0, 0, 0, 0] -> 3: median 0.0, 0-2, above, z=2.45
[1, 1, 1, 1] -> 0: median 1.0, 0-4, normal, z=-1.12
[2, 2, 2, 2] -> 0: median 2.0, 0-6, normal, z=-1.86
[3, 3, 3, 3] -> 0: median 3.0, 1-7, below, z=-2.45
[4, 4, 4, 4] -> 0: median 4.0, 1-9, below, z=-2.96
[1, 0, 5, 0, 6, 1, 0, 1] -> 0: median 1.0, 0-7, normal, z=-0.67
[11, 11, 11, 8] -> 6: median 11.0, 6-18, normal, z=-1.70
[11, 11, 11, 8] -> 5: median 11.0, 6-18, below, z=-2.11
[3, 3, 4, 4] -> 2: median 3.5, 1-8, normal, z=-0.85

## 3. Default-week (2026-07-20, all) flagged series under R2* for every account: see product_default_account_out.md. Detail for account 14:

### account 14, week 2026-07-20, type all
| series | count | median | range | status | deviation (z) | current rule (low,high,status,dev) | baseline |
|---|---|---|---|---|---|---|---|
| TOTAL | 26 | 27.0 | 18–38 | normal | -0.19 | (17, 37, 'normal', -0.19) | [23, 16, 24, 29, 28, 27, 29, 27] |
<!-- total centreT=10.464225 spreadT=1.000000 -->
| 1. Site B | 2 | 6.5 | 3–12 | below | -2.16 | (2, 11, 'normal', -1.77) | [5, 6, 8, 7, 2, 7, 4, 8] |
| 2. Site C | 9 | 6.0 | 2–12 | normal | 0.98 | (1, 11, 'normal', 1.01) | [3, 4, 6, 8, 8, 9, 6, 5] |
| 3. Site A | 9 | 6.5 | 2–16 | normal | 0.61 | (0, 13, 'normal', 0.67) | [6, 1, 4, 7, 13, 4, 9, 8] |
| 4. Site D | 6 | 6.5 | 3–12 | normal | -0.19 | (2, 11, 'normal', -0.2) | [9, 5, 6, 7, 5, 7, 10, 6] |
Site B baseline [5, 6, 8, 7, 2, 7, 4, 8] median 6.5; P(X<=2 | Poisson(median)) = 0.0430

## 4. PLAN 5.2 example (account 6, 2026-07-20, all) under R2*
summary: count 87 median 72.5 low 30 high 134 weeksUsed 8 status normal deviation 0.53
Site M: count 7 median 3.5 low 1 high 9 status normal deviation 1.30

## 5. Type filter: eligibility from first event of ANY type (spec reading) vs first event OF THE TYPE, R2*
status changes (any-type -> of-type): {('normal', 'insufficient_data'): 99, ('above', 'normal'): 2, ('below', 'insufficient_data'): 3, ('normal', 'above'): 2, ('above', 'insufficient_data'): 3, ('normal', 'below'): 1} total 110

## 6. Ranking ties at full precision among evaluated site rows (same group, |z| equal to 1e-12)
rows in |z| ties: 1161; tie groups mixing above and below: 0
