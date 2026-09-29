import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { LocationHealth } from '../../../core/models';
import { statusIcon, statusLabel, usualRange } from '../health-copy';
import { Icon } from './icon';

@Component({
  selector: 'app-location-table',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Icon],
  template: `
    <table>
      <caption>Locations — most unusual first</caption>
      <thead>
        <tr>
          <th scope="col" class="location-column">Location</th>
          <th scope="col" class="count-column">Events</th>
          <th scope="col" class="range-column">Usual range</th>
          <th scope="col" class="status-column">Status</th>
        </tr>
      </thead>
      <tbody>
        @for (row of rows(); track row.location) {
          <tr [attr.data-status]="row.status" [class.flagged]="row.isFlagged">
            <th scope="row">{{ row.location }}</th>
            <td class="count-column">{{ row.count }}</td>
            <td class="range-column">
              @if (row.range; as usual) {
                <span class="range-words">Usually</span> {{ usual }} <span class="range-words">a week</span>
              }
            </td>
            <td>
              @if (row.icon; as iconName) {
                <span class="status-text"><app-icon [name]="iconName" />{{ row.statusText }}</span>
              } @else {
                <span class="status-badge" [attr.data-status]="row.status">{{ row.statusText }}</span>
              }
            </td>
          </tr>
        }
      </tbody>
    </table>
  `,
  styles: `
    :host { display: block; background: var(--color-surface); border: 1px solid var(--color-line); border-radius: var(--radius-card); overflow: hidden; }
    table { width: 100%; border-collapse: collapse; table-layout: fixed; }
    caption { text-align: left; padding: 20px 24px 14px; font-size: 15px; line-height: 20px; font-weight: 600; }
    th, td { text-align: left; padding: 0 24px; border-top: 1px solid var(--color-line-soft); }
    thead th { height: 40px; font-size: 13px; font-weight: 500; color: var(--color-ink-3); border-top-color: var(--color-line); }
    .location-column, .status-column { width: 30%; }
    .count-column { width: 12%; text-align: right; font-size: 16px; font-variant-numeric: tabular-nums; }
    .range-column { width: 28%; padding-left: 48px; font-size: 15px; font-variant-numeric: tabular-nums; }
    thead .count-column, thead .range-column { font-size: 13px; }
    tbody th { height: 56px; font-size: 15px; font-weight: 500; }
    tbody tr:first-child > *, tr.flagged + tr:not(.flagged) > *, tr[data-status='below'] > * { border-top-color: var(--color-line); }
    tbody tr[data-status='above'] > * { border-top-color: var(--color-above-border); }
    tr[data-status='above'] { background: var(--color-above-row); }
    tr[data-status='below'] { background: var(--color-below-row); }
    tr.flagged th, tr.flagged .count-column { font-weight: 600; }
    tr[data-status='insufficient_data'] th { height: 64px; }
    tr[data-status='insufficient_data'] td { padding-block: 10px; }
    .range-words { color: var(--color-ink-3); }
    .status-text { display: inline-flex; align-items: flex-start; gap: 8px; font-size: 14px; line-height: 20px; color: var(--color-ink-2); }
    .status-text app-icon { margin-top: 2px; }
    .status-badge { display: inline-flex; align-items: center; height: 28px; padding: 0 12px; border-radius: 999px; font-size: 14px; font-weight: 600; white-space: nowrap; }
  `,
})
export class LocationTable {
  readonly locations = input.required<readonly LocationHealth[]>();
  readonly minimumEligibleWeeks = input.required<number>();

  protected readonly rows = computed(() =>
    this.locations().map((location) => ({
      location: location.location,
      count: location.count,
      status: location.status,
      isFlagged: location.status === 'above' || location.status === 'below',
      range: usualRange(location),
      statusText: statusLabel(location, this.minimumEligibleWeeks()),
      icon: statusIcon(location.status),
    })),
  );
}
