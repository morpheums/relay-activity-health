import { TestBed } from '@angular/core/testing';
import { ActivityHealthReport } from '../../../core/models';
import { BEACON_HOME_SECURITY, buildReport, withRange, withoutEnoughHistory } from '../../../../testing/activity-health-fixtures';
import { collapsedText } from '../../../../testing/dom-queries';
import { AccountSummary } from './account-summary';

async function renderSummary(report: ActivityHealthReport): Promise<string> {
  TestBed.configureTestingModule({ imports: [AccountSummary] });
  const fixture = TestBed.createComponent(AccountSummary);
  fixture.componentRef.setInput('report', report);
  await fixture.whenStable();
  return collapsedText(fixture.nativeElement as HTMLElement);
}

describe('AccountSummary', () => {
  it('shows "{count} {noun} · usually {low}–{high} a week" with the status label, taking low and high as given', async () => {
    const report = buildReport({ account: BEACON_HOME_SECURITY, weekStart: '2026-07-20', summary: withRange(10, 27, 18, 38, 'below', -3.5), locations: [] });

    const summaryText = await renderSummary(report);

    expect(summaryText).toContain('10 inbound events · usually 18–38 a week');
    expect(summaryText).toContain('▼ Lower than usual');
  });

  it('shows the count with no range and weeks needed from the report minimumEligibleWeeks when history is insufficient', async () => {
    const report = {
      ...buildReport({ account: BEACON_HOME_SECURITY, weekStart: '2026-07-20', eventType: 'call_received', summary: withoutEnoughHistory(5, 2), locations: [] }),
      minimumEligibleWeeks: 6,
    };

    const summaryText = await renderSummary(report);

    expect(summaryText).toContain('5 calls');
    expect(summaryText).not.toContain('5 calls ·');
    expect(summaryText).not.toMatch(/usually \d+–\d+ a week/);
    expect(summaryText).toContain('Not enough history yet (2 of 6 weeks needed)');
  });
});
