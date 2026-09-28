# Recommended variant vs R2 on the same harness; + z-rule agreement check; + ranking symmetry evidence.
import sys; sys.argv=['x']
exec(open('sim.py').read().split("if __name__")[0])
from rules import recommended
RULES['R2* recommended']=recommended; PARAM['R2* recommended']='k'
rng=random.Random(7)
print('| level/type | rule | false above | false below | drop→0 | −50% det/stoch | +100% det/stoch | low 0 | contra |'); print('|---|---|---|---|---|---|---|---|---|')
for level in ('site','account'):
    for tt in TYPES:
        C=list(series_cells(level,tt))
        for r in ('R2 anscombe','R2* recommended'):
            M=metrics(r,C,2.0,rng)
            print(f"| {level}/{tt} | {r} | {M['above']:.1f}% | {M['below']:.1f}% | {M['zero']} | {M['half']} / {M['half_s']} | {M['dbl']} / {M['dbl_s']} | {M['low0']:.1f}% | {M['contra']} |")
for tr in (4,5,6):
  for level in ('site','account'):
    for tt in TYPES:
        C=list(series_cells(level,tt,trunc=tr)); M=metrics('R2* recommended',C,2.0,rng)
        print(f"| {level}/{tt} trunc {tr} | R2* | {M['above']:.1f}% | {M['below']:.1f}% | {M['zero']} | {M['half']} / {M['half_s']} | {M['dbl']} / {M['dbl_s']} | {M['low0']:.1f}% | {M['contra']} |")
b,t=contradictions_exhaustive('R2* recommended',2.0); print('exhaustive contradictions R2*:',b,'of',t)
# agreement of range-derived status with the z-rule |z|>2
dis=0;tot=0
for level in ('site','account'):
    for tt in TYPES:
        for _,v,x in series_cells(level,tt,min_elig=4):
            for xx in range(0,recommended(v,0)[2]+25):
                s,lo,hi,z=recommended(v,xx); zs='below' if z<-2 else 'above' if z>2 else 'normal'; tot+=1; dis+=s!=zs
        for _,v,x in series_cells(level,tt):
            for xx in range(0,recommended(v,0)[2]+25):
                s,lo,hi,z=recommended(v,xx); zs='below' if z<-2 else 'above' if z>2 else 'normal'; tot+=1; dis+=s!=zs
print('range-derived status vs z-rule disagreements:',dis,'of',tot)
# ranking symmetry: on real weeks, distribution of |dev| for positive vs negative deviations (site/all), raw (R0) vs z (R2*)
for r in ('R0 current','R2* recommended'):
    pos=[];neg=[]
    for _,v,x in series_cells('site','all'):
        d=RULES[r](v,x,2.0)[3]; (pos if d>0 else neg).append(abs(d)) if d!=0 else None
    q=lambda L:[round(y,2) for y in st.quantiles(L,n=20)[17:19]]
    print(f'{r}: n+={len(pos)} n-={len(neg)} 90/95th pct |dev| above={q(pos)} below={q(neg)}; share |dev|>2 above={sum(y>2 for y in pos)/len(C):.3f} below={sum(y>2 for y in neg)/len(C):.3f}')
# same-probability example: steady median-8 site, drop to 0 vs doubling to 16
def ptail(l,x,up):
    p=math.exp(-l);c=0;s=0
    for i in range(0,200):
        if (up and i>=x) or (not up and i<=x): s+=p
        p*=l/(i+1)
    return s
v=[8]*8
for x in (0,2,16,14):
    print(f'median-8 series, count {x}: raw dev {current(v,x)[3]:+.2f}, anscombe z {recommended(v,x)[3]:+.2f}, Poisson(8) tail {ptail(8,x,x>8):.4f}')
