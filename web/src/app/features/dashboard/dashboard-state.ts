import { HttpErrorResponse } from '@angular/common/http';
import { Injectable, Signal, computed, effect, inject, signal } from '@angular/core';
import { rxResource, toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { Observable, catchError, of, tap, throwError } from 'rxjs';
import { AccountsApi } from '../../core/api/accounts.api';
import { ActivityHealthApi, ActivityHealthRequest } from '../../core/api/activity-health.api';
import { Account, ActivityHealthReport, EVENT_TYPES, EventType, ProblemDetails } from '../../core/models';
import { isIsoMonday } from './week';

export const DEFAULT_ACCOUNT_ID = 14;
export const DEFAULT_EVENT_TYPE: EventType = 'all';

const ACCOUNT_ID_PATTERN = /^[1-9]\d*$/;

interface ServerResolution {
  requested: ActivityHealthRequest;
  shown: ActivityHealthRequest;
}

function isEventType(candidate: string | null): candidate is EventType {
  return (EVENT_TYPES as readonly (string | null)[]).includes(candidate);
}

function sameRequest(first: ActivityHealthRequest, second: ActivityHealthRequest): boolean {
  return first.accountId === second.accountId && first.week === second.week && first.eventType === second.eventType;
}

function requestShownBy(report: ActivityHealthReport): ActivityHealthRequest {
  return { accountId: report.account.id, week: report.week.start, eventType: report.eventType };
}

function fallbackRequestFor(request: ActivityHealthRequest, failure: unknown): ActivityHealthRequest | null {
  if (!(failure instanceof HttpErrorResponse)) {
    return null;
  }
  if (failure.status === 404 && request.accountId !== DEFAULT_ACCOUNT_ID) {
    return { ...request, accountId: DEFAULT_ACCOUNT_ID };
  }
  if (failure.status === 400 && request.week !== null) {
    return { ...request, week: null };
  }
  return null;
}

function problemDetailsOf(failure: unknown): ProblemDetails {
  if (failure instanceof HttpErrorResponse) {
    const body: unknown = failure.error;
    if (typeof body === 'object' && body !== null && 'title' in body) {
      return body as ProblemDetails;
    }
    return { title: failure.statusText || failure.message, status: failure.status };
  }
  return { title: failure instanceof Error ? failure.message : 'Unknown error' };
}

@Injectable()
export class DashboardState {
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly activityHealthApi = inject(ActivityHealthApi);
  private readonly accountsApi = inject(AccountsApi);

  private readonly queryParamMap = toSignal(this.route.queryParamMap, { requireSync: true });
  private readonly serverResolution = signal<ServerResolution | null>(null);

  readonly accountId: Signal<number> = computed(() => {
    const rawAccountId = this.queryParamMap().get('account');
    return rawAccountId !== null && ACCOUNT_ID_PATTERN.test(rawAccountId) ? Number(rawAccountId) : DEFAULT_ACCOUNT_ID;
  });

  readonly week: Signal<string | null> = computed(() => {
    const rawWeek = this.queryParamMap().get('week');
    return isIsoMonday(rawWeek) ? rawWeek : null;
  });

  readonly eventType: Signal<EventType> = computed(() => {
    const rawEventType = this.queryParamMap().get('type');
    return isEventType(rawEventType) ? rawEventType : DEFAULT_EVENT_TYPE;
  });

  readonly accounts: Signal<readonly Account[]> = toSignal(this.accountsApi.listAccounts().pipe(catchError(() => of<Account[]>([]))), {
    initialValue: [],
  });

  private readonly reportRequest = computed<ActivityHealthRequest>(
    () => ({ accountId: this.accountId(), week: this.week(), eventType: this.eventType() }),
    { equal: (previous, next) => sameRequest(previous, next) || this.isShownByServerResolution(previous, next) },
  );

  private readonly reportResource = rxResource({
    params: () => this.reportRequest(),
    stream: ({ params }) => this.fetchReport(params, params),
  });

  readonly report: Signal<ActivityHealthReport | undefined> = computed(() =>
    this.reportResource.hasValue() ? this.reportResource.value() : undefined,
  );

  readonly isLoading: Signal<boolean> = computed(() => this.reportResource.isLoading());

  readonly error: Signal<ProblemDetails | null> = computed(() => {
    const failure = this.reportResource.error();
    return failure === undefined || this.reportResource.isLoading() ? null : problemDetailsOf(failure.cause ?? failure);
  });

  constructor() {
    effect(() => this.rewriteInvalidParams());
    effect(() => this.rewriteUrlToShownReport());
  }

  selectAccount(accountId: number): void {
    this.writeQueryParams({ account: accountId }, false);
  }

  selectWeek(week: string): void {
    this.writeQueryParams({ week }, false);
  }

  selectEventType(eventType: EventType): void {
    this.writeQueryParams({ type: eventType }, false);
  }

  reload(): void {
    this.reportResource.reload();
  }

  private fetchReport(requested: ActivityHealthRequest, attempt: ActivityHealthRequest): Observable<ActivityHealthReport> {
    return this.activityHealthApi.getActivityHealth(attempt).pipe(
      catchError((failure: unknown) => {
        const fallback = fallbackRequestFor(attempt, failure);
        return fallback ? this.fetchReport(requested, fallback) : throwError(() => failure);
      }),
      tap((report) => this.serverResolution.set({ requested, shown: requestShownBy(report) })),
    );
  }

  private isShownByServerResolution(previous: ActivityHealthRequest, next: ActivityHealthRequest): boolean {
    const resolution = this.serverResolution();
    return resolution !== null && resolution.requested === previous && sameRequest(resolution.shown, next);
  }

  private rewriteInvalidParams(): void {
    const queryParamMap = this.queryParamMap();
    const accountId = this.accountId();
    const week = this.week();
    const eventType = this.eventType();
    const rawWeek = queryParamMap.get('week');
    const hasInvalidParam =
      queryParamMap.get('account') !== String(accountId) || queryParamMap.get('type') !== eventType || (rawWeek !== null && rawWeek !== week);
    if (hasInvalidParam) {
      this.writeQueryParams({ account: accountId, week, type: eventType }, true);
    }
  }

  private rewriteUrlToShownReport(): void {
    const report = this.report();
    if (report === undefined || this.reportResource.status() !== 'resolved') {
      return;
    }
    const shown = requestShownBy(report);
    const urlRequest: ActivityHealthRequest = { accountId: this.accountId(), week: this.week(), eventType: this.eventType() };
    if (!sameRequest(shown, urlRequest)) {
      this.writeQueryParams({ account: shown.accountId, week: shown.week, type: shown.eventType }, true);
    }
  }

  private writeQueryParams(queryParams: Params, replaceUrl: boolean): void {
    void this.router.navigate([], { relativeTo: this.route, queryParams, queryParamsHandling: 'merge', replaceUrl });
  }
}
