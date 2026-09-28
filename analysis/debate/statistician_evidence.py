# Statistician debate evidence: R2* consistency sweep, edge cases, default-account candidates, type-filter first-event impact.
import os,sys; HERE=os.path.dirname(os.path.abspath(__file__)); S=os.path.join(HERE,'..','statistician'); sys.path.insert(0,S)
src=open(os.path.join(S,'golden.py')).read().split("show(6,")[0]
exec(src)
from collections import Counter
WEEKS=[date(2026,1,26)+timedelta(weeks=i) for i in range(26)]
def poisson_cdf(k,lam):
    p=math.exp(-lam); c=p
    for i in range(k): p*=lam/(i+1); c+=p
    return c
print('## 1. Full sweep: status from integer range vs |z|>2, every (account, series, type, week) with >=4 eligible weeks')
n=dis=edge=0; statuses=Counter()
for (a,series) in firstev:
    for tt in TYPES:
        for W in WEEKS:
            r=evaluate(a,series,tt,W)
            if r['status']=='insufficient_data': statuses['insufficient_data']+=1; continue
            n+=1; statuses[r['status']]+=1
            zs='below' if r['deviation']<-2 else 'above' if r['deviation']>2 else 'normal'
            if zs!=r['status']: dis+=1
            if abs(abs(r['deviation'])-2)<1e-9: edge+=1
print(f'evaluated cells {n}; status/z disagreements {dis}; cells with |z| within 1e-9 of 2: {edge}; statuses {dict(statuses)}')
print('\n## 2. Hand-checkable edge cases (baseline -> count: median, low-high, status, z)')
for v,x in [([0,0,0,0],0),([0,0,0,0],3),([1,1,1,1],0),([2,2,2,2],0),([3,3,3,3],0),([4,4,4,4],0),([1,0,5,0,6,1,0,1],0),([11,11,11,8],6),([11,11,11,8],5),([3,3,4,4],2)]:
    s,lo,hi,z=recommended(v,x); print(f'{v} -> {x}: median {st.median(v)}, {lo}-{hi}, {s}, z={z:.2f}')
print('\n## 3. Default-week (2026-07-20, all) flagged series under R2* for every account: see product_default_account_out.md. Detail for account 14:')
show(14,'2026-07-20')
r=evaluate(14,'Site B','all',date(2026,7,20))
print(f"Site B baseline {r['values']} median {r['median']}; P(X<=2 | Poisson(median)) = {poisson_cdf(2,r['median']):.4f}")
print('\n## 4. PLAN 5.2 example (account 6, 2026-07-20, all) under R2*')
t=evaluate(6,'*','all',date(2026,7,20)); m=evaluate(6,'Site M','all',date(2026,7,20))
print(f"summary: count {t['count']} median {t['median']} low {t['low']} high {t['high']} weeksUsed {t['eligible']} status {t['status']} deviation {t['deviation']:.2f}")
print(f"Site M: count {m['count']} median {m['median']} low {m['low']} high {m['high']} status {m['status']} deviation {m['deviation']:.2f}")
print('\n## 5. Type filter: eligibility from first event of ANY type (spec reading) vs first event OF THE TYPE, R2*')
firstweek_type={}
for (a,series,tt,wk),c in cnt.items():
    if c>0:
        k=(a,series,tt); firstweek_type[k]=min(firstweek_type.get(k,wk),wk)
diff=Counter()
for (a,series) in firstev:
    for tt in TYPES[1:]:
        fw_any=week_of(firstev[(a,series)],tzs[a]); fw_t=firstweek_type.get((a,series,tt))
        for W in WEEKS:
            r1=evaluate(a,series,tt,W)
            base=[W-timedelta(weeks=i) for i in range(8,0,-1)]
            el=[b for b in base if fw_t is not None and b>fw_t]
            if len(el)<4: s2='insufficient_data'
            else: s2=recommended([cnt[(a,series,tt,b)] for b in el],cnt[(a,series,tt,W)])[0]
            if r1['status']!=s2: diff[(r1['status'],s2)]+=1
print('status changes (any-type -> of-type):',dict(diff),'total',sum(diff.values()))
print('\n## 6. Ranking ties at full precision among evaluated site rows (same group, |z| equal to 1e-12)')
ties=opp=0
for a in tzs:
    if (a,'*') not in firstev: continue
    for tt in TYPES:
        for W in WEEKS:
            wend=window(W,tzs[a])[1]
            rows=[evaluate(a,l,tt,W) for (aa,l),f in firstev.items() if aa==a and l!='*' and f<wend]
            zs=[(round(abs(r['deviation']),12),r['status']) for r in rows if 'deviation' in r]
            c=Counter(z for z,_ in zs)
            for z,k in c.items():
                if k>1:
                    ties+=k; sts={s for zz,s in zs if zz==z}
                    if 'above' in sts and 'below' in sts: opp+=1
print(f'rows in |z| ties: {ties}; tie groups mixing above and below: {opp}')
