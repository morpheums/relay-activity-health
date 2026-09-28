import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { Account, EVENT_TYPES, EventType, WeekRange } from '../../../core/models';
import { EVENT_TYPE_LABELS } from '../health-copy';
import { addWeeks, formatWeekRange } from '../week';

@Component({
  selector: 'app-dashboard-filters',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <label class="filter">
      Viewing as
      <select (change)="onAccountChange($event)">
        @for (account of accounts(); track account.id) {
          <option [value]="account.id" [selected]="account.id === accountId()">{{ account.name }}</option>
        }
      </select>
    </label>

    @if (weekStepper(); as stepper) {
      <div class="week-stepper" role="group" aria-label="Week">
        <button type="button" [disabled]="!stepper.canGoToPreviousWeek" (click)="stepWeek(stepper.weekStart, -1)">◀ Previous week</button>
        <span class="week-label" aria-live="polite">{{ stepper.weekLabel }}</span>
        <button type="button" [disabled]="!stepper.canGoToNextWeek" (click)="stepWeek(stepper.weekStart, 1)">Next week ▶</button>
      </div>
    }

    <label class="filter">
      Activity type
      <select (change)="onEventTypeChange($event)">
        @for (option of eventTypeOptions; track option.value) {
          <option [value]="option.value" [selected]="option.value === eventType()">{{ option.label }}</option>
        }
      </select>
    </label>
  `,
  styles: `
    :host { display: flex; flex-wrap: wrap; gap: 1rem 2rem; align-items: center; }
    .filter { display: flex; gap: 0.5rem; align-items: center; }
    .week-stepper { display: flex; gap: 0.75rem; align-items: center; }
    .week-label { min-width: 16rem; text-align: center; font-weight: 600; }
  `,
})
export class DashboardFilters {
  readonly accounts = input.required<readonly Account[]>();
  readonly accountId = input.required<number>();
  readonly week = input<WeekRange | null>(null);
  readonly earliestWeek = input<string | null>(null);
  readonly latestCompleteWeek = input<string | null>(null);
  readonly eventType = input.required<EventType>();

  readonly accountSelected = output<number>();
  readonly weekSelected = output<string>();
  readonly eventTypeSelected = output<EventType>();

  protected readonly eventTypeOptions = EVENT_TYPES.map((value) => ({ value, label: EVENT_TYPE_LABELS[value] }));
  protected readonly weekStepper = computed(() => {
    const week = this.week();
    const earliestWeek = this.earliestWeek();
    const latestCompleteWeek = this.latestCompleteWeek();
    if (week === null || earliestWeek === null || latestCompleteWeek === null) {
      return null;
    }
    return {
      weekStart: week.start,
      weekLabel: formatWeekRange(week.start, week.end),
      canGoToPreviousWeek: week.start > earliestWeek,
      canGoToNextWeek: week.start < latestCompleteWeek,
    };
  });

  protected onAccountChange(event: Event): void {
    const selectedAccountId = Number((event.target as HTMLSelectElement).value);
    if (Number.isInteger(selectedAccountId)) {
      this.accountSelected.emit(selectedAccountId);
    }
  }

  protected onEventTypeChange(event: Event): void {
    const selectedValue = (event.target as HTMLSelectElement).value;
    const selectedEventType = EVENT_TYPES.find((eventType) => eventType === selectedValue);
    if (selectedEventType) {
      this.eventTypeSelected.emit(selectedEventType);
    }
  }

  protected stepWeek(weekStart: string, weekCount: number): void {
    this.weekSelected.emit(addWeeks(weekStart, weekCount));
  }
}
