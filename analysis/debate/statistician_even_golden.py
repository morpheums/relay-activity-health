# Golden case isolating centre = T(raw median) vs median of T values, for an even-count baseline.
import math, statistics as st
T=lambda x:2*math.sqrt(x+0.375); T0=T(0)
def rule(v,x,centre_mode):
    m=st.median(v); c=T(m) if centre_mode=='T(median)' else st.median([T(y) for y in v])
    madT=st.median([abs(T(y)-c) for y in v]); s=max(1.4826*madT,1.0)
    lo_t=c-2*s; hi_t=c+2*s
    low=0 if lo_t<=T0 else math.ceil((lo_t/2)**2-0.375); high=math.floor((hi_t/2)**2-0.375)
    status='below' if x<low else 'above' if x>high else 'normal'
    return dict(median=m,centre=round(c,6),madT=round(madT,6),spread=round(s,6),lowT=round(lo_t,6),highT=round(hi_t,6),low=low,high=high,status=status,z=round((T(x)-c)/s,4))
for v in ([2,4,6,20],[2,4,6,8],[1,3,9,9],[0,2,10,10]):
    for x in range(0,31):
        a=rule(v,x,'T(median)'); b=rule(v,x,'median(T)')
        if a['status']!=b['status']:
            print(v,'count',x,'\n  T(median):',a,'\n  median(T):',b); break
    else: print(v,'no status difference; ranges',rule(v,0,'T(median)')['low'],rule(v,0,'T(median)')['high'],'vs',rule(v,0,'median(T)')['low'],rule(v,0,'median(T)')['high'])
