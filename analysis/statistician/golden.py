# PLAN §7 golden scenarios recomputed under the recommended rule (R2*), with the current rule alongside.
import os,sys; HERE=os.path.dirname(os.path.abspath(__file__)); sys.path.insert(0,HERE)
from rules import recommended,current
import statistics as st, math
sys.argv=['x']; exec(open(os.path.join(HERE,'..','reference_model.py')).read().split("rows=[]")[0])
def evaluate(a,series,tt,W):
    fw=week_of(firstev[(a,series)],tzs[a])
    base=[W-timedelta(weeks=i) for i in range(8,0,-1)]; elig=[b for b in base if b>fw]; x=cnt[(a,series,tt,W)]
    if len(elig)<4: return dict(count=x,eligible=len(elig),status='insufficient_data')
    v=[cnt[(a,series,tt,b)] for b in elig]; s,lo,hi,z=recommended(v,x); cs,clo,chi,cd=current(v,x)
    m=st.median(v); c=2*math.sqrt(m+0.375); sp=max(1.4826*st.median([abs(2*math.sqrt(y+0.375)-c) for y in v]),1.0)
    return dict(count=x,eligible=len(elig),values=v,median=m,centreT=c,spreadT=sp,low=lo,high=hi,status=s,deviation=z,current=(clo,chi,cs,round(cd,2)))
def show(a,W,tt='all',sites=True,top=None):
    W=date.fromisoformat(W); wend=window(W,tzs[a])[1]
    r=evaluate(a,'*',tt,W); print(f'\n### account {a}, week {W}, type {tt}')
    fmt=lambda n,r:f"| {n} | {r['count']} | {r.get('median','')} | {r.get('low','')}–{r.get('high','')} | {r['status']} | {round(r['deviation'],2) if 'deviation' in r else ''} | {r.get('current','')} | {r.get('values','')} |"
    print('| series | count | median | range | status | deviation (z) | current rule (low,high,status,dev) | baseline |'); print('|---|---|---|---|---|---|---|---|')
    print(fmt('TOTAL',r)); 
    if 'centreT' in r: print(f"<!-- total centreT={r['centreT']:.6f} spreadT={r['spreadT']:.6f} -->")
    if not sites: return
    L=[(l,evaluate(a,l,tt,W)) for (aa,l),f in firstev.items() if aa==a and l!='*' and f<wend]
    L.sort(key=lambda p:(p[1]['status']=='insufficient_data',-abs(p[1].get('deviation',0)),{'below':0,'above':1}.get(p[1]['status'],2),p[0]))
    for i,(l,rr) in enumerate(L[:top] if top else L,1): print(fmt(f'{i}. {l}',rr))
show(6,'2026-06-01'); show(6,'2026-07-20'); show(6,'2026-07-20','call_received',sites=False)
show(12,'2026-07-20',top=3); show(8,'2026-03-02',sites=False); show(8,'2026-03-09',sites=False)
print('\naccount 1 Site C week 2026-07-06 count:',cnt[(1,'Site C','all',date(2026,7,6))])
