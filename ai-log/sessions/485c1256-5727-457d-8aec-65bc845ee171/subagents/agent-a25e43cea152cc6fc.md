
### 🧑 USER — 2026-09-28T21:32:58.241Z

The user delegated a ruling to you (DASH-247). Decide and justify in ≤8 lines; no code changes, no commit.

Question: should the calendar support time zones whose DST transition happens exactly at local midnight (Monday 00:00 skipped or repeated)? Context:
- Backend's Core implementation (branch `worktree-agent-a440e778df2f76d2c`, commit a7fb3e3, `src/Relay.Core/Calendar/WeekCalendar.cs` `LocalMidnightToUtc`) added an untested branch: skipped midnight → week starts at the jump; ambiguous midnight → .NET's default standard-time (second) reading.
- Reviewer: no test covers it; no seed zone (Chicago, Denver, Los_Angeles, New_York, Phoenix, UTC) can hit it; for an ambiguous midnight (Cuba-style 01:00 DST → 00:00 standard) `WeekContaining` (local date → Monday) and `Window(Monday).StartUtc` (second occurrence) disagree for instants in the first repeated hour, so the grid's `window.StartUtc > firstActivityUtc` rule could treat the first-event week as eligible. Windows stay contiguous (no gaps/overlaps). Reviewer's recommendation: switch to the first occurrence and add tests, or declare out of scope.
- Constraints: CLAUDE.md test-first (no untested behaviour), ~3 h budget, the user dislikes over-engineering, PLAN §7 requires only the US DST weeks, Phoenix, UTC.

Options: (A) out of scope — remove the branch, document the limitation in PLAN §13 (and README limits); what should the code do if such a zone appears (let `ConvertTimeToUtc` throw → 500? or accept default behaviour without a special branch?); (B) support properly — red tests first (which zone/dates), then first-occurrence reading. Rule A or B, state exactly what backend/test-author must do, and the one-line §13 text.


### 🤖 ASSISTANT — 2026-09-28T21:33:10.178Z

**Ruling: A. This is out of scope, and backend removes the special branch.**

1. **Why:** PLAN §7 requires only the US DST weeks, Phoenix and UTC. No seed zone can reach a midnight transition, because the US zones change at 02:00 local time. The current branch is untested behaviour, which breaks CLAUDE.md rule 4, and the reviewer showed its ambiguous-midnight reading is wrong. Supporting it properly would need a synthetic or foreign zone and new tests, which is over-engineering for a 3 h budget.
2. **Backend (`src/Relay.Core/Calendar/WeekCalendar.cs`):** change `LocalMidnightToUtc` to a plain `TimeZoneInfo.ConvertTimeToUtc(localMidnight, zone)` call with no `IsInvalidTime`/`IsAmbiguousTime` handling. .NET's default behaviour then applies:
   - **Skipped midnight:** .NET throws, so the request fails loudly with a 500 through the existing exception handler.
   - **Repeated midnight:** .NET uses the standard-time reading. The known flaw is limited to instants in the first repeated hour, and windows stay contiguous.
3. **Test-author:** add no tests. We don't test behaviour we've declared out of scope, and the existing §7 US DST, Phoenix and UTC tests must stay green.
4. **PLAN §13 line (reuse it under README limits):** "2026-09-28 — Time zones whose DST transition falls at local midnight (Monday 00:00 skipped or repeated) are out of scope: a skipped midnight fails the request (500) and a repeated one uses .NET's standard-time reading; no seed zone is affected."

I didn't change any code or make a commit.

