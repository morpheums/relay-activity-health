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
