# Synthetic calibration: known Poisson / over-dispersed NB series, baselines of 4 and 8 weeks.
import math,random,sys
from rules import current,anscombe,poisson_nb,freeman_tukey
rng=random.Random(11)
def pois(l):
    u=rng.random(); k=0; p=math.exp(-l); c=p
    while u>c and k<10000: k+=1; p*=l/k; c+=p
    return k
def draw(l,phi):
    if phi==1: return pois(l)
    r=l/(phi-1); return pois(rng.gammavariate(r,l/r)) if l>0 else 0
RULES=[('R0 current',current,2.0),('R2 anscombe',anscombe,2.0),('R3 freeman-tukey',freeman_tukey,2.0),('R4 poisson/NB',poisson_nb,0.025)]
REPS=int(sys.argv[1]) if len(sys.argv)>1 else 3000
print('| dispersion | n weeks | λ | rule | false above | false below | drop→0 | −50% | +100% |'); print('|---|---|---|---|---|---|---|---|---|')
for phi in (1,2):
  for n in (4,8):
    for lam in (1,2,3,4,6,8,12,20,40,80):
      res={r[0]:[0,0,0,0,0] for r in RULES}
      for _ in range(REPS):
          v=[draw(lam,phi) for _ in range(n)]; x=draw(lam,phi); xh=draw(lam/2,phi); xd=draw(2*lam,phi)
          for name,f,p in RULES:
              s=f(v,x,p)[0]; R=res[name]; R[0]+=s=='above'; R[1]+=s=='below'
              R[2]+=f(v,0,p)[0]=='below'; R[3]+=f(v,xh,p)[0]=='below'; R[4]+=f(v,xd,p)[0]=='above'
      for name,_,_ in RULES:
          R=res[name]; print(f'| φ={phi} | {n} | {lam} | {name} | '+' | '.join(f'{100*c/REPS:.1f}%' for c in R)+' |')
