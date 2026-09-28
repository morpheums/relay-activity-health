# Real-seed evaluation of candidate normality rules. Same dedup + local-week bucketing as battle/reference/model.py.
import os,sys,math,random,collections,statistics as st
HERE=os.path.dirname(os.path.abspath(__file__)); sys.path.insert(0,HERE)
from rules import *
sys.argv=['x']; exec(open(os.path.join(HERE,'..','reference_model.py')).read().split("rows=[]")[0])
WEEKS=[date(2026,1,26)+timedelta(weeks=i) for i in range(26)]
SPIKE=(6,date(2026,6,1))
RULES={'R0 current':current,'R1 current+zero':zero_rule,'R2 anscombe':anscombe,'R3 freeman-tukey':freeman_tukey,'R4 poisson/NB exact':poisson_nb}
PARAM={'R0 current':'k','R1 current+zero':'k','R2 anscombe':'k','R3 freeman-tukey':'k','R4 poisson/NB exact':'alpha'}
DEFAULT={'k':2.0,'alpha':0.025}
SENS={'k':[1.75,2.0,2.5,3.0],'alpha':[0.05,0.025,0.01,0.005]}
def series_cells(level,tt,min_elig=8,trunc=None):
    for (a,l),f in firstev.items():
        if (l=='*')!=(level=='account'): continue
        fw=week_of(f,tzs[a])
        for W in WEEKS:
            base=[W-timedelta(weeks=i) for i in range(8,0,-1)]; elig=[b for b in base if b>fw]
            if (a,W)==SPIKE: continue
            if trunc is None:
                if len(elig)<min_elig or (min_elig==4 and len(elig)==8): continue
            else:
                if len(elig)<8: continue
                elig=elig[-trunc:]
            yield (a,l,W),[cnt[(a,l,tt,b)] for b in elig],cnt[(a,l,tt,W)]
def call(rule,v,x,p): return RULES[rule](v,x,p)
def metrics(rule,C,p,rng):
    n=len(C); c=collections.Counter(); low0=0; contra=0
    z=[];h=[];d=[];zs=[];hs=[];ds=[]
    for _,v,x in C:
        s,lo,hi,_d=call(rule,v,x,p); c[s]+=1; low0+=(lo==0)
        if (s=='normal')!=(lo<=x<=hi) or (s=='below')!=(x<lo) or (s=='above')!=(x>hi): contra+=1
        m=st.median(v)
        if m>=3: z.append(call(rule,v,0,p)[0]=='below'); d.append(call(rule,v,int(round(2*m)),p)[0]=='above'); ds.append(call(rule,v,2*x,p)[0]=='above')
        if m>=6:
            h.append(call(rule,v,int(m*0.5),p)[0]=='below')
            hs.append(call(rule,v,sum(rng.random()<0.5 for _ in range(x)),p)[0]=='below')
    f=lambda L:(f'{100*sum(L)/len(L):5.1f}%' if L else '   n/a')+f' (n={len(L)})'
    return dict(n=n,above=100*c['above']/n,below=100*c['below']/n,zero=f(z),half=f(h),half_s=f(hs),dbl=f(d),dbl_s=f(ds),low0=100*low0/n,contra=contra)
def contradictions_exhaustive(rule,p):
    bad=0;tot=0
    for level in ('site','account'):
        for tt in TYPES:
            for _,v,_x in series_cells(level,tt,min_elig=4):
                _,lo,hi,_=call(rule,v,0,p)
                for x in range(0,max(hi,0)+25):
                    s,lo2,hi2,_=call(rule,v,x,p); tot+=1
                    if (lo2,hi2)!=(lo,hi) or (s=='normal')!=(lo<=x<=hi) or (s=='below')!=(x<lo) or (s=='above')!=(x>hi): bad+=1
            for _,v,_x in series_cells(level,tt):
                _,lo,hi,_=call(rule,v,0,p)
                for x in range(0,max(hi,0)+25):
                    s,_,_,_=call(rule,v,x,p); tot+=1
                    if (s=='normal')!=(lo<=x<=hi) or (s=='below')!=(x<lo) or (s=='above')!=(x>hi): bad+=1
    return bad,tot
if __name__=='__main__':
    rng=random.Random(7)
    print('## A. Main table, default thresholds, full 8-week baselines, spike week excluded')
    for level in ('site','account'):
        for tt in TYPES:
            C=list(series_cells(level,tt)); print(f'\n### {level} / {tt}: {len(C)} weeks')
            print('| rule | false above | false below | drop→0 (med≥3) | −50% (med≥6) det / stoch | +100% (med≥3) det / stoch | low edge 0 | contradictions |')
            print('|---|---|---|---|---|---|---|---|')
            for r in RULES:
                M=metrics(r,C,DEFAULT[PARAM[r]],rng)
                print(f"| {r} | {M['above']:.1f}% | {M['below']:.1f}% | {M['zero']} | {M['half']} / {M['half_s']} | {M['dbl']} / {M['dbl_s']} | {M['low0']:.1f}% | {M['contra']} |")
    print('\n## B. Short baselines: real full baselines truncated to most recent 4/5/6 weeks (plus genuine 4–7-week rows)')
    for level in ('site','account'):
        for tt in ('all','call_received'):
            for tr in (4,5,6,8,'genuine 4-7'):
                C=list(series_cells(level,tt,min_elig=4)) if tr=='genuine 4-7' else list(series_cells(level,tt,trunc=tr))
                print(f'\n### {level} / {tt} / baseline={tr}: {len(C)} weeks')
                print('| rule | false above | false below | drop→0 | −50% det | +100% det | low edge 0 | contradictions |'); print('|---|---|---|---|---|---|---|---|')
                for r in ('R0 current','R2 anscombe','R3 freeman-tukey','R4 poisson/NB exact'):
                    M=metrics(r,C,DEFAULT[PARAM[r]],rng)
                    print(f"| {r} | {M['above']:.1f}% | {M['below']:.1f}% | {M['zero']} | {M['half']} | {M['dbl']} | {M['low0']:.1f}% | {M['contra']} |")
    print('\n## C. Threshold sensitivity (site level all types + account level all types; full baselines)')
    for level in ('site','account'):
        for tt in ('all','call_received'):
            C=list(series_cells(level,tt)); print(f'\n### {level} / {tt}')
            print('| rule | param | false above | false below | drop→0 | −50% | +100% |'); print('|---|---|---|---|---|---|---|')
            for r in ('R0 current','R2 anscombe','R3 freeman-tukey','R4 poisson/NB exact'):
                for p in SENS[PARAM[r]]:
                    M=metrics(r,C,p,rng)
                    print(f"| {r} | {PARAM[r]}={p} | {M['above']:.1f}% | {M['below']:.1f}% | {M['zero']} | {M['half']} | {M['dbl']} |")
    print('\n## D. Spike week (acct 6, 2026-06-01) and post-spike weeks')
    for r in RULES:
        p=DEFAULT[PARAM[r]]; out=[]
        for tt in TYPES:
            sites=sorted(l for (a,l) in firstev if a==6 and l!='*'); k=sum(call(r,[cnt[(6,l,tt,SPIKE[1]-timedelta(weeks=i))] for i in range(8,0,-1)],cnt[(6,l,tt,SPIKE[1])],p)[0]=='above' for l in sites)
            tot=call(r,[cnt[(6,'*',tt,SPIKE[1]-timedelta(weeks=i))] for i in range(8,0,-1)],cnt[(6,'*',tt,SPIKE[1])],p)[0]
            out.append(f'{tt}: sites above {k}/15, total {tot}')
        # post-spike distortion: baseline with spike vs spike replaced by median of the other 7
        changed=0; n=0; maxshift=0
        for W in [SPIKE[1]+timedelta(weeks=i) for i in range(1,8)]:
            for l in ['*']+sorted(l for (a,l) in firstev if a==6 and l!='*'):
                for tt in TYPES:
                    base=[W-timedelta(weeks=i) for i in range(8,0,-1)]; v=[cnt[(6,l,tt,b)] for b in base]
                    j=base.index(SPIKE[1]); others=v[:j]+v[j+1:]; v2=v[:]; v2[j]=int(round(st.median(others)))
                    x=cnt[(6,l,tt,W)]; a1=call(r,v,x,p); a2=call(r,v2,x,p); n+=1; changed+=a1[0]!=a2[0]; maxshift=max(maxshift,abs(a1[1]-a2[1]),abs(a1[2]-a2[2]))
        print(f'- {r}: '+'; '.join(out)+f' | post-spike (7 weeks × 16 series × 4 types = {n}): status changes vs spike-neutralised baseline {changed}, max range-edge shift {maxshift}')
    print('\n## E. Exhaustive contradiction check (every real baseline with ≥4 eligible weeks, every count 0..high+24)')
    for r in RULES:
        b,t=contradictions_exhaustive(r,DEFAULT[PARAM[r]]); print(f'- {r}: {b} contradictions in {t} (baseline,count) pairs')
