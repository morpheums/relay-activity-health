import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, TestRequest, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { beaconDefaultWeekReport, metroCallsSpikeInBaselineReport } from '../../../testing/activity-health-fixtures';
import { ActivityHealthApi, HttpActivityHealthApi } from './activity-health.api';

function parsedUrl(request: TestRequest): URL {
  return new URL(request.request.urlWithParams, 'http://relay.test');
}

describe('HttpActivityHealthApi', () => {
  let activityHealthApi: ActivityHealthApi;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), { provide: ActivityHealthApi, useClass: HttpActivityHealthApi }],
    });
    activityHealthApi = TestBed.inject(ActivityHealthApi);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('sends GET /api/accounts/{accountId}/activity-health as a relative URL with exactly the week and type params', () => {
    activityHealthApi.getActivityHealth({ accountId: 6, week: '2026-07-20', eventType: 'call_received' }).subscribe();

    const request = httpTesting.expectOne((candidate) => candidate.urlWithParams.startsWith('/api/accounts/6/activity-health?'));
    const searchParams = parsedUrl(request).searchParams;
    expect(request.request.method).toBe('GET');
    expect(parsedUrl(request).pathname).toBe('/api/accounts/6/activity-health');
    expect(searchParams.get('week')).toBe('2026-07-20');
    expect(searchParams.get('type')).toBe('call_received');
    expect(Array.from(searchParams.keys()).sort()).toEqual(['type', 'week']);
    request.flush(metroCallsSpikeInBaselineReport());
  });

  it('omits the week param entirely when week is null so the API picks the latest complete week', () => {
    activityHealthApi.getActivityHealth({ accountId: 14, week: null, eventType: 'all' }).subscribe();

    const request = httpTesting.expectOne((candidate) => candidate.urlWithParams.startsWith('/api/accounts/14/activity-health'));
    expect(request.request.urlWithParams).not.toContain('week');
    expect(parsedUrl(request).searchParams.get('type')).toBe('all');
    request.flush(beaconDefaultWeekReport());
  });
});
