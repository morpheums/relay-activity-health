import { TestBed } from '@angular/core/testing';
import { WeekRange } from '../../../core/models';
import { LATEST_COMPLETE_WEEK, seedAccounts, sundayOf } from '../../../../testing/activity-health-fixtures';
import { findButton, getButton, getSelect, isDisabled, optionTexts } from '../../../../testing/dom-queries';
import { DashboardFilters } from './dashboard-filters';

const PREVIOUS_WEEK = '◀ Previous week';
const NEXT_WEEK = 'Next week ▶';

interface WeekInputs {
  week?: WeekRange | null;
  earliestWeek?: string | null;
  latestCompleteWeek?: string | null;
}

interface FiltersUnderTest {
  root: HTMLElement;
  selectedWeeks: string[];
}

async function renderFilters(weekInputs: WeekInputs): Promise<FiltersUnderTest> {
  TestBed.configureTestingModule({ imports: [DashboardFilters] });
  const fixture = TestBed.createComponent(DashboardFilters);
  fixture.componentRef.setInput('accounts', seedAccounts());
  fixture.componentRef.setInput('accountId', 14);
  fixture.componentRef.setInput('eventType', 'all');
  Object.entries(weekInputs).forEach(([inputName, value]) => fixture.componentRef.setInput(inputName, value));
  const selectedWeeks: string[] = [];
  fixture.componentInstance.weekSelected.subscribe((week) => selectedWeeks.push(week));
  await fixture.whenStable();
  return { root: fixture.nativeElement as HTMLElement, selectedWeeks };
}

function weekInputsAt(weekStart: string, earliestWeek = '2026-01-26'): WeekInputs {
  return { week: { start: weekStart, end: sundayOf(weekStart) }, earliestWeek, latestCompleteWeek: LATEST_COMPLETE_WEEK };
}

describe('DashboardFilters', () => {
  it('lists every account by its plain name under "Viewing as"', async () => {
    const { root } = await renderFilters(weekInputsAt('2026-07-20'));

    expect(optionTexts(getSelect(root, 'Viewing as'))).toEqual(seedAccounts().map((account) => account.name));
  });

  it('offers All activity, Calls, Leads, Appointments under "Activity type"', async () => {
    const { root } = await renderFilters(weekInputsAt('2026-07-20'));

    expect(optionTexts(getSelect(root, 'Activity type'))).toEqual(['All activity', 'Calls', 'Leads', 'Appointments']);
  });

  it.each([
    { position: 'at latestCompleteWeek', weekStart: '2026-07-20', earliestWeek: '2026-01-26', previousDisabled: false, nextDisabled: true },
    { position: 'at earliestWeek', weekStart: '2026-01-26', earliestWeek: '2026-01-26', previousDisabled: true, nextDisabled: false },
    { position: 'between the bounds', weekStart: '2026-03-02', earliestWeek: '2026-01-26', previousDisabled: false, nextDisabled: false },
    { position: 'where earliestWeek equals latestCompleteWeek', weekStart: '2026-07-20', earliestWeek: '2026-07-20', previousDisabled: true, nextDisabled: true },
  ])('bounds the week stepper $position', async ({ weekStart, earliestWeek, previousDisabled, nextDisabled }) => {
    const { root } = await renderFilters(weekInputsAt(weekStart, earliestWeek));

    expect(isDisabled(getButton(root, PREVIOUS_WEEK))).toBe(previousDisabled);
    expect(isDisabled(getButton(root, NEXT_WEEK))).toBe(nextDisabled);
  });

  it.each([
    { button: PREVIOUS_WEEK, weekStart: '2026-07-20', expectedWeek: '2026-07-13' },
    { button: NEXT_WEEK, weekStart: '2026-07-13', expectedWeek: '2026-07-20' },
  ])('"$button" from $weekStart emits weekSelected $expectedWeek', async ({ button, weekStart, expectedWeek }) => {
    const { root, selectedWeeks } = await renderFilters(weekInputsAt(weekStart));

    getButton(root, button).click();

    expect(selectedWeeks).toEqual([expectedWeek]);
  });

  it.each<{ missing: string; weekInputs: WeekInputs; stepperShown: boolean }>([
    { missing: 'every week input', weekInputs: {}, stepperShown: false },
    { missing: 'week', weekInputs: { ...weekInputsAt('2026-07-20'), week: null }, stepperShown: false },
    { missing: 'earliestWeek', weekInputs: { ...weekInputsAt('2026-07-20'), earliestWeek: null }, stepperShown: false },
    { missing: 'latestCompleteWeek', weekInputs: { ...weekInputsAt('2026-07-20'), latestCompleteWeek: null }, stepperShown: false },
    { missing: 'nothing', weekInputs: weekInputsAt('2026-07-20'), stepperShown: true },
  ])('with $missing unset, shows the week stepper only when all week inputs are set, and both selects always (UI-44)', async ({ weekInputs, stepperShown }) => {
    const { root } = await renderFilters(weekInputs);

    expect(findButton(root, PREVIOUS_WEEK) !== null).toBe(stepperShown);
    expect(findButton(root, NEXT_WEEK) !== null).toBe(stepperShown);
    expect(getSelect(root, 'Viewing as').disabled).toBe(false);
    expect(getSelect(root, 'Activity type').disabled).toBe(false);
  });
});
