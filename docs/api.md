# API reference

[← README](../README.md) · [Running](running.md) · [Testing](testing.md) · [API](api.md) · [Interpretation](interpretation.md) · [Decisions](decisions.md) · [Architecture](architecture.md) · [Dashboard](dashboard.md) · [Deferred](deferred.md)

The two endpoints, their validation rules, status codes and response contract (PLAN §13 §5.2).

## API reference

JSON over HTTP on `http://localhost:5080`. Errors are `application/problem+json` (RFC 9457 `ProblemDetails`). Enum values are snake_case and case-sensitive.

### `GET /api/accounts`

Returns every account, **including account 20, which has no events**. Ordered by name (ordinal; ties by id).

```json
[ { "id": 14, "name": "Beacon Home Security", "timezone": "America/New_York" }, … ]
```

### `GET /api/accounts/{accountId}/activity-health?week=YYYY-MM-DD&type=all`

Last week's health for the account and each of its locations.

| Parameter | Required | Default | Rule |
|---|---|---|---|
| `accountId` (route) | yes | — | Integer (`{accountId:int}`). Unknown or non-numeric → **404** |
| `week` (query) | no | `latestCompleteWeek` (2026-07-20 on the seed) | Strict `yyyy-MM-dd` date. It must be a Monday in the account's time zone, no earlier than `earliestWeek` and no later than `latestCompleteWeek`. To get the default, **omit** the parameter; `week=` (empty) is rejected |
| `type` (query) | no | `all` | Exactly one of `all`, `call_received`, `lead_created`, `appointment_set`, case-sensitive |

**Checks run in this order** (the first failure wins):

1. **Malformed input → 400 validation problem.** The body's `errors` holds exactly one key, `Week` or `Type`.
   - Rejected `week` values: `abc`, `2026-13-01`, `2026-02-30`, `20260720`, `07/20/2026`, `2026-7-20`, `2026-07-20T00:00:00`, and an empty value.
   - Rejected `type` values: `ALL`, `Call_Received`, `calls`, `1`, and an empty value.
2. **Unknown account → 404.**
3. **`week` is not a Monday → 400.**
4. **`week` is before `earliestWeek` or after `latestCompleteWeek` → 400.** For account 20 the only valid week is 2026-07-20.

| Status | When |
|---|---|
| 200 | Success. This includes an account with no events (empty state) |
| 400 | Validation problem (above), or one of the week rules |
| 404 | Unknown or non-numeric account |
| 500 | Unexpected error. `ProblemDetails` without the exception message, type or stack trace, in every environment. It also covers the out-of-scope case of a time zone whose DST change skips local midnight |

**Example** (account 6, week 2026-07-20, `all`; trimmed to one location, the §13 §5.2 contract):

```json
{
  "account": { "id": 6, "name": "Metro Collision Centers", "timezone": "America/New_York" },
  "eventType": "all",
  "week": { "start": "2026-07-20", "end": "2026-07-26" },
  "dataAsOf": "2026-07-27T22:20:34Z",
  "latestCompleteWeek": "2026-07-20",
  "earliestWeek": "2026-01-26",
  "baselineWeeks": 8,
  "minimumEligibleWeeks": 4,
  "summary": { "count": 87, "baseline": { "weeksUsed": 8, "median": 72.5, "low": 30, "high": 134 }, "status": "normal", "deviation": 0.53 },
  "locations": [
    { "location": "Site M", "count": 7, "baseline": { "weeksUsed": 8, "median": 3.5, "low": 1, "high": 9 }, "status": "normal", "deviation": 1.3 }
  ]
}
```

| Field | Meaning |
|---|---|
| `status` | `above` \| `below` \| `normal` \| `insufficient_data` |
| `baseline` | **Always present.** For `insufficient_data` it is `{ "weeksUsed": 0–3, "median": null, "low": null, "high": null }` and `deviation` is `null` |
| `low`, `high` | Integers: the "usually low–high" range. The status is read from them (`count < low` → `below`, `count > high` → `above`) |
| `median` | Unrounded (always x or x.5) |
| `deviation` | A z-score on the Anscombe scale, rounded to 2 dp away from zero. It is used for ranking and never shown on screen |
| `locations` | Already sorted (see [How "normal" is decided](interpretation.md#how-normal-is-decided)) |
| `earliestWeek` | Local Monday of the account's first event. Never null: for an account with no events it equals `latestCompleteWeek` |
| `dataAsOf` | The latest event in the whole dataset (UTC), the same for every account. It is `null` only when the database has no events at all |

**Empty account (20):** 200 with `summary.count` 0, `status: "insufficient_data"`, `baseline.weeksUsed` 0, `earliestWeek` = `latestCompleteWeek` = `2026-07-20`, and `locations: []`.
