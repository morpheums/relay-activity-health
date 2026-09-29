import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { WeekRange } from '../../../core/models';

@Component({
  selector: 'app-week-picker',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: '',
})
export class WeekPicker {
  readonly week = input.required<WeekRange>();
  readonly earliestWeek = input.required<string>();
  readonly latestCompleteWeek = input.required<string>();

  readonly weekSelected = output<string>();
}
