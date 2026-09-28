import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { LocationHealth } from '../../../core/models';
import { statusLabel, usualRange } from '../health-copy';

@Component({
  selector: 'app-location-table',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <table>
      <thead>
        <tr>
          <th scope="col">Location</th>
          <th scope="col" class="number">Events</th>
          <th scope="col">Usual range</th>
          <th scope="col">Status</th>
        </tr>
      </thead>
      <tbody>
        @for (row of rows(); track row.location) {
          <tr>
            <th scope="row">{{ row.location }}</th>
            <td class="number">{{ row.count }}</td>
            <td>
              @if (row.range; as usual) {
                Usually {{ usual }} a week
              }
            </td>
            <td [attr.data-status]="row.status">{{ row.statusText }}</td>
          </tr>
        }
      </tbody>
    </table>
  `,
  styles: `
    table { border-collapse: collapse; width: 100%; }
    th, td { text-align: left; padding: 0.4rem 0.75rem; border-bottom: 1px solid #ddd; }
    .number { text-align: right; }
    td[data-status='above'] { color: #9a3412; font-weight: 600; }
    td[data-status='below'] { color: #1e40af; font-weight: 600; }
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
      range: usualRange(location),
      statusText: statusLabel(location, this.minimumEligibleWeeks()),
    })),
  );
}
