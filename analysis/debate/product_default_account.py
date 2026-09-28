# Product: which account best demonstrates DASH-247 in the default week (2026-07-20, all types) under R2*?
import os,sys; HERE=os.path.dirname(os.path.abspath(__file__)); sys.path.insert(0,os.path.join(HERE,'..','statistician'))
os.chdir(os.path.join(HERE,'..','statistician'))
src=open('golden.py').read().split("show(6,")[0]
exec(src)
W=date(2026,7,20)
print('| account | name | tz | sites | total (range, status) | flagged sites (count vs range, z) |')
print('|---|---|---|---|---|---|')
names={}
import sqlite3
for a in range(1,21):
    if (a,'*') not in firstev: print(f'| {a} | | | 0 | no events | |'); continue
    wend=window(W,tzs[a])[1]
    t=evaluate(a,'*','all',W)
    sites=sorted(l for (aa,l),f in firstev.items() if aa==a and l!='*' and f<wend)
    flags=[]
    for l in sites:
        r=evaluate(a,l,'all',W)
        if r['status'] in ('above','below'): flags.append(f"{l} {r['status']} {r['count']} vs {r['low']}–{r['high']} z={r['deviation']:.2f}")
    tot=f"{t['count']} ({t.get('low','')}–{t.get('high','')}, {t['status']})"
    print(f"| {a} | | {tzs[a]} | {len(sites)} | {tot} | {'; '.join(flags) or '—'} |")

print('\n### account 14 (Beacon Home Security), week 2026-07-20, all types, ranked')
show(14,'2026-07-20')
print('\n### account 14, week 2026-07-20, per type totals')
for tt in ['call_received','lead_created','appointment_set']: show(14,'2026-07-20',tt)
