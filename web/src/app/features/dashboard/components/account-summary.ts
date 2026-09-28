import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { ActivityHealthReport } from '../../../core/models';

@Component({
  selector: 'app-account-summary',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: '',
})
export class AccountSummary {
  readonly report = input.required<ActivityHealthReport>();
}
