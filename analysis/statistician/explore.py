import os,sys,math,collections,statistics as st
HERE=os.path.dirname(os.path.abspath(__file__)); sys.argv=['x']; exec(open(os.path.join(HERE,'..','reference_model.py')).read().split("rows=[]")[0])
weeks=[date(2026,1,26)+timedelta(weeks=i) for i in range(26)]
# dispersion index per series over eligible weeks (exclude acct 6 spike week) 
for lvl in ('site','account'):
  for tt in TYPES:
    ratios=[];meds=[]
    for (a,l),f in firstev.items():
      if (l=='*')!=(lvl=='account'): continue
      fw=week_of(f,tzs[a]); v=[cnt[(a,l,tt,W)] for W in weeks if W>fw and W<=date(2026,7,20) and not (a==6 and W==date(2026,6,1))]
      m=st.mean(v); meds.append(st.median(v))
      if m>0: ratios.append(st.variance(v)/m)
    q=st.quantiles(ratios,n=4); qm=st.quantiles(meds,n=10)
    print(lvl,tt,'series',len(ratios),'var/mean quartiles',[round(x,2) for x in q],'| median-of-series deciles',[round(x,1) for x in qm])
