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

    <div class="week-stepper" role="group" aria-label="Week">
      <button type="button" [disabled]="!canGoToPreviousWeek()" (click)="stepWeek(-1)">◀ Previous week</button>
      <span class="week-label" aria-live="polite">{{ weekLabel() }}</span>
      <button type="button" [disabled]="!canGoToNextWeek()" (click)="stepWeek(1)">Next week ▶</button>
    </div>

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
  readonly week = input.required<WeekRange>();
  readonly earliestWeek = input.required<string>();
  readonly latestCompleteWeek = input.required<string>();
  readonly eventType = input.required<EventType>();

  readonly accountSelected = output<number>();
  readonly weekSelected = output<string>();
  readonly eventTypeSelected = output<EventType>();

  protected readonly eventTypeOptions = EVENT_TYPES.map((value) => ({ value, label: EVENT_TYPE_LABELS[value] }));
  protected readonly weekLabel = computed(() => formatWeekRange(this.week().start, this.week().end));
  protected readonly canGoToPreviousWeek = computed(() => this.week().start > this.earliestWeek());
  protected readonly canGoToNextWeek = computed(() => this.week().start < this.latestCompleteWeek());

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

  protected stepWeek(weekCount: number): void {
    this.weekSelected.emit(addWeeks(this.week().start, weekCount));
  }
}
