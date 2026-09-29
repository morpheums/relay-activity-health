import { HarnessLoader, TestKey } from '@angular/cdk/testing';
import { TestbedHarnessEnvironment } from '@angular/cdk/testing/testbed';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DATE_LOCALE } from '@angular/material/core';
import { MatCalendarCellHarness, MatCalendarHarness } from '@angular/material/datepicker/testing';
import { provideDateFnsAdapter } from '@angular/material-date-fns-adapter';
import { enUS } from 'date-fns/locale';
import { LATEST_COMPLETE_WEEK, sundayOf } from '../../../../testing/activity-health-fixtures';
import { collapsedText, isDisabled } from '../../../../testing/dom-queries';
import { WeekPicker } from './week-picker';

const enUSWithMondayWeekStart = { ...enUS, options: { ...enUS.options, weekStartsOn: 1 as const } };

const DEFAULT_WEEK_LABEL = 'Mon Jul 20 – Sun Jul 26, 2026';
const DEFAULT_TRIGGER_NAME = `${DEFAULT_WEEK_LABEL}, choose week`;
const WEEK_BEFORE_LATEST_TRIGGER_NAME = 'Mon Jul 13 – Sun Jul 19, 2026, choose week';
const DIALOG_NAME = 'Choose week';
const WEEKS_RUN_HELPER = 'Weeks run Monday to Sunday.';
const BEACON_RANGE_HELPER = 'Weeks from Mon Jan 26 to Mon Jul 20, 2026';
const LATEST_WEEK = 'Latest week';

interface PickerBounds {
  weekStart: string;
  earliestWeek: string;
  latestCompleteWeek: string;
}

interface PickerUnderTest {
  fixture: ComponentFixture<WeekPicker>;
  root: HTMLElement;
  selectedWeeks: string[];
  overlayLoader: HarnessLoader;
}

const BEACON_BOUNDS: PickerBounds = { weekStart: '2026-07-20', earliestWeek: '2026-01-26', latestCompleteWeek: LATEST_COMPLETE_WEEK };
const BEACON_WEEK_BEFORE_LATEST_BOUNDS: PickerBounds = { weekStart: '2026-07-13', earliestWeek: '2026-01-26', latestCompleteWeek: LATEST_COMPLETE_WEEK };
const LAKESIDE_BOUNDS: PickerBounds = { weekStart: '2026-07-20', earliestWeek: '2026-02-02', latestCompleteWeek: LATEST_COMPLETE_WEEK };
const QUIET_HARBOR_BOUNDS: PickerBounds = { weekStart: '2026-07-20', earliestWeek: LATEST_COMPLETE_WEEK, latestCompleteWeek: LATEST_COMPLETE_WEEK };

async function renderPicker(bounds: PickerBounds): Promise<PickerUnderTest> {
  TestBed.configureTestingModule({
    imports: [WeekPicker],
    providers: [provideDateFnsAdapter(), { provide: MAT_DATE_LOCALE, useValue: enUSWithMondayWeekStart }],
  });
  const fixture = TestBed.createComponent(WeekPicker);
  fixture.componentRef.setInput('week', { start: bounds.weekStart, end: sundayOf(bounds.weekStart) });
  fixture.componentRef.setInput('earliestWeek', bounds.earliestWeek);
  fixture.componentRef.setInput('latestCompleteWeek', bounds.latestCompleteWeek);
  const selectedWeeks: string[] = [];
  fixture.componentInstance.weekSelected.subscribe((week) => selectedWeeks.push(week));
  await fixture.whenStable();
  return { fixture, root: fixture.nativeElement as HTMLElement, selectedWeeks, overlayLoader: TestbedHarnessEnvironment.documentRootLoader(fixture) };
}

function accessibleName(element: Element): string {
  const labelledBy = element.getAttribute('aria-labelledby');
  if (labelledBy) {
    return labelledBy
      .split(/\s+/)
      .map((labelId) => collapsedText(element.ownerDocument.getElementById(labelId)))
      .join(' ')
      .trim();
  }
  return element.getAttribute('aria-label')?.trim() ?? collapsedText(element);
}

function getTrigger(root: HTMLElement, name = DEFAULT_TRIGGER_NAME): HTMLButtonElement {
  const trigger = Array.from(root.querySelectorAll('button')).find((button) => accessibleName(button) === name);
  if (!trigger) {
    throw new Error(`No week picker trigger named "${name}" in: "${collapsedText(root)}"`);
  }
  return trigger;
}

function openDialogs(): Element[] {
  return Array.from(document.querySelectorAll('[role="dialog"]'));
}

function getDialog(): Element {
  const [dialog] = openDialogs();
  if (!dialog) {
    throw new Error('The week picker dialog is not open');
  }
  return dialog;
}

function latestWeekButtonsInDialog(): HTMLButtonElement[] {
  return Array.from(getDialog().querySelectorAll('button')).filter((button) => accessibleName(button) === LATEST_WEEK);
}

function getLatestWeekButton(): HTMLButtonElement {
  const [latestWeekButton] = latestWeekButtonsInDialog();
  if (!latestWeekButton) {
    throw new Error(`No button named "${LATEST_WEEK}" in the week picker dialog: "${collapsedText(getDialog())}"`);
  }
  return latestWeekButton;
}

async function afterScheduledFocus(fixture: ComponentFixture<WeekPicker>): Promise<void> {
  for (let round = 0; round < 3; round++) {
    await new Promise<void>((resolve) => setTimeout(resolve, 0));
    await fixture.whenStable();
  }
}

async function openPicker(picker: PickerUnderTest, triggerName = DEFAULT_TRIGGER_NAME): Promise<MatCalendarHarness> {
  getTrigger(picker.root, triggerName).click();
  await afterScheduledFocus(picker.fixture);
  return picker.overlayLoader.getHarness(MatCalendarHarness);
}

async function dayTexts(cells: MatCalendarCellHarness[]): Promise<string[]> {
  return Promise.all(cells.map((cell) => cell.getText()));
}

async function selectableDays(calendar: MatCalendarHarness): Promise<string[]> {
  return dayTexts(await calendar.getCells({ disabled: false }));
}

async function showMonth(calendar: MatCalendarHarness, monthLabel: RegExp): Promise<void> {
  for (let step = 0; step < 12 && !monthLabel.test(await calendar.getCurrentViewLabel()); step++) {
    await calendar.previous();
  }
  expect(await calendar.getCurrentViewLabel()).toMatch(monthLabel);
}

async function dayCell(calendar: MatCalendarHarness, day: string): Promise<MatCalendarCellHarness> {
  const [cell] = await calendar.getCells({ text: day });
  if (!cell) {
    throw new Error(`No calendar cell for day ${day}`);
  }
  return cell;
}

async function activeCell(calendar: MatCalendarHarness): Promise<MatCalendarCellHarness> {
  const [active] = await calendar.getCells({ active: true });
  if (!active) {
    throw new Error('No calendar cell has keyboard focus');
  }
  return active;
}

async function pressOnActiveCell(picker: PickerUnderTest, calendar: MatCalendarHarness, key: TestKey): Promise<void> {
  await (await (await activeCell(calendar)).host()).sendKeys(key);
  await afterScheduledFocus(picker.fixture);
}

async function cellClasses(cell: MatCalendarCellHarness): Promise<Set<string>> {
  const classAttribute = (await (await cell.host()).getAttribute('class')) ?? '';
  return new Set(classAttribute.split(/\s+/).filter((className) => className.length > 0));
}

describe('WeekPicker', () => {
  describe('trigger', () => {
    it('is a native button showing the week label and named "Mon Jul 20 – Sun Jul 26, 2026, choose week" (C-17, C-28)', async () => {
      const { root } = await renderPicker(BEACON_BOUNDS);

      const trigger = getTrigger(root);

      expect(trigger.type).toBe('button');
      expect(collapsedText(trigger)).toContain(DEFAULT_WEEK_LABEL);
      expect(isDisabled(trigger)).toBe(false);
      expect(trigger.tabIndex).toBeGreaterThanOrEqual(0);
    });

    it('is disabled but still shows the week when earliestWeek equals latestCompleteWeek, and does not open (UI-20, C-07)', async () => {
      const picker = await renderPicker(QUIET_HARBOR_BOUNDS);

      const trigger = getTrigger(picker.root);
      trigger.click();
      await picker.fixture.whenStable();

      expect(isDisabled(trigger)).toBe(true);
      expect(collapsedText(trigger)).toContain(DEFAULT_WEEK_LABEL);
      expect(openDialogs()).toEqual([]);
    });
  });

  describe('dialog', () => {
    it('opens a dialog named "Choose week" with Mon Jul 20 selected and keyboard focus on it (UI-48, UI-49, C-28)', async () => {
      const picker = await renderPicker(BEACON_BOUNDS);

      const calendar = await openPicker(picker);

      expect(accessibleName(getDialog())).toBe(DIALOG_NAME);
      expect(getTrigger(picker.root).getAttribute('aria-expanded')).toBe('true');
      expect(await calendar.getCurrentViewLabel()).toMatch(/jul\w*\s+2026/i);
      expect(await dayTexts(await calendar.getCells({ selected: true }))).toEqual(['20']);
      expect(await (await activeCell(calendar)).getText()).toBe('20');
      expect(getDialog().contains(document.activeElement)).toBe(true);
      expect(collapsedText(document.activeElement)).toBe('20');
    });

    it.each([
      { account: 'account 14', bounds: BEACON_BOUNDS, rangeHelper: BEACON_RANGE_HELPER },
      { account: 'account 8', bounds: LAKESIDE_BOUNDS, rangeHelper: 'Weeks from Mon Feb 2 to Mon Jul 20, 2026' },
      {
        account: 'bounds in different years',
        bounds: { weekStart: '2026-07-20', earliestWeek: '2025-12-29', latestCompleteWeek: LATEST_COMPLETE_WEEK },
        rangeHelper: 'Weeks from Mon Dec 29, 2025 to Mon Jul 20, 2026',
      },
    ])('shows "Weeks run Monday to Sunday." and "$rangeHelper" for $account (C-29, C-30)', async ({ bounds, rangeHelper }) => {
      const picker = await renderPicker(bounds);

      await openPicker(picker);

      expect(collapsedText(getDialog())).toContain(WEEKS_RUN_HELPER);
      expect(collapsedText(getDialog())).toContain(rangeHelper);
    });
  });

  describe('selectable weeks (UI-48)', () => {
    it('in July 2026 lets only Mon Jul 6, 13 and 20 be chosen, not Tue–Sun nor the partial week of Mon Jul 27', async () => {
      const picker = await renderPicker(BEACON_BOUNDS);

      const calendar = await openPicker(picker);

      expect(await selectableDays(calendar)).toEqual(['6', '13', '20']);
      expect(await (await dayCell(calendar, '27')).isDisabled()).toBe(true);
    });

    it('for account 14 in January 2026 lets only Mon Jan 26 be chosen, not Mon Jan 19', async () => {
      const picker = await renderPicker(BEACON_BOUNDS);
      const calendar = await openPicker(picker);

      await showMonth(calendar, /jan\w*\s+2026/i);

      expect(await selectableDays(calendar)).toEqual(['26']);
      expect(await (await dayCell(calendar, '19')).isDisabled()).toBe(true);
    });

    it('for account 8 starts at Mon Feb 2, with no earlier week to choose', async () => {
      const picker = await renderPicker(LAKESIDE_BOUNDS);
      const calendar = await openPicker(picker);

      await showMonth(calendar, /feb\w*\s+2026/i);
      const februaryMondays = await selectableDays(calendar);
      await calendar.previous();
      const monthBeforeFebruary = /jan\w*\s+2026/i.test(await calendar.getCurrentViewLabel());

      expect(februaryMondays).toEqual(['2', '9', '16', '23']);
      expect(monthBeforeFebruary ? await selectableDays(calendar) : []).toEqual([]);
    });

    it('marks Tue Jul 21 to Sun Jul 26 as part of the selected week, and no day outside it', async () => {
      const picker = await renderPicker(BEACON_BOUNDS);
      const calendar = await openPicker(picker);
      const selectedWeekRest = ['21', '22', '23', '24', '25', '26'];

      const cells = await calendar.getCells();
      const classesByDay = new Map(await Promise.all(cells.map(async (cell) => [await cell.getText(), await cellClasses(cell)] as const)));
      const daysOutsideSelectedWeek = [...classesByDay.keys()].filter((day) => day !== '20' && !selectedWeekRest.includes(day));
      const selectedWeekMarkers = [...(classesByDay.get('21') ?? [])].filter(
        (className) =>
          selectedWeekRest.every((day) => classesByDay.get(day)?.has(className)) &&
          daysOutsideSelectedWeek.every((day) => !classesByDay.get(day)?.has(className)),
      );

      expect(selectedWeekMarkers.length).toBeGreaterThan(0);
    });

    it('choosing Mon Jul 13 emits weekSelected "2026-07-13", closes the dialog and returns focus to the trigger', async () => {
      const picker = await renderPicker(BEACON_BOUNDS);
      const calendar = await openPicker(picker);

      await calendar.selectCell({ text: '13' });
      await afterScheduledFocus(picker.fixture);

      expect(picker.selectedWeeks).toEqual(['2026-07-13']);
      expect(openDialogs()).toEqual([]);
      expect(document.activeElement).toBe(getTrigger(picker.root));
    });
  });

  describe('"Latest week" button (UI-50, C-33)', () => {
    it('with Mon Jul 13 selected shows one enabled native button named exactly "Latest week" under the range helper', async () => {
      const picker = await renderPicker(BEACON_WEEK_BEFORE_LATEST_BOUNDS);
      await openPicker(picker, WEEK_BEFORE_LATEST_TRIGGER_NAME);

      const latestWeekButtons = latestWeekButtonsInDialog();
      const dialogText = collapsedText(getDialog());

      expect(latestWeekButtons).toHaveLength(1);
      expect(latestWeekButtons[0].type).toBe('button');
      expect(collapsedText(latestWeekButtons[0])).toBe(LATEST_WEEK);
      expect(latestWeekButtons[0].disabled).toBe(false);
      expect(dialogText.indexOf(LATEST_WEEK)).toBeGreaterThan(dialogText.indexOf(BEACON_RANGE_HELPER));
    });

    it('clicking it with Mon Jul 13 selected emits weekSelected "2026-07-20", closes the dialog and returns focus to the trigger', async () => {
      const picker = await renderPicker(BEACON_WEEK_BEFORE_LATEST_BOUNDS);
      await openPicker(picker, WEEK_BEFORE_LATEST_TRIGGER_NAME);

      getLatestWeekButton().click();
      await afterScheduledFocus(picker.fixture);

      expect(picker.selectedWeeks).toEqual([LATEST_COMPLETE_WEEK]);
      expect(openDialogs()).toEqual([]);
      expect(document.activeElement).toBe(getTrigger(picker.root, WEEK_BEFORE_LATEST_TRIGGER_NAME));
    });

    it('with Mon Jul 20 selected is still shown but natively disabled', async () => {
      const picker = await renderPicker(BEACON_BOUNDS);
      await openPicker(picker);

      const latestWeekButton = getLatestWeekButton();

      expect(latestWeekButton.disabled).toBe(true);
      expect(collapsedText(latestWeekButton)).toBe(LATEST_WEEK);
    });
  });

  describe('keyboard (UI-49)', () => {
    it('↑ moves focus from Mon Jul 20 to Mon Jul 13 and ↓ moves it back', async () => {
      const picker = await renderPicker(BEACON_BOUNDS);
      const calendar = await openPicker(picker);

      await pressOnActiveCell(picker, calendar, TestKey.UP_ARROW);
      const afterUp = await (await activeCell(calendar)).getText();
      const focusedAfterUp = collapsedText(document.activeElement);
      await pressOnActiveCell(picker, calendar, TestKey.DOWN_ARROW);
      const afterDown = await (await activeCell(calendar)).getText();

      expect(afterUp).toBe('13');
      expect(focusedAfterUp).toBe('13');
      expect(afterDown).toBe('20');
      expect(picker.selectedWeeks).toEqual([]);
    });

    it('Enter on Mon Jul 13 chooses it, closes the dialog and returns focus to the trigger', async () => {
      const picker = await renderPicker(BEACON_BOUNDS);
      const calendar = await openPicker(picker);

      await pressOnActiveCell(picker, calendar, TestKey.UP_ARROW);
      await pressOnActiveCell(picker, calendar, TestKey.ENTER);

      expect(picker.selectedWeeks).toEqual(['2026-07-13']);
      expect(openDialogs()).toEqual([]);
      expect(document.activeElement).toBe(getTrigger(picker.root));
    });

    it('Enter on the already selected Mon Jul 20 closes the dialog, returns focus to the trigger and does not emit weekSelected', async () => {
      const picker = await renderPicker(BEACON_BOUNDS);
      const calendar = await openPicker(picker);

      const focusedDay = await (await activeCell(calendar)).getText();
      await pressOnActiveCell(picker, calendar, TestKey.ENTER);

      expect(focusedDay).toBe('20');
      expect(openDialogs()).toEqual([]);
      expect(document.activeElement).toBe(getTrigger(picker.root));
      expect(picker.selectedWeeks).toEqual([]);
    });

    it('Enter on a non-Monday (Sun Jul 19) does nothing and keeps the dialog open', async () => {
      const picker = await renderPicker(BEACON_BOUNDS);
      const calendar = await openPicker(picker);

      await pressOnActiveCell(picker, calendar, TestKey.LEFT_ARROW);
      const focusedDay = await (await activeCell(calendar)).getText();
      await pressOnActiveCell(picker, calendar, TestKey.ENTER);

      expect(focusedDay).toBe('19');
      expect(picker.selectedWeeks).toEqual([]);
      expect(openDialogs()).toHaveLength(1);
    });

    it('Escape closes the dialog without choosing and returns focus to the trigger', async () => {
      const picker = await renderPicker(BEACON_BOUNDS);
      const calendar = await openPicker(picker);

      await pressOnActiveCell(picker, calendar, TestKey.ESCAPE);

      expect(openDialogs()).toEqual([]);
      expect(picker.selectedWeeks).toEqual([]);
      expect(document.activeElement).toBe(getTrigger(picker.root));
      expect(getTrigger(picker.root).getAttribute('aria-expanded')).toBe('false');
    });
  });
});
