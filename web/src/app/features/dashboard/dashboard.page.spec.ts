import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { routes } from '../../app.routes';
import { AccountsApi } from '../../core/api/accounts.api';
import { ActivityHealthApi, ActivityHealthRequest } from '../../core/api/activity-health.api';
import { EventType } from '../../core/models';
import { BEACON_HOME_SECURITY, buildReport, quietHarborEmptyReport, withoutEnoughHistory } from '../../../testing/activity-health-fixtures';
import {
  cellTexts,
  chooseOption,
  collapsedText,
  findButton,
  getButton,
  getSelect,
  hasTable,
  isDisabled,
  locationRows,
  selectedOptionText,
  textOutsideTables,
} from '../../../testing/dom-queries';
import { FakeAccountsApi, FakeActivityHealthApi, networkFailure, serverError } from '../../../testing/fake-apis';
import { RecordedNavigation, currentQueryParams, queryParamsOf, recordNavigations, settle } from '../../../testing/router-testing';
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
const DEFAULT_URL = '/dashboard?account=14&week=2026-07-20&type=all';
const DEFAULT_SUMMARY_LINE = '26 inbound events · usually 18–38 a week';

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

const FIRST_LOAD_FAILURES = [
  { label: '5xx', failure: serverError },
  { label: 'network', failure: networkFailure },
];

describe('DashboardPage', () => {
  describe('default view (account 14, week 2026-07-20, all)', () => {
    it('opens /dashboard on Beacon Home Security under "Viewing as" (UI-01)', async () => {
      const { root } = await openPage('/dashboard');

      expect(selectedOptionText(getSelect(root, 'Viewing as'))).toBe('Beacon Home Security');
    });

    it('shows the summary "26 inbound events · usually 18–38 a week" and "Within usual range" (UI-02)', async () => {
      const { root } = await openPage('/dashboard');

      expect(textOutsideTables(root)).toContain(DEFAULT_SUMMARY_LINE);
      expect(textOutsideTables(root)).toContain(WITHIN);
    });

    it('shows Site B first with 2, "Usually 3–12 a week", lower than usual, then C, A, D within usual range (UI-03)', async () => {
      const { root } = await openPage('/dashboard');

      expect(rowNames(root)).toEqual(['Site B', 'Site C', 'Site A', 'Site D']);
      expect(cellTexts(firstRow(root))).toContain('2');
      expect(collapsedText(firstRow(root))).toContain('Usually 3–12 a week');
      expect(collapsedText(firstRow(root))).toContain(BELOW);
      ['Site C', 'Site A', 'Site D'].forEach((location) => expect(collapsedText(rowFor(root, location))).toContain(WITHIN));
    });

    it('shows the method line, the capitalised footnote lines and "Data as of Mon Jul 27, 2026", without the per-type line (UI-06)', async () => {
      const { root } = await openPage('/dashboard');

      [ACCOUNT_METHOD_LINE, ...FOOTNOTE_LINES, DATA_AS_OF_LINE].forEach((line) => expect(pageText(root)).toContain(line));
      expect(pageText(root)).not.toContain(PER_TYPE_LINE);
    });
  });

  describe('scenarios', () => {
    it('spike week: summary and all 15 rows higher than usual, Site C first with 67 and "Usually 1–7 a week" (UI-10)', async () => {
      const { root } = await openPage('/dashboard?account=6&week=2026-06-01&type=all');

      expect(textOutsideTables(root)).toContain('880 inbound events · usually 39–101 a week');
      expect(textOutsideTables(root)).toContain(ABOVE);
      const rows = locationRows(root);
      expect(rows).toHaveLength(15);
      rows.forEach((row) => expect(collapsedText(row)).toContain(ABOVE));
      expect(cellTexts(firstRow(root))).toContain('Site C');
      expect(cellTexts(firstRow(root))).toContain('67');
      expect(collapsedText(firstRow(root))).toContain('Usually 1–7 a week');
    });

    it('week after the spike: summary within usual range (102, usually 37–104) with Site C then Site J higher than usual at the top (UI-11)', async () => {
      const { root } = await openPage('/dashboard?account=6&week=2026-06-08&type=all');

      expect(textOutsideTables(root)).toContain('102 inbound events · usually 37–104 a week');
      expect(textOutsideTables(root)).toContain(WITHIN);
      expect(textOutsideTables(root)).not.toContain(ABOVE);
      expect(rowNames(root).slice(0, 2)).toEqual(['Site C', 'Site J']);
      expect(collapsedText(locationRows(root)[0])).toContain(ABOVE);
      expect(collapsedText(locationRows(root)[1])).toContain(ABOVE);
    });

    it('spike in the baseline: summary "87 inbound events · usually 30–134 a week" and all 15 rows within usual range (UI-12)', async () => {
      const { root } = await openPage('/dashboard?account=6&week=2026-07-20&type=all');

      expect(textOutsideTables(root)).toContain('87 inbound events · usually 30–134 a week');
      expect(textOutsideTables(root)).toContain(WITHIN);
      const rows = locationRows(root);
      expect(rows).toHaveLength(15);
      rows.forEach((row) => expect(collapsedText(row)).toContain(WITHIN));
    });

    it('choosing Calls writes type=call_received as a new history entry and shows "51 calls · usually 17–79 a week" plus the per-type footnote line (UI-13)', async () => {
      const { root, navigations, harness } = await openPage('/dashboard?account=6&week=2026-07-20&type=all');

      chooseOption(getSelect(root, 'Activity type'), 'Calls');
      await settle(harness);

      expect(currentQueryParams()).toEqual({ account: '6', week: '2026-07-20', type: 'call_received' });
      expect(navigations.at(-1)?.replaceUrl).toBe(false);
      expect(textOutsideTables(root)).toContain('51 calls · usually 17–79 a week');
      expect(pageText(root)).not.toContain('inbound event');
      expect(pageText(root)).toContain(PER_TYPE_LINE);
    });

    it('single-site account 8: one row, Site A, 7, "Usually 5–17 a week", within usual range, and the same figures in the summary (UI-14)', async () => {
      const { root } = await openPage('/dashboard?account=8');

      const rows = locationRows(root);
      expect(rows).toHaveLength(1);
      expect(cellTexts(rows[0])).toContain('Site A');
      expect(cellTexts(rows[0])).toContain('7');
      expect(collapsedText(rows[0])).toContain('Usually 5–17 a week');
      expect(collapsedText(rows[0])).toContain(WITHIN);
      expect(textOutsideTables(root)).toContain('7 inbound events · usually 5–17 a week');
    });

    it('insufficient history: summary reads "8 inbound events" with no range and "Not enough history yet (3 of 4 weeks needed)" (UI-15)', async () => {
      const { root } = await openPage('/dashboard?account=8&week=2026-03-02&type=all');

      const summaryText = textOutsideTables(root);
      expect(summaryText).toContain('8 inbound events');
      expect(summaryText).not.toContain('8 inbound events ·');
      expect(summaryText).not.toMatch(/usually \d+–\d+ a week/);
      expect(summaryText).toContain('Not enough history yet (3 of 4 weeks needed)');
    });

    it('no eligible weeks: every row shows its count and "Not enough history yet (0 of 4 weeks needed)" with no range (UI-16)', async () => {
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

    it('mixed history: Site D higher than usual first, Sites A and C last with "Not enough history yet (3 of 4 weeks needed)" (UI-17)', async () => {
      const { root } = await openPage('/dashboard?account=14&week=2026-03-02&type=all');

      expect(rowNames(root)).toEqual(['Site D', 'Site B', 'Site A', 'Site C']);
      expect(collapsedText(rowFor(root, 'Site D'))).toContain(ABOVE);
      ['Site A', 'Site C'].forEach((location) =>
        expect(collapsedText(rowFor(root, location))).toContain('Not enough history yet (3 of 4 weeks needed)'),
      );
    });

    it('zero-activity location: first row Site G, 0, "Usually 2–9 a week", "▼ Lower than usual" (UI-18)', async () => {
      const { root } = await openPage('/dashboard?account=6&week=2026-06-29&type=all');

      const topRow = firstRow(root);
      expect(cellTexts(topRow)).toContain('Site G');
      expect(cellTexts(topRow)).toContain('0');
      expect(collapsedText(topRow)).toContain('Usually 2–9 a week');
      expect(collapsedText(topRow)).toContain(BELOW);
    });

    it('appointments: Sites A and B show 0 within usual range, Site B "Usually 0–2 a week", and the small-location footnote (UI-19)', async () => {
      const { root } = await openPage('/dashboard?account=14&week=2026-07-20&type=appointment_set');

      ['Site A', 'Site B'].forEach((location) => {
        expect(cellTexts(rowFor(root, location))).toContain('0');
        expect(collapsedText(rowFor(root, location))).toContain(WITHIN);
      });
      expect(collapsedText(rowFor(root, 'Site B'))).toContain('Usually 0–2 a week');
      expect(pageText(root)).toContain("Locations that usually get 2 or fewer events a week can't show 'lower than usual'");
    });

    it('earliest week: "◀ Previous week" disabled and only Sites B and D listed, without the empty-account message (UI-21)', async () => {
      const { root } = await openPage('/dashboard?account=14&week=2026-01-26&type=all');

      expect(isDisabled(getButton(root, PREVIOUS_WEEK))).toBe(true);
      expect(rowNames(root)).toEqual(['Site B', 'Site D']);
      expect(pageText(root)).not.toContain(EMPTY_ACCOUNT_MESSAGE);
    });

    it.each<[EventType, string]>([
      ['all', DEFAULT_SUMMARY_LINE],
      ['call_received', '16 calls · usually 9–24 a week'],
      ['lead_created', '8 leads · usually 2–12 a week'],
      ['appointment_set', '2 appointments · usually 1–8 a week'],
    ])('account 14, 2026-07-20, type %s reads "%s" (UI-42)', async (eventType, expectedSummaryLine) => {
      const { root } = await openPage(`/dashboard?account=14&week=2026-07-20&type=${eventType}`);

      expect(textOutsideTables(root)).toContain(expectedSummaryLine);
    });

    it.each([
      { url: '/dashboard', anchor: DEFAULT_SUMMARY_LINE },
      { url: '/dashboard?account=6&week=2026-06-01&type=all', anchor: '880 inbound events · usually 39–101 a week' },
      { url: '/dashboard?account=6&week=2026-07-20&type=call_received', anchor: '51 calls · usually 17–79 a week' },
      { url: '/dashboard?account=8&week=2026-03-02&type=all', anchor: 'Not enough history yet (3 of 4 weeks needed)' },
      { url: '/dashboard?account=20', anchor: EMPTY_ACCOUNT_MESSAGE },
    ])('never shows deviation, z, σ, ±, median, typical or a standalone "Normal" at $url (UI-04)', async ({ url, anchor }) => {
      const { root } = await openPage(url);

      expect(pageText(root)).toContain(anchor);
      FORBIDDEN_ON_SCREEN.forEach((forbidden) => expect(pageText(root)).not.toMatch(forbidden));
    });
  });

  describe('empty account', () => {
    it('replaces the summary and table with the empty message, keeps the selects usable and disables both week buttons (UI-20)', async () => {
      const { root } = await openPage('/dashboard?account=20');

      expect(pageText(root)).toContain(EMPTY_ACCOUNT_MESSAGE);
      expect(pageText(root)).not.toContain('0 inbound events');
      expect(pageText(root)).not.toContain('Not enough history yet');
      expect(hasTable(root)).toBe(false);
      expect(pageText(root)).not.toContain(LOAD_ERROR_MESSAGE);
      expect(getSelect(root, 'Viewing as').disabled).toBe(false);
      expect(getSelect(root, 'Activity type').disabled).toBe(false);
      expect(isDisabled(getButton(root, PREVIOUS_WEEK))).toBe(true);
      expect(isDisabled(getButton(root, NEXT_WEEK))).toBe(true);
    });

    it('still shows the full footnote including "Data as of Mon Jul 27, 2026" (UI-20b)', async () => {
      const { root } = await openPage('/dashboard?account=20');

      [...FOOTNOTE_LINES, DATA_AS_OF_LINE].forEach((line) => expect(pageText(root)).toContain(line));
    });

    it('hides the "Data as of" line when dataAsOf is null and still shows the other footnote lines (UI-22)', async () => {
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
      const { root } = await openPage(DEFAULT_URL, (api) => api.register(emptyBeaconReport));

      expect(pageText(root)).toContain(EMPTY_ACCOUNT_MESSAGE);
      expect(hasTable(root)).toBe(false);
    });
  });

  describe('loading and errors', () => {
    it('shows "Loading…" while the report is pending and removes it when the report arrives', async () => {
      const { root, activityHealthApi, harness } = await openPage(DEFAULT_URL, (api) => api.holdNext());

      expect(pageText(root)).toContain('Loading…');

      activityHealthApi.releaseHeld();
      await settle(harness);

      expect(pageText(root)).not.toContain('Loading…');
      expect(textOutsideTables(root)).toContain(DEFAULT_SUMMARY_LINE);
    });

    it('shows the load error message and a "Try again" button when the API fails, keeping the filters in the URL (UI-40)', async () => {
      const { root } = await openPage('/dashboard?account=6&week=2026-06-01&type=call_received', (api) => api.failNext(serverError()));

      expect(pageText(root)).toContain(LOAD_ERROR_MESSAGE);
      expect(getButton(root, 'Try again')).toBeTruthy();
      expect(currentQueryParams()).toEqual({ account: '6', week: '2026-06-01', type: 'call_received' });
    });

    it('"Try again" reloads the same params and shows the data without changing the URL (UI-40)', async () => {
      const { root, activityHealthApi, harness } = await openPage(DEFAULT_URL, (api) => api.failNext(serverError()));
      const urlBeforeRetry = TestBed.inject(Router).url;

      getButton(root, 'Try again').click();
      await settle(harness);

      expect(activityHealthApi.requests).toHaveLength(2);
      expect(activityHealthApi.requests[1]).toEqual({ accountId: 14, week: '2026-07-20', eventType: 'all' });
      expect(pageText(root)).not.toContain(LOAD_ERROR_MESSAGE);
      expect(textOutsideTables(root)).toContain(DEFAULT_SUMMARY_LINE);
      expect(TestBed.inject(Router).url).toBe(urlBeforeRetry);
    });
  });

  describe('controls write the URL', () => {
    it('"◀ Previous week" from the default writes week=2026-07-13 as a new history entry, relabels the week and enables "Next week ▶" (UI-31)', async () => {
      const { root, navigations, harness } = await openPage(DEFAULT_URL);

      getButton(root, PREVIOUS_WEEK).click();
      await settle(harness);

      expect(currentQueryParams()).toEqual({ account: '14', week: '2026-07-13', type: 'all' });
      expect(navigations.at(-1)?.replaceUrl).toBe(false);
      expect(isDisabled(getButton(root, NEXT_WEEK))).toBe(false);
      expect(pageText(root)).toContain('Mon Jul 13 – Sun Jul 19, 2026');
    });

    it('switching "Viewing as" keeps week and type (14 to 6 at 2026-03-02, calls) as a new history entry (UI-39)', async () => {
      const { root, navigations, harness } = await openPage('/dashboard?account=14&week=2026-03-02&type=call_received');

      chooseOption(getSelect(root, 'Viewing as'), 'Metro Collision Centers');
      await settle(harness);

      expect(currentQueryParams()).toEqual({ account: '6', week: '2026-03-02', type: 'call_received' });
      expect(navigations.at(-1)?.replaceUrl).toBe(false);
    });

    it('switching "Viewing as" to an account that rejects the kept week falls back to the latest complete week with replaceUrl and no error (UI-39b)', async () => {
      const { root, navigations, harness } = await openPage('/dashboard?account=14&week=2026-01-26&type=call_received');

      chooseOption(getSelect(root, 'Viewing as'), 'Lakeside Physio');
      await settle(harness);

      expect(currentQueryParams()).toEqual({ account: '8', week: '2026-07-20', type: 'call_received' });
      expect(navigations.at(-1)?.replaceUrl).toBe(true);
      expect(pageText(root)).not.toContain(LOAD_ERROR_MESSAGE);
    });

    it('selections made with the controls survive reopening the written URL (UI-30)', async () => {
      const first = await openPage('/dashboard?account=14&week=2026-06-01&type=all');
      chooseOption(getSelect(first.root, 'Viewing as'), 'Metro Collision Centers');
      await settle(first.harness);
      chooseOption(getSelect(first.root, 'Activity type'), 'Calls');
      await settle(first.harness);
      const writtenUrl = TestBed.inject(Router).url;
      TestBed.resetTestingModule();

      const reopened = await openPage(writtenUrl);

      expect(queryParamsOf(writtenUrl)).toEqual({ account: '6', week: '2026-06-01', type: 'call_received' });
      expect(currentQueryParams()).toEqual({ account: '6', week: '2026-06-01', type: 'call_received' });
      expect(selectedOptionText(getSelect(reopened.root, 'Viewing as'))).toBe('Metro Collision Centers');
      expect(selectedOptionText(getSelect(reopened.root, 'Activity type'))).toBe('Calls');
      expect(reopened.activityHealthApi.requests.at(-1)).toEqual({ accountId: 6, week: '2026-06-01', eventType: 'call_received' });
    });
  });

  describe('page heading (UI-43)', () => {
    it.each([
      { pageState: 'loaded', url: DEFAULT_URL, prepareApi: undefined },
      { pageState: 'loading', url: DEFAULT_URL, prepareApi: (api: FakeActivityHealthApi) => api.holdNext() },
      { pageState: 'load error', url: DEFAULT_URL, prepareApi: (api: FakeActivityHealthApi) => api.failNext(serverError()) },
      { pageState: 'empty account', url: '/dashboard?account=20', prepareApi: undefined },
    ])('has exactly one <h1>, "Activity health", when $pageState', async ({ url, prepareApi }) => {
      const { root } = await openPage(url, prepareApi);

      expect(pageHeadingTexts(root)).toEqual(['Activity health']);
    });

    it('renders the summary heading "Beacon Home Security — all locations", and not as an <h1>', async () => {
      const { root } = await openPage(DEFAULT_URL);

      const summaryHeadings = elementsWithExactText(root, 'Beacon Home Security — all locations');
      expect(summaryHeadings.length).toBeGreaterThan(0);
      summaryHeadings.forEach((summaryHeading) => expect(summaryHeading.tagName).not.toBe('H1'));
    });
  });

  describe('first load fails before any report (UI-44)', () => {
    it.each(FIRST_LOAD_FAILURES)('$label: shows the load error with both selects usable and no week stepper', async ({ failure }) => {
      const { root } = await openPage(DEFAULT_URL, (api) => api.failNext(failure()));

      expect(pageText(root)).toContain(LOAD_ERROR_MESSAGE);
      expect(getSelect(root, 'Viewing as').disabled).toBe(false);
      expect(selectedOptionText(getSelect(root, 'Viewing as'))).toBe('Beacon Home Security');
      expect(getSelect(root, 'Activity type').disabled).toBe(false);
      expect(selectedOptionText(getSelect(root, 'Activity type'))).toBe('All activity');
      expect(findButton(root, PREVIOUS_WEEK)).toBeNull();
      expect(findButton(root, NEXT_WEEK)).toBeNull();
    });

    it.each([
      {
        control: 'Viewing as',
        option: 'Metro Collision Centers',
        failure: serverError,
        expectedParams: { account: '6', week: '2026-07-20', type: 'all' },
        expectedRequest: { accountId: 6, week: '2026-07-20', eventType: 'all' },
        expectedSummaryLine: '87 inbound events · usually 30–134 a week',
      },
      {
        control: 'Activity type',
        option: 'Calls',
        failure: networkFailure,
        expectedParams: { account: '14', week: '2026-07-20', type: 'call_received' },
        expectedRequest: { accountId: 14, week: '2026-07-20', eventType: 'call_received' },
        expectedSummaryLine: '16 calls · usually 9–24 a week',
      },
    ])(
      'choosing "$option" under "$control" writes the URL as a new history entry, requests it and shows its report',
      async ({ control, option, failure, expectedParams, expectedRequest, expectedSummaryLine }) => {
        const { root, activityHealthApi, navigations, harness } = await openPage(DEFAULT_URL, (api) => api.failNext(failure()));

        chooseOption(getSelect(root, control), option);
        await settle(harness);

        expect(currentQueryParams()).toEqual(expectedParams);
        const choice = navigations.find((navigation) => queryParamsOf(navigation.url)['account'] === expectedParams.account && queryParamsOf(navigation.url)['type'] === expectedParams.type);
        expect(choice?.replaceUrl).toBe(false);
        expect(activityHealthApi.requests).toEqual<ActivityHealthRequest[]>([
          { accountId: 14, week: '2026-07-20', eventType: 'all' },
          expectedRequest as ActivityHealthRequest,
        ]);
        expect(pageText(root)).not.toContain(LOAD_ERROR_MESSAGE);
        expect(textOutsideTables(root)).toContain(expectedSummaryLine);
        expect(findButton(root, PREVIOUS_WEEK)).not.toBeNull();
      },
    );
  });
});
