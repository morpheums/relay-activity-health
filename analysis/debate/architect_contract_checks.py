# Architect: checks that the contract simplifications are behaviour-preserving on the seed under R2*.
import os,sys,math
S=os.path.join(os.path.dirname(os.path.abspath(__file__)),'..','statistician'); sys.path.insert(0,S)
src=open(os.path.join(S,'golden.py')).read().split("show(6,")[0]
exec(src)
from collections import Counter
def low_simplified(lowT): return 0 if lowT<=0 else max(0,math.ceil((lowT/2)*(lowT/2)-0.375))
T0=2*math.sqrt(0.375)
def low_statistician(lowT): return 0 if lowT<=T0 else math.ceil((lowT/2)*(lowT/2)-0.375)
types=['all','call_received','lead_created','appointment_set']
weeks=[date(2026,1,26)+timedelta(weeks=i) for i in range(26)]
cells=0; guard_diff=0; status_vs_z=0; evaluated=0; edge_exact=0; halfway=Counter(); insuff=0
mixed_groups=0; rank_violation=0
for (a,series),f in firstev.items():
    for tt in types:
        for W in weeks:
            if week_of(f,tzs[a])>W: continue
            r=evaluate(a,series,tt,W); cells+=1
            if r['status']=='insufficient_data': insuff+=1; continue
            evaluated+=1
            lowT=r['centreT']-2*r['spreadT']
            if low_simplified(lowT)!=low_statistician(lowT): guard_diff+=1
            z=r['deviation']; zs='below' if z<-2 else 'above' if z>2 else 'normal'
            if zs!=r['status']: status_vs_z+=1
            if abs(abs(z)-2)<1e-9: edge_exact+=1
            if abs(round(abs(z)*1000)%10-5)==0 and abs(abs(z)*100-math.floor(abs(z)*100)-0.5)<1e-9: halfway['exact .xx5']+=1
print(f'cells evaluated or insufficient: {cells}; evaluated {evaluated}; insufficient {insuff}')
print(f'low: simplified guard (lowT<=0 then max(0,ceil)) vs statistician guard (lowT<=1.2247): {guard_diff} differences')
print(f'status from integer range vs sign(|z|>2): {status_vs_z} disagreements')
print(f'|z| exactly 2 (float edge): {edge_exact}')
print(f'deviation exactly at a 2-dp rounding midpoint: {halfway["exact .xx5"]}')
# ranking: within each (account,type,week) group, does |z| order put every flagged site before every normal site?
for a in sorted(x for x in tzs if (x,'*') in firstev):
    for tt in types:
        for W in weeks:
            wend=window(W,tzs[a])[1]
            rs=[evaluate(a,l,tt,W) for (aa,l),f in firstev.items() if aa==a and l!='*' and f<wend]
            ev=[r for r in rs if r['status']!='insufficient_data']
            if len(ev)!=len(rs) and ev: mixed_groups+=1
            flagged=[abs(r['deviation']) for r in ev if r['status']!='normal']
            normal=[abs(r['deviation']) for r in ev if r['status']=='normal']
            if flagged and normal and min(flagged)<=max(normal): rank_violation+=1
print(f'groups where a normal site would out-rank a flagged site by |z|: {rank_violation}')
print(f'groups mixing evaluated and insufficient sites: {mixed_groups}')
# earliestWeek per account (local week containing account first event, any type)
print('earliestWeek per account:',{a:str(week_of(firstev[(a,"*")],tzs[a])) for a in sorted(tzs) if (a,'*') in firstev}, '| account 20: no events')
