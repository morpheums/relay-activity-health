# Product: under R2*, how often does an admin's Monday view (all types) show >=1 flagged location, and which direction?
import os,sys
S=os.path.join(os.path.dirname(os.path.abspath(__file__)),'..','statistician'); sys.path.insert(0,S)
src=open(os.path.join(S,'golden.py')).read().split("show(6,")[0]
exec(src)
from collections import Counter
print('| account | sites | weeks judged | weeks with >=1 flagged site | above-only | below-only | both | total flagged |')
print('|---|---|---|---|---|---|---|---|')
allw=Counter()
for a in sorted(x for x in tzs if (x,'*') in firstev):
    judged=anyf=ab=be=both=totf=0
    for i in range(8,26):
        W=date(2026,1,26)+timedelta(weeks=i)
        if (a,W)==(6,date(2026,6,1)): continue
        wend=window(W,tzs[a])[1]
        sites=[l for (aa,l),f in firstev.items() if aa==a and l!='*' and f<wend]
        statuses=[evaluate(a,l,'all',W)['status'] for l in sites]
        judged+=1
        u='above' in statuses; d='below' in statuses
        if u or d: anyf+=1
        if u and not d: ab+=1
        if d and not u: be+=1
        if u and d: both+=1
        if evaluate(a,'*','all',W)['status'] in('above','below'): totf+=1
    print(f'| {a} | {len(sites)} | {judged} | {anyf} ({anyf/judged:.0%}) | {ab} | {be} | {both} | {totf} |')
    allw['judged']+=judged; allw['any']+=anyf
print(f"\nall accounts: {allw['any']}/{allw['judged']} account-weeks ({allw['any']/allw['judged']:.0%}) show >=1 flagged site (weeks 2026-03-23..07-20, spike week excluded)")
