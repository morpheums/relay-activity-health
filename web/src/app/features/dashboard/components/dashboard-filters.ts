import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { Account, EventType, WeekRange } from '../../../core/models';

@Component({
  selector: 'app-dashboard-filters',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: '',
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
}
