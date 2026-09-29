import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { Account, EVENT_TYPES, EventType, WeekRange } from '../../../core/models';
import { isEventType } from '../event-type-guard';
import { EVENT_TYPE_LABELS } from '../health-copy';
import { addWeeks } from '../week';
import { Icon } from './icon';
import { WeekPicker } from './week-picker';

@Component({
  selector: 'app-dashboard-filters',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Icon, WeekPicker],
  template: `
    <div class="field account-field">
      <label class="field-label" for="viewing-as">Viewing as</label>
      <div class="select-control">
        <select id="viewing-as" (change)="onAccountChange($event)">
          @for (account of accounts(); track account.id) {
            <option [value]="account.id" [selected]="account.id === accountId()">{{ account.name }}</option>
          }
        </select>
        <app-icon class="select-chevron" name="chevron-down" [size]="18" />
      </div>
    </div>

    @if (weekStepper(); as stepper) {
      <div class="field week-field" role="group" aria-labelledby="week-control-label">
        <span class="field-label" id="week-control-label">Week</span>
        <div class="week-control">
          <button type="button" class="step previous" [disabled]="!stepper.canGoToPreviousWeek" (click)="stepWeek(stepper.week.start, -1)">
            ◀ Previous week
          </button>
          <app-week-picker
            [week]="stepper.week"
            [earliestWeek]="stepper.earliestWeek"
            [latestCompleteWeek]="stepper.latestCompleteWeek"
            (weekSelected)="weekSelected.emit($event)"
          />
          <button type="button" class="step next" [disabled]="!stepper.canGoToNextWeek" (click)="stepWeek(stepper.week.start, 1)">Next week ▶</button>
        </div>
      </div>
    } @else {
      <div class="field week-field" aria-hidden="true">
        <span class="field-label">Week</span>
        <div class="week-placeholder"><span></span><span></span><span></span></div>
      </div>
    }

    <div class="field type-field">
      <label class="field-label" for="activity-type">Activity type</label>
      <div class="select-control">
        <select id="activity-type" (change)="onEventTypeChange($event)">
          @for (option of eventTypeOptions; track option.value) {
            <option [value]="option.value" [selected]="option.value === eventType()">{{ option.label }}</option>
          }
        </select>
        <app-icon class="select-chevron" name="chevron-down" [size]="18" />
      </div>
    </div>
  `,
  styles: `
    :host { display: flex; gap: 24px; align-items: flex-end; }
    .field { display: flex; flex-direction: column; gap: 6px; }
    .field-label { font-size: 13px; line-height: 18px; font-weight: 500; color: var(--color-ink-2); }
    .account-field { width: var(--account-field-width); }
    .week-field { width: var(--week-field-width); }
    .type-field { width: var(--type-field-width); }
    .select-control { position: relative; }
    select {
      appearance: none; width: 100%; height: 44px; padding: 0 40px 0 14px; font: inherit; font-size: 15px; color: var(--color-ink);
      border: 1px solid var(--color-control-border); border-radius: 8px; background: var(--color-surface); cursor: pointer;
    }
    .select-chevron { position: absolute; right: 14px; top: 13px; pointer-events: none; color: var(--color-ink-2); }
    .week-control { display: flex; height: 44px; }
    .week-control > * { margin-left: -1px; }
    .week-control > :first-child { margin-left: 0; }
    .week-control > :focus-visible, app-week-picker:focus-within { z-index: 1; }
    .step {
      box-sizing: border-box; flex: none; height: 44px; padding: 0 16px; font: inherit; font-size: 14px; font-weight: 500; color: var(--color-ink); white-space: nowrap;
      border: 1px solid var(--color-control-border); background: var(--color-surface); cursor: pointer;
    }
    .previous { width: var(--week-step-previous-width); border-radius: 8px 0 0 8px; }
    .next { width: var(--week-step-next-width); border-radius: 0 8px 8px 0; }
    .step:disabled { background: var(--color-disabled-fill); border-color: var(--color-disabled-border); color: var(--color-disabled-ink); cursor: not-allowed; }
    .week-placeholder {
      box-sizing: border-box; height: 44px; display: flex; align-items: center; gap: 12px; padding: 0 16px;
      border: 1px dashed var(--color-disabled-border); border-radius: 8px; background: var(--color-disabled-fill);
    }
    .week-placeholder span { width: 110px; height: 12px; border-radius: 6px; background: var(--color-line); }
    .week-placeholder span:nth-child(2) { width: 220px; margin-left: 40px; }
    .week-placeholder span:nth-child(3) { width: 90px; margin-left: auto; }
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
      week,
      earliestWeek,
      latestCompleteWeek,
      canGoToPreviousWeek: week.start > earliestWeek,
      canGoToNextWeek: week.start < latestCompleteWeek,
    };
  });

  protected onAccountChange(event: Event): void {
    this.accountSelected.emit(Number((event.target as HTMLSelectElement).value));
  }

  protected onEventTypeChange(event: Event): void {
    const selectedValue = (event.target as HTMLSelectElement).value;
    if (isEventType(selectedValue)) {
      this.eventTypeSelected.emit(selectedValue);
    }
  }

  protected stepWeek(weekStart: string, weekCount: number): void {
    this.weekSelected.emit(addWeeks(weekStart, weekCount));
  }
}
