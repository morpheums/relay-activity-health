import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Account, EventType, WeekRange } from '../../../core/models';
import { LATEST_COMPLETE_WEEK, seedAccounts, sundayOf } from '../../../../testing/activity-health-fixtures';
import { chooseOption, collapsedText, findButton, getButton, getSelect, isDisabled, optionTexts, selectedOptionText } from '../../../../testing/dom-queries';
import { DashboardFilters } from './dashboard-filters';

const PREVIOUS_WEEK = '◀ Previous week';
const NEXT_WEEK = 'Next week ▶';

interface FiltersInputs {
  accounts: readonly Account[];
  accountId: number;
  weekStart: string;
  earliestWeek: string;
  latestCompleteWeek: string;
  eventType: EventType;
}

interface FiltersUnderTest {
  root: HTMLElement;
  fixture: ComponentFixture<DashboardFilters>;
  selectedAccountIds: number[];
  selectedWeeks: string[];
  selectedEventTypes: EventType[];
}

const BEACON_DEFAULT_INPUTS: FiltersInputs = {
  accounts: seedAccounts(),
  accountId: 14,
  weekStart: '2026-07-20',
  earliestWeek: '2026-01-26',
  latestCompleteWeek: LATEST_COMPLETE_WEEK,
  eventType: 'all',
};

async function renderFilters(overrides: Partial<FiltersInputs> = {}): Promise<FiltersUnderTest> {
  const inputs = { ...BEACON_DEFAULT_INPUTS, ...overrides };
  TestBed.configureTestingModule({ imports: [DashboardFilters] });
  const fixture = TestBed.createComponent(DashboardFilters);
  fixture.componentRef.setInput('accounts', inputs.accounts);
  fixture.componentRef.setInput('accountId', inputs.accountId);
  fixture.componentRef.setInput('week', { start: inputs.weekStart, end: sundayOf(inputs.weekStart) });
  fixture.componentRef.setInput('earliestWeek', inputs.earliestWeek);
  fixture.componentRef.setInput('latestCompleteWeek', inputs.latestCompleteWeek);
  fixture.componentRef.setInput('eventType', inputs.eventType);
  const selectedAccountIds: number[] = [];
  const selectedWeeks: string[] = [];
  const selectedEventTypes: EventType[] = [];
  fixture.componentInstance.accountSelected.subscribe((accountId) => selectedAccountIds.push(accountId));
  fixture.componentInstance.weekSelected.subscribe((week) => selectedWeeks.push(week));
  fixture.componentInstance.eventTypeSelected.subscribe((eventType) => selectedEventTypes.push(eventType));
  await fixture.whenStable();
  return { root: fixture.nativeElement as HTMLElement, fixture, selectedAccountIds, selectedWeeks, selectedEventTypes };
}

interface WeekInputs {
  week?: WeekRange | null;
  earliestWeek?: string | null;
  latestCompleteWeek?: string | null;
}

async function renderFiltersWithWeekInputs(weekInputs: WeekInputs): Promise<FiltersUnderTest> {
  TestBed.configureTestingModule({ imports: [DashboardFilters] });
  const fixture = TestBed.createComponent(DashboardFilters);
  fixture.componentRef.setInput('accounts', seedAccounts());
  fixture.componentRef.setInput('accountId', 14);
  fixture.componentRef.setInput('eventType', 'all');
  Object.entries(weekInputs).forEach(([inputName, value]) => fixture.componentRef.setInput(inputName, value));
  const selectedAccountIds: number[] = [];
  const selectedWeeks: string[] = [];
  const selectedEventTypes: EventType[] = [];
  fixture.componentInstance.accountSelected.subscribe((accountId) => selectedAccountIds.push(accountId));
  fixture.componentInstance.weekSelected.subscribe((week) => selectedWeeks.push(week));
  fixture.componentInstance.eventTypeSelected.subscribe((eventType) => selectedEventTypes.push(eventType));
  await fixture.whenStable();
  return { root: fixture.nativeElement as HTMLElement, fixture, selectedAccountIds, selectedWeeks, selectedEventTypes };
}

const DEFAULT_WEEK_RANGE: WeekRange = { start: '2026-07-20', end: '2026-07-26' };

describe('DashboardFilters', () => {
  describe('Viewing as', () => {
    it('lists every account by its plain name', async () => {
      const { root } = await renderFilters();

      expect(optionTexts(getSelect(root, 'Viewing as'))).toEqual(seedAccounts().map((account) => account.name));
    });

    it('selects the option of the accountId input', async () => {
      const { root } = await renderFilters({ accountId: 6 });

      expect(selectedOptionText(getSelect(root, 'Viewing as'))).toBe('Metro Collision Centers');
    });

    it('emits accountSelected with the numeric account id when another account is chosen', async () => {
      const { root, selectedAccountIds } = await renderFilters();

      chooseOption(getSelect(root, 'Viewing as'), 'Lakeside Physio');

      expect(selectedAccountIds.at(-1)).toBe(8);
      expect(typeof selectedAccountIds.at(-1)).toBe('number');
    });
  });

  describe('Activity type', () => {
    it('offers All activity, Calls, Leads, Appointments', async () => {
      const { root } = await renderFilters();

      expect(optionTexts(getSelect(root, 'Activity type'))).toEqual(['All activity', 'Calls', 'Leads', 'Appointments']);
    });

    it.each<{ eventType: EventType; optionText: string }>([
      { eventType: 'all', optionText: 'All activity' },
      { eventType: 'call_received', optionText: 'Calls' },
      { eventType: 'lead_created', optionText: 'Leads' },
      { eventType: 'appointment_set', optionText: 'Appointments' },
    ])('selects "$optionText" for eventType $eventType', async ({ eventType, optionText }) => {
      const { root } = await renderFilters({ eventType });

      expect(selectedOptionText(getSelect(root, 'Activity type'))).toBe(optionText);
    });

    it.each<{ optionText: string; eventType: EventType }>([
      { optionText: 'Calls', eventType: 'call_received' },
      { optionText: 'Leads', eventType: 'lead_created' },
      { optionText: 'Appointments', eventType: 'appointment_set' },
    ])('emits eventTypeSelected "$eventType" when "$optionText" is chosen', async ({ optionText, eventType }) => {
      const { root, selectedEventTypes } = await renderFilters();

      chooseOption(getSelect(root, 'Activity type'), optionText);

      expect(selectedEventTypes.at(-1)).toBe(eventType);
    });

    it('emits eventTypeSelected "all" when "All activity" is chosen from Calls', async () => {
      const { root, selectedEventTypes } = await renderFilters({ eventType: 'call_received' });

      chooseOption(getSelect(root, 'Activity type'), 'All activity');

      expect(selectedEventTypes.at(-1)).toBe('all');
    });
  });

  describe('week stepper', () => {
    it('shows the week label "Mon Jul 20 – Sun Jul 26, 2026"', async () => {
      const { root } = await renderFilters();

      expect(collapsedText(root)).toContain('Mon Jul 20 – Sun Jul 26, 2026');
    });

    it('disables "Next week ▶" and enables "◀ Previous week" at the latest complete week', async () => {
      const { root } = await renderFilters();

      expect(isDisabled(getButton(root, NEXT_WEEK))).toBe(true);
      expect(isDisabled(getButton(root, PREVIOUS_WEEK))).toBe(false);
    });

    it('disables "◀ Previous week" and enables "Next week ▶" at earliestWeek', async () => {
      const { root } = await renderFilters({ weekStart: '2026-01-26' });

      expect(isDisabled(getButton(root, PREVIOUS_WEEK))).toBe(true);
      expect(isDisabled(getButton(root, NEXT_WEEK))).toBe(false);
    });

    it('enables both buttons between the bounds', async () => {
      const { root } = await renderFilters({ weekStart: '2026-03-02' });

      expect(isDisabled(getButton(root, PREVIOUS_WEEK))).toBe(false);
      expect(isDisabled(getButton(root, NEXT_WEEK))).toBe(false);
    });

    it('disables both buttons for account 20, where earliestWeek equals latestCompleteWeek', async () => {
      const { root } = await renderFilters({ accountId: 20, weekStart: '2026-07-20', earliestWeek: '2026-07-20' });

      expect(isDisabled(getButton(root, PREVIOUS_WEEK))).toBe(true);
      expect(isDisabled(getButton(root, NEXT_WEEK))).toBe(true);
    });

    it('keeps both selects enabled for account 20', async () => {
      const { root } = await renderFilters({ accountId: 20, weekStart: '2026-07-20', earliestWeek: '2026-07-20' });

      expect(getSelect(root, 'Viewing as').disabled).toBe(false);
      expect(getSelect(root, 'Activity type').disabled).toBe(false);
    });

    it('emits weekSelected with the previous Monday when "◀ Previous week" is clicked', async () => {
      const { root, selectedWeeks } = await renderFilters();

      getButton(root, PREVIOUS_WEEK).click();

      expect(selectedWeeks).toEqual(['2026-07-13']);
    });

    it('emits weekSelected with the next Monday when "Next week ▶" is clicked', async () => {
      const { root, selectedWeeks } = await renderFilters({ weekStart: '2026-07-13' });

      getButton(root, NEXT_WEEK).click();

      expect(selectedWeeks).toEqual(['2026-07-20']);
    });

    it('crosses a month boundary: previous week of 2026-03-02 is 2026-02-23', async () => {
      const { root, selectedWeeks } = await renderFilters({ weekStart: '2026-03-02' });

      getButton(root, PREVIOUS_WEEK).click();

      expect(selectedWeeks).toEqual(['2026-02-23']);
    });

    it.each([
      { transition: 'US DST end 2026-11-01', weekStart: '2026-10-26', button: NEXT_WEEK, expectedWeek: '2026-11-02' },
      { transition: 'EU DST end 2026-10-25', weekStart: '2026-10-19', button: NEXT_WEEK, expectedWeek: '2026-10-26' },
      { transition: 'US DST start 2026-03-08', weekStart: '2026-03-09', button: PREVIOUS_WEEK, expectedWeek: '2026-03-02' },
      { transition: 'EU DST start 2026-03-29', weekStart: '2026-03-30', button: PREVIOUS_WEEK, expectedWeek: '2026-03-23' },
    ])('steps across the $transition week from $weekStart to $expectedWeek', async ({ weekStart, button, expectedWeek }) => {
      const { root, selectedWeeks } = await renderFilters({ weekStart, latestCompleteWeek: '2026-11-09' });

      getButton(root, button).click();

      expect(selectedWeeks).toEqual([expectedWeek]);
    });
  });

  describe('before the first report (UI-44)', () => {
    it('renders "Viewing as" with Beacon Home Security selected when no week inputs are set', async () => {
      const { root } = await renderFiltersWithWeekInputs({});

      expect(getSelect(root, 'Viewing as').disabled).toBe(false);
      expect(selectedOptionText(getSelect(root, 'Viewing as'))).toBe('Beacon Home Security');
    });

    it('renders "Activity type" with All activity selected when no week inputs are set', async () => {
      const { root } = await renderFiltersWithWeekInputs({});

      expect(getSelect(root, 'Activity type').disabled).toBe(false);
      expect(selectedOptionText(getSelect(root, 'Activity type'))).toBe('All activity');
    });

    it('renders no week buttons and no week label when no week inputs are set', async () => {
      const { root } = await renderFiltersWithWeekInputs({});

      expect(findButton(root, PREVIOUS_WEEK)).toBeNull();
      expect(findButton(root, NEXT_WEEK)).toBeNull();
      expect(collapsedText(root)).not.toContain('Mon Jul');
    });

    it('emits accountSelected 6 when Metro Collision Centers is chosen with no week inputs set', async () => {
      const { root, selectedAccountIds } = await renderFiltersWithWeekInputs({});

      chooseOption(getSelect(root, 'Viewing as'), 'Metro Collision Centers');

      expect(selectedAccountIds).toEqual([6]);
    });

    it('emits eventTypeSelected "call_received" when Calls is chosen with no week inputs set', async () => {
      const { root, selectedEventTypes } = await renderFiltersWithWeekInputs({});

      chooseOption(getSelect(root, 'Activity type'), 'Calls');

      expect(selectedEventTypes).toEqual(['call_received']);
    });

    it.each<{ missingInput: string; weekInputs: WeekInputs }>([
      { missingInput: 'week', weekInputs: { week: null, earliestWeek: '2026-01-26', latestCompleteWeek: LATEST_COMPLETE_WEEK } },
      { missingInput: 'earliestWeek', weekInputs: { week: DEFAULT_WEEK_RANGE, earliestWeek: null, latestCompleteWeek: LATEST_COMPLETE_WEEK } },
      { missingInput: 'latestCompleteWeek', weekInputs: { week: DEFAULT_WEEK_RANGE, earliestWeek: '2026-01-26', latestCompleteWeek: null } },
    ])('renders no week buttons while $missingInput is null, and both selects still render', async ({ weekInputs }) => {
      const { root } = await renderFiltersWithWeekInputs(weekInputs);

      expect(findButton(root, PREVIOUS_WEEK)).toBeNull();
      expect(findButton(root, NEXT_WEEK)).toBeNull();
      expect(getSelect(root, 'Viewing as')).toBeTruthy();
      expect(getSelect(root, 'Activity type')).toBeTruthy();
    });

    it('renders both week buttons once week, earliestWeek and latestCompleteWeek are all set', async () => {
      const { root } = await renderFiltersWithWeekInputs({ week: DEFAULT_WEEK_RANGE, earliestWeek: '2026-01-26', latestCompleteWeek: LATEST_COMPLETE_WEEK });

      expect(findButton(root, PREVIOUS_WEEK)).not.toBeNull();
      expect(findButton(root, NEXT_WEEK)).not.toBeNull();
    });
  });
});
