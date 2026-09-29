# DASH-247 design

Canvas URL (live review copy): https://claude.ai/artifact/45DqxHV91b5QgrrxhxKqtn

The files are Design Component (`.dc.html`) sources for that canvas. They load the canvas runtime (`./support.js`), so they don't render standalone; view them on the canvas. `canvas.json` holds the board layout.

`Spec.dc.html` is the implementation spec for frontend.

Approved by the user on 2026-09-29: desktop only, no pagination, Geist type, light red for higher.

## Artboards

- `Main.dc.html`: desktop, account 14, default week of Jul 20.
- `Picker.dc.html`: desktop, week picker open, keyboard focus on Jul 13.
- `Spike.dc.html`: desktop, account 6, spike week of Jun 1 with flagged locations.
- `SpikeAfter.dc.html`: desktop, account 6, the week after the spike (Jun 8).
- `InsufficientData.dc.html`: not enough history state (account 14, week of Feb 2).
- `EmptyAccount.dc.html`: empty account state (account 20).
- `Loading.dc.html`: first load state, week slot held.
- `Error.dc.html`: load failed after a week change.
- `Spec.dc.html`: design spec with tokens, badges, picker and library choice.
- `PickerOptionA.dc.html`: week picker option A, keep MatCalendar. CHOSEN by the user on 2026-09-29.
- `PickerOptionB.dc.html`: week picker option B. Rejected; kept for the record.
- `PickerOptionC.dc.html`: week picker option C. Rejected; kept for the record.
- `LatestWeekPlacement.dc.html`: where the "Latest week" shortcut goes. Chosen: the footer variant, so "Latest week" goes in the picker footer.
- `FooterOptionA.dc.html`: footer option A, grouped columns with subheads. Rejected; kept for the record.
- `FooterOptionB.dc.html`: footer option B, fact strip with icons and caveats below, All activity state. CHOSEN by the user on 2026-09-29, with green fact tiles and a more prominent date.
- `Piece-iynb.dc.html`: the Leads/C-14 state of footer option B (account 6, week of Jun 29), same green tiles and date.
- `FooterOptionC.dc.html`: footer option C, freshness tag and ruled notes. Rejected; kept for the record.

## Footer tokens and styles (option B)

Fact tiles only; nothing else on the page uses these:

- `--color-info-tint: #E6F7E9`: tile background.
- `--color-info-border: #B8D8BD`: tile border and the icon circle's border (the circle itself is `#FFFFFF`).
- `--color-info-icon: #498D5A`: tile icon stroke (decorative, `aria-hidden`).
- Tile text stays `--color-ink` `#1A1B1E`, 14px/20px, weight 500, about 15.2:1 on `#E6F7E9`.

"Data as of" date: one `<p>` after the h2, text verbatim, vertically centred with the h2.

- Outlined tag, not a filled pill and not green: `#FFFFFF` background, 1px `--color-line` `#E3E1DC` border, radius 8px, padding 5px 12px, min-height 32px, gap 8px.
- Text `--color-ink` `#1A1B1E`, 14px/20px, weight 500, tabular numbers: about 17:1 on `#FFFFFF`.
- Clock icon 16px, stroke `#1A1B1E`, `aria-hidden`.
- The border is decorative; it is lighter than `--color-control-border` so the tag does not read as a button.
