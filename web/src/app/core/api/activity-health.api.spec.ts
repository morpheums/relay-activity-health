import { HttpErrorResponse, provideHttpClient } from '@angular/common/http';
import { HttpTestingController, TestRequest, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ActivityHealthReport, EventType, ProblemDetails } from '../models';
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

  it('sends GET /api/accounts/{accountId}/activity-health as a relative URL', () => {
    activityHealthApi.getActivityHealth({ accountId: 6, week: '2026-07-20', eventType: 'call_received' }).subscribe();

    const request = httpTesting.expectOne((candidate) => new URL(candidate.urlWithParams, 'http://relay.test').pathname === '/api/accounts/6/activity-health');
    expect(request.request.method).toBe('GET');
    expect(request.request.urlWithParams.startsWith('/api/accounts/6/activity-health?')).toBe(true);
    request.flush(metroCallsSpikeInBaselineReport());
  });

  it('passes week and type as query params', () => {
    activityHealthApi.getActivityHealth({ accountId: 6, week: '2026-07-20', eventType: 'call_received' }).subscribe();

    const request = httpTesting.expectOne((candidate) => candidate.urlWithParams.startsWith('/api/accounts/6/activity-health'));
    const searchParams = parsedUrl(request).searchParams;
    expect(searchParams.get('week')).toBe('2026-07-20');
    expect(searchParams.get('type')).toBe('call_received');
    expect(Array.from(searchParams.keys()).sort()).toEqual(['type', 'week']);
    request.flush(metroCallsSpikeInBaselineReport());
  });

  it('omits the week param entirely when week is null so the API picks the latest complete week', () => {
    activityHealthApi.getActivityHealth({ accountId: 14, week: null, eventType: 'all' }).subscribe();

    const request = httpTesting.expectOne((candidate) => candidate.urlWithParams.startsWith('/api/accounts/14/activity-health'));
    const searchParams = parsedUrl(request).searchParams;
    expect(searchParams.has('week')).toBe(false);
    expect(request.request.urlWithParams).not.toContain('week=');
    expect(searchParams.get('type')).toBe('all');
    request.flush(beaconDefaultWeekReport());
  });

  it.each<EventType>(['all', 'call_received', 'lead_created', 'appointment_set'])('sends type=%s exactly as given', (eventType) => {
    activityHealthApi.getActivityHealth({ accountId: 14, week: '2026-07-20', eventType }).subscribe();

    const request = httpTesting.expectOne((candidate) => candidate.urlWithParams.startsWith('/api/accounts/14/activity-health'));
    expect(parsedUrl(request).searchParams.get('type')).toBe(eventType);
    request.flush(beaconDefaultWeekReport());
  });

  it('emits the report body unchanged', () => {
    const reportInResponse = beaconDefaultWeekReport();
    let receivedReport: ActivityHealthReport | undefined;

    activityHealthApi.getActivityHealth({ accountId: 14, week: '2026-07-20', eventType: 'all' }).subscribe((report) => (receivedReport = report));
    httpTesting.expectOne((candidate) => candidate.urlWithParams.startsWith('/api/accounts/14/activity-health')).flush(reportInResponse);

    expect(receivedReport).toEqual(reportInResponse);
  });

  it('surfaces a 400 ProblemDetails as an HttpErrorResponse with status 400 and the problem body', () => {
    const problem: ProblemDetails = { title: 'One or more validation errors occurred.', status: 400, errors: { Week: ['The week is outside the available range.'] } };
    let receivedError: unknown;

    activityHealthApi
      .getActivityHealth({ accountId: 8, week: '2026-01-26', eventType: 'all' })
      .subscribe({ error: (error: unknown) => (receivedError = error) });
    httpTesting
      .expectOne((candidate) => candidate.urlWithParams.startsWith('/api/accounts/8/activity-health'))
      .flush(problem, { status: 400, statusText: 'Bad Request' });

    expect(receivedError).toBeInstanceOf(HttpErrorResponse);
    const httpError = receivedError as HttpErrorResponse;
    expect(httpError.status).toBe(400);
    expect(httpError.error).toEqual(problem);
  });
});
