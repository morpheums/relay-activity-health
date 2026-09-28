import sqlite3, csv, json, math, statistics
from datetime import datetime, date, timedelta, timezone
from zoneinfo import ZoneInfo
from collections import defaultdict

from pathlib import Path
REPO_ROOT = Path(__file__).resolve().parent.parent
SQL_DIR = next(d for d in (REPO_ROOT / "db", REPO_ROOT) if (d / "seed.sql").exists())
db = sqlite3.connect(":memory:")
db.executescript((SQL_DIR / "schema.sql").read_text())
db.executescript((SQL_DIR / "seed.sql").read_text())

accounts = {r[0]: r[1] for r in db.execute("select id, timezone from accounts")}
raw = db.execute("select id, account_id, location, event_type, occurred_at, duration_seconds, outcome from activity_events order by id").fetchall()
seen = set(); events = []
for r in raw:
    key = r[1:]  # all columns except id; NULL==NULL via Python None equality
    if key in seen: continue
    seen.add(key); events.append(r)
print("raw", len(raw), "dedup", len(events), "removed", len(raw) - len(events))

def parse_utc(s): return datetime.strptime(s, "%Y-%m-%d %H:%M:%S").replace(tzinfo=timezone.utc)
def local_week(dt_utc, tz):
    d = dt_utc.astimezone(ZoneInfo(tz)).date()
    return d - timedelta(days=d.weekday())
def week_end_utc(ws, tz):
    nd = ws + timedelta(days=7)
    return datetime(nd.year, nd.month, nd.day, tzinfo=ZoneInfo(tz)).astimezone(timezone.utc)

anchor = max(parse_utc(r[4]) for r in raw)
TYPES = ["all", "call_received", "lead_created", "appointment_set"]

counts = defaultdict(int)          # (acct, series, type, week) -> count
acct_first = {}; site_first = {}   # instants (any type)
for (_id, a, loc, et, occ, _d, _o) in events:
    t = parse_utc(occ); tz = accounts[a]; w = local_week(t, tz)
    for s in ("*", loc):
        for ty in ("all", et):
            counts[(a, s, ty, w)] += 1
    acct_first[a] = min(acct_first.get(a, t), t)
    site_first[(a, loc)] = min(site_first.get((a, loc), t), t)

def latest_complete_week(tz):
    cur = local_week(anchor, tz)
    return cur - timedelta(days=7)

W0 = date(2026, 1, 26); WN = date(2026, 7, 20)
weeks = [W0 + timedelta(days=7 * i) for i in range((WN - W0).days // 7 + 1)]

def evaluate(count, baseline):
    n = len(baseline)
    if n < 4: return None
    med = statistics.median(baseline)
    mad = statistics.median([abs(x - med) for x in baseline])
    spread = max(1.4826 * mad, math.sqrt(max(med, 1)))
    lo_edge = med - 2 * spread; hi_edge = med + 2 * spread
    status = "below" if count < lo_edge else "above" if count > hi_edge else "normal"
    return dict(median=med, spread=spread, low=max(0, math.ceil(lo_edge)), high=math.floor(hi_edge),
                status=status, deviation=(count - med) / spread)

rows = []
for a in range(1, 21):
    tz = accounts[a]
    for W in weeks:
        wend = week_end_utc(W, tz)
        sites = sorted(l for (aa, l), f in site_first.items() if aa == a and f < wend)
        for ty in TYPES:
            group = []
            for s in ["*"] + sites:
                first = acct_first.get(a) if s == "*" else site_first[(a, s)]
                cnt = counts.get((a, s, ty, W), 0)
                if first is None: elig = []
                else:
                    fw = local_week(first, tz)
                    elig = [counts.get((a, s, ty, W - timedelta(days=7 * k)), 0)
                            for k in range(1, 9) if W - timedelta(days=7 * k) > fw]
                ev = evaluate(cnt, elig)
                group.append(dict(account_id=a, series=s, event_type=ty, week_start=W.isoformat(), count=cnt,
                                  eligible_weeks=len(elig), ev=ev))
            site_rows = group[1:]
            site_rows.sort(key=lambda r: (r["ev"] is None, -abs(r["ev"]["deviation"]) if r["ev"] else 0, r["series"]))
            for i, r in enumerate(site_rows): r["rank"] = i + 1
            group[0]["rank"] = ""
            rows.extend([group[0]] + sorted(site_rows, key=lambda r: r["rank"]))

f6 = lambda x: f"{x:.6f}"
with open("cells.csv", "w", newline="") as fh:
    wr = csv.writer(fh)
    wr.writerow("account_id,series,event_type,week_start,count,eligible_weeks,median,spread,low,high,status,deviation,rank".split(","))
    for r in rows:
        ev = r["ev"]
        wr.writerow([r["account_id"], r["series"], r["event_type"], r["week_start"], r["count"], r["eligible_weeks"],
                     f6(ev["median"]) if ev else "", f6(ev["spread"]) if ev else "",
                     ev["low"] if ev else "", ev["high"] if ev else "",
                     ev["status"] if ev else "insufficient_data", f6(ev["deviation"]) if ev else "", r["rank"]])
print("rows", len(rows))

lcw = {a: latest_complete_week(accounts[a]).isoformat() for a in accounts}
derived = dict(
    data_anchor=anchor.strftime("%Y-%m-%dT%H:%M:%SZ"),
    latest_complete_week=lcw,
    earliest_week={a: (local_week(acct_first[a], accounts[a]).isoformat() if a in acct_first else None) for a in accounts},
    default_week=lcw,
    default_week_note="Default week = latest complete local week of the selected account; identical (2026-07-20) for all accounts on this data.",
    exact_duplicates_removed=len(raw) - len(events),
)
json.dump(derived, open("derived.json", "w"), indent=2)
