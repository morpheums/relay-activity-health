import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { ActivityHealthReport } from '../../../core/models';
import { activityNoun, statusIcon, statusLabel, usualRange } from '../health-copy';
import { Icon } from './icon';

@Component({
  selector: 'app-account-summary',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Icon],
  template: `
    <section class="summary card" aria-labelledby="summary-heading">
      <div>
        <h2 id="summary-heading">{{ report().account.name }} — all locations</h2>
        <p class="method">Compared with the last {{ report().baselineWeeks }} full weeks for this account</p>
        <p class="count-line">
          <span class="figure">{{ report().summary.count }}</span> {{ noun() }}@if (range(); as usual) { · usually <span class="range">{{ usual }}</span> a week}
        </p>
      </div>
      <p class="status-badge" [attr.data-status]="report().summary.status">
        @if (icon(); as iconName) {
          <app-icon [name]="iconName" [size]="18" />
        }
        {{ status() }}
      </p>
    </section>
  `,
  styles: `
    :host { display: block; }
    .summary { display: flex; justify-content: space-between; align-items: flex-end; gap: 32px; padding: 32px 40px; }
    h2 { margin: 0; font-size: 20px; line-height: 28px; font-weight: 600; letter-spacing: -0.01em; }
    .method { margin: 4px 0 0; font-size: 14px; line-height: 20px; color: var(--color-ink-2); }
    .count-line { margin: 20px 0 0; font-size: 18px; line-height: 24px; color: var(--color-ink-2); }
    .figure { font-size: 72px; line-height: 72px; font-weight: 600; letter-spacing: -0.035em; color: var(--color-ink); font-variant-numeric: tabular-nums; }
    .range { font-weight: 600; color: var(--color-ink); font-variant-numeric: tabular-nums; }
    .status-badge {
      margin: 0 0 10px; display: inline-flex; align-items: center; gap: 8px; height: 40px; padding: 0 16px;
      border-radius: 999px; font-size: 15px; font-weight: 600; white-space: nowrap;
    }
    .status-badge[data-status='normal'] { background: var(--color-fill-muted); }
    .status-badge[data-status='insufficient_data'] { background: var(--color-surface); border: 1px dashed var(--color-control-border); font-weight: 500; }
    .status-badge[data-status='insufficient_data'] app-icon { color: var(--color-ink-2); }
  `,
})
export class AccountSummary {
  readonly report = input.required<ActivityHealthReport>();

  protected readonly noun = computed(() => activityNoun(this.report().summary.count, this.report().eventType));
  protected readonly range = computed(() => usualRange(this.report().summary));
  protected readonly icon = computed(() => statusIcon(this.report().summary.status));
  protected readonly status = computed(() => statusLabel(this.report().summary, this.report().minimumEligibleWeeks));
}
