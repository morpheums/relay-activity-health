# analysis/

Throwaway-quality Python used to battle-test PLAN.md against the seed before any product code existed. Not part of the product; standard library only (Python 3.9+).

| Script | What it is |
|---|---|
| `reference_model.py` | Coordinator's reference implementation of PLAN §5.3 → `cells.csv`, `counts.csv`, `derived.json` |
| `independent_model.py` | Written by a separate agent that saw only PLAN.md; matched the reference on all 9,044 cells |
| `rule_comparison.py` | Simulates candidate normality rules (false flags, drop/rise detection, spike handling) |

Run from this folder: `python3 reference_model.py` (outputs land in the current directory).
Findings: `docs/battle-test/`.
