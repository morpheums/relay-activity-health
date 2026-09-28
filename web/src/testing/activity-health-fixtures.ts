import {
  Account,
  ActivityHealthReport,
  EventType,
  HealthStatus,
  LocationHealth,
  SeriesHealth,
} from '../app/core/models';

export const DATA_AS_OF = '2026-07-27T22:20:34Z';
export const LATEST_COMPLETE_WEEK = '2026-07-20';

export const METRO_COLLISION_CENTERS: Account = { id: 6, name: 'Metro Collision Centers', timezone: 'America/New_York' };
export const LAKESIDE_PHYSIO: Account = { id: 8, name: 'Lakeside Physio', timezone: 'America/Chicago' };
export const REDLINE_TIRE_AND_SERVICE: Account = { id: 12, name: 'Redline Tire & Service', timezone: 'America/Los_Angeles' };
export const BEACON_HOME_SECURITY: Account = { id: 14, name: 'Beacon Home Security', timezone: 'America/New_York' };
export const QUIET_HARBOR_SPA: Account = { id: 20, name: 'Quiet Harbor Spa', timezone: 'America/Los_Angeles' };

export const EARLIEST_WEEK_BY_ACCOUNT_ID: ReadonlyMap<number, string> = new Map([
  [METRO_COLLISION_CENTERS.id, '2026-01-26'],
  [LAKESIDE_PHYSIO.id, '2026-02-02'],
  [REDLINE_TIRE_AND_SERVICE.id, '2026-01-26'],
  [BEACON_HOME_SECURITY.id, '2026-01-26'],
  [QUIET_HARBOR_SPA.id, LATEST_COMPLETE_WEEK],
]);

export function seedAccounts(): Account[] {
  return [METRO_COLLISION_CENTERS, LAKESIDE_PHYSIO, REDLINE_TIRE_AND_SERVICE, BEACON_HOME_SECURITY, QUIET_HARBOR_SPA].map((account) => ({ ...account }));
}

const ACCOUNT_6_SITE_NAMES = [
  'Site A', 'Site B', 'Site C', 'Site D', 'Site E', 'Site F', 'Site G', 'Site H',
  'Site I', 'Site J', 'Site K', 'Site L', 'Site M', 'Site N', 'Site O',
] as const;

export function withRange(
  count: number,
  median: number,
  low: number,
  high: number,
  status: Exclude<HealthStatus, 'insufficient_data'>,
  deviation: number,
  weeksUsed = 8,
): SeriesHealth {
  return { count, baseline: { weeksUsed, median, low, high }, status, deviation };
}

export function withoutEnoughHistory(count: number, weeksUsed: number): SeriesHealth {
  return { count, baseline: { weeksUsed, median: null, low: null, high: null }, status: 'insufficient_data', deviation: null };
}

export function locationRow(location: string, series: SeriesHealth): LocationHealth {
  return { location, ...series };
}

export function sundayOf(weekStart: string): string {
  const monday = new Date(`${weekStart}T00:00:00Z`);
  monday.setUTCDate(monday.getUTCDate() + 6);
  return monday.toISOString().slice(0, 10);
}

export interface ReportSpec {
  account: Account;
  weekStart: string;
  summary: SeriesHealth;
  locations: LocationHealth[];
  eventType?: EventType;
  dataAsOf?: string | null;
  earliestWeek?: string;
}

export function buildReport(spec: ReportSpec): ActivityHealthReport {
  return {
    account: { ...spec.account },
    eventType: spec.eventType ?? 'all',
    week: { start: spec.weekStart, end: sundayOf(spec.weekStart) },
    dataAsOf: spec.dataAsOf === undefined ? DATA_AS_OF : spec.dataAsOf,
    latestCompleteWeek: LATEST_COMPLETE_WEEK,
    earliestWeek: spec.earliestWeek ?? EARLIEST_WEEK_BY_ACCOUNT_ID.get(spec.account.id) ?? '2026-01-26',
    baselineWeeks: 8,
    minimumEligibleWeeks: 4,
    summary: spec.summary,
    locations: spec.locations,
  };
}

export function syntheticFillerRow(location: string, count: number, status: Exclude<HealthStatus, 'insufficient_data'>, deviation: number): LocationHealth {
  return locationRow(location, withRange(count, count, Math.max(0, count - 4), count + 4, status, deviation));
}

export function genericReport(account: Account, weekStart: string, eventType: EventType): ActivityHealthReport {
  return buildReport({
    account,
    weekStart,
    eventType,
    summary: withRange(10, 10, 5, 15, 'normal', 0),
    locations: [syntheticFillerRow('Site A', 10, 'normal', 0)],
  });
}

export function beaconDefaultWeekReport(): ActivityHealthReport {
  return buildReport({
    account: BEACON_HOME_SECURITY,
    weekStart: '2026-07-20',
    summary: withRange(26, 27, 18, 38, 'normal', -0.19),
    locations: [
      locationRow('Site B', withRange(2, 6.5, 3, 12, 'below', -2.16)),
      locationRow('Site C', withRange(9, 6, 2, 12, 'normal', 0.98)),
      locationRow('Site A', withRange(9, 6.5, 2, 16, 'normal', 0.61)),
      locationRow('Site D', withRange(6, 6.5, 3, 12, 'normal', -0.19)),
    ],
  });
}

export function beaconCallsDefaultWeekReport(): ActivityHealthReport {
  return buildReport({
    account: BEACON_HOME_SECURITY,
    weekStart: '2026-07-20',
    eventType: 'call_received',
    summary: withRange(16, 15.5, 9, 24, 'normal', 0.12),
    locations: [
      syntheticFillerRow('Site A', 5, 'normal', 0.4),
      syntheticFillerRow('Site C', 5, 'normal', 0.2),
      syntheticFillerRow('Site D', 4, 'normal', 0.1),
      syntheticFillerRow('Site B', 2, 'normal', 0),
    ],
  });
}

export function beaconLeadsDefaultWeekReport(): ActivityHealthReport {
  return buildReport({
    account: BEACON_HOME_SECURITY,
    weekStart: '2026-07-20',
    eventType: 'lead_created',
    summary: withRange(8, 6, 2, 12, 'normal', 0.74),
    locations: [
      syntheticFillerRow('Site A', 3, 'normal', 0.5),
      syntheticFillerRow('Site C', 2, 'normal', 0.3),
      syntheticFillerRow('Site D', 2, 'normal', 0.2),
      syntheticFillerRow('Site B', 1, 'normal', 0),
    ],
  });
}

export function beaconAppointmentsDefaultWeekReport(): ActivityHealthReport {
  return buildReport({
    account: BEACON_HOME_SECURITY,
    weekStart: '2026-07-20',
    eventType: 'appointment_set',
    summary: withRange(2, 3.5, 1, 8, 'normal', -0.85),
    locations: [
      locationRow('Site A', withRange(0, 1, 0, 4, 'normal', -1.12)),
      locationRow('Site B', withRange(0, 0, 0, 2, 'normal', 0)),
      locationRow('Site C', withRange(1, 1, 0, 6, 'normal', 0)),
      locationRow('Site D', withRange(1, 1, 0, 4, 'normal', 0)),
    ],
  });
}

export function beaconMixedHistoryReport(): ActivityHealthReport {
  return buildReport({
    account: BEACON_HOME_SECURITY,
    weekStart: '2026-03-02',
    summary: withRange(40, 25, 16, 36, 'above', 2.63),
    locations: [
      locationRow('Site D', withRange(16, 5.5, 2, 11, 'above', 3.25)),
      locationRow('Site B', withRange(9, 5, 2, 10, 'normal', 1.49)),
      locationRow('Site A', withoutEnoughHistory(7, 3)),
      locationRow('Site C', withoutEnoughHistory(8, 3)),
    ],
  });
}

export function beaconNoEligibleWeeksReport(): ActivityHealthReport {
  return buildReport({
    account: BEACON_HOME_SECURITY,
    weekStart: '2026-02-02',
    summary: withoutEnoughHistory(27, 0),
    locations: [
      locationRow('Site A', withoutEnoughHistory(9, 0)),
      locationRow('Site B', withoutEnoughHistory(5, 0)),
      locationRow('Site C', withoutEnoughHistory(7, 0)),
      locationRow('Site D', withoutEnoughHistory(6, 0)),
    ],
  });
}

export function beaconEarliestWeekReport(): ActivityHealthReport {
  return buildReport({
    account: BEACON_HOME_SECURITY,
    weekStart: '2026-01-26',
    summary: withoutEnoughHistory(2, 0),
    locations: [
      locationRow('Site B', withoutEnoughHistory(1, 0)),
      locationRow('Site D', withoutEnoughHistory(1, 0)),
    ],
  });
}

export function metroSpikeWeekReport(): ActivityHealthReport {
  const syntheticSpikeSiteCounts = [58, 58, 58, 58, 58, 58, 58, 58, 58, 58, 58, 58, 58, 59];
  const syntheticSpikeSites = ACCOUNT_6_SITE_NAMES.filter((site) => site !== 'Site C').map((site, index) =>
    syntheticFillerRow(site, syntheticSpikeSiteCounts[index], 'above', 12 - index * 0.5),
  );
  return buildReport({
    account: METRO_COLLISION_CENTERS,
    weekStart: '2026-06-01',
    summary: withRange(880, 66, 39, 101, 'above', 22.37),
    locations: [locationRow('Site C', withRange(67, 3, 1, 7, 'above', 12.74)), ...syntheticSpikeSites],
  });
}

export function metroWeekAfterSpikeReport(): ActivityHealthReport {
  return buildReport({
    account: METRO_COLLISION_CENTERS,
    weekStart: '2026-06-08',
    summary: withRange(102, 66, 37, 104, 'normal', 1.88),
    locations: [
      locationRow('Site C', withRange(11, 3.5, 1, 8, 'above', 2.81)),
      locationRow('Site J', withRange(11, 5, 2, 10, 'above', 2.11)),
      locationRow('Site K', withRange(9, 4.5, 2, 9, 'normal', 1.71)),
      locationRow('Site H', withRange(7, 3.5, 1, 8, 'normal', 1.44)),
      locationRow('Site L', withRange(10, 5, 1, 12, 'normal', 1.39)),
      locationRow('Site O', withRange(9, 5, 1, 12, 'normal', 1.14)),
      locationRow('Site D', withRange(4, 5.5, 2, 11, 'normal', -0.66)),
      locationRow('Site I', withRange(5, 3.5, 1, 9, 'normal', 0.61)),
      locationRow('Site B', withRange(6, 4, 1, 12, 'normal', 0.59)),
      locationRow('Site N', withRange(5, 4, 1, 9, 'normal', 0.44)),
      locationRow('Site E', withRange(8, 6.5, 2, 16, 'normal', 0.38)),
      locationRow('Site M', withRange(3, 3.5, 1, 8, 'normal', -0.26)),
      locationRow('Site F', withRange(5, 5.5, 2, 11, 'normal', -0.21)),
      locationRow('Site G', withRange(4, 4.5, 1, 11, 'normal', -0.18)),
      locationRow('Site A', withRange(5, 4.5, 1, 11, 'normal', 0.17)),
    ],
  });
}

export function metroSilentLocationReport(): ActivityHealthReport {
  return buildReport({
    account: METRO_COLLISION_CENTERS,
    weekStart: '2026-06-29',
    summary: withRange(69, 72, 41, 111, 'normal', -0.17),
    locations: [
      locationRow('Site G', withRange(0, 4.5, 2, 9, 'below', -3.19)),
      locationRow('Site N', withRange(2, 4.5, 2, 9, 'normal', -1.33)),
      locationRow('Site M', withRange(1, 3, 1, 7, 'normal', -1.33)),
      locationRow('Site F', withRange(3, 5, 2, 10, 'normal', -0.96)),
      locationRow('Site D', withRange(6, 4, 1, 9, 'normal', 0.87)),
      locationRow('Site I', withRange(6, 4, 1, 9, 'normal', 0.87)),
      locationRow('Site E', withRange(9, 6, 2, 14, 'normal', 0.84)),
      locationRow('Site B', withRange(4, 6, 1, 15, 'normal', -0.6)),
      locationRow('Site J', withRange(3, 4.5, 1, 11, 'normal', -0.57)),
      locationRow('Site H', withRange(7, 5, 1, 13, 'normal', 0.56)),
      locationRow('Site L', withRange(4, 5.5, 1, 13, 'normal', -0.49)),
      locationRow('Site C', withRange(6, 5, 1, 13, 'normal', 0.29)),
      locationRow('Site O', withRange(6, 5, 1, 13, 'normal', 0.29)),
      locationRow('Site A', withRange(7, 6, 1, 15, 'normal', 0.27)),
      locationRow('Site K', withRange(5, 5, 2, 10, 'normal', 0)),
    ],
  });
}

export function metroSpikeInBaselineReport(): ActivityHealthReport {
  const syntheticSitesAfterSiteM = ['Site O', 'Site I', 'Site A', 'Site E', 'Site J', 'Site G', 'Site N', 'Site L', 'Site B', 'Site H', 'Site F', 'Site K', 'Site D', 'Site C'];
  const syntheticSiteCounts = [6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 4, 4];
  return buildReport({
    account: METRO_COLLISION_CENTERS,
    weekStart: '2026-07-20',
    summary: withRange(87, 72.5, 30, 134, 'normal', 0.53),
    locations: [
      locationRow('Site M', withRange(7, 3.5, 1, 9, 'normal', 1.3)),
      ...syntheticSitesAfterSiteM.map((site, index) => syntheticFillerRow(site, syntheticSiteCounts[index], 'normal', 1.2 - index * 0.08)),
    ],
  });
}

export function metroCallsSpikeInBaselineReport(): ActivityHealthReport {
  const syntheticCallCounts = [4, 4, 4, 4, 4, 4, 3, 3, 3, 3, 3, 3, 3, 3, 3];
  return buildReport({
    account: METRO_COLLISION_CENTERS,
    weekStart: '2026-07-20',
    eventType: 'call_received',
    summary: withRange(51, 42, 17, 79, 'normal', 0.54),
    locations: ACCOUNT_6_SITE_NAMES.map((site, index) => syntheticFillerRow(site, syntheticCallCounts[index], 'normal', 0.9 - index * 0.05)),
  });
}

export function lakesideDefaultWeekReport(): ActivityHealthReport {
  return buildReport({
    account: LAKESIDE_PHYSIO,
    weekStart: '2026-07-20',
    summary: withRange(7, 10, 5, 17, 'normal', -1.01),
    locations: [locationRow('Site A', withRange(7, 10, 5, 17, 'normal', -1.01))],
  });
}

export function lakesideInsufficientHistoryReport(): ActivityHealthReport {
  return buildReport({
    account: LAKESIDE_PHYSIO,
    weekStart: '2026-03-02',
    summary: withoutEnoughHistory(8, 3),
    locations: [locationRow('Site A', withoutEnoughHistory(8, 3))],
  });
}

export function quietHarborEmptyReport(dataAsOf: string | null = DATA_AS_OF): ActivityHealthReport {
  return buildReport({
    account: QUIET_HARBOR_SPA,
    weekStart: LATEST_COMPLETE_WEEK,
    earliestWeek: LATEST_COMPLETE_WEEK,
    dataAsOf,
    summary: withoutEnoughHistory(0, 0),
    locations: [],
  });
}

export function scenarioReports(): ActivityHealthReport[] {
  return [
    beaconDefaultWeekReport(),
    beaconCallsDefaultWeekReport(),
    beaconLeadsDefaultWeekReport(),
    beaconAppointmentsDefaultWeekReport(),
    beaconMixedHistoryReport(),
    beaconNoEligibleWeeksReport(),
    beaconEarliestWeekReport(),
    metroSpikeWeekReport(),
    metroWeekAfterSpikeReport(),
    metroSilentLocationReport(),
    metroSpikeInBaselineReport(),
    metroCallsSpikeInBaselineReport(),
    lakesideDefaultWeekReport(),
    lakesideInsufficientHistoryReport(),
    quietHarborEmptyReport(),
  ];
}
