
### 🧑 USER — 2026-09-28T21:18:32.384Z

Review this change for security vulnerabilities.

Changed files (you may Read these and any other file in the repo):
  - web/src/app/features/dashboard/components/account-summary.spec.ts
  - web/src/app/features/dashboard/components/dashboard-filters.spec.ts
  - web/src/app/features/dashboard/components/location-table.spec.ts
  - web/src/app/features/dashboard/dashboard-state.spec.ts
  - web/src/app/features/dashboard/dashboard.page.spec.ts
  - web/src/testing/activity-health-fixtures.ts

Unified diff (only + lines are new):

=== DIFF: web/src/app/features/dashboard/components/account-summary.spec.ts ===
@@ -0,0 +1,127 @@
+import { TestBed } from '@angular/core/testing';
+import { ActivityHealthReport, EventType, SeriesHealth } from '../../../core/models';
+import {
+  BEACON_HOME_SECURITY,
+  beaconAppointmentsDefaultWeekReport,
+  beaconCallsDefaultWeekReport,
+  beaconDefaultWeekReport,
+  beaconLeadsDefaultWeekReport,
+  buildReport,
+  lakesideInsufficientHistoryReport,
+  metroCallsSpikeInBaselineReport,
+  metroSpikeWeekReport,
+  withRange,
+  withoutEnoughHistory,
+} from '../../../../testing/activity-health-fixtures';
+import { collapsedText } from '../../../../testing/dom-queries';
+import { AccountSummary } from './account-summary';
+
+const FORBIDDEN_ON_SCREEN: RegExp[] = [/\bz\b/, /σ/, /±/, /\bmedian\b/i, /\btypical\b/i, /\bdeviation\b/i, /\bNormal\b/];
+
+async function renderSummary(report: ActivityHealthReport): Promise<string> {
+  TestBed.configureTestingModule({ imports: [AccountSummary] });
+  const fixture = TestBed.createComponent(AccountSummary);
+  fixture.componentRef.setInput('report', report);
+  await fixture.whenStable();
+  return collapsedText(fixture.nativeElement as HTMLElement);
+}
+
+function beaconReportWith(eventType: EventType, summary: SeriesHealth): ActivityHealthReport {
+  return buildReport({ account: BEACON_HOME_SECURITY, weekStart: '2026-07-20', eventType, summary, locations: [] });
+}
+
+describe('AccountSummary', () => {
+  it('shows the heading "Beacon Home Security — all locations"', async () => {
+    const summaryText = await renderSummary(beaconDefaultWeekReport());
+
+    expect(summaryText).toContain('Beacon Home Security — all locations');
+  });
+
+  it('shows the capitalised account method line', async () => {
+    const summaryText = await renderSummary(beaconDefaultWeekReport());
+
+    expect(summaryText).toContain('Compared with the last 8 full weeks for this account');
+  });
+
+  it('shows "26 inbound events · usually 18–38 a week" and "Within usual range" for the default week', async () => {
+    const summaryText = await renderSummary(beaconDefaultWeekReport());
+
+    expect(summaryText).toContain('26 inbound events · usually 18–38 a week');
+    expect(summaryText).toContain('Within usual range');
+  });
+
+  it('shows "880 inbound events · usually 39–101 a week" and "▲ Higher than usual" for the spike week', async () => {
+    const summaryText = await renderSummary(metroSpikeWeekReport());
+
+    expect(summaryText).toContain('880 inbound events · usually 39–101 a week');
+    expect(summaryText).toContain('▲ Higher than usual');
+  });
+
+  it('shows "▼ Lower than usual" for a below summary', async () => {
+    const summaryText = await renderSummary(beaconReportWith('all', withRange(10, 27, 18, 38, 'below', -3.5)));
+
+    expect(summaryText).toContain('10 inbound events · usually 18–38 a week');
+    expect(summaryText).toContain('▼ Lower than usual');
+  });
+
+  it('prints low and high exactly as given, never recomputed', async () => {
+    const summaryText = await renderSummary(beaconReportWith('all', withRange(26, 27, 25, 26, 'normal', 0)));
+
+    expect(summaryText).toContain('26 inbound events · usually 25–26 a week');
+  });
+
+  it.each([
+    { eventType: 'all', report: beaconDefaultWeekReport, expectedLine: '26 inbound events · usually 18–38 a week' },
+    { eventType: 'call_received', report: beaconCallsDefaultWeekReport, expectedLine: '16 calls · usually 9–24 a week' },
+    { eventType: 'lead_created', report: beaconLeadsDefaultWeekReport, expectedLine: '8 leads · usually 2–12 a week' },
+    { eventType: 'appointment_set', report: beaconAppointmentsDefaultWeekReport, expectedLine: '2 appointments · usually 1–8 a week' },
+  ])('uses the plural noun for type $eventType: "$expectedLine"', async ({ report, expectedLine }) => {
+    const summaryText = await renderSummary(report());
+
+    expect(summaryText).toContain(expectedLine);
+  });
+
+  it('uses "calls", not "inbound events", for 51 calls at account 6', async () => {
+    const summaryText = await renderSummary(metroCallsSpikeInBaselineReport());
+
+    expect(summaryText).toContain('51 calls · usually 17–79 a week');
+    expect(summaryText).not.toContain('inbound event');
+  });
+
+  it.each<{ eventType: EventType; expectedLine: string }>([
+    { eventType: 'all', expectedLine: '1 inbound event · usually 0–4 a week' },
+    { eventType: 'call_received', expectedLine: '1 call · usually 0–4 a week' },
+    { eventType: 'lead_created', expectedLine: '1 lead · usually 0–4 a week' },
+    { eventType: 'appointment_set', expectedLine: '1 appointment · usually 0–4 a week' },
+  ])('uses the singular noun for a count of 1 with type $eventType: "$expectedLine"', async ({ eventType, expectedLine }) => {
+    const summaryText = await renderSummary(beaconReportWith(eventType, withRange(1, 1, 0, 4, 'normal', 0)));
+
+    expect(summaryText).toContain(expectedLine);
+  });
+
+  it('shows "8 inbound events" with no range and "Not enough history yet (3 of 4 weeks needed)" when history is insufficient', async () => {
+    const summaryText = await renderSummary(lakesideInsufficientHistoryReport());
+
+    expect(summaryText).toContain('8 inbound events');
+    expect(summaryText).not.toContain('8 inbound events ·');
+    expect(summaryText).not.toMatch(/usually \d+–\d+ a week/);
+    expect(summaryText).toContain('Not enough history yet (3 of 4 weeks needed)');
+  });
+
+  it('uses the type noun and minimumEligibleWeeks from the report for an insufficient summary', async () => {
+    const report = { ...beaconReportWith('call_received', withoutEnoughHistory(5, 2)), minimumEligibleWeeks: 6 };
+
+    const summaryText = await renderSummary(report);
+
+    expect(summaryText).toContain('5 calls');
+    expect(summaryText).toContain('Not enough history yet (2 of 6 weeks needed)');
+  });
+
+  it('never shows deviation, median, z, σ, ± or a standalone "Normal"', async () => {
+    const summaryText = await renderSummary(beaconDefaultWeekReport());
+
+    expect(summaryText).toContain('26 inbound events · usually 18–38 a week');
+    FORBIDDEN_ON_SCREEN.forEach((forbidden) => expect(summaryText).not.toMatch(forbidden));
+    expect(summaryText).not.toContain('-0.19');
+  });
+});


=== DIFF: web/src/app/features/dashboard/components/dashboard-filters.spec.ts ===
@@ -0,0 +1,198 @@
+import { ComponentFixture, TestBed } from '@angular/core/testing';
+import { Account, EventType } from '../../../core/models';
+import { LATEST_COMPLETE_WEEK, seedAccounts, sundayOf } from '../../../../testing/activity-health-fixtures';
+import { chooseOption, collapsedText, getButton, getSelect, isDisabled, optionTexts, selectedOptionText } from '../../../../testing/dom-queries';
+import { DashboardFilters } from './dashboard-filters';
+
+const PREVIOUS_WEEK = '◀ Previous week';
+const NEXT_WEEK = 'Next week ▶';
+
+interface FiltersInputs {
+  accounts: readonly Account[];
+  accountId: number;
+  weekStart: string;
+  earliestWeek: string;
+  latestCompleteWeek: string;
+  eventType: EventType;
+}
+
+interface FiltersUnderTest {
+  root: HTMLElement;
+  fixture: ComponentFixture<DashboardFilters>;
+  selectedAccountIds: number[];
+  selectedWeeks: string[];
+  selectedEventTypes: EventType[];
+}
+
+const BEACON_DEFAULT_INPUTS: FiltersInputs = {
+  accounts: seedAccounts(),
+  accountId: 14,
+  weekStart: '2026-07-20',
+  earliestWeek: '2026-01-26',
+  latestCompleteWeek: LATEST_COMPLETE_WEEK,
+  eventType: 'all',
+};
+
+async function renderFilters(overrides: Partial<FiltersInputs> = {}): Promise<FiltersUnderTest> {
+  const inputs = { ...BEACON_DEFAULT_INPUTS, ...overrides };
+  TestBed.configureTestingModule({ imports: [DashboardFilters] });
+  const fixture = TestBed.createComponent(DashboardFilters);
+  fixture.componentRef.setInput('accounts', inputs.accounts);
+  fixture.componentRef.setInput('accountId', inputs.accountId);
+  fixture.componentRef.setInput('week', { start: inputs.weekStart, end: sundayOf(inputs.weekStart) });
+  fixture.componentRef.setInput('earliestWeek', inputs.earliestWeek);
+  fixture.componentRef.setInput('latestCompleteWeek', inputs.latestCompleteWeek);
+  fixture.componentRef.setInput('eventType', inputs.eventType);
+  const selectedAccountIds: number[] = [];
+  const selectedWeeks: string[] = [];
+  const selectedEventTypes: EventType[] = [];
+  fixture.componentInstance.accountSelected.subscribe((accountId) => selectedAccountIds.push(accountId));
+  fixture.componentInstance.weekSelected.subscribe((week) => selectedWeeks.push(week));
+  fixture.componentInstance.eventTypeSelected.subscribe((eventType) => selectedEventTypes.push(eventType));
+  await fixture.whenStable();
+  return { root: fixture.nativeElement as HTMLElement, fixture, selectedAccountIds, selectedWeeks, selectedEventTypes };
+}
+
+describe('DashboardFilters', () => {
+  describe('Viewing as', () => {
+    it('lists every account by its plain name', async () => {
+      const { root } = await renderFilters();
+
+      expect(optionTexts(getSelect(root, 'Viewing as'))).toEqual(seedAccounts().map((account) => account.name));
+    });
+
+    it('selects the option of the accountId input', async () => {
+      const { root } = await renderFilters({ accountId: 6 });
+
+      expect(selectedOptionText(getSelect(root, 'Viewing as'))).toBe('Metro Collision Centers');
+    });
+
+    it('emits accountSelected with the numeric account id when another account is chosen', async () => {
+      const { root, selectedAccountIds } = await renderFilters();
+
+      chooseOption(getSelect(root, 'Viewing as'), 'Lakeside Physio');
+
+      expect(selectedAccountIds.at(-1)).toBe(8);
+      expect(typeof selectedAccountIds.at(-1)).toBe('number');
+    });
+  });
+
+  describe('Activity type', () => {
+    it('offers All activity, Calls, Leads, Appointments', async () => {
+      const { root } = await renderFilters();
+
+      expect(optionTexts(getSelect(root, 'Activity type'))).toEqual(['All activity', 'Calls', 'Leads', 'Appointments']);
+    });
+
+    it.each<{ eventType: EventType; optionText: string }>([
+      { eventType: 'all', optionText: 'All activity' },
+      { eventType: 'call_received', optionText: 'Calls' },
+      { eventType: 'lead_created', optionText: 'Leads' },
+      { eventType: 'appointment_set', optionText: 'Appointments' },
+    ])('selects "$optionText" for eventType $eventType', async ({ eventType, optionText }) => {
+      const { root } = await renderFilters({ eventType });
+
+      expect(selectedOptionText(getSelect(root, 'Activity type'))).toBe(optionText);
+    });
+
+    it.each<{ optionText: string; eventType: EventType }>([
+      { optionText: 'Calls', eventType: 'call_received' },
+      { optionText: 'Leads', eventType: 'lead_created' },
+      { optionText: 'Appointments', eventType: 'appointment_set' },
+    ])('emits eventTypeSelected "$eventType" when "$optionText" is chosen', async ({ optionText, eventType }) => {
+      const { root, selectedEventTypes } = await renderFilters();
+
+      chooseOption(getSelect(root, 'Activity type'), optionText);
+
+      expect(selectedEventTypes.at(-1)).toBe(eventType);
+    });
+
+    it('emits eventTypeSelected "all" when "All activity" is chosen from Calls', async () => {
+      const { root, selectedEventTypes } = await renderFilters({ eventType: 'call_received' });
+
+      chooseOption(getSelect(root, 'Activity type'), 'All activity');
+
+      expect(selectedEventTypes.at(-1)).toBe('all');
+    });
+  });
+
+  describe('week stepper', () => {
+    it('shows the week label "Mon Jul 20 – Sun Jul 26, 2026"', async () => {
+      const { root } = await renderFilters();
+
+      expect(collapsedText(root)).toContain('Mon Jul 20 – Sun Jul 26, 2026');
+    });
+
+    it('disables "Next week ▶" and enables "◀ Previous week" at the latest complete week', async () => {
+      const { root } = await renderFilters();
+
+      expect(isDisabled(getButton(root, NEXT_WEEK))).toBe(true);
+      expect(isDisabled(getButton(root, PREVIOUS_WEEK))).toBe(false);
+    });
+
+    it('disables "◀ Previous week" and enables "Next week ▶" at earliestWeek', async () => {
+      const { root } = await renderFilters({ weekStart: '2026-01-26' });
+
+      expect(isDisabled(getButton(root, PREVIOUS_WEEK))).toBe(true);
+      expect(isDisabled(getButton(root, NEXT_WEEK))).toBe(false);
+    });
+
+    it('enables both buttons between the bounds', async () => {
+      const { root } = await renderFilters({ weekStart: '2026-03-02' });
+
+      expect(isDisabled(getButton(root, PREVIOUS_WEEK))).toBe(false);
+      expect(isDisabled(getButton(root, NEXT_WEEK))).toBe(false);
+    });
+
+    it('disables both buttons for account 20, where earliestWeek equals latestCompleteWeek', async () => {
+      const { root } = await renderFilters({ accountId: 20, weekStart: '2026-07-20', earliestWeek: '2026-07-20' });
+
+      expect(isDisabled(getButton(root, PREVIOUS_WEEK))).toBe(true);
+      expect(isDisabled(getButton(root, NEXT_WEEK))).toBe(true);
+    });
+
+    it('keeps both selects enabled for account 20', async () => {
+      const { root } = await renderFilters({ accountId: 20, weekStart: '2026-07-20', earliestWeek: '2026-07-20' });
+
+      expect(getSelect(root, 'Viewing as').disabled).toBe(false);
+      expect(getSelect(root, 'Activity type').disabled).toBe(false);
+    });
+
+    it('emits weekSelected with the previous Monday when "◀ Previous week" is clicked', async () => {
+      const { root, selectedWeeks } = await renderFilters();
+
+      getButton(root, PREVIOUS_WEEK).click();
+
+      expect(selectedWeeks).toEqual(['2026-07-13']);
+    });
+
+    it('emits weekSelected with the next Monday when "Next week ▶" is clicked', async () => {
+      const { root, selectedWeeks } = await renderFilters({ weekStart: '2026-07-13' });
+
+      getButton(root, NEXT_WEEK).click();
+
+      expect(selectedWeeks).toEqual(['2026-07-20']);
+    });
+
+    it('crosses a month boundary: previous week of 2026-03-02 is 2026-02-23', async () => {
+      const { root, selectedWeeks } = await renderFilters({ weekStart: '2026-03-02' });
+
+      getButton(root, PREVIOUS_WEEK).click();
+
+      expect(selectedWeeks).toEqual(['2026-02-23']);
+    });
+
+    it.each([
+      { transition: 'US DST end 2026-11-01', weekStart: '2026-10-26', button: NEXT_WEEK, expectedWeek: '2026-11-02' },
+      { transition: 'EU DST end 2026-10-25', weekStart: '2026-10-19', button: NEXT_WEEK, expectedWeek: '2026-10-26' },
+      { transition: 'US DST start 2026-03-08', weekStart: '2026-03-09', button: PREVIOUS_WEEK, expectedWeek: '2026-03-02' },
+      { transition: 'EU DST start 2026-03-29', weekStart: '2026-03-30', button: PREVIOUS_WEEK, expectedWeek: '2026-03-23' },
+    ])('steps across the $transition week from $weekStart to $expectedWeek', async ({ weekStart, button, expectedWeek }) => {
+      const { root, selectedWeeks } = await renderFilters({ weekStart, latestCompleteWeek: '2026-11-09' });
+
+      getButton(root, button).click();
+
+      expect(selectedWeeks).toEqual([expectedWeek]);
+    });
+  });
+});


=== DIFF: web/src/app/features/dashboard/components/location-table.spec.ts ===
@@ -0,0 +1,106 @@
+import { ComponentFixture, TestBed } from '@angular/core/testing';
+import { LocationHealth } from '../../../core/models';
+import { locationRow, withRange, withoutEnoughHistory } from '../../../../testing/activity-health-fixtures';
+import { cellTexts, collapsedText, columnHeaderTexts, locationRows } from '../../../../testing/dom-queries';
+import { LocationTable } from './location-table';
+
+const FORBIDDEN_ON_SCREEN: RegExp[] = [/\bz\b/, /σ/, /±/, /\bmedian\b/i, /\btypical\b/i, /\bdeviation\b/i, /\bNormal\b/];
+
+async function renderTable(locations: LocationHealth[], minimumEligibleWeeks = 4): Promise<HTMLElement> {
+  TestBed.configureTestingModule({ imports: [LocationTable] });
+  const fixture: ComponentFixture<LocationTable> = TestBed.createComponent(LocationTable);
+  fixture.componentRef.setInput('locations', locations);
+  fixture.componentRef.setInput('minimumEligibleWeeks', minimumEligibleWeeks);
+  await fixture.whenStable();
+  return fixture.nativeElement as HTMLElement;
+}
+
+function rowFor(root: HTMLElement, location: string): HTMLElement {
+  const row = locationRows(root).find((candidate) => cellTexts(candidate).includes(location));
+  if (!row) {
+    throw new Error(`No row for ${location} in: ${collapsedText(root)}`);
+  }
+  return row;
+}
+
+function unsortedPayloadWithUnreproducibleRanges(): LocationHealth[] {
+  return [
+    locationRow('Site C', withRange(7, 7.5, 7, 8, 'normal', 0.1)),
+    locationRow('Site A', withRange(2, 7.5, 7, 8, 'below', -2.5)),
+    locationRow('Site D', withRange(7, 7.5, 7, 8, 'normal', -1.0)),
+    locationRow('Site B', withRange(12, 7.5, 7, 8, 'above', 3.0)),
+  ];
+}
+
+describe('LocationTable', () => {
+  it('shows the column headers Location, Events, Usual range, Status', async () => {
+    const root = await renderTable(unsortedPayloadWithUnreproducibleRanges());
+
+    expect(columnHeaderTexts(root)).toEqual(['Location', 'Events', 'Usual range', 'Status']);
+  });
+
+  it('renders the rows in exactly the payload order C, A, D, B without re-sorting', async () => {
+    const root = await renderTable(unsortedPayloadWithUnreproducibleRanges());
+
+    const renderedLocations = locationRows(root).map((row) => cellTexts(row).find((cell) => /^Site [A-Z]$/.test(cell)));
+    expect(renderedLocations).toEqual(['Site C', 'Site A', 'Site D', 'Site B']);
+  });
+
+  it('prints low and high exactly as given ("Usually 7–8 a week") without recomputing them', async () => {
+    const root = await renderTable(unsortedPayloadWithUnreproducibleRanges());
+
+    const rows = locationRows(root);
+    expect(rows).toHaveLength(4);
+    rows.forEach((row) => expect(collapsedText(row)).toContain('Usually 7–8 a week'));
+  });
+
+  it('shows each count, including 0', async () => {
+    const root = await renderTable([
+      locationRow('Site G', withRange(0, 4.5, 2, 9, 'below', -3.19)),
+      locationRow('Site N', withRange(2, 4.5, 2, 9, 'normal', -1.33)),
+    ]);
+
+    expect(cellTexts(rowFor(root, 'Site G'))).toContain('0');
+    expect(cellTexts(rowFor(root, 'Site N'))).toContain('2');
+  });
+
+  it.each([
+    { status: 'above', row: locationRow('Site B', withRange(12, 7.5, 7, 8, 'above', 3.0)), label: '▲ Higher than usual' },
+    { status: 'below', row: locationRow('Site B', withRange(2, 6.5, 3, 12, 'below', -2.16)), label: '▼ Lower than usual' },
+    { status: 'normal', row: locationRow('Site B', withRange(9, 6, 2, 12, 'normal', 0.98)), label: 'Within usual range' },
+  ])('labels status $status as "$label" (symbol and text together)', async ({ row, label }) => {
+    const root = await renderTable([row]);
+
+    expect(collapsedText(rowFor(root, 'Site B'))).toContain(label);
+  });
+
+  it('shows "Not enough history yet (3 of 4 weeks needed)" with the count and no range for an insufficient row', async () => {
+    const root = await renderTable([locationRow('Site A', withoutEnoughHistory(7, 3))]);
+
+    const row = rowFor(root, 'Site A');
+    expect(cellTexts(row)).toContain('7');
+    expect(collapsedText(row)).toContain('Not enough history yet (3 of 4 weeks needed)');
+    expect(collapsedText(row)).not.toContain('Usually');
+  });
+
+  it('keeps the 0 case: "Not enough history yet (0 of 4 weeks needed)"', async () => {
+    const root = await renderTable([locationRow('Site B', withoutEnoughHistory(1, 0))]);
+
+    expect(collapsedText(rowFor(root, 'Site B'))).toContain('Not enough history yet (0 of 4 weeks needed)');
+  });
+
+  it('takes N from baseline.weeksUsed and the weeks needed from the minimumEligibleWeeks input', async () => {
+    const root = await renderTable([locationRow('Site A', withoutEnoughHistory(9, 3))], 6);
+
+    expect(collapsedText(rowFor(root, 'Site A'))).toContain('Not enough history yet (3 of 6 weeks needed)');
+  });
+
+  it('never shows deviation, median, z, σ, ± or a standalone "Normal"', async () => {
+    const root = await renderTable(unsortedPayloadWithUnreproducibleRanges());
+
+    expect(collapsedText(root)).toContain('Site C');
+    FORBIDDEN_ON_SCREEN.forEach((forbidden) => expect(collapsedText(root)).not.toMatch(forbidden));
+    expect(collapsedText(root)).not.toContain('7.5');
+    expect(collapsedText(root)).not.toContain('2.5');
+  });
+});


=== DIFF: web/src/app/features/dashboard/dashboard-state.spec.ts ===
@@ -151,7 +151,11 @@ describe('DashboardState', () => {
       expectNormalisedWithReplaceUrl(navigations);
     });
 
-    it.each(['abc', '', '6.5'])('rewrites a non-numeric account "%s" to account 14', async (invalidAccount) => {
+    it.each([
+      { label: 'abc', invalidAccount: 'abc' },
+      { label: '(empty value)', invalidAccount: '' },
+      { label: '6.5', invalidAccount: '6.5' },
+    ])('rewrites a non-integer account $label to account 14', async ({ invalidAccount }) => {
       const { navigations } = await openState(`/dashboard?account=${invalidAccount}&week=2026-07-20&type=all`);
 
       expect(currentQueryParams()).toEqual(DEFAULT_QUERY_PARAMS);
@@ -190,7 +194,13 @@ describe('DashboardState', () => {
       expectNormalisedWithReplaceUrl(navigations);
     });
 
-    it.each(['abc', '2026-13-01', '20260720', '2026-7-20', ''])('rewrites a malformed week "%s" to the latest complete week', async (malformedWeek) => {
+    it.each([
+      { label: 'abc', malformedWeek: 'abc' },
+      { label: '2026-13-01', malformedWeek: '2026-13-01' },
+      { label: '20260720', malformedWeek: '20260720' },
+      { label: '2026-7-20', malformedWeek: '2026-7-20' },
+      { label: '(empty value)', malformedWeek: '' },
+    ])('rewrites a malformed week $label to the latest complete week', async ({ malformedWeek }) => {
       const { navigations } = await openState(`/dashboard?account=14&week=${malformedWeek}&type=all`);
 
       expect(currentQueryParams()).toEqual(DEFAULT_QUERY_PARAMS);
@@ -206,7 +216,12 @@ describe('DashboardState', () => {
       expectNormalisedWithReplaceUrl(navigations);
     });
 
-    it.each(['ALL', 'foo', 'Call_Received', ''])('rewrites an invalid type "%s" to all', async (invalidType) => {
+    it.each([
+      { label: 'ALL', invalidType: 'ALL' },
+      { label: 'foo', invalidType: 'foo' },
+      { label: 'Call_Received', invalidType: 'Call_Received' },
+      { label: '(empty value)', invalidType: '' },
+    ])('rewrites an invalid type $label to all', async ({ invalidType }) => {
       const { state, navigations } = await openState(`/dashboard?account=14&week=2026-07-20&type=${invalidType}`);
 
       expect(currentQueryParams()).toEqual(DEFAULT_QUERY_PARAMS);
@@ -214,13 +229,6 @@ describe('DashboardState', () => {
       expectNormalisedWithReplaceUrl(navigations);
     });
 
-    it('never sends an invalid type to the API', async () => {
-      const { activityHealthApi } = await openState('/dashboard?account=14&week=2026-07-20&type=ALL');
-
-      expect(activityHealthApi.requests.length).toBeGreaterThan(0);
-      expect(activityHealthApi.requests.every((request) => (EVENT_TYPES as readonly string[]).includes(request.eventType))).toBe(true);
-    });
-
     it('rewrites only the invalid param and keeps the valid account and week', async () => {
       await openState('/dashboard?account=6&week=2026-06-01&type=foo');
 
@@ -246,6 +254,36 @@ describe('DashboardState', () => {
       expect(activityHealthApi.requests.at(-1)).toEqual({ accountId: 14, week: '2026-07-13', eventType: 'call_received' });
     });
 
+    it('selectWeek adds a history entry (no replaceUrl) so Back returns to the previous week', async () => {
+      const { state, navigations, harness } = await openState(DEFAULT_URL);
+
+      state.selectWeek('2026-07-13');
+      await settle(harness);
+
+      expect(navigationsAfterOpening(navigations)).toHaveLength(1);
+      expect(navigations.at(-1)?.replaceUrl).toBe(false);
+    });
+
+    it('selectEventType adds a history entry (no replaceUrl)', async () => {
+      const { state, navigations, harness } = await openState(DEFAULT_URL);
+
+      state.selectEventType('call_received');
+      await settle(harness);
+
+      expect(navigationsAfterOpening(navigations)).toHaveLength(1);
+      expect(navigations.at(-1)?.replaceUrl).toBe(false);
+    });
+
+    it('selectAccount adds a history entry (no replaceUrl) when the kept week is valid', async () => {
+      const { state, navigations, harness } = await openState(DEFAULT_URL);
+
+      state.selectAccount(6);
+      await settle(harness);
+
+      expect(navigationsAfterOpening(navigations)).toHaveLength(1);
+      expect(navigations.at(-1)?.replaceUrl).toBe(false);
+    });
+
     it('selectEventType writes the type and keeps account and week', async () => {
       const { state, harness } = await openState(DEFAULT_URL);
 
@@ -276,6 +314,7 @@ describe('DashboardState', () => {
 
       expect(activityHealthApi.requests).toContainEqual({ accountId: 8, week: '2026-01-26', eventType: 'call_received' });
       expect(currentQueryParams()).toEqual({ account: '8', week: '2026-07-20', type: 'call_received' });
+      expect(navigationsAfterOpening(navigations)[0]?.replaceUrl).toBe(false);
       expect(navigations.at(-1)?.replaceUrl).toBe(true);
       expect(state.error()).toBeNull();
       expect(state.report()?.account.id).toBe(8);
@@ -390,4 +429,38 @@ describe('DashboardState', () => {
       expect(state.report()).toEqual(lakesideDefaultWeekReport());
     });
   });
+
+  describe('PLAN §13 "Phase 1 red-suite decisions" (SPEC)', () => {
+    it('checks the type against EVENT_TYPES on the client and never sends an invalid type to the API', async () => {
+      const { activityHealthApi } = await openState('/dashboard?account=14&week=2026-07-20&type=ALL');
+
+      expect(currentQueryParams()).toEqual(DEFAULT_QUERY_PARAMS);
+      expect(activityHealthApi.requests.length).toBeGreaterThan(0);
+      expect(activityHealthApi.requests.every((request) => (EVENT_TYPES as readonly string[]).includes(request.eventType))).toBe(true);
+    });
+
+    it.each([
+      { label: '5xx', failure: serverError },
+      { label: 'network failure', failure: networkFailure },
+    ])('with no week in the URL and a first-load $label, keeps account=14&type=all without a week param and exposes the error', async ({ failure }) => {
+      const { state } = await openState('/dashboard?account=14&type=all', (api) => api.failNext(failure()));
+
+      expect(currentQueryParams()).toEqual({ account: '14', type: 'all' });
+      expect(state.error()).not.toBeNull();
+      expect(state.isLoading()).toBe(false);
+    });
+
+    it('with no week in the URL, fills in the latest complete week with replaceUrl after a successful reload', async () => {
+      const { state, activityHealthApi, navigations, harness } = await openState('/dashboard?account=14&type=all', (api) => api.failNext(serverError()));
+
+      state.reload();
+      await settle(harness);
+
+      expect(activityHealthApi.requests[1]).toEqual({ accountId: 14, week: null, eventType: 'all' });
+      expect(currentQueryParams()).toEqual(DEFAULT_QUERY_PARAMS);
+      expect(navigations.at(-1)?.replaceUrl).toBe(true);
+      expect(state.error()).toBeNull();
+      expect(state.report()).toEqual(beaconDefaultWeekReport());
+    });
+  });
 });


=== DIFF: web/src/app/features/dashboard/dashboard.page.spec.ts ===
@@ -111,15 +111,14 @@ describe('DashboardPage', () => {
     it('shows Beacon Home Security selected under "Viewing as"', async () => {
       const { root } = await openPage('/dashboard');
 
-      expect(selectedOptionText(getSelect(root, 'Viewing as'))).toContain('Beacon Home Security');
+      expect(selectedOptionText(getSelect(root, 'Viewing as'))).toBe('Beacon Home Security');
     });
 
     it('lists every account in the Viewing-as select, including the empty account Quiet Harbor Spa', async () => {
       const { root } = await openPage('/dashboard');
 
-      const accountOptions = optionTexts(getSelect(root, 'Viewing as')).join(' | ');
-      ['Metro Collision Centers', 'Lakeside Physio', 'Redline Tire & Service', 'Beacon Home Security', 'Quiet Harbor Spa'].forEach((accountName) =>
-        expect(accountOptions).toContain(accountName),
+      expect([...optionTexts(getSelect(root, 'Viewing as'))].sort()).toEqual(
+        ['Beacon Home Security', 'Lakeside Physio', 'Metro Collision Centers', 'Quiet Harbor Spa', 'Redline Tire & Service'],
       );
     });
 
@@ -237,13 +236,14 @@ describe('DashboardPage', () => {
       rows.forEach((row) => expect(collapsedText(row)).toContain(WITHIN));
     });
 
-    it('choosing Calls writes type=call_received and shows "51 calls · usually 17–79 a week" plus the per-type footnote line', async () => {
-      const { root, harness } = await openPage('/dashboard?account=6&week=2026-07-20&type=all');
+    it('choosing Calls writes type=call_received as a new history entry and shows "51 calls · usually 17–79 a week" plus the per-type footnote line', async () => {
+      const { root, navigations, harness } = await openPage('/dashboard?account=6&week=2026-07-20&type=all');
 
       chooseOption(getSelect(root, 'Activity type'), 'Calls');
       await settle(harness);
 
       expect(currentQueryParams()).toEqual({ account: '6', week: '2026-07-20', type: 'call_received' });
+      expect(navigations.at(-1)?.replaceUrl).toBe(false);
       expect(textOutsideTables(root)).toContain('51 calls · usually 17–79 a week');
       expect(pageText(root)).not.toContain('inbound event');
       expect(pageText(root)).toContain(PER_TYPE_LINE);
@@ -326,17 +326,17 @@ describe('DashboardPage', () => {
     });
 
     it.each([
-      '/dashboard?account=6&week=2026-06-01&type=all',
-      '/dashboard?account=6&week=2026-06-08&type=all',
-      '/dashboard?account=6&week=2026-07-20&type=call_received',
-      '/dashboard?account=8&week=2026-03-02&type=all',
-      '/dashboard?account=14&week=2026-03-02&type=all',
-      '/dashboard?account=14&week=2026-07-20&type=appointment_set',
-      '/dashboard?account=20',
-    ])('never shows deviation, z, σ, ±, median, typical or a standalone "Normal" at %s', async (url) => {
+      { url: '/dashboard?account=6&week=2026-06-01&type=all', anchor: '880 inbound events · usually 39–101 a week' },
+      { url: '/dashboard?account=6&week=2026-06-08&type=all', anchor: '102 inbound events · usually 37–104 a week' },
+      { url: '/dashboard?account=6&week=2026-07-20&type=call_received', anchor: '51 calls · usually 17–79 a week' },
+      { url: '/dashboard?account=8&week=2026-03-02&type=all', anchor: 'Not enough history yet (3 of 4 weeks needed)' },
+      { url: '/dashboard?account=14&week=2026-03-02&type=all', anchor: '40 inbound events · usually 16–36 a week' },
+      { url: '/dashboard?account=14&week=2026-07-20&type=appointment_set', anchor: '2 appointments · usually 1–8 a week' },
+      { url: '/dashboard?account=20', anchor: EMPTY_ACCOUNT_MESSAGE },
+    ])('never shows deviation, z, σ, ±, median, typical or a standalone "Normal" at $url', async ({ url, anchor }) => {
       const { root } = await openPage(url);
 
-      expect(pageText(root).length).toBeGreaterThan(0);
+      expect(pageText(root)).toContain(anchor);
       FORBIDDEN_ON_SCREEN.forEach((forbidden) => expect(pageText(root)).not.toMatch(forbidden));
     });
   });
@@ -454,33 +454,36 @@ describe('DashboardPage', () => {
   });
 
   describe('controls write the URL', () => {
-    it('"◀ Previous week" from the default writes week=2026-07-13 and enables "Next week ▶"', async () => {
-      const { root, harness } = await openPage('/dashboard?account=14&week=2026-07-20&type=all');
+    it('"◀ Previous week" from the default writes week=2026-07-13 as a new history entry and enables "Next week ▶"', async () => {
+      const { root, navigations, harness } = await openPage('/dashboard?account=14&week=2026-07-20&type=all');
 
       getButton(root, PREVIOUS_WEEK).click();
       await settle(harness);
 
       expect(currentQueryParams()).toEqual({ account: '14', week: '2026-07-13', type: 'all' });
+      expect(navigations.at(-1)?.replaceUrl).toBe(false);
       expect(isDisabled(getButton(root, NEXT_WEEK))).toBe(false);
       expect(pageText(root)).toContain('Mon Jul 13 – Sun Jul 19, 2026');
     });
 
-    it('"Next week ▶" from 2026-07-13 writes week=2026-07-20', async () => {
-      const { root, harness } = await openPage('/dashboard?account=14&week=2026-07-13&type=all');
+    it('"Next week ▶" from 2026-07-13 writes week=2026-07-20 as a new history entry', async () => {
+      const { root, navigations, harness } = await openPage('/dashboard?account=14&week=2026-07-13&type=all');
 
       getButton(root, NEXT_WEEK).click();
       await settle(harness);
 
       expect(currentQueryParams()).toEqual({ account: '14', week: '2026-07-20', type: 'all' });
+      expect(navigations.at(-1)?.replaceUrl).toBe(false);
     });
 
-    it('switching "Viewing as" keeps week and type (14 to 6 at 2026-03-02, calls)', async () => {
-      const { root, harness } = await openPage('/dashboard?account=14&week=2026-03-02&type=call_received');
+    it('switching "Viewing as" keeps week and type (14 to 6 at 2026-03-02, calls) as a new history entry', async () => {
+      const { root, navigations, harness } = await openPage('/dashboard?account=14&week=2026-03-02&type=call_received');
 
       chooseOption(getSelect(root, 'Viewing as'), 'Metro Collision Centers');
       await settle(harness);
 
       expect(currentQueryParams()).toEqual({ account: '6', week: '2026-03-02', type: 'call_received' });
+      expect(navigations.at(-1)?.replaceUrl).toBe(false);
     });
 
     it('switching "Viewing as" to an account that rejects the kept week falls back to the latest complete week with replaceUrl and no error', async () => {
@@ -507,7 +510,7 @@ describe('DashboardPage', () => {
       const reopened = await openPage(writtenUrl);
 
       expect(currentQueryParams()).toEqual({ account: '6', week: '2026-06-01', type: 'call_received' });
-      expect(selectedOptionText(getSelect(reopened.root, 'Viewing as'))).toContain('Metro Collision Centers');
+      expect(selectedOptionText(getSelect(reopened.root, 'Viewing as'))).toBe('Metro Collision Centers');
       expect(selectedOptionText(getSelect(reopened.root, 'Activity type'))).toBe('Calls');
       expect(reopened.activityHealthApi.requests.at(-1)).toEqual({ accountId: 6, week: '2026-06-01', eventType: 'call_received' });
     });
@@ -598,4 +601,26 @@ describe('DashboardPage', () => {
       expect(pageText(root)).not.toContain('of 4 weeks needed');
     });
   });
+
+  describe('PLAN §13 "Phase 1 red-suite decisions" (SPEC)', () => {
+    it('with no week in the URL and a failed first load, keeps account=14&type=all without a week and shows the load error with "Try again"', async () => {
+      const { root } = await openPage('/dashboard?account=14&type=all', (api) => api.failNext(serverError()));
+
+      expect(currentQueryParams()).toEqual({ account: '14', type: 'all' });
+      expect(pageText(root)).toContain(LOAD_ERROR_MESSAGE);
+      expect(getButton(root, 'Try again')).toBeTruthy();
+    });
+
+    it('with no week in the URL, "Try again" loads the data and fills in week=2026-07-20 with replaceUrl', async () => {
+      const { root, navigations, harness } = await openPage('/dashboard?account=14&type=all', (api) => api.failNext(serverError()));
+
+      getButton(root, 'Try again').click();
+      await settle(harness);
+
+      expect(currentQueryParams()).toEqual({ account: '14', week: '2026-07-20', type: 'all' });
+      expect(navigations.at(-1)?.replaceUrl).toBe(true);
+      expect(pageText(root)).not.toContain(LOAD_ERROR_MESSAGE);
+      expect(textOutsideTables(root)).toContain('26 inbound events · usually 18–38 a week');
+    });
+  });
 });


=== DIFF: web/src/testing/activity-health-fixtures.ts ===
@@ -84,13 +84,17 @@ export function buildReport(spec: ReportSpec): ActivityHealthReport {
   };
 }
 
+export function syntheticFillerRow(location: string, count: number, status: Exclude<HealthStatus, 'insufficient_data'>, deviation: number): LocationHealth {
+  return locationRow(location, withRange(count, count, Math.max(0, count - 4), count + 4, status, deviation));
+}
+
 export function genericReport(account: Account, weekStart: string, eventType: EventType): ActivityHealthReport {
   return buildReport({
     account,
     weekStart,
     eventType,
     summary: withRange(10, 10, 5, 15, 'normal', 0),
-    locations: [locationRow('Site A', withRange(10, 10, 5, 15, 'normal', 0))],
+    locations: [syntheticFillerRow('Site A', 10, 'normal', 0)],
   });
 }
 
@@ -98,11 +102,11 @@ export function beaconDefaultWeekReport(): ActivityHealthReport {
   return buildReport({
     account: BEACON_HOME_SECURITY,
     weekStart: '2026-07-20',
-    summary: withRange(26, 27, 18, 38, 'normal', -0.1),
+    summary: withRange(26, 27, 18, 38, 'normal', -0.19),
     locations: [
       locationRow('Site B', withRange(2, 6.5, 3, 12, 'below', -2.16)),
       locationRow('Site C', withRange(9, 6, 2, 12, 'normal', 0.98)),
-      locationRow('Site A', withRange(9, 7, 2, 16, 'normal', 0.61)),
+      locationRow('Site A', withRange(9, 6.5, 2, 16, 'normal', 0.61)),
       locationRow('Site D', withRange(6, 6.5, 3, 12, 'normal', -0.19)),
     ],
   });
@@ -113,12 +117,12 @@ export function beaconCallsDefaultWeekReport(): ActivityHealthReport {
     account: BEACON_HOME_SECURITY,
     weekStart: '2026-07-20',
     eventType: 'call_received',
-    summary: withRange(16, 15.5, 9, 24, 'normal', 0.1),
+    summary: withRange(16, 15.5, 9, 24, 'normal', 0.12),
     locations: [
-      locationRow('Site A', withRange(5, 4, 1, 9, 'normal', 0.4)),
-      locationRow('Site B', withRange(2, 3, 0, 7, 'normal', -0.3)),
-      locationRow('Site C', withRange(5, 5, 1, 10, 'normal', 0)),
-      locationRow('Site D', withRange(4, 4, 1, 9, 'normal', 0)),
+      syntheticFillerRow('Site A', 5, 'normal', 0.4),
+      syntheticFillerRow('Site C', 5, 'normal', 0.2),
+      syntheticFillerRow('Site D', 4, 'normal', 0.1),
+      syntheticFillerRow('Site B', 2, 'normal', 0),
     ],
   });
 }
@@ -128,12 +132,12 @@ export function beaconLeadsDefaultWeekReport(): ActivityHealthReport {
     account: BEACON_HOME_SECURITY,
     weekStart: '2026-07-20',
     eventType: 'lead_created',
-    summary: withRange(8, 6, 2, 12, 'normal', 0.4),
+    summary: withRange(8, 6, 2, 12, 'normal', 0.74),
     locations: [
-      locationRow('Site A', withRange(3, 2, 0, 6, 'normal', 0.5)),
-      locationRow('Site C', withRange(2, 2, 0, 6, 'normal', 0)),
-      locationRow('Site D', withRange(2, 1, 0, 4, 'normal', 0.3)),
-      locationRow('Site B', withRange(1, 1, 0, 4, 'normal', 0)),
+      syntheticFillerRow('Site A', 3, 'normal', 0.5),
+      syntheticFillerRow('Site C', 2, 'normal', 0.3),
+      syntheticFillerRow('Site D', 2, 'normal', 0.2),
+      syntheticFillerRow('Site B', 1, 'normal', 0),
     ],
   });
 }
@@ -143,7 +147,7 @@ export function beaconAppointmentsDefaultWeekReport(): ActivityHealthReport {
     account: BEACON_HOME_SECURITY,
     weekStart: '2026-07-20',
     eventType: 'appointment_set',
-    summary: withRange(2, 3.5, 1, 8, 'normal', -0.6),
+    summary: withRange(2, 3.5, 1, 8, 'normal', -0.85),
     locations: [
       locationRow('Site A', withRange(0, 1, 0, 4, 'normal', -1.12)),
       locationRow('Site B', withRange(0, 0, 0, 2, 'normal', 0)),
@@ -194,21 +198,19 @@ export function beaconEarliestWeekReport(): ActivityHealthReport {
 }
 
 export function metroSpikeWeekReport(): ActivityHealthReport {
-  const otherSites = ACCOUNT_6_SITE_NAMES.filter((site) => site !== 'Site C').map((site, index) =>
-    locationRow(site, withRange(60 - index, 4, 1, 9, 'above', 12 - index * 0.5)),
+  const syntheticSpikeSiteCounts = [58, 58, 58, 58, 58, 58, 58, 58, 58, 58, 58, 58, 58, 59];
+  const syntheticSpikeSites = ACCOUNT_6_SITE_NAMES.filter((site) => site !== 'Site C').map((site, index) =>
+    syntheticFillerRow(site, syntheticSpikeSiteCounts[index], 'above', 12 - index * 0.5),
   );
   return buildReport({
     account: METRO_COLLISION_CENTERS,
     weekStart: '2026-06-01',
     summary: withRange(880, 66, 39, 101, 'above', 22.37),
-    locations: [locationRow('Site C', withRange(67, 3, 1, 7, 'above', 12.74)), ...otherSites],
+    locations: [locationRow('Site C', withRange(67, 3, 1, 7, 'above', 12.74)), ...syntheticSpikeSites],
   });
 }
 
 export function metroWeekAfterSpikeReport(): ActivityHealthReport {
-  const normalSites = ACCOUNT_6_SITE_NAMES.filter((site) => site !== 'Site C' && site !== 'Site J').map((site, index) =>
-    locationRow(site, withRange(6, 5, 2, 10, 'normal', 1.5 - index * 0.1)),
-  );
   return buildReport({
     account: METRO_COLLISION_CENTERS,
     weekStart: '2026-06-08',
@@ -216,43 +218,70 @@ export function metroWeekAfterSpikeReport(): ActivityHealthReport {
     locations: [
       locationRow('Site C', withRange(11, 3.5, 1, 8, 'above', 2.81)),
       locationRow('Site J', withRange(11, 5, 2, 10, 'above', 2.11)),
-      ...normalSites,
+      locationRow('Site K', withRange(9, 4.5, 2, 9, 'normal', 1.71)),
+      locationRow('Site H', withRange(7, 3.5, 1, 8, 'normal', 1.44)),
+      locationRow('Site L', withRange(10, 5, 1, 12, 'normal', 1.39)),
+      locationRow('Site O', withRange(9, 5, 1, 12, 'normal', 1.14)),
+      locationRow('Site D', withRange(4, 5.5, 2, 11, 'normal', -0.66)),
+      locationRow('Site I', withRange(5, 3.5, 1, 9, 'normal', 0.61)),
+      locationRow('Site B', withRange(6, 4, 1, 12, 'normal', 0.59)),
+      locationRow('Site N', withRange(5, 4, 1, 9, 'normal', 0.44)),
+      locationRow('Site E', withRange(8, 6.5, 2, 16, 'normal', 0.38)),
+      locationRow('Site M', withRange(3, 3.5, 1, 8, 'normal', -0.26)),
+      locationRow('Site F', withRange(5, 5.5, 2, 11, 'normal', -0.21)),
+      locationRow('Site G', withRange(4, 4.5, 1, 11, 'normal', -0.18)),
+      locationRow('Site A', withRange(5, 4.5, 1, 11, 'normal', 0.17)),
     ],
   });
 }
 
 export function metroSilentLocationReport(): ActivityHealthReport {
-  const normalSites = ACCOUNT_6_SITE_NAMES.filter((site) => site !== 'Site G').map((site, index) =>
-    locationRow(site, withRange(5, 5, 2, 10, 'normal', 1.2 - index * 0.1)),
-  );
   return buildReport({
     account: METRO_COLLISION_CENTERS,
     weekStart: '2026-06-29',
-    summary: withRange(69, 70, 41, 111, 'normal', -0.05),
-    locations: [locationRow('Site G', withRange(0, 5, 2, 9, 'below', -3.19)), ...normalSites],
+    summary: withRange(69, 72, 41, 111, 'normal', -0.17),
+    locations: [
+      locationRow('Site G', withRange(0, 4.5, 2, 9, 'below', -3.19)),
+      locationRow('Site N', withRange(2, 4.5, 2, 9, 'normal', -1.33)),
+      locationRow('Site M', withRange(1, 3, 1, 7, 'normal', -1.33)),
+      locationRow('Site F', withRange(3, 5, 2, 10, 'normal', -0.96)),
+      locationRow('Site D', withRange(6, 4, 1, 9, 'normal', 0.87)),
+      locationRow('Site I', withRange(6, 4, 1, 9, 'normal', 0.87)),
+      locationRow('Site E', withRange(9, 6, 2, 14, 'normal', 0.84)),
+      locationRow('Site B', withRange(4, 6, 1, 15, 'normal', -0.6)),
+      locationRow('Site J', withRange(3, 4.5, 1, 11, 'normal', -0.57)),
+      locationRow('Site H', withRange(7, 5, 1, 13, 'normal', 0.56)),
+      locationRow('Site L', withRange(4, 5.5, 1, 13, 'normal', -0.49)),
+      locationRow('Site C', withRange(6, 5, 1, 13, 'normal', 0.29)),
+      locationRow('Site O', withRange(6, 5, 1, 13, 'normal', 0.29)),
+      locationRow('Site A', withRange(7, 6, 1, 15, 'normal', 0.27)),
+      locationRow('Site K', withRange(5, 5, 2, 10, 'normal', 0)),
+    ],
   });
 }
 
 export function metroSpikeInBaselineReport(): ActivityHealthReport {
-  const orderAfterSiteM = ['Site O', 'Site I', 'Site A', 'Site E', 'Site J', 'Site G', 'Site N', 'Site L', 'Site B', 'Site H', 'Site F', 'Site K', 'Site D', 'Site C'];
+  const syntheticSitesAfterSiteM = ['Site O', 'Site I', 'Site A', 'Site E', 'Site J', 'Site G', 'Site N', 'Site L', 'Site B', 'Site H', 'Site F', 'Site K', 'Site D', 'Site C'];
+  const syntheticSiteCounts = [6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 4, 4];
   return buildReport({
     account: METRO_COLLISION_CENTERS,
     weekStart: '2026-07-20',
     summary: withRange(87, 72.5, 30, 134, 'normal', 0.53),
     locations: [
       locationRow('Site M', withRange(7, 3.5, 1, 9, 'normal', 1.3)),
-      ...orderAfterSiteM.map((site, index) => locationRow(site, withRange(5, 5, 2, 10, 'normal', 1.2 - index * 0.08))),
+      ...syntheticSitesAfterSiteM.map((site, index) => syntheticFillerRow(site, syntheticSiteCounts[index], 'normal', 1.2 - index * 0.08)),
     ],
   });
 }
 
 export function metroCallsSpikeInBaselineReport(): ActivityHealthReport {
+  const syntheticCallCounts = [4, 4, 4, 4, 4, 4, 3, 3, 3, 3, 3, 3, 3, 3, 3];
   return buildReport({
     account: METRO_COLLISION_CENTERS,
     weekStart: '2026-07-20',
     eventType: 'call_received',
     summary: withRange(51, 42, 17, 79, 'normal', 0.54),
-    locations: ACCOUNT_6_SITE_NAMES.map((site, index) => locationRow(site, withRange(3, 3, 0, 7, 'normal', 0.9 - index * 0.05))),
+    locations: ACCOUNT_6_SITE_NAMES.map((site, index) => syntheticFillerRow(site, syntheticCallCounts[index], 'normal', 0.9 - index * 0.05)),
   });
 }
 


Investigate per the method in your instructions, then return the findings list.


**tool call — Read**
```json
{"file_path":"/Users/josemejia/Documents/Software Development/Qualitara/relay-activity-health/.claude/worktrees/agent-a325076056495e3c4/web/src/testing/activity-health-fixtures.ts"}
```


<details><summary>tool result</summary>

```
1	import {
2	  Account,
3	  ActivityHealthReport,
4	  EventType,
5	  HealthStatus,
6	  LocationHealth,
7	  SeriesHealth,
8	} from '../app/core/models';
9	
10	export const DATA_AS_OF = '2026-07-27T22:20:34Z';
11	export const LATEST_COMPLETE_WEEK = '2026-07-20';
12	
13	export const METRO_COLLISION_CENTERS: Account = { id: 6, name: 'Metro Collision Centers', timezone: 'America/New_York' };
14	export const LAKESIDE_PHYSIO: Account = { id: 8, name: 'Lakeside Physio', timezone: 'America/Chicago' };
15	export const REDLINE_TIRE_AND_SERVICE: Account = { id: 12, name: 'Redline Tire & Service', timezone: 'America/Los_Angeles' };
16	export const BEACON_HOME_SECURITY: Account = { id: 14, name: 'Beacon Home Security', timezone: 'America/New_York' };
17	export const QUIET_HARBOR_SPA: Account = { id: 20, name: 'Quiet Harbor Spa', timezone: 'America/Los_Angeles' };
18	
19	export const EARLIEST_WEEK_BY_ACCOUNT_ID: ReadonlyMap<number, string> = new Map([
20	  [METRO_COLLISION_CENTERS.id, '2026-01-26'],
21	  [LAKESIDE_PHYSIO.id, '2026-02-02'],
22	  [REDLINE_TIRE_AND_SERVICE.id, '2026-01-26'],
23	  [BEACON_HOME_SECURITY.id, '2026-01-26'],
24	  [QUIET_HARBOR_SPA.id, LATEST_COMPLETE_WEEK],
25	]);
26	
27	export function seedAccounts(): Account[] {
28	  return [METRO_COLLISION_CENTERS, LAKESIDE_PHYSIO, REDLINE_TIRE_AND_SERVICE, BEACON_HOME_SECURITY, QUIET_HARBOR_SPA].map((account) => ({ ...account }));
29	}
30	
31	const ACCOUNT_6_SITE_NAMES = [
32	  'Site A', 'Site B', 'Site C', 'Site D', 'Site E',  …[clipped in markdown; full text in raw/]
```
</details>


### 🤖 ASSISTANT — 2026-09-28T21:18:57.350Z

All six changed files are test/spec files or a test-only fixtures module (`web/src/testing/activity-health-fixtures.ts`). Per the review guidance, test/mock files are explicitly out of scope.

Reviewing the fixtures file to confirm: it exports only pure in-memory report/summary builders (`buildReport`, `syntheticFillerRow`, `withRange`, `withoutEnoughHistory`, `locationRow`, `sundayOf`, `seedAccounts`, and scenario helpers). There are:

- No network, filesystem, subprocess, eval/dynamic-loader, HTML/template, deserialization, DB, or logging sinks introduced.
- No user-controllable inputs — all values are hard-coded synthetic account names ("Metro Collision Centers", "Beacon Home Security", etc.), integer IDs, and numeric counts.
- No credentials, tokens, PII, or secrets.
- No production-code changes; nothing in `web/src/testing/*` or `*.spec.ts` is imported by production bundles.

The spec files exercise Angular component and state behavior with `TestBed`, subscribing to output events and asserting rendered text/URLs. No sinks are added; the only "external" call is `Date` construction in `sundayOf`, which is deterministic UTC arithmetic on a hard-coded ISO string.

No source→sink path with any impact exists in this diff.


**tool call — StructuredOutput**
```json
{"findings":[]}
```


<details><summary>tool result</summary>

```
Structured output provided successfully
```
</details>

