import { ChangeDetectionStrategy, Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { AccountsApi } from '../../core/api/accounts.api';
import { ActivityHealthApi } from '../../core/api/activity-health.api';
import { EVENT_TYPES } from '../../core/models';
import {
  beaconDefaultWeekReport,
  beaconLeadsDefaultWeekReport,
  lakesideDefaultWeekReport,
  METRO_COLLISION_CENTERS,
  genericReport,
  metroSpikeWeekReport,
  quietHarborEmptyReport,
  seedAccounts,
} from '../../../testing/activity-health-fixtures';
import { FakeAccountsApi, FakeActivityHealthApi, networkFailure, serverError } from '../../../testing/fake-apis';
import { RecordedNavigation, currentPath, currentQueryParams, queryParamsOf, recordNavigations, settle } from '../../../testing/router-testing';
import { DEFAULT_ACCOUNT_ID, DEFAULT_EVENT_TYPE, DashboardState } from './dashboard-state';

@Component({
  selector: 'app-dashboard-route-stub',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: '',
})
class DashboardRouteStub {}

interface StateUnderTest {
  state: DashboardState;
  activityHealthApi: FakeActivityHealthApi;
  accountsApi: FakeAccountsApi;
  navigations: RecordedNavigation[];
  harness: RouterTestingHarness;
}

const DEFAULT_URL = '/dashboard?account=14&week=2026-07-20&type=all';
const DEFAULT_QUERY_PARAMS = { account: '14', week: '2026-07-20', type: 'all' };

async function openState(url: string, prepareApi?: (activityHealthApi: FakeActivityHealthApi) => void): Promise<StateUnderTest> {
  const activityHealthApi = new FakeActivityHealthApi();
  const accountsApi = new FakeAccountsApi();
  prepareApi?.(activityHealthApi);
  TestBed.configureTestingModule({
    providers: [
      provideRouter([{ path: 'dashboard', component: DashboardRouteStub }]),
      { provide: ActivityHealthApi, useValue: activityHealthApi },
      { provide: AccountsApi, useValue: accountsApi },
      DashboardState,
    ],
  });
  const navigations = recordNavigations(TestBed.inject(Router));
  const harness = await RouterTestingHarness.create();
  await harness.navigateByUrl(url);
  const state = TestBed.inject(DashboardState);
  await settle(harness);
  return { state, activityHealthApi, accountsApi, navigations, harness };
}

function navigationsAfterOpening(navigations: RecordedNavigation[]): RecordedNavigation[] {
  return navigations.slice(1);
}

function expectNormalisedWithReplaceUrl(navigations: RecordedNavigation[]): void {
  const rewrites = navigationsAfterOpening(navigations);
  expect(rewrites.length).toBeGreaterThan(0);
  expect(rewrites.every((rewrite) => rewrite.replaceUrl)).toBe(true);
}

describe('DashboardState', () => {
  describe('reading the URL', () => {
    it('exposes account, week and type from the query params', async () => {
      const { state } = await openState('/dashboard?account=6&week=2026-06-01&type=all');

      expect(state.accountId()).toBe(6);
      expect(state.week()).toBe('2026-06-01');
      expect(state.eventType()).toBe('all');
    });

    it('requests the report for exactly the account, week and type in the URL', async () => {
      const { activityHealthApi } = await openState('/dashboard?account=6&week=2026-07-20&type=call_received');

      expect(activityHealthApi.requests.at(-1)).toEqual({ accountId: 6, week: '2026-07-20', eventType: 'call_received' });
    });

    it('exposes the API report once loaded', async () => {
      const { state } = await openState('/dashboard?account=6&week=2026-06-01&type=all');

      expect(state.report()).toEqual(metroSpikeWeekReport());
      expect(state.isLoading()).toBe(false);
      expect(state.error()).toBeNull();
    });

    it('exposes the accounts list for the Viewing-as select', async () => {
      const { state, accountsApi } = await openState(DEFAULT_URL);

      expect(accountsApi.listAccountsCalls).toBeGreaterThan(0);
      expect(state.accounts()).toEqual(seedAccounts());
    });

    it('leaves a complete valid URL unchanged once the report has loaded', async () => {
      const { state } = await openState('/dashboard?account=6&week=2026-06-01&type=call_received');

      expect(state.report()?.week.start).toBe('2026-06-01');
      expect(currentPath()).toBe('/dashboard');
      expect(currentQueryParams()).toEqual({ account: '6', week: '2026-06-01', type: 'call_received' });
    });
  });

  describe('normalising missing params', () => {
    it('rewrites /dashboard to account 14, the latest complete week from the response and type all', async () => {
      const { navigations } = await openState('/dashboard');

      expect(currentPath()).toBe('/dashboard');
      expect(currentQueryParams()).toEqual(DEFAULT_QUERY_PARAMS);
      expectNormalisedWithReplaceUrl(navigations);
    });

    it('exposes the defaults as state after normalising /dashboard', async () => {
      const { state } = await openState('/dashboard');

      expect(state.accountId()).toBe(DEFAULT_ACCOUNT_ID);
      expect(state.accountId()).toBe(14);
      expect(state.week()).toBe('2026-07-20');
      expect(state.eventType()).toBe(DEFAULT_EVENT_TYPE);
      expect(state.eventType()).toBe('all');
      expect(state.report()).toEqual(beaconDefaultWeekReport());
    });

    it('asks the API for the latest complete week by omitting week, never by sending an empty or guessed week', async () => {
      const { activityHealthApi } = await openState('/dashboard');

      expect(activityHealthApi.requests[0]).toEqual({ accountId: 14, week: null, eventType: 'all' });
    });

    it('fills only the missing week and keeps the given account and type', async () => {
      const { navigations } = await openState('/dashboard?account=6&type=call_received');

      expect(currentQueryParams()).toEqual({ account: '6', week: '2026-07-20', type: 'call_received' });
      expectNormalisedWithReplaceUrl(navigations);
    });
  });

  describe('normalising invalid params', () => {
    it('rewrites an unknown account 999 to account 14 with replaceUrl', async () => {
      const { state, navigations } = await openState('/dashboard?account=999&week=2026-07-20&type=all');

      expect(currentQueryParams()).toEqual(DEFAULT_QUERY_PARAMS);
      expect(state.accountId()).toBe(14);
      expect(state.error()).toBeNull();
      expectNormalisedWithReplaceUrl(navigations);
    });

    it.each([
      { label: 'abc', invalidAccount: 'abc' },
      { label: '(empty value)', invalidAccount: '' },
      { label: '6.5', invalidAccount: '6.5' },
    ])('rewrites a non-integer account $label to account 14', async ({ invalidAccount }) => {
      const { navigations } = await openState(`/dashboard?account=${invalidAccount}&week=2026-07-20&type=all`);

      expect(currentQueryParams()).toEqual(DEFAULT_QUERY_PARAMS);
      expectNormalisedWithReplaceUrl(navigations);
    });

    it('rewrites a Tuesday week to the latest complete week', async () => {
      const { navigations } = await openState('/dashboard?account=14&week=2026-07-21&type=all');

      expect(currentQueryParams()).toEqual(DEFAULT_QUERY_PARAMS);
      expectNormalisedWithReplaceUrl(navigations);
    });

    it('rewrites a Wednesday week 2026-03-04 to the latest complete week 2026-07-20, not the nearest Monday 2026-03-02', async () => {
      const { state, navigations } = await openState('/dashboard?account=14&week=2026-03-04&type=all');

      expect(currentQueryParams()['week']).toBe('2026-07-20');
      expect(currentQueryParams()['week']).not.toBe('2026-03-02');
      expect(state.week()).toBe('2026-07-20');
      expectNormalisedWithReplaceUrl(navigations);
    });

    it('rewrites the current partial week 2026-07-27 to 2026-07-20', async () => {
      const { navigations } = await openState('/dashboard?account=14&week=2026-07-27&type=all');

      expect(currentQueryParams()).toEqual(DEFAULT_QUERY_PARAMS);
      expectNormalisedWithReplaceUrl(navigations);
    });

    it('recovers from the API 400 for a week before earliestWeek by rewriting to the latest complete week without an error', async () => {
      const { state, navigations } = await openState('/dashboard?account=14&week=2025-12-29&type=all');

      expect(currentQueryParams()).toEqual(DEFAULT_QUERY_PARAMS);
      expect(state.error()).toBeNull();
      expect(state.report()).toEqual(beaconDefaultWeekReport());
      expectNormalisedWithReplaceUrl(navigations);
    });

    it.each([
      { label: 'abc', malformedWeek: 'abc' },
      { label: '2026-13-01', malformedWeek: '2026-13-01' },
      { label: '20260720', malformedWeek: '20260720' },
      { label: '2026-7-20', malformedWeek: '2026-7-20' },
      { label: '(empty value)', malformedWeek: '' },
    ])('rewrites a malformed week $label to the latest complete week', async ({ malformedWeek }) => {
      const { navigations } = await openState(`/dashboard?account=14&week=${malformedWeek}&type=all`);

      expect(currentQueryParams()).toEqual(DEFAULT_QUERY_PARAMS);
      expectNormalisedWithReplaceUrl(navigations);
    });

    it('rewrites account 20 with week 2026-03-02 to 2026-07-20 and shows its empty report, not an error', async () => {
      const { state, navigations } = await openState('/dashboard?account=20&week=2026-03-02&type=all');

      expect(currentQueryParams()).toEqual({ account: '20', week: '2026-07-20', type: 'all' });
      expect(state.report()).toEqual(quietHarborEmptyReport());
      expect(state.error()).toBeNull();
      expectNormalisedWithReplaceUrl(navigations);
    });

    it.each([
      { label: 'ALL', invalidType: 'ALL' },
      { label: 'foo', invalidType: 'foo' },
      { label: 'Call_Received', invalidType: 'Call_Received' },
      { label: '(empty value)', invalidType: '' },
    ])('rewrites an invalid type $label to all', async ({ invalidType }) => {
      const { state, navigations } = await openState(`/dashboard?account=14&week=2026-07-20&type=${invalidType}`);

      expect(currentQueryParams()).toEqual(DEFAULT_QUERY_PARAMS);
      expect(state.eventType()).toBe('all');
      expectNormalisedWithReplaceUrl(navigations);
    });

    it('rewrites only the invalid param and keeps the valid account and week', async () => {
      await openState('/dashboard?account=6&week=2026-06-01&type=foo');

      expect(currentQueryParams()).toEqual({ account: '6', week: '2026-06-01', type: 'all' });
    });

    it('keeps unrelated query params when rewriting (merge)', async () => {
      await openState('/dashboard?account=abc&week=2026-07-20&type=all&ref=monday-email');

      expect(currentQueryParams()).toEqual({ ...DEFAULT_QUERY_PARAMS, ref: 'monday-email' });
    });
  });

  describe('writing state to the URL', () => {
    it('selectWeek writes the week and keeps account and type', async () => {
      const { state, activityHealthApi, harness } = await openState('/dashboard?account=14&week=2026-07-20&type=call_received');

      state.selectWeek('2026-07-13');
      await settle(harness);

      expect(currentQueryParams()).toEqual({ account: '14', week: '2026-07-13', type: 'call_received' });
      expect(state.week()).toBe('2026-07-13');
      expect(activityHealthApi.requests.at(-1)).toEqual({ accountId: 14, week: '2026-07-13', eventType: 'call_received' });
    });

    it('selectWeek adds a history entry (no replaceUrl) so Back returns to the previous week', async () => {
      const { state, navigations, harness } = await openState(DEFAULT_URL);

      state.selectWeek('2026-07-13');
      await settle(harness);

      expect(navigationsAfterOpening(navigations)).toHaveLength(1);
      expect(navigations.at(-1)?.replaceUrl).toBe(false);
    });

    it('selectEventType adds a history entry (no replaceUrl)', async () => {
      const { state, navigations, harness } = await openState(DEFAULT_URL);

      state.selectEventType('call_received');
      await settle(harness);

      expect(navigationsAfterOpening(navigations)).toHaveLength(1);
      expect(navigations.at(-1)?.replaceUrl).toBe(false);
    });

    it('selectAccount adds a history entry (no replaceUrl) when the kept week is valid', async () => {
      const { state, navigations, harness } = await openState(DEFAULT_URL);

      state.selectAccount(6);
      await settle(harness);

      expect(navigationsAfterOpening(navigations)).toHaveLength(1);
      expect(navigations.at(-1)?.replaceUrl).toBe(false);
    });

    it('selectEventType writes the type and keeps account and week', async () => {
      const { state, harness } = await openState(DEFAULT_URL);

      state.selectEventType('lead_created');
      await settle(harness);

      expect(currentQueryParams()).toEqual({ account: '14', week: '2026-07-20', type: 'lead_created' });
      expect(state.eventType()).toBe('lead_created');
      expect(state.report()).toEqual(beaconLeadsDefaultWeekReport());
    });

    it('selectAccount keeps the week and type (account 14 to 6 at 2026-03-02, calls)', async () => {
      const { state, activityHealthApi, harness } = await openState('/dashboard?account=14&week=2026-03-02&type=call_received');

      state.selectAccount(6);
      await settle(harness);

      expect(currentQueryParams()).toEqual({ account: '6', week: '2026-03-02', type: 'call_received' });
      expect(state.accountId()).toBe(6);
      expect(activityHealthApi.requests.at(-1)).toEqual({ accountId: 6, week: '2026-03-02', eventType: 'call_received' });
    });

    it('selectAccount falls back to the latest complete week with replaceUrl when the API rejects the kept week (14 to 8 at 2026-01-26)', async () => {
      const { state, activityHealthApi, navigations, harness } = await openState('/dashboard?account=14&week=2026-01-26&type=call_received');

      state.selectAccount(8);
      await settle(harness);

      expect(activityHealthApi.requests).toContainEqual({ accountId: 8, week: '2026-01-26', eventType: 'call_received' });
      expect(currentQueryParams()).toEqual({ account: '8', week: '2026-07-20', type: 'call_received' });
      expect(navigationsAfterOpening(navigations)[0]?.replaceUrl).toBe(false);
      expect(navigations.at(-1)?.replaceUrl).toBe(true);
      expect(state.error()).toBeNull();
      expect(state.report()?.account.id).toBe(8);
      expect(state.report()?.week.start).toBe('2026-07-20');
      expect(state.report()?.eventType).toBe('call_received');
    });

    it('keeps unrelated query params when writing a selection (merge)', async () => {
      const { state, harness } = await openState('/dashboard?account=14&week=2026-07-20&type=all&ref=monday-email');

      state.selectWeek('2026-07-13');
      await settle(harness);

      expect(currentQueryParams()).toEqual({ account: '14', week: '2026-07-13', type: 'all', ref: 'monday-email' });
    });

    it('never writes an empty week= param in any navigation', async () => {
      const { state, activityHealthApi, navigations, harness } = await openState('/dashboard?week=');
      state.selectAccount(6);
      await settle(harness);
      state.selectEventType('lead_created');
      await settle(harness);
      state.selectWeek('2026-07-13');
      await settle(harness);

      expect(navigationsAfterOpening(navigations).length).toBeGreaterThan(0);
      expect(navigationsAfterOpening(navigations).every((navigation) => queryParamsOf(navigation.url)['week'] !== '')).toBe(true);
      expect(activityHealthApi.requests.every((request) => request.week !== '')).toBe(true);
    });

    it('reproduces the same view when the URL written by the selections is reopened', async () => {
      const first = await openState(DEFAULT_URL);
      first.state.selectAccount(6);
      await settle(first.harness);
      first.state.selectWeek('2026-06-01');
      await settle(first.harness);
      first.state.selectEventType('call_received');
      await settle(first.harness);
      const writtenUrl = TestBed.inject(Router).url;
      TestBed.resetTestingModule();

      const reopened = await openState(writtenUrl);

      expect(queryParamsOf(writtenUrl)).toEqual({ account: '6', week: '2026-06-01', type: 'call_received' });
      expect(reopened.state.accountId()).toBe(6);
      expect(reopened.state.week()).toBe('2026-06-01');
      expect(reopened.state.eventType()).toBe('call_received');
      expect(reopened.state.report()).toEqual(genericReport(METRO_COLLISION_CENTERS, '2026-06-01', 'call_received'));
      expect(reopened.activityHealthApi.requests.at(-1)).toEqual({ accountId: 6, week: '2026-06-01', eventType: 'call_received' });
      expect(currentQueryParams()).toEqual({ account: '6', week: '2026-06-01', type: 'call_received' });
    });
  });

  describe('loading, errors and reload', () => {
    it('is loading while the report request is pending and stops when it arrives', async () => {
      const { state, activityHealthApi, harness } = await openState(DEFAULT_URL, (api) => api.holdNext());

      expect(state.isLoading()).toBe(true);

      activityHealthApi.releaseHeld();
      await settle(harness);

      expect(state.isLoading()).toBe(false);
      expect(state.report()).toEqual(beaconDefaultWeekReport());
    });

    it('exposes an error and stops loading when the API fails with a 5xx', async () => {
      const { state } = await openState(DEFAULT_URL, (api) => api.failNext(serverError()));

      expect(state.error()).not.toBeNull();
      expect(state.isLoading()).toBe(false);
    });

    it('exposes an error when the API is unreachable', async () => {
      const { state } = await openState(DEFAULT_URL, (api) => api.failNext(networkFailure()));

      expect(state.error()).not.toBeNull();
      expect(state.isLoading()).toBe(false);
    });

    it('keeps the filters in the URL when the load fails', async () => {
      const { state, navigations } = await openState('/dashboard?account=6&week=2026-06-01&type=call_received', (api) => api.failNext(serverError()));

      expect(state.error()).not.toBeNull();
      expect(currentQueryParams()).toEqual({ account: '6', week: '2026-06-01', type: 'call_received' });
      expect(navigationsAfterOpening(navigations)).toEqual([]);
    });

    it('reload() after a failure re-requests the same params, shows the data and leaves the URL unchanged', async () => {
      const { state, activityHealthApi, harness } = await openState(DEFAULT_URL, (api) => api.failNext(serverError()));
      const urlBeforeReload = TestBed.inject(Router).url;

      state.reload();
      await settle(harness);

      expect(activityHealthApi.requests).toHaveLength(2);
      expect(activityHealthApi.requests[1]).toEqual(activityHealthApi.requests[0]);
      expect(state.error()).toBeNull();
      expect(state.report()).toEqual(beaconDefaultWeekReport());
      expect(TestBed.inject(Router).url).toBe(urlBeforeReload);
    });

    it('reload() re-requests the current params when the last load succeeded', async () => {
      const { state, activityHealthApi, harness } = await openState('/dashboard?account=8&week=2026-07-20&type=all');
      const requestsBeforeReload = activityHealthApi.requests.length;

      state.reload();
      await settle(harness);

      expect(activityHealthApi.requests).toHaveLength(requestsBeforeReload + 1);
      expect(activityHealthApi.requests.at(-1)).toEqual({ accountId: 8, week: '2026-07-20', eventType: 'all' });
      expect(state.report()).toEqual(lakesideDefaultWeekReport());
    });
  });

  describe('PLAN §13 "Phase 1 red-suite decisions" (SPEC)', () => {
    it('checks the type against EVENT_TYPES on the client and never sends an invalid type to the API', async () => {
      const { activityHealthApi } = await openState('/dashboard?account=14&week=2026-07-20&type=ALL');

      expect(currentQueryParams()).toEqual(DEFAULT_QUERY_PARAMS);
      expect(activityHealthApi.requests.length).toBeGreaterThan(0);
      expect(activityHealthApi.requests.every((request) => (EVENT_TYPES as readonly string[]).includes(request.eventType))).toBe(true);
    });

    it.each([
      { label: '5xx', failure: serverError },
      { label: 'network failure', failure: networkFailure },
    ])('with no week in the URL and a first-load $label, keeps account=14&type=all without a week param and exposes the error', async ({ failure }) => {
      const { state } = await openState('/dashboard?account=14&type=all', (api) => api.failNext(failure()));

      expect(currentQueryParams()).toEqual({ account: '14', type: 'all' });
      expect(state.error()).not.toBeNull();
      expect(state.isLoading()).toBe(false);
    });

    it('with no week in the URL, fills in the latest complete week with replaceUrl after a successful reload', async () => {
      const { state, activityHealthApi, navigations, harness } = await openState('/dashboard?account=14&type=all', (api) => api.failNext(serverError()));

      state.reload();
      await settle(harness);

      expect(activityHealthApi.requests[1]).toEqual({ accountId: 14, week: null, eventType: 'all' });
      expect(currentQueryParams()).toEqual(DEFAULT_QUERY_PARAMS);
      expect(navigations.at(-1)?.replaceUrl).toBe(true);
      expect(state.error()).toBeNull();
      expect(state.report()).toEqual(beaconDefaultWeekReport());
    });
  });
});
