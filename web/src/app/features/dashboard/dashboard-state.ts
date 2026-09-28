import { Injectable, Signal, computed } from '@angular/core';
import { Account, ActivityHealthReport, EventType, ProblemDetails } from '../../core/models';

export const DEFAULT_ACCOUNT_ID = 14;
export const DEFAULT_EVENT_TYPE: EventType = 'all';

const notImplemented = (): never => {
  throw new Error('Not implemented');
};

@Injectable()
export class DashboardState {
  readonly accountId: Signal<number> = computed(notImplemented);
  readonly week: Signal<string | null> = computed(notImplemented);
  readonly eventType: Signal<EventType> = computed(notImplemented);

  readonly accounts: Signal<readonly Account[]> = computed(notImplemented);
  readonly report: Signal<ActivityHealthReport | undefined> = computed(notImplemented);
  readonly isLoading: Signal<boolean> = computed(notImplemented);
  readonly error: Signal<ProblemDetails | null> = computed(notImplemented);

  selectAccount(accountId: number): void {
    notImplemented();
  }

  selectWeek(week: string): void {
    notImplemented();
  }

  selectEventType(eventType: EventType): void {
    notImplemented();
  }

  reload(): void {
    notImplemented();
  }
}
