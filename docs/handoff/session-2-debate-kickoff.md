# Session 2 kickoff — design debate (paste into a new `claude` session started in this repo)

You are the **coordinator** for DASH-247 in `relay-activity-health`. Session 1 (planning, battle-testing) is exported in `ai-log/`. Before acting, read:
`CLAUDE.md`, `PLAN.md` (draft — §13 is where approved changes go), `AI_LOG.md` (keep appending live; next entry is 17), `docs/battle-test/README.md` and every file it links.

## Standing rules from the user
- The main thread coordinates only; no product code. Still in planning — **no implementation until the user approves the revised PLAN.md.**
- Specialist agents debate **directly with each other** via `SendMessage` and reach an agreed design. **You do not send messages into the debate, relay, summarise to them, or steer.** You launch it, wait, then bring the result to the user.
- Models: Opus for all agents except `reviewer` (Sonnet) — as set in `.claude/agents/*.md`.

## Your task
Launch the debate: four background agents in one message, using the project agent types, each with a `name` so they can address each other:

| name | subagent_type |
|---|---|
| `statistician` | statistician |
| `product` | product |
| `architect` | architect |
| `reviewer` | reviewer |

Give each the **same debate brief** below (plus one line naming its own role in the debate). Then wait for completion without intervening. When done:
1. Read `docs/design-consensus.md`. Verify every numeric claim in it has a script/output behind it (spot-check by running `analysis/` scripts yourself).
2. Present to the user: agreed decisions, recorded dissent, open items needing their call. Do not edit PLAN.md until the user approves.
3. Log the debate in `AI_LOG.md` (who argued what, where positions changed, what the user decided) and run `scripts/export-ai-log.sh`; commit.

## Debate brief (give verbatim to all four)
> You are one of four specialists — `statistician`, `product`, `architect`, `reviewer` — who must agree the revised design for DASH-247 before any code is written. Talk **directly** to the others with `SendMessage` (by name). No coordinator will intervene or relay; the outcome is yours.
>
> **Read first:** `CLAUDE.md`, `PLAN.md` (§1–§7, §11), `docs/battle-test/README.md` and all files it links (statistician report, industry survey, SQL Server findings, independent-implementation ambiguities, plan review), and the brief at `../Requirements.md`.
>
> **Agenda (decide each):**
> 1. **Normality rule** — method, threshold, minimum eligible weeks, behaviour for very small medians, spread floor. Must catch a normally-busy location going quiet; must survive account 6's spike in the baseline; displayed range must never contradict status; no ML/forecasting; plain C#.
> 2. **Presentation** — statuses, "usually X–Y" wording, severity tier or not, ranking (magnitude only, or "below" first), what the admin sees for insufficient data.
> 3. **Contract gaps** — `baseline.weeksUsed` when insufficient; `earliestWeek` definition and requests before it; site existence under a type filter; rounding and band-edge handling; anything else in the ambiguities file.
> 4. **Data layer** — adopt the SQL Server findings (index INCLUDE columns, NULL-safe dedup only, UTC `Z` windows, exact `type` validation in the API, `DateTimeKind.Utc`).
>
> **Protocol:**
> - Round 0: each of you sends your opening position on all four items to all three others (one message each).
> - Then debate item by item, max **3 rebuttal rounds per item**. Address arguments, not people. Change your mind when the evidence says so, and say so explicitly.
> - **Evidence rule:** any claim about behaviour on the data must cite output from a script run on the seed (`analysis/` or a new script in `analysis/debate/`). The statistician runs simulations on request; anyone may.
> - An item is **decided** only when all four agree. If not agreed after 3 rounds, record it as **open** with each position and the evidence, for the user.
> - Budget: the whole project has ~3 h left for implementation; weigh complexity accordingly (the brief rewards a correct, small slice).
> - **Scribe: `architect`.** Writes `docs/design-consensus.md`: for each item — decision, rationale, rejected options, evidence (script + numbers), and any dissent; then the exact replacement text for PLAN §5.2 / §5.3 / §7 golden values under the agreed rule. Sends it to the others; each replies `AGREE` or `AGREE WITH DISSENT: …` or `DISAGREE: …`. Final only when all four have replied AGREE or AGREE WITH DISSENT.
> - Nobody edits `PLAN.md`, `CLAUDE.md`, or writes product code/tests. Only the scribe writes `docs/design-consensus.md`; others may write scripts under `analysis/debate/`.
> - When final, each agent ends with a short handoff (CLAUDE.md format) stating its own final position.
