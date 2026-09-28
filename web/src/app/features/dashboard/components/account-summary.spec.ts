import { TestBed } from '@angular/core/testing';
import { ActivityHealthReport, EventType, SeriesHealth } from '../../../core/models';
import {
  BEACON_HOME_SECURITY,
  beaconAppointmentsDefaultWeekReport,
  beaconCallsDefaultWeekReport,
  beaconDefaultWeekReport,
  beaconLeadsDefaultWeekReport,
  buildReport,
  lakesideInsufficientHistoryReport,
  metroCallsSpikeInBaselineReport,
  metroSpikeWeekReport,
  withRange,
  withoutEnoughHistory,
} from '../../../../testing/activity-health-fixtures';
import { collapsedText } from '../../../../testing/dom-queries';
import { AccountSummary } from './account-summary';

const FORBIDDEN_ON_SCREEN: RegExp[] = [/\bz\b/, /σ/, /±/, /\bmedian\b/i, /\btypical\b/i, /\bdeviation\b/i, /\bNormal\b/];

async function renderSummary(report: ActivityHealthReport): Promise<string> {
  TestBed.configureTestingModule({ imports: [AccountSummary] });
  const fixture = TestBed.createComponent(AccountSummary);
  fixture.componentRef.setInput('report', report);
  await fixture.whenStable();
  return collapsedText(fixture.nativeElement as HTMLElement);
}

function beaconReportWith(eventType: EventType, summary: SeriesHealth): ActivityHealthReport {
  return buildReport({ account: BEACON_HOME_SECURITY, weekStart: '2026-07-20', eventType, summary, locations: [] });
}

describe('AccountSummary', () => {
  it('shows the heading "Beacon Home Security — all locations"', async () => {
    const summaryText = await renderSummary(beaconDefaultWeekReport());

    expect(summaryText).toContain('Beacon Home Security — all locations');
  });

  it('shows the capitalised account method line', async () => {
    const summaryText = await renderSummary(beaconDefaultWeekReport());

    expect(summaryText).toContain('Compared with the last 8 full weeks for this account');
  });

  it('shows "26 inbound events · usually 18–38 a week" and "Within usual range" for the default week', async () => {
    const summaryText = await renderSummary(beaconDefaultWeekReport());

    expect(summaryText).toContain('26 inbound events · usually 18–38 a week');
    expect(summaryText).toContain('Within usual range');
  });

  it('shows "880 inbound events · usually 39–101 a week" and "▲ Higher than usual" for the spike week', async () => {
    const summaryText = await renderSummary(metroSpikeWeekReport());

    expect(summaryText).toContain('880 inbound events · usually 39–101 a week');
    expect(summaryText).toContain('▲ Higher than usual');
  });

  it('shows "▼ Lower than usual" for a below summary', async () => {
    const summaryText = await renderSummary(beaconReportWith('all', withRange(10, 27, 18, 38, 'below', -3.5)));

    expect(summaryText).toContain('10 inbound events · usually 18–38 a week');
    expect(summaryText).toContain('▼ Lower than usual');
  });

  it('prints low and high exactly as given, never recomputed', async () => {
    const summaryText = await renderSummary(beaconReportWith('all', withRange(26, 27, 25, 26, 'normal', 0)));

    expect(summaryText).toContain('26 inbound events · usually 25–26 a week');
  });

  it.each([
    { eventType: 'all', report: beaconDefaultWeekReport, expectedLine: '26 inbound events · usually 18–38 a week' },
    { eventType: 'call_received', report: beaconCallsDefaultWeekReport, expectedLine: '16 calls · usually 9–24 a week' },
    { eventType: 'lead_created', report: beaconLeadsDefaultWeekReport, expectedLine: '8 leads · usually 2–12 a week' },
    { eventType: 'appointment_set', report: beaconAppointmentsDefaultWeekReport, expectedLine: '2 appointments · usually 1–8 a week' },
  ])('uses the plural noun for type $eventType: "$expectedLine"', async ({ report, expectedLine }) => {
    const summaryText = await renderSummary(report());

    expect(summaryText).toContain(expectedLine);
  });

  it('uses "calls", not "inbound events", for 51 calls at account 6', async () => {
    const summaryText = await renderSummary(metroCallsSpikeInBaselineReport());

    expect(summaryText).toContain('51 calls · usually 17–79 a week');
    expect(summaryText).not.toContain('inbound event');
  });

  it.each<{ eventType: EventType; expectedLine: string }>([
    { eventType: 'all', expectedLine: '1 inbound event · usually 0–4 a week' },
    { eventType: 'call_received', expectedLine: '1 call · usually 0–4 a week' },
    { eventType: 'lead_created', expectedLine: '1 lead · usually 0–4 a week' },
    { eventType: 'appointment_set', expectedLine: '1 appointment · usually 0–4 a week' },
  ])('uses the singular noun for a count of 1 with type $eventType: "$expectedLine"', async ({ eventType, expectedLine }) => {
    const summaryText = await renderSummary(beaconReportWith(eventType, withRange(1, 1, 0, 4, 'normal', 0)));

    expect(summaryText).toContain(expectedLine);
  });

  it('shows "8 inbound events" with no range and "Not enough history yet (3 of 4 weeks needed)" when history is insufficient', async () => {
    const summaryText = await renderSummary(lakesideInsufficientHistoryReport());

    expect(summaryText).toContain('8 inbound events');
    expect(summaryText).not.toContain('8 inbound events ·');
    expect(summaryText).not.toMatch(/usually \d+–\d+ a week/);
    expect(summaryText).toContain('Not enough history yet (3 of 4 weeks needed)');
  });

  it('uses the type noun and minimumEligibleWeeks from the report for an insufficient summary', async () => {
    const report = { ...beaconReportWith('call_received', withoutEnoughHistory(5, 2)), minimumEligibleWeeks: 6 };

    const summaryText = await renderSummary(report);

    expect(summaryText).toContain('5 calls');
    expect(summaryText).toContain('Not enough history yet (2 of 6 weeks needed)');
  });

  it('never shows deviation, median, z, σ, ± or a standalone "Normal"', async () => {
    const summaryText = await renderSummary(beaconDefaultWeekReport());

    expect(summaryText).toContain('26 inbound events · usually 18–38 a week');
    FORBIDDEN_ON_SCREEN.forEach((forbidden) => expect(summaryText).not.toMatch(forbidden));
    expect(summaryText).not.toContain('-0.19');
  });
});
