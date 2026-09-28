import sys; sys.argv=['x']
exec(open('golden.py').read().split("show(6,")[0])
import math
p=math.exp(-5.5);c=0
for i in range(11): c+=p; p*=5.5/(i+1)
print('P(X>=11 | Poisson 5.5) =',round(1-c,4))
print('account 12 flagged series-weeks under recommended rule (all types):')
for W in [date(2026,1,26)+timedelta(weeks=i) for i in range(26)]:
    wend=window(W,tzs[12])[1]
    for l in ['*']+sorted(l for (a,l),f in firstev.items() if a==12 and l!='*' and f<wend):
        r=evaluate(12,l,'all',W)
        if r['status'] in ('above','below'): print(' ',W,l,r['count'],f"{r['low']}–{r['high']}",r['status'],round(r['deviation'],2))
