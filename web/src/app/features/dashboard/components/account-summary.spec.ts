import { TestBed } from '@angular/core/testing';
import { ActivityHealthReport } from '../../../core/models';
import { BEACON_HOME_SECURITY, buildReport, withRange, withoutEnoughHistory } from '../../../../testing/activity-health-fixtures';
import { collapsedText } from '../../../../testing/dom-queries';
import { AccountSummary } from './account-summary';

async function renderSummaryElement(report: ActivityHealthReport): Promise<HTMLElement> {
  TestBed.configureTestingModule({ imports: [AccountSummary] });
  const fixture = TestBed.createComponent(AccountSummary);
  fixture.componentRef.setInput('report', report);
  await fixture.whenStable();
  return fixture.nativeElement as HTMLElement;
}

async function renderSummary(report: ActivityHealthReport): Promise<string> {
  return collapsedText(await renderSummaryElement(report));
}

function textReadByScreenReader(element: Element): string {
  const withoutHidden = element.cloneNode(true) as Element;
  withoutHidden.querySelectorAll('[aria-hidden="true"]').forEach((hidden) => hidden.remove());
  return collapsedText(withoutHidden);
}

function occurrencesOf(text: string, fragment: string): number {
  return text.split(fragment).length - 1;
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

  it.each([
    { status: 'above', summary: withRange(40, 25, 16, 36, 'above', 2.63), statusText: '▲ Higher than usual' },
    { status: 'below', summary: withRange(10, 27, 18, 38, 'below', -3.5), statusText: '▼ Lower than usual' },
    { status: 'normal', summary: withRange(26, 27, 18, 38, 'normal', -0.19), statusText: 'Within usual range' },
  ])('states the $status badge as "$statusText" exactly once, with any icon aria-hidden (UI-05)', async ({ summary, statusText }) => {
    const report = buildReport({ account: BEACON_HOME_SECURITY, weekStart: '2026-07-20', summary, locations: [] });

    const summaryElement = await renderSummaryElement(report);

    expect(occurrencesOf(collapsedText(summaryElement), statusText)).toBe(1);
    expect(occurrencesOf(textReadByScreenReader(summaryElement), statusText)).toBe(1);
    const iconsNotHidden = Array.from(summaryElement.querySelectorAll('svg, mat-icon, img, i')).filter((icon) => icon.closest('[aria-hidden="true"]') === null);
    expect(iconsNotHidden).toEqual([]);
  });
});
