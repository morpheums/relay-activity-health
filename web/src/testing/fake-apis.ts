import { HttpErrorResponse } from '@angular/common/http';
import { Observable, Subject, from, switchMap, throwError, of } from 'rxjs';
import { AccountsApi } from '../app/core/api/accounts.api';
import { ActivityHealthApi, ActivityHealthRequest } from '../app/core/api/activity-health.api';
import { Account, ActivityHealthReport, EVENT_TYPES, ProblemDetails } from '../app/core/models';
import { EARLIEST_WEEK_BY_ACCOUNT_ID, LATEST_COMPLETE_WEEK, genericReport, scenarioReports, seedAccounts } from './activity-health-fixtures';

const MONDAY_PATTERN = /^\d{4}-\d{2}-\d{2}$/;

function isMonday(week: string): boolean {
  if (!MONDAY_PATTERN.test(week)) {
    return false;
  }
  const parsed = new Date(`${week}T00:00:00Z`);
  return !Number.isNaN(parsed.getTime()) && parsed.toISOString().slice(0, 10) === week && parsed.getUTCDay() === 1;
}

function reportKey(accountId: number, weekStart: string, eventType: string): string {
  return `${accountId}|${weekStart}|${eventType}`;
}

function afterMicrotask<T>(outcome: () => Observable<T>): Observable<T> {
  return from(Promise.resolve()).pipe(switchMap(outcome));
}

export function validationProblem(field: string, message: string): HttpErrorResponse {
  const problem: ProblemDetails = {
    type: 'https://tools.ietf.org/html/rfc9110#section-15.5.1',
    title: 'One or more validation errors occurred.',
    status: 400,
    errors: { [field]: [message] },
  };
  return new HttpErrorResponse({ status: 400, statusText: 'Bad Request', error: problem });
}

export function notFoundProblem(): HttpErrorResponse {
  const problem: ProblemDetails = { title: 'Not Found', status: 404 };
  return new HttpErrorResponse({ status: 404, statusText: 'Not Found', error: problem });
}

export function serverError(): HttpErrorResponse {
  const problem: ProblemDetails = { title: 'An error occurred while processing your request.', status: 500 };
  return new HttpErrorResponse({ status: 500, statusText: 'Internal Server Error', error: problem });
}

export function networkFailure(): HttpErrorResponse {
  return new HttpErrorResponse({ status: 0, statusText: 'Unknown Error' });
}

export class FakeAccountsApi extends AccountsApi {
  listAccountsCalls = 0;
  readonly accounts: Account[] = seedAccounts();

  listAccounts(): Observable<Account[]> {
    this.listAccountsCalls += 1;
    return afterMicrotask(() => of(this.accounts.map((account) => ({ ...account }))));
  }
}

export class FakeActivityHealthApi extends ActivityHealthApi {
  readonly requests: ActivityHealthRequest[] = [];
  private readonly reportsByKey = new Map<string, ActivityHealthReport>();
  private readonly queuedFailures: HttpErrorResponse[] = [];
  private readonly heldResponses: { request: ActivityHealthRequest; response: Subject<ActivityHealthReport> }[] = [];
  private holdNextResponse = false;

  constructor(private readonly accounts: readonly Account[] = seedAccounts()) {
    super();
    scenarioReports().forEach((report) => this.register(report));
  }

  register(report: ActivityHealthReport): this {
    this.reportsByKey.set(reportKey(report.account.id, report.week.start, report.eventType), report);
    return this;
  }

  failNext(failure: HttpErrorResponse): this {
    this.queuedFailures.push(failure);
    return this;
  }

  holdNext(): this {
    this.holdNextResponse = true;
    return this;
  }

  releaseHeld(): void {
    const held = this.heldResponses.shift();
    if (!held) {
      throw new Error('No held activity-health request to release');
    }
    held.response.next(this.resolve(held.request));
    held.response.complete();
  }

  getActivityHealth(request: ActivityHealthRequest): Observable<ActivityHealthReport> {
    this.requests.push({ ...request });
    const queuedFailure = this.queuedFailures.shift();
    if (queuedFailure) {
      return afterMicrotask(() => throwError(() => queuedFailure));
    }
    if (this.holdNextResponse) {
      this.holdNextResponse = false;
      const response = new Subject<ActivityHealthReport>();
      this.heldResponses.push({ request: { ...request }, response });
      return response.asObservable();
    }
    return afterMicrotask(() => {
      const rejection = this.rejectionFor(request);
      return rejection ? throwError(() => rejection) : of(this.resolve(request));
    });
  }

  private rejectionFor(request: ActivityHealthRequest): HttpErrorResponse | null {
    const earliestWeek = EARLIEST_WEEK_BY_ACCOUNT_ID.get(request.accountId);
    if (!earliestWeek || !this.accounts.some((account) => account.id === request.accountId)) {
      return notFoundProblem();
    }
    if (!(EVENT_TYPES as readonly string[]).includes(request.eventType)) {
      return validationProblem('Type', 'The type field is invalid.');
    }
    if (request.week === null) {
      return null;
    }
    if (!isMonday(request.week)) {
      return validationProblem('Week', 'The week must be a Monday in yyyy-MM-dd format.');
    }
    if (request.week < earliestWeek || request.week > LATEST_COMPLETE_WEEK) {
      return validationProblem('Week', 'The week is outside the available range.');
    }
    return null;
  }

  private resolve(request: ActivityHealthRequest): ActivityHealthReport {
    const weekStart = request.week ?? LATEST_COMPLETE_WEEK;
    const registered = this.reportsByKey.get(reportKey(request.accountId, weekStart, request.eventType));
    if (registered) {
      return structuredClone(registered);
    }
    const account = this.accounts.find((candidate) => candidate.id === request.accountId);
    if (!account) {
      throw new Error(`Fake has no account ${request.accountId}`);
    }
    return genericReport(account, weekStart, request.eventType);
  }
}
