import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DATE_LOCALE } from '@angular/material/core';
import { provideDateFnsAdapter } from '@angular/material-date-fns-adapter';
import { By } from '@angular/platform-browser';
import { enUS } from 'date-fns/locale';
import { WeekRange } from '../../../core/models';
import { LATEST_COMPLETE_WEEK, seedAccounts, sundayOf } from '../../../../testing/activity-health-fixtures';
import { collapsedText, findButton, getButton, getSelect, isDisabled, optionTexts } from '../../../../testing/dom-queries';
import { DashboardFilters } from './dashboard-filters';
import { WeekPicker } from './week-picker';

const enUSWithMondayWeekStart = { ...enUS, options: { ...enUS.options, weekStartsOn: 1 as const } };

const PREVIOUS_WEEK = '◀ Previous week';
const NEXT_WEEK = 'Next week ▶';

interface WeekInputs {
  week?: WeekRange | null;
  earliestWeek?: string | null;
  latestCompleteWeek?: string | null;
}

interface FiltersUnderTest {
  fixture: ComponentFixture<DashboardFilters>;
  root: HTMLElement;
  selectedWeeks: string[];
}

async function renderFilters(weekInputs: WeekInputs): Promise<FiltersUnderTest> {
  TestBed.configureTestingModule({
    imports: [DashboardFilters],
    providers: [provideDateFnsAdapter(), { provide: MAT_DATE_LOCALE, useValue: enUSWithMondayWeekStart }],
  });
  const fixture = TestBed.createComponent(DashboardFilters);
  fixture.componentRef.setInput('accounts', seedAccounts());
  fixture.componentRef.setInput('accountId', 14);
  fixture.componentRef.setInput('eventType', 'all');
  Object.entries(weekInputs).forEach(([inputName, value]) => fixture.componentRef.setInput(inputName, value));
  const selectedWeeks: string[] = [];
  fixture.componentInstance.weekSelected.subscribe((week) => selectedWeeks.push(week));
  await fixture.whenStable();
  return { fixture, root: fixture.nativeElement as HTMLElement, selectedWeeks };
}

function renderedWeekPicker(fixture: ComponentFixture<DashboardFilters>): WeekPicker | null {
  return fixture.debugElement.query(By.directive(WeekPicker))?.componentInstance ?? null;
}

function groupAccessibleName(group: Element): string {
  const labelledBy = group.getAttribute('aria-labelledby');
  if (labelledBy) {
    return labelledBy
      .split(/\s+/)
      .map((labelId) => collapsedText(group.ownerDocument.getElementById(labelId)))
      .join(' ')
      .trim();
  }
  return group.getAttribute('aria-label')?.trim() ?? '';
}

function elementsWithExactText(root: HTMLElement, text: string): Element[] {
  return Array.from(root.querySelectorAll('*')).filter((element) => collapsedText(element) === text && element.closest('select') === null);
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
  ])('with $missing unset, shows the week stepper and week picker only when all week inputs are set, and both selects always (UI-44)', async ({ weekInputs, stepperShown }) => {
    const { fixture, root } = await renderFilters(weekInputs);

    expect(findButton(root, PREVIOUS_WEEK) !== null).toBe(stepperShown);
    expect(findButton(root, NEXT_WEEK) !== null).toBe(stepperShown);
    expect(renderedWeekPicker(fixture) !== null).toBe(stepperShown);
    expect(getSelect(root, 'Viewing as').disabled).toBe(false);
    expect(getSelect(root, 'Activity type').disabled).toBe(false);
  });

  it('keeps the "Week" slot labelled while no report has loaded, without stepper or week picker (UI-44)', async () => {
    const { fixture, root } = await renderFilters({});

    expect(elementsWithExactText(root, 'Week').length).toBeGreaterThan(0);
    expect(findButton(root, PREVIOUS_WEEK)).toBeNull();
    expect(renderedWeekPicker(fixture)).toBeNull();
  });

  it('labels the week control "Week", visibly and as the group accessible name (C-27, UI-49)', async () => {
    const { root } = await renderFilters(weekInputsAt('2026-07-20'));

    const weekGroup = Array.from(root.querySelectorAll('[role="group"]')).find((group) => group.contains(getButton(root, PREVIOUS_WEEK)));
    expect(weekGroup).toBeDefined();
    expect(groupAccessibleName(weekGroup as Element)).toBe('Week');
    expect(elementsWithExactText(root, 'Week').length).toBeGreaterThan(0);
  });

  it('renders the week picker with the week, earliestWeek and latestCompleteWeek it was given (UI-48)', async () => {
    const { fixture } = await renderFilters(weekInputsAt('2026-03-02', '2026-02-02'));

    const weekPicker = renderedWeekPicker(fixture);

    expect(weekPicker).not.toBeNull();
    expect(weekPicker?.week()).toEqual({ start: '2026-03-02', end: '2026-03-08' });
    expect(weekPicker?.earliestWeek()).toBe('2026-02-02');
    expect(weekPicker?.latestCompleteWeek()).toBe(LATEST_COMPLETE_WEEK);
  });

  it('emits the week chosen in the week picker as weekSelected (UI-48)', async () => {
    const { fixture, selectedWeeks } = await renderFilters(weekInputsAt('2026-07-20'));

    const weekPicker = renderedWeekPicker(fixture);
    if (!weekPicker) {
      throw new Error('DashboardFilters does not render app-week-picker');
    }
    weekPicker.weekSelected.emit('2026-07-13');

    expect(selectedWeeks).toEqual(['2026-07-13']);
  });
});
