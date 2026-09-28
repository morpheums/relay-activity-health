import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { routes } from '../../app.routes';
import { AccountsApi } from '../../core/api/accounts.api';
import { ActivityHealthApi, ActivityHealthRequest } from '../../core/api/activity-health.api';
import { EventType } from '../../core/models';
import {
  BEACON_HOME_SECURITY,
  beaconDefaultWeekReport,
  buildReport,
  locationRow,
  quietHarborEmptyReport,
  withRange,
  withoutEnoughHistory,
} from '../../../testing/activity-health-fixtures';
import {
  cellTexts,
  columnHeaderTexts,
  collapsedText,
  findButton,
  getButton,
  getSelect,
  chooseOption,
  hasTable,
  isDisabled,
  locationRows,
  optionTexts,
  selectedOptionText,
  textOutsideTables,
} from '../../../testing/dom-queries';
import { FakeAccountsApi, FakeActivityHealthApi, networkFailure, serverError } from '../../../testing/fake-apis';
import { RecordedNavigation, currentPath, currentQueryParams, queryParamsOf, recordNavigations, settle } from '../../../testing/router-testing';
import { DashboardState } from './dashboard-state';

interface PageUnderTest {
  root: HTMLElement;
  activityHealthApi: FakeActivityHealthApi;
  navigations: RecordedNavigation[];
  harness: RouterTestingHarness;
}

const PREVIOUS_WEEK = '◀ Previous week';
const NEXT_WEEK = 'Next week ▶';
const ABOVE = '▲ Higher than usual';
const BELOW = '▼ Lower than usual';
const WITHIN = 'Within usual range';
const EMPTY_ACCOUNT_MESSAGE = 'No activity recorded for this account yet.';
const LOAD_ERROR_MESSAGE = "We couldn't load this week's activity. Try again.";
const ACCOUNT_METHOD_LINE = 'Compared with the last 8 full weeks for this account';
const FOOTNOTE_LINES = [
  'Compared with the last 8 full weeks at this location',
  'Inbound events, not unique customers',
  'Exact duplicates counted once',
  "Locations that usually get 2 or fewer events a week can't show 'lower than usual'",
];
const DATA_AS_OF_LINE = 'Data as of Mon Jul 27, 2026';
const PER_TYPE_LINE = 'Per-type counts at a single location are small; only large changes show up.';
const FORBIDDEN_ON_SCREEN: RegExp[] = [/\bz\b/, /σ/, /±/, /\bmedian\b/i, /\btypical\b/i, /\bdeviation\b/i, /\bNormal\b/];

async function openPage(url: string, prepareApi?: (activityHealthApi: FakeActivityHealthApi) => void): Promise<PageUnderTest> {
  const activityHealthApi = new FakeActivityHealthApi();
  prepareApi?.(activityHealthApi);
  TestBed.configureTestingModule({
    providers: [
      provideRouter(routes),
      { provide: ActivityHealthApi, useValue: activityHealthApi },
      { provide: AccountsApi, useValue: new FakeAccountsApi() },
      DashboardState,
    ],
  });
  const navigations = recordNavigations(TestBed.inject(Router));
  const harness = await RouterTestingHarness.create();
  await harness.navigateByUrl(url);
  await settle(harness);
  return { root: harness.fixture.nativeElement as HTMLElement, activityHealthApi, navigations, harness };
}

function pageText(root: HTMLElement): string {
  return collapsedText(root);
}

function rowNames(root: HTMLElement): string[] {
  return locationRows(root).map((row) => cellTexts(row).find((cell) => /^Site [A-Z]$/.test(cell)) ?? collapsedText(row));
}

function firstRow(root: HTMLElement): HTMLElement {
  const [row] = locationRows(root);
  if (!row) {
    throw new Error(`No location rows in: ${collapsedText(root)}`);
  }
  return row;
}

function rowFor(root: HTMLElement, location: string): HTMLElement {
  const row = locationRows(root).find((candidate) => cellTexts(candidate).includes(location));
  if (!row) {
    throw new Error(`No row for ${location} in: ${collapsedText(root)}`);
  }
  return row;
}

function pageHeadingTexts(root: HTMLElement): string[] {
  return Array.from(root.querySelectorAll('h1')).map((heading) => collapsedText(heading));
}

function elementsWithExactText(root: HTMLElement, text: string): Element[] {
  return Array.from(root.querySelectorAll('*')).filter((element) => collapsedText(element) === text);
}

function usualRangeCellText(root: HTMLElement, location: string): string {
  const usualRangeColumn = columnHeaderTexts(root).indexOf('Usual range');
  expect(usualRangeColumn).toBeGreaterThanOrEqual(0);
  return cellTexts(rowFor(root, location))[usualRangeColumn];
}

const FIRST_LOAD_FAILURES = [
  { label: '5xx', failure: serverError },
  { label: 'network', failure: networkFailure },
];

describe('DashboardPage', () => {
  describe('default view (account 14, week 2026-07-20, all)', () => {
    it('rewrites /dashboard to ?account=14&week=2026-07-20&type=all', async () => {
      await openPage('/dashboard');

      expect(currentPath()).toBe('/dashboard');
      expect(currentQueryParams()).toEqual({ account: '14', week: '2026-07-20', type: 'all' });
    });

    it('shows Beacon Home Security selected under "Viewing as"', async () => {
      const { root } = await openPage('/dashboard');

      expect(selectedOptionText(getSelect(root, 'Viewing as'))).toBe('Beacon Home Security');
    });

    it('lists every account in the Viewing-as select, including the empty account Quiet Harbor Spa', async () => {
      const { root } = await openPage('/dashboard');

      expect([...optionTexts(getSelect(root, 'Viewing as'))].sort()).toEqual(
        ['Beacon Home Security', 'Lakeside Physio', 'Metro Collision Centers', 'Quiet Harbor Spa', 'Redline Tire & Service'],
      );
    });

    it('shows the summary heading and the account method line', async () => {
      const { root } = await openPage('/dashboard');

      expect(pageText(root)).toContain('Beacon Home Security — all locations');
      expect(pageText(root)).toContain(ACCOUNT_METHOD_LINE);
    });

    it('shows the summary as "26 inbound events · usually 18–38 a week" and "Within usual range"', async () => {
      const { root } = await openPage('/dashboard');

      expect(textOutsideTables(root)).toContain('26 inbound events · usually 18–38 a week');
      expect(textOutsideTables(root)).toContain(WITHIN);
    });

    it('shows the table headers Location, Events, Usual range, Status', async () => {
      const { root } = await openPage('/dashboard');

      expect(columnHeaderTexts(root)).toEqual(['Location', 'Events', 'Usual range', 'Status']);
    });

    it('shows Site B first with 2, "Usually 3–12 a week" and "▼ Lower than usual"', async () => {
      const { root } = await openPage('/dashboard');

      const topRow = firstRow(root);
      expect(cellTexts(topRow)).toContain('Site B');
      expect(cellTexts(topRow)).toContain('2');
      expect(collapsedText(topRow)).toContain('Usually 3–12 a week');
      expect(collapsedText(topRow)).toContain(BELOW);
    });

    it('shows the rows in the order B, C, A, D with C, A and D "Within usual range"', async () => {
      const { root } = await openPage('/dashboard');

      expect(rowNames(root)).toEqual(['Site B', 'Site C', 'Site A', 'Site D']);
      ['Site C', 'Site A', 'Site D'].forEach((location) => expect(collapsedText(rowFor(root, location))).toContain(WITHIN));
    });

    it('shows the week label "Mon Jul 20 – Sun Jul 26, 2026"', async () => {
      const { root } = await openPage('/dashboard');

      expect(pageText(root)).toContain('Mon Jul 20 – Sun Jul 26, 2026');
    });

    it('disables "Next week ▶" at the latest complete week and enables "◀ Previous week"', async () => {
      const { root } = await openPage('/dashboard');

      expect(isDisabled(getButton(root, NEXT_WEEK))).toBe(true);
      expect(isDisabled(getButton(root, PREVIOUS_WEEK))).toBe(false);
    });

    it('offers the Activity type options All activity, Calls, Leads, Appointments with All activity selected', async () => {
      const { root } = await openPage('/dashboard');

      const typeSelect = getSelect(root, 'Activity type');
      expect(optionTexts(typeSelect)).toEqual(['All activity', 'Calls', 'Leads', 'Appointments']);
      expect(selectedOptionText(typeSelect)).toBe('All activity');
    });

    it('shows the capitalised footnote lines and "Data as of Mon Jul 27, 2026", without the per-type line', async () => {
      const { root } = await openPage('/dashboard');

      [...FOOTNOTE_LINES, DATA_AS_OF_LINE].forEach((line) => expect(pageText(root)).toContain(line));
      expect(pageText(root)).not.toContain(PER_TYPE_LINE);
    });

    it('never shows deviation, z, σ, ±, median, typical or a standalone "Normal"', async () => {
      const { root } = await openPage('/dashboard');

      expect(pageText(root)).toContain('Beacon Home Security — all locations');
      FORBIDDEN_ON_SCREEN.forEach((forbidden) => expect(pageText(root)).not.toMatch(forbidden));
    });
  });

  describe('scenarios', () => {
    it('spike week: summary "880 inbound events · usually 39–101 a week" is higher than usual and every one of the 15 rows too', async () => {
      const { root } = await openPage('/dashboard?account=6&week=2026-06-01&type=all');

      expect(textOutsideTables(root)).toContain('880 inbound events · usually 39–101 a week');
      expect(textOutsideTables(root)).toContain(ABOVE);
      const rows = locationRows(root);
      expect(rows).toHaveLength(15);
      rows.forEach((row) => expect(collapsedText(row)).toContain(ABOVE));
    });

    it('spike week: first row is Site C with 67 and "Usually 1–7 a week"', async () => {
      const { root } = await openPage('/dashboard?account=6&week=2026-06-01&type=all');

      const topRow = firstRow(root);
      expect(cellTexts(topRow)).toContain('Site C');
      expect(cellTexts(topRow)).toContain('67');
      expect(collapsedText(topRow)).toContain('Usually 1–7 a week');
    });

    it('week after the spike: summary within usual range (102, usually 37–104) with Site C then Site J higher than usual at the top', async () => {
      const { root } = await openPage('/dashboard?account=6&week=2026-06-08&type=all');

      expect(textOutsideTables(root)).toContain('102 inbound events · usually 37–104 a week');
      expect(textOutsideTables(root)).toContain(WITHIN);
      expect(textOutsideTables(root)).not.toContain(ABOVE);
      expect(rowNames(root).slice(0, 2)).toEqual(['Site C', 'Site J']);
      expect(collapsedText(firstRow(root))).toContain(ABOVE);
      expect(collapsedText(locationRows(root)[1])).toContain(ABOVE);
    });

    it('spike in the baseline: summary "87 inbound events · usually 30–134 a week" and all 15 rows within usual range', async () => {
      const { root } = await openPage('/dashboard?account=6&week=2026-07-20&type=all');

      expect(textOutsideTables(root)).toContain('87 inbound events · usually 30–134 a week');
      expect(textOutsideTables(root)).toContain(WITHIN);
      const rows = locationRows(root);
      expect(rows).toHaveLength(15);
      rows.forEach((row) => expect(collapsedText(row)).toContain(WITHIN));
    });

    it('choosing Calls writes type=call_received as a new history entry and shows "51 calls · usually 17–79 a week" plus the per-type footnote line', async () => {
      const { root, navigations, harness } = await openPage('/dashboard?account=6&week=2026-07-20&type=all');

      chooseOption(getSelect(root, 'Activity type'), 'Calls');
      await settle(harness);

      expect(currentQueryParams()).toEqual({ account: '6', week: '2026-07-20', type: 'call_received' });
      expect(navigations.at(-1)?.replaceUrl).toBe(false);
      expect(textOutsideTables(root)).toContain('51 calls · usually 17–79 a week');
      expect(pageText(root)).not.toContain('inbound event');
      expect(pageText(root)).toContain(PER_TYPE_LINE);
    });

    it('single-site account 8: one row, Site A, 7, "Usually 5–17 a week", within usual range, and the same figures in the summary', async () => {
      const { root } = await openPage('/dashboard?account=8');

      const rows = locationRows(root);
      expect(rows).toHaveLength(1);
      expect(cellTexts(rows[0])).toContain('Site A');
      expect(cellTexts(rows[0])).toContain('7');
      expect(collapsedText(rows[0])).toContain('Usually 5–17 a week');
      expect(collapsedText(rows[0])).toContain(WITHIN);
      expect(textOutsideTables(root)).toContain('7 inbound events · usually 5–17 a week');
    });

    it('insufficient history: summary reads "8 inbound events" with no range and "Not enough history yet (3 of 4 weeks needed)"', async () => {
      const { root } = await openPage('/dashboard?account=8&week=2026-03-02&type=all');

      const summaryText = textOutsideTables(root);
      expect(summaryText).toContain('8 inbound events');
      expect(summaryText).not.toContain('8 inbound events ·');
      expect(summaryText).not.toMatch(/usually \d+–\d+ a week/);
      expect(summaryText).toContain('Not enough history yet (3 of 4 weeks needed)');
    });

    it('no eligible weeks: every row shows its count and "Not enough history yet (0 of 4 weeks needed)" with no range', async () => {
      const { root } = await openPage('/dashboard?account=14&week=2026-02-02&type=all');

      const rows = locationRows(root);
      expect(rows).toHaveLength(4);
      const expectedCounts: Record<string, string> = { 'Site A': '9', 'Site B': '5', 'Site C': '7', 'Site D': '6' };
      rows.forEach((row) => {
        const location = cellTexts(row).find((cell) => cell in expectedCounts) ?? '';
        expect(cellTexts(row)).toContain(expectedCounts[location]);
        expect(collapsedText(row)).toContain('Not enough history yet (0 of 4 weeks needed)');
        expect(collapsedText(row)).not.toContain('Usually');
      });
    });

    it('mixed history: Site D higher than usual first, Sites A and C last with "Not enough history yet (3 of 4 weeks needed)"', async () => {
      const { root } = await openPage('/dashboard?account=14&week=2026-03-02&type=all');

      expect(rowNames(root)).toEqual(['Site D', 'Site B', 'Site A', 'Site C']);
      expect(collapsedText(rowFor(root, 'Site D'))).toContain(ABOVE);
      ['Site A', 'Site C'].forEach((location) =>
        expect(collapsedText(rowFor(root, location))).toContain('Not enough history yet (3 of 4 weeks needed)'),
      );
    });

    it('zero-activity location: first row Site G, 0, "Usually 2–9 a week", "▼ Lower than usual"', async () => {
      const { root } = await openPage('/dashboard?account=6&week=2026-06-29&type=all');

      const topRow = firstRow(root);
      expect(cellTexts(topRow)).toContain('Site G');
      expect(cellTexts(topRow)).toContain('0');
      expect(collapsedText(topRow)).toContain('Usually 2–9 a week');
      expect(collapsedText(topRow)).toContain(BELOW);
    });

    it('appointments: Sites A and B show 0 within usual range, Site B "Usually 0–2 a week", and the small-location footnote', async () => {
      const { root } = await openPage('/dashboard?account=14&week=2026-07-20&type=appointment_set');

      ['Site A', 'Site B'].forEach((location) => {
        expect(cellTexts(rowFor(root, location))).toContain('0');
        expect(collapsedText(rowFor(root, location))).toContain(WITHIN);
      });
      expect(collapsedText(rowFor(root, 'Site B'))).toContain('Usually 0–2 a week');
      expect(pageText(root)).toContain("Locations that usually get 2 or fewer events a week can't show 'lower than usual'");
    });

    it('earliest week: "◀ Previous week" disabled and only Sites B and D listed, without the empty-account message', async () => {
      const { root } = await openPage('/dashboard?account=14&week=2026-01-26&type=all');

      expect(isDisabled(getButton(root, PREVIOUS_WEEK))).toBe(true);
      expect(isDisabled(getButton(root, NEXT_WEEK))).toBe(false);
      expect(rowNames(root)).toEqual(['Site B', 'Site D']);
      expect(pageText(root)).not.toContain(EMPTY_ACCOUNT_MESSAGE);
    });

    it.each([
      { url: '/dashboard?account=6&week=2026-06-01&type=all', anchor: '880 inbound events · usually 39–101 a week' },
      { url: '/dashboard?account=6&week=2026-06-08&type=all', anchor: '102 inbound events · usually 37–104 a week' },
      { url: '/dashboard?account=6&week=2026-07-20&type=call_received', anchor: '51 calls · usually 17–79 a week' },
      { url: '/dashboard?account=8&week=2026-03-02&type=all', anchor: 'Not enough history yet (3 of 4 weeks needed)' },
      { url: '/dashboard?account=14&week=2026-03-02&type=all', anchor: '40 inbound events · usually 16–36 a week' },
      { url: '/dashboard?account=14&week=2026-07-20&type=appointment_set', anchor: '2 appointments · usually 1–8 a week' },
      { url: '/dashboard?account=20', anchor: EMPTY_ACCOUNT_MESSAGE },
    ])('never shows deviation, z, σ, ±, median, typical or a standalone "Normal" at $url', async ({ url, anchor }) => {
      const { root } = await openPage(url);

      expect(pageText(root)).toContain(anchor);
      FORBIDDEN_ON_SCREEN.forEach((forbidden) => expect(pageText(root)).not.toMatch(forbidden));
    });
  });

  describe('empty account', () => {
    it('replaces both the summary and the table with "No activity recorded for this account yet."', async () => {
      const { root } = await openPage('/dashboard?account=20');

      expect(pageText(root)).toContain(EMPTY_ACCOUNT_MESSAGE);
      expect(pageText(root)).not.toContain('0 inbound events');
      expect(pageText(root)).not.toContain('Not enough history yet');
      expect(hasTable(root)).toBe(false);
      expect(pageText(root)).not.toContain('Usual range');
      expect(pageText(root)).not.toContain(LOAD_ERROR_MESSAGE);
    });

    it('keeps the filters visible and usable and disables both week buttons', async () => {
      const { root } = await openPage('/dashboard?account=20');

      expect(getSelect(root, 'Viewing as').disabled).toBe(false);
      expect(getSelect(root, 'Activity type').disabled).toBe(false);
      expect(isDisabled(getButton(root, PREVIOUS_WEEK))).toBe(true);
      expect(isDisabled(getButton(root, NEXT_WEEK))).toBe(true);
    });

    it('still shows the full footnote including "Data as of Mon Jul 27, 2026"', async () => {
      const { root } = await openPage('/dashboard?account=20');

      [...FOOTNOTE_LINES, DATA_AS_OF_LINE].forEach((line) => expect(pageText(root)).toContain(line));
    });

    it('hides the "Data as of" line when dataAsOf is null and still shows the other footnote lines', async () => {
      const { root } = await openPage('/dashboard?account=20', (api) => api.register(quietHarborEmptyReport(null)));

      expect(pageText(root)).toContain(EMPTY_ACCOUNT_MESSAGE);
      expect(pageText(root)).not.toContain('Data as of');
      expect(pageText(root)).not.toContain('Invalid Date');
      expect(pageText(root)).not.toContain('null');
      FOOTNOTE_LINES.forEach((line) => expect(pageText(root)).toContain(line));
    });

    it('renders "Data as of" in the account timezone (2026-07-27T02:00:00Z is Sun Jul 26 in America/Los_Angeles)', async () => {
      const { root } = await openPage('/dashboard?account=20', (api) => api.register(quietHarborEmptyReport('2026-07-27T02:00:00Z')));

      expect(pageText(root)).toContain('Data as of Sun Jul 26, 2026');
    });

    it('shows the empty-account message whenever locations are empty and weeksUsed is 0, regardless of earliestWeek', async () => {
      const emptyBeaconReport = buildReport({
        account: BEACON_HOME_SECURITY,
        weekStart: '2026-07-20',
        earliestWeek: '2026-01-26',
        summary: withoutEnoughHistory(0, 0),
        locations: [],
      });
      const { root } = await openPage('/dashboard?account=14&week=2026-07-20&type=all', (api) => api.register(emptyBeaconReport));

      expect(pageText(root)).toContain(EMPTY_ACCOUNT_MESSAGE);
      expect(hasTable(root)).toBe(false);
    });
  });

  describe('loading and errors', () => {
    it('shows "Loading…" while the report is pending and removes it when the report arrives', async () => {
      const { root, activityHealthApi, harness } = await openPage('/dashboard?account=14&week=2026-07-20&type=all', (api) => api.holdNext());

      expect(pageText(root)).toContain('Loading…');

      activityHealthApi.releaseHeld();
      await settle(harness);

      expect(pageText(root)).not.toContain('Loading…');
      expect(textOutsideTables(root)).toContain('26 inbound events · usually 18–38 a week');
    });

    it('shows the load error message and a "Try again" button when the API fails, keeping the filters in the URL', async () => {
      const { root } = await openPage('/dashboard?account=6&week=2026-06-01&type=call_received', (api) => api.failNext(serverError()));

      expect(pageText(root)).toContain(LOAD_ERROR_MESSAGE);
      expect(getButton(root, 'Try again')).toBeTruthy();
      expect(currentQueryParams()).toEqual({ account: '6', week: '2026-06-01', type: 'call_received' });
    });

    it('"Try again" reloads the same params and shows the data without changing the URL', async () => {
      const { root, activityHealthApi, harness } = await openPage('/dashboard?account=14&week=2026-07-20&type=all', (api) =>
        api.failNext(serverError()),
      );
      const urlBeforeRetry = TestBed.inject(Router).url;

      getButton(root, 'Try again').click();
      await settle(harness);

      expect(activityHealthApi.requests).toHaveLength(2);
      expect(activityHealthApi.requests[1]).toEqual({ accountId: 14, week: '2026-07-20', eventType: 'all' });
      expect(pageText(root)).not.toContain(LOAD_ERROR_MESSAGE);
      expect(textOutsideTables(root)).toContain('26 inbound events · usually 18–38 a week');
      expect(TestBed.inject(Router).url).toBe(urlBeforeRetry);
    });

    it('recovers from a week before earliestWeek by showing the default week without an error message', async () => {
      const { root } = await openPage('/dashboard?account=14&week=2025-12-29&type=all');

      expect(currentQueryParams()).toEqual({ account: '14', week: '2026-07-20', type: 'all' });
      expect(pageText(root)).not.toContain(LOAD_ERROR_MESSAGE);
      expect(textOutsideTables(root)).toContain('26 inbound events · usually 18–38 a week');
    });

    it('shows the empty state, not an error, for account 20 at week 2026-03-02', async () => {
      const { root } = await openPage('/dashboard?account=20&week=2026-03-02&type=all');

      expect(currentQueryParams()).toEqual({ account: '20', week: '2026-07-20', type: 'all' });
      expect(pageText(root)).toContain(EMPTY_ACCOUNT_MESSAGE);
      expect(pageText(root)).not.toContain(LOAD_ERROR_MESSAGE);
    });
  });

  describe('controls write the URL', () => {
    it('"◀ Previous week" from the default writes week=2026-07-13 as a new history entry and enables "Next week ▶"', async () => {
      const { root, navigations, harness } = await openPage('/dashboard?account=14&week=2026-07-20&type=all');

      getButton(root, PREVIOUS_WEEK).click();
      await settle(harness);

      expect(currentQueryParams()).toEqual({ account: '14', week: '2026-07-13', type: 'all' });
      expect(navigations.at(-1)?.replaceUrl).toBe(false);
      expect(isDisabled(getButton(root, NEXT_WEEK))).toBe(false);
      expect(pageText(root)).toContain('Mon Jul 13 – Sun Jul 19, 2026');
    });

    it('"Next week ▶" from 2026-07-13 writes week=2026-07-20 as a new history entry', async () => {
      const { root, navigations, harness } = await openPage('/dashboard?account=14&week=2026-07-13&type=all');

      getButton(root, NEXT_WEEK).click();
      await settle(harness);

      expect(currentQueryParams()).toEqual({ account: '14', week: '2026-07-20', type: 'all' });
      expect(navigations.at(-1)?.replaceUrl).toBe(false);
    });

    it('switching "Viewing as" keeps week and type (14 to 6 at 2026-03-02, calls) as a new history entry', async () => {
      const { root, navigations, harness } = await openPage('/dashboard?account=14&week=2026-03-02&type=call_received');

      chooseOption(getSelect(root, 'Viewing as'), 'Metro Collision Centers');
      await settle(harness);

      expect(currentQueryParams()).toEqual({ account: '6', week: '2026-03-02', type: 'call_received' });
      expect(navigations.at(-1)?.replaceUrl).toBe(false);
    });

    it('switching "Viewing as" to an account that rejects the kept week falls back to the latest complete week with replaceUrl and no error', async () => {
      const { root, navigations, harness } = await openPage('/dashboard?account=14&week=2026-01-26&type=call_received');

      chooseOption(getSelect(root, 'Viewing as'), 'Lakeside Physio');
      await settle(harness);

      expect(currentQueryParams()).toEqual({ account: '8', week: '2026-07-20', type: 'call_received' });
      expect(navigations.at(-1)?.replaceUrl).toBe(true);
      expect(pageText(root)).not.toContain(LOAD_ERROR_MESSAGE);
    });

    it('selections made with the controls survive reopening the written URL', async () => {
      const first = await openPage('/dashboard?account=14&week=2026-06-01&type=all');
      chooseOption(getSelect(first.root, 'Viewing as'), 'Metro Collision Centers');
      await settle(first.harness);
      chooseOption(getSelect(first.root, 'Activity type'), 'Calls');
      await settle(first.harness);
      const writtenUrl = TestBed.inject(Router).url;
      expect(currentQueryParams()).toEqual({ account: '6', week: '2026-06-01', type: 'call_received' });
      TestBed.resetTestingModule();

      const reopened = await openPage(writtenUrl);

      expect(currentQueryParams()).toEqual({ account: '6', week: '2026-06-01', type: 'call_received' });
      expect(selectedOptionText(getSelect(reopened.root, 'Viewing as'))).toBe('Metro Collision Centers');
      expect(selectedOptionText(getSelect(reopened.root, 'Activity type'))).toBe('Calls');
      expect(reopened.activityHealthApi.requests.at(-1)).toEqual({ accountId: 6, week: '2026-06-01', eventType: 'call_received' });
    });
  });

  describe('nouns by activity type', () => {
    it.each<[EventType, string]>([
      ['all', '26 inbound events · usually 18–38 a week'],
      ['call_received', '16 calls · usually 9–24 a week'],
      ['lead_created', '8 leads · usually 2–12 a week'],
      ['appointment_set', '2 appointments · usually 1–8 a week'],
    ])('account 14, 2026-07-20, type %s reads "%s"', async (eventType, expectedSummaryLine) => {
      const { root } = await openPage(`/dashboard?account=14&week=2026-07-20&type=${eventType}`);

      expect(textOutsideTables(root)).toContain(expectedSummaryLine);
    });

    it.each<[EventType, string]>([
      ['all', '1 inbound event · usually 0–4 a week'],
      ['call_received', '1 call · usually 0–4 a week'],
      ['lead_created', '1 lead · usually 0–4 a week'],
      ['appointment_set', '1 appointment · usually 0–4 a week'],
    ])('uses the singular noun for a count of 1 with type %s: "%s"', async (eventType, expectedSummaryLine) => {
      const singleEventReport = buildReport({
        account: BEACON_HOME_SECURITY,
        weekStart: '2026-07-20',
        eventType,
        summary: withRange(1, 1, 0, 4, 'normal', 0),
        locations: [locationRow('Site A', withRange(1, 1, 0, 4, 'normal', 0))],
      });
      const { root } = await openPage(`/dashboard?account=14&week=2026-07-20&type=${eventType}`, (api) => api.register(singleEventReport));

      expect(textOutsideTables(root)).toContain(expectedSummaryLine);
    });

    it.each<EventType>(['call_received', 'lead_created', 'appointment_set'])('shows the per-type footnote line for type %s', async (eventType) => {
      const { root } = await openPage(`/dashboard?account=14&week=2026-07-20&type=${eventType}`);

      expect(pageText(root)).toContain(PER_TYPE_LINE);
    });
  });

  describe('location table rendering', () => {
    it('renders rows in exactly the payload order and prints low/high as given', async () => {
      const unsortedReport = buildReport({
        account: BEACON_HOME_SECURITY,
        weekStart: '2026-07-20',
        summary: withRange(28, 27, 18, 38, 'normal', 0.1),
        locations: [
          locationRow('Site C', withRange(7, 7.5, 7, 8, 'normal', 0.1)),
          locationRow('Site A', withRange(2, 7.5, 7, 8, 'below', -2.5)),
          locationRow('Site D', withRange(7, 7.5, 7, 8, 'normal', -1.0)),
          locationRow('Site B', withRange(12, 7.5, 7, 8, 'above', 3.0)),
        ],
      });
      const { root } = await openPage('/dashboard?account=14&week=2026-07-20&type=all', (api) => api.register(unsortedReport));

      expect(rowNames(root)).toEqual(['Site C', 'Site A', 'Site D', 'Site B']);
      locationRows(root).forEach((row) => expect(collapsedText(row)).toContain('Usually 7–8 a week'));
    });

    it('labels each status with its symbol and text', async () => {
      const everyStatusReport = buildReport({
        account: BEACON_HOME_SECURITY,
        weekStart: '2026-07-20',
        summary: withRange(26, 27, 18, 38, 'normal', 0),
        locations: [
          locationRow('Site A', withRange(20, 6, 2, 12, 'above', 3.1)),
          locationRow('Site B', withRange(0, 7, 3, 13, 'below', -3.0)),
          locationRow('Site C', withRange(6, 6, 2, 12, 'normal', 0)),
          locationRow('Site D', withoutEnoughHistory(0, 2)),
        ],
      });
      const { root } = await openPage('/dashboard?account=14&week=2026-07-20&type=all', (api) => api.register(everyStatusReport));

      expect(collapsedText(rowFor(root, 'Site A'))).toContain(ABOVE);
      expect(collapsedText(rowFor(root, 'Site B'))).toContain(BELOW);
      expect(collapsedText(rowFor(root, 'Site C'))).toContain(WITHIN);
      expect(collapsedText(rowFor(root, 'Site D'))).toContain('Not enough history yet (2 of 4 weeks needed)');
      expect(collapsedText(rowFor(root, 'Site D'))).not.toContain('Usually');
    });

    it('takes the weeks-needed figure from minimumEligibleWeeks in the response', async () => {
      const customMinimumReport = { ...beaconDefaultWeekReport(), minimumEligibleWeeks: 6, summary: withoutEnoughHistory(26, 3), locations: [locationRow('Site A', withoutEnoughHistory(9, 3))] };
      const { root } = await openPage('/dashboard?account=14&week=2026-07-20&type=all', (api) => api.register(customMinimumReport));

      expect(pageText(root)).toContain('Not enough history yet (3 of 6 weeks needed)');
      expect(pageText(root)).not.toContain('of 4 weeks needed');
    });
  });

  describe('PLAN §13 "Phase 1 red-suite decisions" (SPEC)', () => {
    it('with no week in the URL and a failed first load, keeps account=14&type=all without a week and shows the load error with "Try again"', async () => {
      const { root } = await openPage('/dashboard?account=14&type=all', (api) => api.failNext(serverError()));

      expect(currentQueryParams()).toEqual({ account: '14', type: 'all' });
      expect(pageText(root)).toContain(LOAD_ERROR_MESSAGE);
      expect(getButton(root, 'Try again')).toBeTruthy();
    });

    it('with no week in the URL, "Try again" loads the data and fills in week=2026-07-20 with replaceUrl', async () => {
      const { root, navigations, harness } = await openPage('/dashboard?account=14&type=all', (api) => api.failNext(serverError()));

      getButton(root, 'Try again').click();
      await settle(harness);

      expect(currentQueryParams()).toEqual({ account: '14', week: '2026-07-20', type: 'all' });
      expect(navigations.at(-1)?.replaceUrl).toBe(true);
      expect(pageText(root)).not.toContain(LOAD_ERROR_MESSAGE);
      expect(textOutsideTables(root)).toContain('26 inbound events · usually 18–38 a week');
    });
  });

  describe('page heading (UI-43, C-23)', () => {
    it('default view: exactly one <h1>, reading "Activity health"', async () => {
      const { root } = await openPage('/dashboard');

      expect(pageHeadingTexts(root)).toEqual(['Activity health']);
    });

    it('default view: the summary heading "Beacon Home Security — all locations" is not an <h1>', async () => {
      const { root } = await openPage('/dashboard');

      const summaryHeadings = elementsWithExactText(root, 'Beacon Home Security — all locations');
      expect(summaryHeadings.length).toBeGreaterThan(0);
      summaryHeadings.forEach((summaryHeading) => expect(summaryHeading.tagName).not.toBe('H1'));
    });

    it.each([
      '/dashboard?account=6&week=2026-06-01&type=all',
      '/dashboard?account=6&week=2026-06-08&type=all',
      '/dashboard?account=6&week=2026-07-20&type=call_received',
      '/dashboard?account=8',
      '/dashboard?account=8&week=2026-03-02&type=all',
      '/dashboard?account=14&week=2026-02-02&type=all',
      '/dashboard?account=14&week=2026-03-02&type=all',
      '/dashboard?account=14&week=2026-01-26&type=all',
      '/dashboard?account=999',
      '/dashboard?account=14&week=2025-12-29&type=all',
    ])('%s: exactly one <h1>, reading "Activity health"', async (url) => {
      const { root } = await openPage(url);

      expect(pageHeadingTexts(root)).toEqual(['Activity health']);
    });

    it('empty account 20: exactly one <h1>, reading "Activity health"', async () => {
      const { root } = await openPage('/dashboard?account=20');

      expect(pageText(root)).toContain(EMPTY_ACCOUNT_MESSAGE);
      expect(pageHeadingTexts(root)).toEqual(['Activity health']);
    });

    it('while loading: exactly one <h1>, reading "Activity health"', async () => {
      const { root } = await openPage('/dashboard?account=14&week=2026-07-20&type=all', (api) => api.holdNext());

      expect(pageText(root)).toContain('Loading…');
      expect(pageHeadingTexts(root)).toEqual(['Activity health']);
    });

    it.each(FIRST_LOAD_FAILURES)('after a $label load error: exactly one <h1>, reading "Activity health"', async ({ failure }) => {
      const { root } = await openPage('/dashboard?account=14&week=2026-07-20&type=all', (api) => api.failNext(failure()));

      expect(pageText(root)).toContain(LOAD_ERROR_MESSAGE);
      expect(pageHeadingTexts(root)).toEqual(['Activity health']);
    });
  });

  describe('first load fails before any report (UI-44)', () => {
    it.each(FIRST_LOAD_FAILURES)('$label: shows the load error with "Viewing as" and "Activity type" rendered and enabled', async ({ failure }) => {
      const { root } = await openPage('/dashboard?account=14&week=2026-07-20&type=all', (api) => api.failNext(failure()));

      expect(pageText(root)).toContain(LOAD_ERROR_MESSAGE);
      expect(getSelect(root, 'Viewing as').disabled).toBe(false);
      expect(selectedOptionText(getSelect(root, 'Viewing as'))).toBe('Beacon Home Security');
      expect(getSelect(root, 'Activity type').disabled).toBe(false);
      expect(selectedOptionText(getSelect(root, 'Activity type'))).toBe('All activity');
    });

    it.each(FIRST_LOAD_FAILURES)('$label: shows no week stepper buttons', async ({ failure }) => {
      const { root } = await openPage('/dashboard?account=14&week=2026-07-20&type=all', (api) => api.failNext(failure()));

      expect(pageText(root)).toContain(LOAD_ERROR_MESSAGE);
      expect(findButton(root, PREVIOUS_WEEK)).toBeNull();
      expect(findButton(root, NEXT_WEEK)).toBeNull();
    });

    it.each(FIRST_LOAD_FAILURES)(
      '$label: choosing Metro Collision Centers writes account=6 as a new history entry, requests it and shows its report',
      async ({ failure }) => {
        const { root, activityHealthApi, navigations, harness } = await openPage('/dashboard?account=14&week=2026-07-20&type=all', (api) =>
          api.failNext(failure()),
        );

        chooseOption(getSelect(root, 'Viewing as'), 'Metro Collision Centers');
        await settle(harness);

        expect(currentQueryParams()).toEqual({ account: '6', week: '2026-07-20', type: 'all' });
        const accountChoice = navigations.find((navigation) => queryParamsOf(navigation.url)['account'] === '6');
        expect(accountChoice?.replaceUrl).toBe(false);
        expect(activityHealthApi.requests).toEqual<ActivityHealthRequest[]>([
          { accountId: 14, week: '2026-07-20', eventType: 'all' },
          { accountId: 6, week: '2026-07-20', eventType: 'all' },
        ]);
        expect(pageText(root)).not.toContain(LOAD_ERROR_MESSAGE);
        expect(textOutsideTables(root)).toContain('87 inbound events · usually 30–134 a week');
      },
    );

    it.each(FIRST_LOAD_FAILURES)(
      '$label: choosing Calls writes type=call_received as a new history entry, requests it and shows its report',
      async ({ failure }) => {
        const { root, activityHealthApi, navigations, harness } = await openPage('/dashboard?account=14&week=2026-07-20&type=all', (api) =>
          api.failNext(failure()),
        );

        chooseOption(getSelect(root, 'Activity type'), 'Calls');
        await settle(harness);

        expect(currentQueryParams()).toEqual({ account: '14', week: '2026-07-20', type: 'call_received' });
        const typeChoice = navigations.find((navigation) => queryParamsOf(navigation.url)['type'] === 'call_received');
        expect(typeChoice?.replaceUrl).toBe(false);
        expect(activityHealthApi.requests).toEqual<ActivityHealthRequest[]>([
          { accountId: 14, week: '2026-07-20', eventType: 'all' },
          { accountId: 14, week: '2026-07-20', eventType: 'call_received' },
        ]);
        expect(pageText(root)).not.toContain(LOAD_ERROR_MESSAGE);
        expect(textOutsideTables(root)).toContain('16 calls · usually 9–24 a week');
      },
    );

    it('shows the week stepper once a report loads after the failed first load', async () => {
      const { root, harness } = await openPage('/dashboard?account=14&week=2026-07-20&type=all', (api) => api.failNext(serverError()));

      chooseOption(getSelect(root, 'Viewing as'), 'Metro Collision Centers');
      await settle(harness);

      expect(isDisabled(getButton(root, PREVIOUS_WEEK))).toBe(false);
      expect(isDisabled(getButton(root, NEXT_WEEK))).toBe(true);
    });
  });

  describe('insufficient rows leave the "Usual range" cell empty (UI-45)', () => {
    it.each(['Site A', 'Site B', 'Site C', 'Site D'])('account 14, 2026-02-02: %s has an empty "Usual range" cell', async (location) => {
      const { root } = await openPage('/dashboard?account=14&week=2026-02-02&type=all');

      expect(usualRangeCellText(root, location)).toBe('');
    });

    it('account 14, 2026-02-02: the table rows show C-04 but no "Usually" and no "0–0"', async () => {
      const { root } = await openPage('/dashboard?account=14&week=2026-02-02&type=all');

      const tableRowsText = locationRows(root).map((row) => collapsedText(row)).join(' ');
      expect(tableRowsText).toContain('Not enough history yet (0 of 4 weeks needed)');
      expect(tableRowsText).not.toContain('Usually');
      expect(tableRowsText).not.toContain('0–0');
    });

    it.each(['Site A', 'Site C'])('account 14, 2026-03-02: insufficient %s has an empty "Usual range" cell', async (location) => {
      const { root } = await openPage('/dashboard?account=14&week=2026-03-02&type=all');

      expect(usualRangeCellText(root, location)).toBe('');
    });

    it.each([
      { location: 'Site A', count: '7' },
      { location: 'Site C', count: '8' },
    ])('account 14, 2026-03-02: $location still shows its count $count and "Not enough history yet (3 of 4 weeks needed)"', async ({ location, count }) => {
      const { root } = await openPage('/dashboard?account=14&week=2026-03-02&type=all');

      expect(cellTexts(rowFor(root, location))).toContain(count);
      expect(collapsedText(rowFor(root, location))).toContain('Not enough history yet (3 of 4 weeks needed)');
    });

    it('account 14, 2026-03-02: Site D still shows "Usually 2–11 a week"', async () => {
      const { root } = await openPage('/dashboard?account=14&week=2026-03-02&type=all');

      expect(usualRangeCellText(root, 'Site D')).toBe('Usually 2–11 a week');
    });
  });

  describe('request counts (Phase 2 review)', () => {
    it('/dashboard sends exactly one request: account 14, no week, type all', async () => {
      const { activityHealthApi, harness } = await openPage('/dashboard');
      await settle(harness);

      expect(activityHealthApi.requests).toEqual<ActivityHealthRequest[]>([{ accountId: 14, week: null, eventType: 'all' }]);
    });

    it('/dashboard: writing week=2026-07-20 into the URL uses replaceUrl and sends no further request', async () => {
      const { activityHealthApi, navigations, harness } = await openPage('/dashboard');
      await settle(harness);

      expect(currentQueryParams()).toEqual({ account: '14', week: '2026-07-20', type: 'all' });
      const weekRewrite = navigations.find((navigation) => queryParamsOf(navigation.url)['week'] === '2026-07-20');
      expect(weekRewrite?.replaceUrl).toBe(true);
      expect(navigations.slice(1).every((navigation) => navigation.replaceUrl)).toBe(true);
      expect(activityHealthApi.requests).toHaveLength(1);
    });

    it('a Tuesday week (2026-07-21) is rewritten on the client and sends exactly one request, without a week', async () => {
      const { activityHealthApi, harness } = await openPage('/dashboard?account=14&week=2026-07-21&type=all');
      await settle(harness);

      expect(currentQueryParams()).toEqual({ account: '14', week: '2026-07-20', type: 'all' });
      expect(activityHealthApi.requests).toEqual<ActivityHealthRequest[]>([{ accountId: 14, week: null, eventType: 'all' }]);
    });

    it('account=999 with a valid week sends the 404 attempt, then exactly one request for account 14', async () => {
      const { activityHealthApi, harness } = await openPage('/dashboard?account=999&week=2026-07-20&type=all');
      await settle(harness);

      expect(currentQueryParams()).toEqual({ account: '14', week: '2026-07-20', type: 'all' });
      expect(activityHealthApi.requests).toEqual<ActivityHealthRequest[]>([
        { accountId: 999, week: '2026-07-20', eventType: 'all' },
        { accountId: 14, week: '2026-07-20', eventType: 'all' },
      ]);
    });

    it('account=999 alone sends the 404 attempt, then exactly one request for account 14, both without a week', async () => {
      const { activityHealthApi, harness } = await openPage('/dashboard?account=999');
      await settle(harness);

      expect(currentQueryParams()).toEqual({ account: '14', week: '2026-07-20', type: 'all' });
      expect(activityHealthApi.requests).toEqual<ActivityHealthRequest[]>([
        { accountId: 999, week: null, eventType: 'all' },
        { accountId: 14, week: null, eventType: 'all' },
      ]);
    });

    it('a week before earliestWeek (2025-12-29) sends the 400 attempt, then exactly one request without a week', async () => {
      const { activityHealthApi, harness } = await openPage('/dashboard?account=14&week=2025-12-29&type=all');
      await settle(harness);

      expect(currentQueryParams()).toEqual({ account: '14', week: '2026-07-20', type: 'all' });
      expect(activityHealthApi.requests).toEqual<ActivityHealthRequest[]>([
        { accountId: 14, week: '2025-12-29', eventType: 'all' },
        { accountId: 14, week: null, eventType: 'all' },
      ]);
    });
  });
});
