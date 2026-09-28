import { ChangeDetectionStrategy, Component, computed, inject, linkedSignal } from '@angular/core';
import { ActivityHealthReport, WeekRange } from '../../core/models';
import { AccountSummary } from './components/account-summary';
import { DashboardFilters } from './components/dashboard-filters';
import { LocationTable } from './components/location-table';
import { DashboardState } from './dashboard-state';
import { formatCalendarDay, sundayOfWeek } from './week';

@Component({
  selector: 'app-dashboard-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [AccountSummary, DashboardFilters, LocationTable],
  providers: [DashboardState],
  template: `
    <main>
      <h1>Activity health</h1>

      <app-dashboard-filters
        [accounts]="state.accounts()"
        [accountId]="state.accountId()"
        [week]="stepperWeek()"
        [earliestWeek]="lastLoadedReport()?.earliestWeek ?? null"
        [latestCompleteWeek]="lastLoadedReport()?.latestCompleteWeek ?? null"
        [eventType]="state.eventType()"
        (accountSelected)="state.selectAccount($event)"
        (weekSelected)="state.selectWeek($event)"
        (eventTypeSelected)="state.selectEventType($event)"
      />

      @if (state.error()) {
        <div class="load-error" role="alert">
          <p>We couldn't load this week's activity. Try again.</p>
          <button type="button" (click)="state.reload()">Try again</button>
        </div>
      } @else if (state.report(); as report) {
        @if (isEmptyAccount(report)) {
          <p class="empty-account">No activity recorded for this account yet.</p>
        } @else {
          <app-account-summary [report]="report" />
          <app-location-table [locations]="report.locations" [minimumEligibleWeeks]="report.minimumEligibleWeeks" />
        }

        <footer class="footnote">
          <ul>
            <li>Compared with the last {{ report.baselineWeeks }} full weeks at this location</li>
            <li>Inbound events, not unique customers</li>
            <li>Exact duplicates counted once</li>
            <li>Locations that usually get 2 or fewer events a week can't show 'lower than usual'</li>
            @if (report.eventType !== 'all') {
              <li>Per-type counts at a single location are small; only large changes show up.</li>
            }
            @if (dataAsOfLabel(); as dataAsOf) {
              <li>Data as of {{ dataAsOf }}</li>
            }
          </ul>
        </footer>
      } @else {
        <p class="loading" role="status">Loading…</p>
      }
    </main>
  `,
  styles: `
    main { max-width: 60rem; margin: 0 auto; padding: 1.5rem; font-family: system-ui, sans-serif; }
    app-dashboard-filters { margin-bottom: 1.5rem; }
    app-account-summary { margin-bottom: 1.5rem; }
    .footnote { margin-top: 1.5rem; color: #555; font-size: 0.875rem; }
    .footnote ul { padding-left: 1.25rem; }
    .load-error { display: flex; gap: 1rem; align-items: center; }
  `,
})
export class DashboardPage {
  protected readonly state = inject(DashboardState);

  protected readonly lastLoadedReport = linkedSignal<ActivityHealthReport | undefined, ActivityHealthReport | undefined>({
    source: this.state.report,
    computation: (report, previous) => report ?? previous?.value,
  });

  protected readonly selectedWeekRange = computed<WeekRange | null>(() => {
    const report = this.state.report();
    const week = this.state.week();
    if (report !== undefined) {
      return report.week;
    }
    return week === null ? null : { start: week, end: sundayOfWeek(week) };
  });

  protected readonly stepperWeek = computed<WeekRange | null>(() => {
    const boundsReport = this.lastLoadedReport();
    return boundsReport === undefined ? null : (this.selectedWeekRange() ?? boundsReport.week);
  });

  protected readonly dataAsOfLabel = computed(() => {
    const report = this.state.report();
    return report?.dataAsOf ? formatCalendarDay(report.dataAsOf, report.account.timezone) : null;
  });

  protected isEmptyAccount(report: ActivityHealthReport): boolean {
    return report.locations.length === 0 && report.summary.baseline.weeksUsed === 0;
  }
}
