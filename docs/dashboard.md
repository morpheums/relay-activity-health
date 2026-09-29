# The dashboard

[← README](../README.md) · [Running](running.md) · [Testing](testing.md) · [API](api.md) · [Interpretation](interpretation.md) · [Decisions](decisions.md) · [Architecture](architecture.md) · [Dashboard](dashboard.md) · [Deferred](deferred.md)

What the admin sees on the page, part by part. What the default view shows on first open: [running.md](running.md#step-by-step-windows-or-any-os) ("What you should see").

One page, desktop only (no mobile layout) and no pagination: every location of the account is listed. The approved mockups and design spec are in [docs/design/](design/) (see [its README](design/README.md) for the artboards and the choices behind them).

| Part | What the admin sees |
|---|---|
| Header | `Relay` · `Customer admin`, plain text (not a link, so it never resets the URL) |
| Filters | "Viewing as" (account), "Week" and "Activity type" (All activity, Calls, Leads, Appointments) |
| Week control | `◀ Previous week`, a week button showing the week label, and `Next week ▶` |
| Week picker | The week button opens a calendar (Angular Material). Only Mondays from the account's first week to the latest complete week can be chosen, and the chosen week is shaded Monday to Sunday. Helper lines: "Weeks run Monday to Sunday." and "Weeks from Mon Jan 26 to Mon Jul 20, 2026" (both dates come from the API). A **Latest week** button jumps to the latest complete week; it is disabled when you are already there, and it never picks the week in progress. Works by keyboard; Escape closes it |
| Summary | The account total, its usual range and its status |
| Locations table | "Locations — most unusual first": count, "Usually X–Y a week" and status per location |
| Footer | "About these numbers", with the "Data as of" date beside the heading. Three fact tiles: "Compared with the last 8 full weeks at this location", "Inbound events, not unique customers", "Exact duplicates counted once". Under **Keep in mind**: "Locations that usually get 2 or fewer events a week can't show 'lower than usual'", plus "Per-type counts at a single location are small; only large changes show up." when a single activity type is selected |

- **Status is never colour alone.** Every status shows a symbol and words ("▲ Higher than usual", "▼ Lower than usual", "Within usual range", "Not enough history yet (N of 4 weeks needed)"). Colour only adds direction: light red for higher, blue for lower. The green of the fact tiles is the one colour not tied to status.
- **Every filter lives in the URL**, so a reload, Back or a shared link shows the same view. An invalid parameter is reset to its default and the URL is rewritten.
- **States:** "Loading…" while loading; an error card, "We couldn't load this week's activity. Try again.", with a "Try again" button; for account 20, "No activity recorded for this account yet." with the whole week control disabled.
