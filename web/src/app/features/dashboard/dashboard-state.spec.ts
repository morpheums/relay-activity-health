import { ChangeDetectionStrategy, Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Params, Router, provideRouter } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { AccountsApi } from '../../core/api/accounts.api';
import { ActivityHealthApi, ActivityHealthRequest } from '../../core/api/activity-health.api';
import { EVENT_TYPES } from '../../core/models';
import { beaconDefaultWeekReport } from '../../../testing/activity-health-fixtures';
import { FakeAccountsApi, FakeActivityHealthApi, serverError } from '../../../testing/fake-apis';
import { RecordedNavigation, currentQueryParams, queryParamsOf, recordNavigations, settle } from '../../../testing/router-testing';
import { DashboardState } from './dashboard-state';

@Component({
  selector: 'app-dashboard-route-stub',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: '',
})
class DashboardRouteStub {}

interface StateUnderTest {
  state: DashboardState;
  activityHealthApi: FakeActivityHealthApi;
  navigations: RecordedNavigation[];
  harness: RouterTestingHarness;
}

const DEFAULT_QUERY_PARAMS = { account: '14', week: '2026-07-20', type: 'all' };

async function openState(url: string, prepareApi?: (activityHealthApi: FakeActivityHealthApi) => void): Promise<StateUnderTest> {
  const activityHealthApi = new FakeActivityHealthApi();
  prepareApi?.(activityHealthApi);
  TestBed.configureTestingModule({
    providers: [
      provideRouter([{ path: 'dashboard', component: DashboardRouteStub }]),
      { provide: ActivityHealthApi, useValue: activityHealthApi },
      { provide: AccountsApi, useValue: new FakeAccountsApi() },
      DashboardState,
    ],
  });
  const navigations = recordNavigations(TestBed.inject(Router));
  const harness = await RouterTestingHarness.create();
  await harness.navigateByUrl(url);
  const state = TestBed.inject(DashboardState);
  await settle(harness);
  return { state, activityHealthApi, navigations, harness };
}

function navigationsAfterOpening(navigations: RecordedNavigation[]): RecordedNavigation[] {
  return navigations.slice(1);
}

describe('DashboardState', () => {
  it('reads account, week and type from the URL and requests exactly them', async () => {
    const { state, activityHealthApi } = await openState('/dashboard?account=6&week=2026-06-01&type=call_received');

    expect(state.accountId()).toBe(6);
    expect(state.week()).toBe('2026-06-01');
    expect(state.eventType()).toBe('call_received');
    expect(activityHealthApi.requests).toEqual<ActivityHealthRequest[]>([{ accountId: 6, week: '2026-06-01', eventType: 'call_received' }]);
  });

  it('leaves a complete valid URL unchanged', async () => {
    const { navigations } = await openState('/dashboard?account=6&week=2026-06-01&type=call_received');

    expect(currentQueryParams()).toEqual({ account: '6', week: '2026-06-01', type: 'call_received' });
    expect(navigationsAfterOpening(navigations)).toEqual([]);
  });

  it.each<{ case: string; url: string; expectedParams: Params }>([
    { case: 'no params → defaults (UI-01)', url: '/dashboard', expectedParams: DEFAULT_QUERY_PARAMS },
    { case: 'missing week only', url: '/dashboard?account=6&type=call_received', expectedParams: { account: '6', week: '2026-07-20', type: 'call_received' } },
    { case: 'unknown account 999 (UI-32)', url: '/dashboard?account=999&week=2026-07-20&type=all', expectedParams: DEFAULT_QUERY_PARAMS },
    { case: 'non-integer account', url: '/dashboard?account=abc&week=2026-07-20&type=all', expectedParams: DEFAULT_QUERY_PARAMS },
    { case: 'Tuesday week (UI-33)', url: '/dashboard?account=14&week=2026-07-21&type=all', expectedParams: DEFAULT_QUERY_PARAMS },
    { case: 'Wednesday week goes to latest, not nearest Monday (UI-33b)', url: '/dashboard?account=14&week=2026-03-04&type=all', expectedParams: DEFAULT_QUERY_PARAMS },
    { case: 'current partial week (UI-34)', url: '/dashboard?account=14&week=2026-07-27&type=all', expectedParams: DEFAULT_QUERY_PARAMS },
    { case: 'week before earliestWeek, API 400 (UI-35)', url: '/dashboard?account=14&week=2025-12-29&type=all', expectedParams: DEFAULT_QUERY_PARAMS },
    { case: 'malformed week', url: '/dashboard?account=14&week=2026-13-01&type=all', expectedParams: DEFAULT_QUERY_PARAMS },
    { case: 'account 20 with a week it rejects (UI-36)', url: '/dashboard?account=20&week=2026-03-02&type=all', expectedParams: { account: '20', week: '2026-07-20', type: 'all' } },
    { case: 'wrong-case type ALL (UI-37)', url: '/dashboard?account=14&week=2026-07-20&type=ALL', expectedParams: DEFAULT_QUERY_PARAMS },
    { case: 'only the invalid type, keeping account and week', url: '/dashboard?account=6&week=2026-06-01&type=foo', expectedParams: { account: '6', week: '2026-06-01', type: 'all' } },
    {
      case: 'unrelated params kept (merge)',
      url: '/dashboard?account=abc&week=2026-07-20&type=all&ref=monday-email',
      expectedParams: { ...DEFAULT_QUERY_PARAMS, ref: 'monday-email' },
    },
  ])('rewrites $case with replaceUrl, no error and only valid types sent to the API (UI-38)', async ({ url, expectedParams }) => {
    const { state, activityHealthApi, navigations } = await openState(url);

    expect(currentQueryParams()).toEqual(expectedParams);
    expect(navigationsAfterOpening(navigations).length).toBeGreaterThan(0);
    expect(navigationsAfterOpening(navigations).every((navigation) => navigation.replaceUrl)).toBe(true);
    expect(state.error()).toBeNull();
    expect(activityHealthApi.requests.every((request) => (EVENT_TYPES as readonly string[]).includes(request.eventType))).toBe(true);
  });

  it.each<{ case: string; url: string; expectedRequests: ActivityHealthRequest[] }>([
    { case: '/dashboard', url: '/dashboard', expectedRequests: [{ accountId: 14, week: null, eventType: 'all' }] },
    { case: 'a Tuesday week', url: '/dashboard?account=14&week=2026-07-21&type=all', expectedRequests: [{ accountId: 14, week: null, eventType: 'all' }] },
    {
      case: 'account 999 with a valid week',
      url: '/dashboard?account=999&week=2026-07-20&type=all',
      expectedRequests: [
        { accountId: 999, week: '2026-07-20', eventType: 'all' },
        { accountId: 14, week: '2026-07-20', eventType: 'all' },
      ],
    },
    {
      case: 'account 999 alone',
      url: '/dashboard?account=999',
      expectedRequests: [
        { accountId: 999, week: null, eventType: 'all' },
        { accountId: 14, week: null, eventType: 'all' },
      ],
    },
    {
      case: 'a week before earliestWeek',
      url: '/dashboard?account=14&week=2025-12-29&type=all',
      expectedRequests: [
        { accountId: 14, week: '2025-12-29', eventType: 'all' },
        { accountId: 14, week: null, eventType: 'all' },
      ],
    },
  ])('sends no extra request after normalising $case', async ({ url, expectedRequests }) => {
    const { activityHealthApi, harness } = await openState(url);
    await settle(harness);

    expect(activityHealthApi.requests).toEqual(expectedRequests);
  });

  it('keeps unrelated query params when writing a selection (merge)', async () => {
    const { state, harness } = await openState('/dashboard?account=14&week=2026-07-20&type=all&ref=monday-email');

    state.selectWeek('2026-07-13');
    await settle(harness);

    expect(currentQueryParams()).toEqual({ account: '14', week: '2026-07-13', type: 'all', ref: 'monday-email' });
  });

  it('never writes an empty week= param in any navigation (UI-38b)', async () => {
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

  it('with no week in the URL and a failed first load, keeps account=14&type=all without a week and exposes the error', async () => {
    const { state } = await openState('/dashboard?account=14&type=all', (api) => api.failNext(serverError()));

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
