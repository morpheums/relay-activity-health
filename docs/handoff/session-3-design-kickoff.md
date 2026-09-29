# Session 3 kickoff — UI/UX redesign (paste into a new `claude` session started in this repo)

You are the **coordinator** for DASH-247. Read `CLAUDE.md`, `PLAN.md` (§13 is the record of decisions), and `AI_LOG.md` (last entry is 43; append from 44) before acting.
The standing workflow is in `CLAUDE.md`:
- every agent works in its own worktree;
- the reviewer runs after every feature;
- decisions go to the user, then the architect;
- agents stay in their specialty;
- use one-shot subagents.

## State at handoff (main `c94541b`)
- Functionality is accepted by the user. There are 173 backend tests and 124 web tests, pruned per §13 "Test suite pruned to business value", all green. The full pre-submission review and product acceptance both passed.
- One-command local start: `cp .env.example .env` (set the password), then `scripts/dev.sh`. A local `.env` already exists in the main checkout. It is git-ignored; never print it.
- New agent: `designer` (`.claude/agents/designer.md`).

## Task: UI/UX redesign, design first
The user asked for:
- a proper week date picker;
- a header and a footer;
- a proper minimalist, modern design.

The user reviews and approves a **design artifact first**. No Angular changes until they approve.

1. Dispatch `designer` to write the mockups as files for the existing design canvas:
   - Canvas: https://claude.ai/artifact/45DqxHV91b5QgrrxhxKqtn (created from the Design type, currently empty).
   - The designer writes `project/canvas.json` and `project/*.dc.html` into a scratchpad folder.
   - The coordinator publishes them to that `url` with `root` set to the folder, `project/canvas.json` first. Read the canvas first (`action: "read"`) to get the type's format rules.
   - Artboards:
     - desktop default: account 14, week of 07-20;
     - the date picker open (only weeks between `earliestWeek` and `latestCompleteWeek` selectable);
     - the account 6 spike;
     - states: insufficient data, empty account 20, loading, error;
     - mobile 390×844;
     - the design spec: tokens, status badges, the picker library choice.
   - Use real golden values only. Use copy from `docs/acceptance-criteria.md`; new header, footer and picker copy is a proposal for `product`.
2. Bring the canvas to the user. Once they approve:
   - Product finalises the copy.
   - The architect validates any behaviour or contract impact.
   - Frontend adds the date picker library (Angular Material is the leading candidate) and implements.
   - Then the reviewer, then merge.

## Still open from session 2
- **Playwright (Phase 4).** Branch `e2e-scaffold` (`ea4010a`) is unmerged. It has the Playwright tooling and a placeholder spec, and it has not been reviewed yet. It deviates from PLAN §13: Playwright starts only `ng serve`, and the API must already be running (`scripts/dev.sh`).
  - Confirm that with the user, record it in §13, then review and merge.
  - Test-author then writes lean `web/e2e/*.e2e.ts` specs, proven by fault injection, after the redesign lands so they target the final UI.
- **AI_LOG.** Entries 44+ are needed for:
  - the DotNetEnv/.env work;
  - the test prune;
  - `scripts/dev.sh`;
  - the designer agent and the redesign.
  Then run `scripts/export-ai-log.sh` and check that the leak counts (email and old password) are 0 before committing.
