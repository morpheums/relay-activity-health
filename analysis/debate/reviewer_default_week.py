import os,sys
S=os.path.join(os.path.dirname(os.path.abspath(__file__)),'..','statistician'); sys.path.insert(0,S)
src=open(os.path.join(S,'golden.py')).read().split("show(6,")[0]
exec(src)
from collections import Counter
W=date(2026,7,20)
print('flagged in default week 2026-07-20, type all, R2* (site level and total):')
for a in sorted(x for x in tzs if (x,'*') in firstev):
    wend=window(W,tzs[a])[1]
    tot=evaluate(a,'*','all',W)
    out=[]
    if tot['status'] in('above','below'): out.append(('TOTAL',tot))
    n=0
    for (aa,l),f in firstev.items():
        if aa==a and l!='*' and f<wend:
            n+=1; r=evaluate(a,l,'all',W)
            if r['status'] in('above','below'): out.append((l,r))
    print(a,'sites',n,[(l,r['count'],r['low'],r['high'],r['status'],round(r['deviation'],2)) for l,r in out])
print()
print('large relative drops NOT flagged (all types, site level, median>=6, x<=0.5*median), R2*:')
c=Counter()
for (a,l),f in firstev.items():
    if l=='*': continue
    fw=week_of(f,tzs[a])
    for i in range(26):
        Wk=date(2026,1,26)+timedelta(weeks=i)
        base=[Wk-timedelta(weeks=j) for j in range(8,0,-1)]; el=[b for b in base if b>fw]
        if len(el)<4 or (a,Wk)==(6,date(2026,6,1)): continue
        r=evaluate(a,l,'all',Wk)
        if r['median']>=6 and r['count']<=0.5*r['median']:
            c[r['status']]+=1
            if r['status']=='normal': print(' ',a,l,Wk,r['count'],'median',r['median'],f"{r['low']}-{r['high']}",round(r['deviation'],2))
print(c)
