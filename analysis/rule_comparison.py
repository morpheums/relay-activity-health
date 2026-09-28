import sys, math, random, collections, statistics as st
sys.argv=['x']; exec(open(__file__.replace('rule_comparison.py','reference_model.py')).read().split("rows=[]")[0])
T=lambda x:2*math.sqrt(x+0.375)
Tinv=lambda y:(y/2)**2-0.375
def current(v,x):
    m=st.median(v); s=max(1.4826*st.median([abs(y-m) for y in v]), math.sqrt(max(m,1)))
    return 'below' if x<m-2*s else 'above' if x>m+2*s else 'normal', (max(0,math.ceil(m-2*s)), math.floor(m+2*s))
def zero_rule(v,x):
    s,b=current(v,x); m=st.median(v)
    if x==0 and m>=3: return 'below',b
    return s,b
def anscombe(v,x,k=2.0):
    tv=[T(y) for y in v]; tm=st.median(tv); s=max(1.4826*st.median([abs(y-tm) for y in tv]),1.0)
    z=(T(x)-tm)/s
    lo=Tinv(tm-k*s) if tm-k*s>0 else -1; hi=Tinv(tm+k*s)
    return ('below' if z<-k else 'above' if z>k else 'normal'), (max(0,math.floor(lo)+1) if lo>=0 else 0, math.ceil(hi)-1)
RULES={'current':current,'zero_rule':zero_rule,'anscombe':anscombe}
weeks=[date(2026,1,26)+timedelta(weeks=i) for i in range(26)]
def cells(tt):
    for (a,l),f in firstev.items():
        if l=='*': continue
        fw=week_of(f,tzs[a])
        for W in weeks:
            base=[W-timedelta(weeks=i) for i in range(8,0,-1)]; elig=[b for b in base if b>fw]
            if len(elig)<8: continue
            if a==6 and W==date(2026,6,1): continue
            yield (a,l,W),[cnt[(a,l,tt,b)] for b in elig],cnt[(a,l,tt,W)]
random.seed(7)
for tt in ('all','call_received','lead_created','appointment_set'):
    C=list(cells(tt)); print(f'\n### {tt}: {len(C)} site-weeks with full baseline')
    for n,r in RULES.items():
        c=collections.Counter(r(v,x)[0] for _,v,x in C)
        # sensitivity: replace current week by 0 / by half the median / by 2x median
        zero=[r(v,0)[0]=='below' for _,v,x in C if st.median(v)>=3]
        half=[r(v,int(st.median(v)*0.5))[0]=='below' for _,v,x in C if st.median(v)>=6]
        dbl=[r(v,int(round(st.median(v)*2)))[0]=='above' for _,v,x in C if st.median(v)>=3]
        lowzero=sum(1 for _,v,x in C if r(v,x)[1][0]==0)/len(C)
        print(f'  {n:10} false-flags above {c["above"]/len(C):5.1%} below {c["below"]/len(C):5.1%} | detects: drop-to-0 (med>=3) {sum(zero)/max(1,len(zero)):5.1%}  halved (med>=6) {sum(half)/max(1,len(half)):5.1%}  doubled (med>=3) {sum(dbl)/max(1,len(dbl)):5.1%} | low==0 {lowzero:5.1%}')
print('\n### spike week acct 6 2026-06-01, all: sites flagged above per rule')
for n,r in RULES.items():
    k=0
    for l in sorted({l for (a,l) in firstev if a==6 and l!='*'}):
        W=date(2026,6,1); v=[cnt[(6,l,'all',W-timedelta(weeks=i))] for i in range(8,0,-1)]; k+=r(v,cnt[(6,l,'all',W)])[0]=='above'
    print(' ',n,k,'/15')
print('\n### example bands (anscombe) for baseline medians')
for v in ([4]*8,[2,3,4,4,5,5,6,3],[8,6,9,7,10,8,7,9],[70,55,64,86,68,78,62,53],[0,0,1,0,0,1,0,0]):
    print(' ',v,'median',st.median(v),'current',current(v,0)[1],'anscombe',anscombe(v,0)[1])
