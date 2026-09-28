# Concrete baselines for each branch of the low guard: lowT < 0, 0 < lowT <= T(0), lowT just above T(0).
import math, statistics as st
T=lambda x:2*math.sqrt(x+0.375); T0=T(0)
def parts(v):
    m=st.median(v); c=T(m); s=max(1.4826*st.median([abs(T(y)-c) for y in v]),1.0); lo=c-2*s; hi=c+2*s
    low=0 if lo<=T0 else math.ceil((lo/2)**2-0.375)
    return m,round(c,6),round(s,6),round(lo,6),low,math.floor((hi/2)**2-0.375),round((lo/2)**2-0.375,6)
for v in ([1,1,1,1],[0,1,5,9],[3,3,3,3],[2,4,6,20]):
    m,c,s,lo,low,high,raw=parts(v); print(f'{v}: median {m} centre {c} spread {s} lowT {lo} (unguarded (lowT/2)^2-0.375 = {raw}) -> range {low}-{high}')
