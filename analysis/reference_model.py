import sqlite3, collections, statistics as st, math, csv, json
from datetime import datetime, timezone, date, timedelta, time
from zoneinfo import ZoneInfo
from pathlib import Path
repo_root=Path(__file__).resolve().parent.parent
sql_dir=next(d for d in (repo_root/'db', repo_root) if (d/'seed.sql').exists())
db=sqlite3.connect(':memory:')
db.executescript((sql_dir/'schema.sql').read_text()); db.executescript((sql_dir/'seed.sql').read_text())
TYPES=('all','call_received','lead_created','appointment_set')
tzs={i:ZoneInfo(t) for i,t in db.execute('select id,timezone from accounts')}
def week_of(dt_utc,tz):
    ld=dt_utc.astimezone(tz).date(); return ld-timedelta(days=ld.weekday())
def window(wk,tz):
    s=datetime.combine(wk,time(),tz).astimezone(timezone.utc); e=datetime.combine(wk+timedelta(7),time(),tz).astimezone(timezone.utc); return s,e
seen=set(); cnt=collections.Counter(); firstev={}
for a,l,t,ts,d,o in db.execute('select account_id,location,event_type,occurred_at,duration_seconds,outcome from activity_events order by occurred_at,id'):
    k=(a,l,t,ts,d,o)
    if k in seen: continue
    seen.add(k); u=datetime.fromisoformat(ts).replace(tzinfo=timezone.utc); wk=week_of(u,tzs[a])
    for tt in ('all',t): cnt[(a,l,tt,wk)]+=1; cnt[(a,'*',tt,wk)]+=1
    firstev.setdefault((a,l),u); firstev.setdefault((a,'*'),u)
anchor=max(datetime.fromisoformat(r[0]).replace(tzinfo=timezone.utc) for r in db.execute('select max(occurred_at) from activity_events'))
def latest_complete(tz):
    wk=week_of(anchor,tz)
    return wk if window(wk,tz)[1]<=anchor else wk-timedelta(7)
def ev(a,series,tt,W):
    fw=week_of(firstev[(a,series)],tzs[a]) if (a,series) in firstev else None
    base=[W-timedelta(weeks=i) for i in range(8,0,-1)]
    elig=[b for b in base if fw is not None and b>fw]
    x=cnt[(a,series,tt,W)]
    if len(elig)<4: return dict(count=x,eligible=len(elig),status='insufficient_data')
    v=[cnt[(a,series,tt,b)] for b in elig]; m=st.median(v); mad=st.median([abs(y-m) for y in v]); s=max(1.4826*mad, math.sqrt(max(m,1)))
    status='below' if x<m-2*s else 'above' if x>m+2*s else 'normal'
    return dict(count=x,eligible=len(elig),median=m,spread=s,low=max(0,math.ceil(m-2*s)),high=math.floor(m+2*s),status=status,deviation=(x-m)/s,values=v)
rows=[]; weeks=[date(2026,1,26)+timedelta(weeks=i) for i in range(26)]
for a in range(1,21):
    tz=tzs[a]
    for tt in TYPES:
        for W in weeks:
            wend=window(W,tz)[1]
            sites=sorted(l for (aa,l),f in firstev.items() if aa==a and l!='*' and f<wend)
            tot=ev(a,'*',tt,W) if (a,'*') in firstev else dict(count=0,eligible=0,status='insufficient_data')
            srows=[(l,ev(a,l,tt,W)) for l in sites]
            srows.sort(key=lambda r:(r[1]['status']=='insufficient_data', -abs(r[1].get('deviation',0)) if r[1]['status']!='insufficient_data' else 0, r[0]))
            def fmt(series,r,rank):
                f=lambda k:'' if k not in r else f'{r[k]:.6f}'
                return [a,series,tt,W.isoformat(),r['count'],r['eligible'],f('median'),f('spread'),r.get('low',''),r.get('high',''),r['status'],f('deviation'),rank]
            rows.append(fmt('*',tot,''))
            for i,(l,r) in enumerate(srows,1): rows.append(fmt(l,r,i))
with open('cells.csv','w',newline='') as fh:
    w=csv.writer(fh); w.writerow('account_id,series,event_type,week_start,count,eligible_weeks,median,spread,low,high,status,deviation,rank'.split(',')); w.writerows(rows)
with open('counts.csv','w',newline='') as fh:
    w=csv.writer(fh); w.writerow(['account_id','location','event_type','week_start','count'])
    for (a,l,tt,wk),n in sorted(cnt.items(),key=lambda kv:(kv[0][0],kv[0][1],kv[0][2],kv[0][3])):
        if l!='*' and n>0 and date(2026,1,26)<=wk<=date(2026,7,20): w.writerow([a,l,tt,wk.isoformat(),n])
json.dump({'anchor':anchor.isoformat(),'latest_complete':{a:latest_complete(tzs[a]).isoformat() for a in tzs}},open('derived.json','w'),indent=1)
print(len(rows),'cell rows')
