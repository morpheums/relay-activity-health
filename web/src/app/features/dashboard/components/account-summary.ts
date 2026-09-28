import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { ActivityHealthReport } from '../../../core/models';
import { activityCount, statusLabel, usualRange } from '../health-copy';

@Component({
  selector: 'app-account-summary',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <h2>{{ report().account.name }} — all locations</h2>
    <p class="method">Compared with the last {{ report().baselineWeeks }} full weeks for this account</p>
    <p class="count">
      @if (range(); as usual) {
        {{ countText() }} · usually {{ usual }} a week
      } @else {
        {{ countText() }}
      }
    </p>
    <p class="status" [attr.data-status]="report().summary.status">{{ status() }}</p>
  `,
  styles: `
    :host { display: block; }
    .method { color: #555; margin-top: 0; }
    .count { font-size: 1.5rem; margin: 0.5rem 0; }
    .status { font-weight: 600; }
    .status[data-status='above'] { color: #9a3412; }
    .status[data-status='below'] { color: #1e40af; }
  `,
})
export class AccountSummary {
  readonly report = input.required<ActivityHealthReport>();

  protected readonly countText = computed(() => activityCount(this.report().summary.count, this.report().eventType));
  protected readonly range = computed(() => usualRange(this.report().summary));
  protected readonly status = computed(() => statusLabel(this.report().summary, this.report().minimumEligibleWeeks));
}
