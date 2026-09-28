import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { LocationHealth } from '../../../core/models';

@Component({
  selector: 'app-location-table',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: '',
})
export class LocationTable {
  readonly locations = input.required<readonly LocationHealth[]>();
  readonly minimumEligibleWeeks = input.required<number>();
}
