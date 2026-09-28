# How products tell a business user "is this normal?" — industry survey

Context: weekly inbound counts (calls / leads / appointments) per location, 1–15 locations, weekly counts ≈ 3–10. No ML or alerting. Question: what is the standard method underneath, and what UX works?

Evidence tags: **[DOC]** = stated in vendor or standards documentation (cited). **[SEC]** = secondary source (blog, search snippet, or vendor marketing). **[INF]** = my inference. **[SIM]** = my own simulation (method below).

---

## 1. Comparison table

| # | Product | Method (plain terms) | Baseline / seasonality | How "normal" is shown / phrased | Low-volume / sparse handling | Sensitivity convention |
|---|---|---|---|---|---|---|
| 1 | **Google Analytics 4** (Insights, anomaly detection) | Bayesian state-space time-series model. It predicts the value and a credible interval, and flags a value that falls outside the interval. [DOC] | Training windows: hourly 2 wks, daily 90 days, **weekly 32 weeks**. [DOC] | An expected-value line with a band in Explorations, plus "Insights" cards. [DOC] | A segment is only surfaced if it is **≥0.05% of users**, which is a minimum-volume floor. [DOC] | Not disclosed. Users can change the training period in Explorations. [DOC] |
| 2 | **Adobe Analytics** (Anomaly Detection, Intelligent Alerts) | Daily: ETS time-series model, which falls back to "functional filtering" if MAPE > 15%. **Weekly/monthly: GESD outlier test plus an adjusted box-plot** (robust outlier statistics, not ML). [DOC] | Daily: 35 days plus the same period a year earlier, with 9 named holidays adjusted. Weekly/monthly: **15-period lookback plus the prior-year window**. [DOC] | "The light shaded area is the confidence band, or expected range". Each flagged point shows the **% above/below expected**. Tables show a ◥ marker on the row. [DOC] | Choosing a model by fit quality (the MAPE check) is an implicit robustness safeguard. No explicit "not enough data" rule is documented. [DOC/INF] | Alerts offer thresholds of **90 / 95 / 99 (/99.9)%**. Practitioners say 95% "flags minor fluctuations… alert fatigue" and suggest starting at 99%. [DOC/SEC] |
| 3 | **Amplitude** (Anomaly + Forecast) | Prophet (decomposition model). [DOC] | "Agile" uses 120 days at **95% CI**. "Robust" adds a year of training data for seasonality. "Custom" lets the user set both. [DOC] | A light-blue band plus a dashed expected line, with anomalies drawn as orange dots. The docs give ready-made plain wording: "*Based on 120 days of training data, this data point represents an unexpected change with 95% confidence.*" [DOC] | "In cases where enough data isn't available, Amplitude may not detect or apply seasonality." [DOC] | Named presets (Agile/Robust) instead of σ values. [DOC] |
| 4 | **Mixpanel** (Anomaly alerts) | Prophet, now moving to TimesFM. [DOC] | Only on time-series line charts. [DOC] | An expected range, with an alert that links to root-cause analysis. [DOC] | The docs warn that hourly granularity "may produce more false positives". [DOC] | The user adjusts the confidence interval; a wider interval gives fewer alerts. [DOC] |
| 5 | **Datadog** (anomaly monitors) | **Basic = "simple lagging rolling quantile"** (no seasonality). Agile = robust SARIMA. Robust = seasonal-trend decomposition. [DOC] | Seasonal algorithms need **3 weeks for weekly seasonality**, and up to 6 weeks of history are used. [DOC] | A grey "normal" band. [DOC] | Seasonal algorithms show **no bounds until enough history exists**. The docs warn that a short alert window gives "false alarms due to spurious noise". [DOC] | "Bounds" ≈ standard deviations. "A value of **2 or 3** should be large enough to include most normal points." [DOC] |
| 6 | **AWS CloudWatch** | ML model covering trend plus hourly, daily and weekly patterns. [DOC] | Trains on **up to 2 weeks** of data. Users can exclude periods (such as deploys) from training. [DOC] | Grey band. Out-of-band values are drawn red. **The band is clamped to logical values (never below 0 for counts).** [DOC] | Uses "predictors to improve the models of metrics that are… spiky, or sparse." [DOC] | The band width is a σ multiplier: 3 = low, 2 = medium, 1.5 = high sensitivity. [SEC] |
| 7 | **New Relic** (anomaly thresholds) | A predicted baseline with a "gray band" of acceptable variation. Recent data gets more weight. [DOC] | 1–4 weeks of history. Seasonality can be auto, none, hourly, daily or weekly. [DOC] | "Baseline" plus a band. [DOC] | "The less history a signal has, the more its baseline will fluctuate". For volatile metrics, whose bands are "too wide to be useful", use static thresholds. [DOC] | σ: 1 is very sensitive, **2 is the common default, 3 is conservative**. Advice is to "start with 2–3". [DOC] |
| 8 | **Grafana** (ML outlier detection) | **MAD** (median absolute deviation from a rolling median) or DBSCAN. It compares **members of a group against each other**. [DOC] | Rolling window (24 h median for MAD). [DOC] | The flagged series is highlighted against the group band. [DOC] | Low sensitivity is advised "when the group has natural variation to avoid noise". [DOC] | Sensitivity from 0 to 1 (low/medium/high). [DOC] |
| 9 | **Power BI** (line-chart anomalies) | SR-CNN (Azure Anomaly Detector). [DOC] | Any time series. [DOC] | Shaded "expected range". The explanation reads: "*revenue was $5,187, which is above the expected range of $2,447 to $3,423*". [DOC] | **Requires at least 4 data points.** [DOC] | Sensitivity slider, **default 70%**. [SEC] |
| 10 | **Tableau** (Explain Data) | A statistical model predicts each mark. The **expected range is the 15th–85th percentile**. "Extreme values" use the 1.5×IQR rule. [DOC] | Cross-sectional, comparing a mark with its peers. [DOC] | "Lower than expected" / "higher than expected". [DOC] | "When the analyzed mark has a low number of records, there may not be enough data… to form a statistically significant explanation." Marks that are too granular are refused. [DOC] | Fixed at P15–P85. The band is deliberately narrow because it is exploratory, not an alert. [DOC/INF] |
| 11 | **Looker / BigQuery ML** | ARIMA_PLUS with `ML.DETECT_ANOMALIES`. [DOC] | User-defined. [DOC] | lower_bound / upper_bound columns that are then charted. [DOC] | — | `anomaly_prob_threshold` **default 0.95**. [DOC] |
| 12 | **CallRail** (call tracking) | **Period-over-period only**, e.g. "Compare to previous period" (the aggregate of two periods). No expected range. [DOC] | Previous period or a custom window. [DOC] | Two overlaid series or totals. [DOC] | None. It shows raw deltas. [DOC] | n/a |
| 13 | **Yext** (multi-location listings) | Period-over-period %, calculated against the previous period of the same length. **Benchmarks: a "benchmark range and median" built from anonymized customer data** for impressions, calls and directions. [SEC: help-page snippet; page returned 403] | Previous equal-length period. [SEC] | % change plus "benchmark range + median". [SEC] | Not documented. | n/a |
| 14 | **ServiceTitan** | Period comparison (same month last year on hover). **Benchmark Reports against anonymized peers** of similar size and trade, quarterly. Call metrics are rolled up by location. [SEC: help/marketing snippet] | YoY, and quarterly for benchmarks. [SEC] | Totals versus the prior year and versus the industry average. [SEC] | Quarterly aggregation (larger n) [INF] | n/a |
| 15 | **Birdeye** | Location leaderboards ("which locations rank highest and lowest") and a per-location score against the industry average. [SEC: marketing] | — | Ranking and score. [SEC] | — | n/a |
| 16 | **Invoca** | Dashboards support trendlines and **user-set reference/threshold lines**. [SEC: community article] | — | Reference line of "meet or beat". [SEC] | — | n/a |
| 17 | **SPC / XmR** (Wheeler; NHS England "Making Data Count") | **Natural process limits = mean ± 2.66 × average moving range** (≈3σ, not assuming any distribution). Signal rules: a point outside the limits, **7 in a row on one side** (a shift), and 2 of 3 points near a limit. [DOC] | A fixed baseline period that is **re-based at known changes**. MDC advises **≥15 points** (Wheeler allows starting provisionally with fewer). [DOC/SEC] | **Icons plus colour: orange = concern, blue = improvement, grey = "common cause / no significant change"**. Used in NHS board papers for non-statisticians. [DOC] | **Count data**: a c-chart uses Poisson limits (mean ± 3√mean) and is **not valid when c̄ < 2**, where exact Poisson probability limits are used instead. Wheeler's "**chunky data**" warning: with coarse integers the moving range takes ≤3 distinct values and the chart **gives false alarms**. [DOC] | 3σ (≈99.7%) by convention, **deliberately conservative to avoid chasing noise**. [DOC] |

Sources are in §5.

---

## 2. Synthesis

### 2.1 What is the de-facto standard?
- **The shared pattern is an expected value plus a band, with values outside the band flagged.** Every product in categories 1–11 has the same user-facing contract: a centre line, a shaded band called "expected range", "normal" or "baseline", and an out-of-band marker that says above or below and by how much. [DOC, all]
- **The engine under that contract varies.** Web analytics and observability mostly use time-series or ML models (Bayesian state-space, Prophet, SARIMA, STL, SR-CNN, AWS ML), because they work at hourly or daily grain where seasonality dominates and there is a lot of data. [DOC]
- **Simple robust statistics sit underneath in the cases most like ours.** Adobe's **weekly/monthly** detection uses GESD plus an adjusted box plot. Datadog's **basic** algorithm is a rolling quantile. Grafana's outlier detector uses the median and MAD. Tableau uses P15–P85 and 1.5×IQR. Operations dashboards use SPC/XmR. [DOC]
- **Inference:** ML earns its place for high-frequency, strongly seasonal data. At a weekly grain with 8–32 points, even Adobe switches to non-ML outlier tests. Simple statistics are the accepted standard here, not a compromise. [INF]
- **Sensitivity conventions:**
  - The common default is a 95% interval or 2σ (New Relic, BigQuery, Amplitude, CloudWatch "medium").
  - Conservative settings are 99% or 3σ (Adobe's practitioner advice, Datadog's "2 or 3", SPC's 3σ).
  - Products that aim at non-analysts hide the number behind named presets (Amplitude's Agile/Robust, Grafana's low/medium/high, the Power BI slider). [DOC]
- **Vertical and multi-location SaaS don't do statistical "normal" at all.** CallRail, Yext and ServiceTitan show **period-over-period % change** and, at best, **peer or industry benchmarks** (a Yext "benchmark range + median", ServiceTitan's peer benchmarks, Birdeye's location leaderboards). [DOC/SEC]
- **Inference:** a per-location "normal for *you*" range would stand out in this segment. It also means users are used to seeing "% vs last week", which at n≈5 is extremely noisy (5→3 reads as "−40%"). [INF]

### 2.2 UX conventions that work for non-technical users
1. **State the range in concrete units, not σ.**
   - Power BI: "above the expected range of $2,447 to $3,423". Amplitude: "we're 95% confident that this metric is between X and Y". Tableau: "higher/lower than expected". [DOC]
   - "Usually 4–8" follows this pattern exactly. [INF]
2. **Three states plus a neutral state, with colour carrying the meaning.** NHS MDC uses grey for common-cause variation, orange for concern and blue for improvement, deliberately avoiding red/green. It is designed for board members who are not statisticians. [DOC]
3. **Show the size of the deviation, not just the fact of it.** Adobe shows "% above/below expected". Power BI ranks its explanations by "strength". [DOC]
4. **Handle "not enough data" explicitly.**
   - Datadog shows no bounds until 3 weeks of history exist. Power BI needs ≥4 points. Tableau says a mark has too few records. GA4 applies a 0.05% segment floor. New Relic warns that a short-history baseline fluctuates. [DOC]
   - The convention is to **suppress the band rather than show a misleading one**.
5. **Rank what needs attention by deviation, with peer comparison.** Grafana's outlier detection is explicitly "which member of the group is behaving differently". Birdeye's leaderboards and Yext benchmarks rank locations. [DOC/SEC]
6. **Avoid alert fatigue with a wider band and persistence.** The levers are:
   - a wider band (99% / 3σ);
   - a minimum duration or window (Datadog trigger windows, SPC's "7 in a row" run rule for sustained shifts);
   - excluding known-abnormal periods from the baseline (CloudWatch exclusions, SPC re-basing). [DOC]

### 2.3 Avoiding crying wolf on small counts
- **From the docs:**
  - SPC literature is the only place that addresses it head-on. Normal-approximation limits for counts are invalid when the mean count is < 2, so exact Poisson probability limits should be used. [DOC]
  - Wheeler's chunky-data rule: coarse integers make the moving range degenerate, which **produces false alarms**. [DOC]
  - CloudWatch clamps bands at zero and mentions "sparse" predictors. [DOC]
  - GA4 applies a volume floor. [DOC]
- **Variance-stabilising and Poisson methods in products:**
  - None of the surveyed products documents a √-transform or a Poisson/NB band for dashboard counts.
  - The methods exist in SPC: the c-chart, exact Poisson limits, and negative-binomial "G" charts for overdispersion. [DOC]
  - The Anscombe √(x+3/8) transform is known to be biased for counts below ~20. [DOC]
  - So √-transforms are *not* the right tool at n≈3–10. Exact Poisson or quantile limits are. [INF]
- **My simulation [SIM].** Setup: stable Poisson data, 8 history weeks, next week tested, 40k trials per scenario. Results for the false-flag rate per location per week:

| Rule | λ=3 | λ=5 | λ=8 |
|---|---|---|---|
| Outside the 8-week **min–max** | 14% | 15% | 17% |
| Outside the 2nd-lowest to 2nd-highest (≈P10–P90 of 8) | 30% | 33% | 36% |
| \|x − median\| > 2 × 1.4826·MAD (σ̂ floored at 1) | 13% | 16% | 18% |
| \|x − median\| > **3** × 1.4826·MAD (σ̂ floored at 1) | 3% | 5% | 7% |
| Outside the Poisson(median) central 99% | 1.5% | 1.1% | 1.3% |

  - MAD = 0 happened in 4% of histories at λ=3, which is Wheeler's chunky-data problem. [SIM]
  - The Poisson 95% ranges are wide: λ=5 gives [1, 10], and λ=3 gives [0, 7], where a zero week has a 5% chance. [SIM]
  - **Inference:** with 10 locations and a ~15% per-location false-flag rate, **≈80% of Mondays show at least one false "needs attention"**. At ~1–5% per location the figure is ≈10–40%. [INF, from 1 − (1−p)^10]

---

## 3. Fit against the candidate design

The candidate: a per-location baseline from the median of the previous 8 complete weeks; an expected range shown as "usually X–Y"; status above / below / normal / not-enough-history; locations ranked by how far outside their range they are.

| Design element | Verdict | Evidence |
|---|---|---|
| **Per-location baseline** ("normal for us") | **Supported.** Every anomaly product models each series separately. Vertical SaaS only offers peer or industry benchmarks, so this is a differentiator. | [DOC] GA4, Adobe, Datadog; [SEC] Yext, ServiceTitan |
| **Median-based / robust** | **Supported.** Grafana MAD, Adobe GESD plus adjusted box plot, Tableau IQR, and Datadog's "robust" option all resist outliers. One caveat: a median/MAD spread degenerates on small integers (MAD=0), so the spread needs a floor. | [DOC]; [SIM] |
| **8 complete weeks** | **Partly challenged.** It sits at the short end of the norms: Adobe uses 15 periods for weekly, GA4 uses 32 weeks, MDC wants ≥15 points, and Datadog needs 3 weeks minimum. It is defensible for "recent normal" and for not modelling seasonality, but 8 points is the minimum that gives a stable median. Excluding the current, incomplete week matches Datadog's warning about incomplete windows and fits SPC practice. | [DOC]; [INF] |
| **No seasonality** | **Supported for now.** Weekly grain removes day-of-week effects. Yearly seasonality needs ≥1 year of history, which is where Amplitude "Robust" and Adobe's prior-year adjustment come in. It is acceptable to accept holiday weeks as noise. | [DOC]; [INF] |
| **"Usually X–Y"** | **Strongly supported.** It matches the concrete-units phrasing of Power BI, Amplitude and Tableau. It must be integers, clamped at 0 (as CloudWatch does). | [DOC] |
| **Range = empirical min–max or P10–P90 of 8 weeks** (if that is the plan) | **Challenged.** It flags 15–35% of normal weeks per location. | [SIM] |
| **Above / below / normal / not-enough-history** | **Supported.** It maps onto MDC's concern / improvement / common-cause icons plus Datadog and Tableau's "no bounds / not enough data". MDC's point applies: say "higher" or "lower", and let colour carry good or bad per metric. | [DOC] |
| **Rank by distance outside the range** | **Supported with a caveat.** Distance in raw counts favours high-volume locations. Rank on a scale-free measure (Poisson tail probability, or deviation ÷ spread), and show absolute and % difference as the label, as Adobe does. | [DOC] Adobe, Grafana; [INF] |

---

## 4. Recommendations

Each recommendation is tagged with the evidence it rests on.

1. **Band.** Centre = median of the last 8 complete weeks. Band = **Poisson-quantile limits around the median** at about the 1st and 99th percentile, *or* median ± 3 × max(1.4826·MAD, √median, 1).
   - Either gives ~1–5% false flags per location per week. [SIM]
   - The √median floor makes it a variance-aware band for counts, fixing both MAD=0 and chunky data. [DOC: Wheeler/c-chart; INF]
   - Shown as integers and clamped at ≥0. [DOC: CloudWatch]
2. **Wording.**
   - "Usually 3–9 a week", with "Last week: 12 · above usual".
   - Keep "normal" wording neutral ("within usual range"), as in NHS "common cause".
   - Avoid "anomaly" and σ in the UI. [DOC: Power BI, Amplitude, MDC]
3. **Not enough history.**
   - Require ≥6 of 8 complete weeks with data; below that, show "Building baseline — N more weeks". Suppress the band rather than guess. [DOC: Datadog, Power BI ≥4; INF for the threshold]
   - Treat a very low median (<2/week) as "too few to judge" or use exact Poisson limits. [DOC: c-chart c̄<2]
4. **Severity tiers (optional, max two).**
   - "Outside usual" (beyond the band).
   - "Well outside" (e.g. beyond Poisson 99.9%, or 2 consecutive weeks on the same side), echoing the SPC run rules and Datadog trigger windows. [DOC; INF]
5. **Ranking.**
   - Order locations by scale-free surprise: the Poisson tail p-value, or (x − band edge)/spread.
   - Within-range locations sink below all out-of-range ones, and "not enough history" goes last. [INF; DOC Grafana/Adobe]
6. **Don't lead with % change at n≈5.** If week-over-week is shown, put it secondary to the range, because it is the vertical-SaaS default and also the main source of false alarm at low counts. [DOC: CallRail/Yext; INF]
7. **Explain on hover.**
   - "Based on your last 8 full weeks at this location (holidays included)."
   - This mirrors Amplitude's "Based on 120 days of training data…" disclosure. [DOC]
8. **Later.** Allow excluding abnormal weeks (closures) from the baseline, as in CloudWatch exclusions and SPC re-basing. [DOC]

---

## 5. Sources
- GA4 anomaly detection: https://support.google.com/analytics/answer/9517187
- Adobe statistical techniques: https://experienceleague.adobe.com/en/docs/analytics/analyze/analysis-workspace/anomaly-detection/statistics-anomaly-detection
- Adobe view anomalies: https://experienceleague.adobe.com/en/docs/analytics/analyze/analysis-workspace/anomaly-detection/view-anomalies
- Adobe alerts: https://experienceleague.adobe.com/en/docs/analytics/components/alerts/alert-builder ; practitioner guidance on 95 vs 99%: https://www.trackingplan.com/blog/anomaly-detection-adobe-analytics
- Amplitude Anomaly + Forecast: https://amplitude.com/docs/analytics/anomaly-forecast
- Mixpanel alerts: https://docs.mixpanel.com/docs/features/alerts
- Datadog anomaly monitor: https://docs.datadoghq.com/monitors/types/anomaly/ ; algorithms: https://docs.datadoghq.com/dashboards/functions/algorithms/
- AWS CloudWatch: https://docs.aws.amazon.com/AmazonCloudWatch/latest/monitoring/CloudWatch_Anomaly_Detection.html ; sensitivity presets (secondary): https://oneuptime.com/blog/post/2026-02-12-cloudwatch-anomaly-detection-alarms/view
- New Relic: https://docs.newrelic.com/docs/alerts/create-alert/set-thresholds/anomaly-detection/
- Grafana outlier detection: https://grafana.com/docs/grafana-cloud/machine-learning/dynamic-alerting/outlier-detection/
- Power BI: https://learn.microsoft.com/en-us/power-bi/visuals/power-bi-visualization-anomaly-detection
- Tableau Explain Data: https://help.tableau.com/current/pro/desktop/en-us/explain_data_explained.htm
- BigQuery ML.DETECT_ANOMALIES: https://docs.cloud.google.com/bigquery/docs/reference/standard-sql/bigqueryml-syntax-detect-anomalies
- CallRail call log comparison: https://support.callrail.com/hc/en-us/articles/5711301368717-Call-Log
- Yext Listings insights (snippet; page 403): https://help.yext.com/hc/en-us/articles/360000001163 ; https://hitchhikers.yext.com/docs/analytics/location-listings-metrics/
- ServiceTitan: https://help.servicetitan.com/docs/measure-performance-and-make-decisions
- Birdeye: https://birdeye.com/competitors-ai/
- Invoca dashboard: https://community.invoca.com/t5/getting-started-with-reporting/an-introduction-to-your-invoca-dashboard/ta-p/494
- Wheeler, XmR: https://www.spcpress.com/pdf/DJW250.pdf ; chunky data: https://www.qualitydigest.com/inside/statistics-article/problem-chunky-data-071023.html , https://www.spcforexcel.com/knowledge/variable-control-charts/chunky-data-and-control-charts/
- c-chart small counts: https://www.spcforexcel.com/knowledge/attribute-control-charts/small-sample-case-for-c-and-u-control-charts/
- NHS Making Data Count: https://www.england.nhs.uk/publication/making-data-count/ ; https://nhsrplotthedots.nhsrcommunity.com/ ; icon colours: https://pubmed.ncbi.nlm.nih.gov/41667235/
- Anscombe transform: https://en.wikipedia.org/wiki/Anscombe_transform

Simulation method [SIM]: Python, i.i.d. Poisson(λ) history of 8 weeks plus 1 test week, 40,000 trials per λ. It measures the fraction of in-control test weeks flagged. It assumes no overdispersion; real data is likely overdispersed, which would raise all the false-flag rates [INF].
