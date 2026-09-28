import math, statistics as st
def med(v): return st.median(v)
def mad(v,c): return st.median([abs(y-c) for y in v])
# ---- R0 current (PLAN 5.3) ----
def current(v,x,k=2.0):
    m=med(v); s=max(1.4826*mad(v,m), math.sqrt(max(m,1)))
    lo=max(0,math.ceil(m-k*s)); hi=math.floor(m+k*s)
    st_='below' if x<m-k*s else 'above' if x>m+k*s else 'normal'
    return st_,lo,hi,(x-m)/s
# ---- R1 current + zero override ----
def zero_rule(v,x,k=2.0):
    s,lo,hi,d=current(v,x,k)
    if x==0 and med(v)>=3: s='below'
    return s,lo,hi,d
# ---- transformed robust z (Anscombe / Freeman-Tukey) ----
A =lambda x:2*math.sqrt(x+0.375)
Ai=lambda y:(y/2)**2-0.375
F =lambda x:math.sqrt(x)+math.sqrt(x+1)
Fi=lambda y:((y*y-1)/(2*y))**2 if y>=1 else -1.0
def transformed(T,Ti,v,x,k):
    tv=[T(y) for y in v]; c=med(tv); s=max(1.4826*mad(tv,c),1.0)
    z=(T(x)-c)/s
    lo_t=c-k*s; hi_t=c+k*s
    lo=0 if lo_t<=T(0) else max(0,math.ceil(Ti(lo_t)))
    hi=math.floor(Ti(hi_t)) if hi_t>=T(0) else -1
    st_='below' if z<-k else 'above' if z>k else 'normal'
    return st_,lo,hi,z
def anscombe(v,x,k=2.0): return transformed(A,Ai,v,x,k)
def freeman_tukey(v,x,k=2.0): return transformed(F,Fi,v,x,k)
# ---- Poisson / negative-binomial exact tails around the median ----
def pmf_list(lam,var,upto):
    if var<=lam*(1+1e-12):
        p=[math.exp(-lam)]
        for i in range(upto): p.append(p[-1]*lam/(i+1))
    else:
        r=lam*lam/(var-lam); q=lam/(r+lam)
        p=[(r/(r+lam))**r]
        for i in range(upto): p.append(p[-1]*(i+r)/(i+1)*q)
    return p
def poisson_nb(v,x,alpha=0.025):
    m=med(v); lam=max(m,0.5); var=max(lam,(1.4826*mad(v,m))**2)
    upto=int(lam+12*math.sqrt(var)+20+x)
    p=pmf_list(lam,var,upto); cdf=[];c=0
    for q in p: c+=q; cdf.append(c)
    lo=next(i for i in range(upto+1) if cdf[i]>=alpha)             # P(X<=lo)>=alpha
    hi=next(i for i in range(upto+1) if 1-(cdf[i])<alpha)          # P(X>=hi+1)<alpha
    Pl=cdf[x] if x<=upto else 1.0; Pu=1-(cdf[x-1] if x>=1 else 0.0) if x<=upto else 0.0
    st_='below' if Pl<alpha else 'above' if Pu<alpha else 'normal'
    d=(x-lam)/math.sqrt(var)
    return st_,lo,hi,d
# ---- RECOMMENDED: Anscombe robust z, centre = T(raw median), status derived from the integer range ----
T0=2*math.sqrt(0.375)
def recommended(v,x,k=2.0):
    m=med(v); centre=2*math.sqrt(m+0.375)
    spread=max(1.4826*med([abs(2*math.sqrt(c+0.375)-centre) for c in v]),1.0)
    lowT=centre-k*spread; highT=centre+k*spread
    low=0 if lowT<=T0 else math.ceil((lowT/2)*(lowT/2)-0.375)
    high=math.floor((highT/2)*(highT/2)-0.375)
    status='below' if x<low else 'above' if x>high else 'normal'
    z=(2*math.sqrt(x+0.375)-centre)/spread
    return status,low,high,z
