# Even-n baselines: T(raw median) vs median of T values, over every evaluated seed cell.
import os,sys; HERE=os.path.dirname(os.path.abspath(__file__)); S=os.path.join(HERE,'..','statistician'); sys.path.insert(0,S)
src=open(os.path.join(S,'golden.py')).read().split("show(6,")[0]
exec(src)
from rules import anscombe
WEEKS=[date(2026,1,26)+timedelta(weeks=i) for i in range(26)]
T=lambda x:2*math.sqrt(x+0.375)
diffs=[];flips=0;n=0
for (a,s) in firstev:
    for tt in TYPES:
        for W in WEEKS:
            r=evaluate(a,s,tt,W)
            if 'values' not in r: continue
            n+=1; d=abs(T(r['median'])-st.median([T(v) for v in r['values']])); diffs.append(d)
            if anscombe(r['values'],r['count'])[0]!=r['status']: flips+=1
diffs.sort()
print(f'cells {n}; |T(median)-median(T)| max {diffs[-1]:.4f}, p99 {diffs[int(.99*len(diffs))]:.4f}, share >0.01: {sum(d>0.01 for d in diffs)/n:.1%}')
print(f'status differs between centre=T(median) and centre=median(T): {flips} of {n} cells')
