import { ChangeDetectionStrategy, Component, computed, inject, linkedSignal } from '@angular/core';
import { ActivityHealthReport, WeekRange } from '../../core/models';
import { AccountSummary } from './components/account-summary';
import { DashboardFilters } from './components/dashboard-filters';
import { Icon } from './components/icon';
import { LocationTable } from './components/location-table';
import { DashboardState } from './dashboard-state';
import { formatCalendarDay, sundayOfWeek } from './week';

@Component({
  selector: 'app-dashboard-page',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [AccountSummary, DashboardFilters, Icon, LocationTable],
  providers: [DashboardState],
  template: `
    <header class="page-header">
      <span class="wordmark">Relay</span>
      <span class="header-divider" aria-hidden="true"></span>
      <span class="area-label">Customer admin</span>
    </header>

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
        <div class="card load-error" role="alert">
          <span class="load-error-icon"><app-icon name="alert" [size]="20" [strokeWidth]="2" /></span>
          <p>We couldn't load this week's activity. Try again.</p>
          <button type="button" class="primary-button" (click)="state.reload()">
            <app-icon name="retry" [strokeWidth]="2" />Try again
          </button>
        </div>
      } @else if (state.report(); as report) {
        @if (isEmptyAccount(report)) {
          <section class="card empty-account">
            <span class="empty-account-icon"><app-icon name="empty-inbox" [size]="24" /></span>
            <p>No activity recorded for this account yet.</p>
          </section>
        } @else {
          <app-account-summary [report]="report" />
          <app-location-table [locations]="report.locations" [minimumEligibleWeeks]="report.minimumEligibleWeeks" />
        }
      } @else {
        <section class="card loading-summary">
          <div class="loading-status">
            <p role="status"><app-icon name="loading" [size]="18" [strokeWidth]="2" />Loading…</p>
            <div class="skeleton-stack" aria-hidden="true">
              <span class="skeleton heading-skeleton"></span>
              <span class="skeleton figure-skeleton"></span>
            </div>
          </div>
          <span class="skeleton badge-skeleton" aria-hidden="true"></span>
        </section>
        <section class="card loading-table" aria-hidden="true">
          <div class="skeleton-caption"><span class="skeleton"></span></div>
          @for (skeletonRow of skeletonRows; track skeletonRow) {
            <div class="skeleton-row"><span class="skeleton"></span><span class="skeleton"></span><span class="skeleton"></span><span class="skeleton"></span></div>
          }
        </section>
      }
    </main>

    <footer class="page-footer" [class.with-footnotes]="footnoteReport()">
      @if (footnoteReport(); as report) {
        <div class="footnotes-heading-row">
          <h2>About these numbers</h2>
          @if (dataAsOfLabel(); as dataAsOf) {
            <p class="data-as-of"><app-icon name="clock" />Data as of {{ dataAsOf }}</p>
          }
        </div>
        <ul class="fact-tiles">
          <li>
            <span class="fact-icon"><app-icon name="history" [size]="20" /></span>
            <span>Compared with the last {{ report.baselineWeeks }} full weeks at this location</span>
          </li>
          <li>
            <span class="fact-icon"><app-icon name="empty-inbox" [size]="20" /></span>
            <span>Inbound events, not unique customers</span>
          </li>
          <li>
            <span class="fact-icon"><app-icon name="copies" [size]="20" /></span>
            <span>Exact duplicates counted once</span>
          </li>
        </ul>
        <div class="keep-in-mind">
          <h3>Keep in mind</h3>
          <ul>
            <li><app-icon name="info" /><span>Locations that usually get 2 or fewer events a week can't show 'lower than usual'</span></li>
            @if (report.eventType !== 'all') {
              <li><app-icon name="info" /><span>Per-type counts at a single location are small; only large changes show up.</span></li>
            }
          </ul>
        </div>
      }
      <p class="footer-base">Relay · Activity health</p>
    </footer>
  `,
  styles: `
    :host { display: flex; flex-direction: column; min-height: 100vh; --page-gutter: max(24px, calc((100% - 1120px) / 2)); }
    .page-header {
      height: 64px; flex-shrink: 0; display: flex; align-items: center; gap: 16px; padding: 0 var(--page-gutter);
      background: var(--color-surface); border-bottom: 1px solid var(--color-line);
    }
    .wordmark { font-size: 18px; font-weight: 600; letter-spacing: -0.02em; }
    .header-divider { width: 1px; height: 20px; background: var(--color-line); }
    .area-label { font-size: 14px; color: var(--color-ink-2); }
    main { flex-grow: 1; padding: 48px var(--page-gutter) 64px; }
    h1 { margin: 0; font-size: 48px; line-height: 52px; font-weight: 600; letter-spacing: -0.025em; }
    app-dashboard-filters { margin-top: 28px; }
    app-account-summary, .card { margin-top: 32px; }
    app-location-table, .loading-table { margin-top: 24px; }
    .load-error { display: flex; align-items: center; gap: 16px; padding: 28px 40px; }
    .load-error p { margin: 0; flex-grow: 1; font-size: 16px; line-height: 24px; font-weight: 500; }
    .load-error-icon, .empty-account-icon { display: flex; align-items: center; justify-content: center; border-radius: 999px; }
    .load-error-icon { width: 40px; height: 40px; background: var(--color-danger-tint); color: var(--color-danger); }
    .primary-button {
      height: var(--control-height); padding: 0 20px; display: flex; align-items: center; gap: 8px; border: none; border-radius: var(--radius-control);
      background: var(--color-ink); color: var(--color-surface); font: inherit; font-size: 15px; font-weight: 600; cursor: pointer;
    }
    .empty-account { padding: 64px 40px; display: flex; flex-direction: column; align-items: center; gap: 16px; }
    .empty-account-icon { width: 48px; height: 48px; background: var(--color-disabled-fill); color: var(--color-ink-2); }
    .empty-account p { margin: 0; font-size: 18px; line-height: 26px; font-weight: 500; }
    .loading-summary { display: flex; justify-content: space-between; align-items: flex-end; gap: 32px; padding: 32px 40px; }
    .loading-status p { margin: 0; display: flex; align-items: center; gap: 10px; font-size: 15px; line-height: 20px; font-weight: 500; color: var(--color-ink-2); }
    .loading-status app-icon { color: var(--color-ink); }
    .skeleton-stack { display: flex; flex-direction: column; gap: 14px; margin-top: 20px; }
    .skeleton { display: block; height: 12px; border-radius: 6px; background: var(--color-fill-muted); animation: shimmer 1.2s ease-in-out infinite; }
    .heading-skeleton { width: 260px; }
    .figure-skeleton { width: 380px; height: 56px; border-radius: 8px; }
    .badge-skeleton { width: 180px; height: 40px; border-radius: 999px; margin-bottom: 10px; }
    .loading-table { overflow: hidden; }
    .skeleton-caption { padding: 24px; }
    .skeleton-caption .skeleton { width: 200px; }
    .skeleton-row { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 24px; align-items: center; height: 56px; padding: 0 24px; border-top: 1px solid var(--color-line-soft); }
    .skeleton-row .skeleton:nth-child(1) { width: 70px; }
    .skeleton-row .skeleton:nth-child(2) { width: 24px; justify-self: end; }
    .skeleton-row .skeleton:nth-child(3) { width: 150px; }
    .skeleton-row .skeleton:nth-child(4) { width: 140px; }
    @keyframes shimmer { 50% { opacity: 0.55; } }
    @media (prefers-reduced-motion: reduce) { .skeleton { animation: none; } }
    .page-footer { flex-shrink: 0; padding: 20px var(--page-gutter) 24px; background: var(--color-surface); border-top: 1px solid var(--color-line); }
    .with-footnotes { padding-top: 40px; }
    .footnotes-heading-row { display: flex; justify-content: space-between; align-items: center; gap: 24px; }
    .footnotes-heading-row h2 { margin: 0; font-size: 15px; line-height: 22px; font-weight: 600; }
    .data-as-of {
      margin: 0; display: inline-flex; align-items: center; gap: 8px; box-sizing: border-box; min-height: 32px; padding: 5px 12px;
      background: var(--color-surface); border: 1px solid var(--color-line); border-radius: 8px;
      font-size: 14px; line-height: 20px; font-weight: 500; color: var(--color-ink); font-variant-numeric: tabular-nums;
    }
    .fact-tiles { margin: 20px 0 0; padding: 0; list-style: none; display: grid; grid-template-columns: repeat(3, minmax(0, 1fr)); gap: 16px; }
    .fact-tiles li {
      display: flex; align-items: center; gap: 14px; min-height: 76px; box-sizing: border-box; padding: 16px 20px;
      background: var(--color-info-tint); border: 1px solid var(--color-info-border); border-radius: var(--radius-card);
      font-size: 14px; line-height: 20px; font-weight: 500;
    }
    .fact-icon {
      flex-shrink: 0; width: 40px; height: 40px; box-sizing: border-box; display: flex; align-items: center; justify-content: center;
      border-radius: 999px; background: var(--color-surface); border: 1px solid var(--color-info-border); color: var(--color-info-icon);
    }
    .keep-in-mind { margin-top: 24px; display: grid; grid-template-columns: 120px minmax(0, 1fr); gap: 24px; align-items: start; }
    .keep-in-mind h3 { margin: 0; font-size: 13px; line-height: 20px; font-weight: 500; color: var(--color-ink-3); }
    .keep-in-mind ul { margin: 0; padding: 0; list-style: none; display: flex; flex-direction: column; gap: 8px; font-size: 14px; line-height: 20px; color: var(--color-ink-2); }
    .keep-in-mind li { display: flex; align-items: flex-start; gap: 10px; }
    .keep-in-mind app-icon { margin-top: 2px; }
    .footer-base { margin: 0; font-size: 13px; line-height: 18px; color: var(--color-ink-3); }
    .with-footnotes .footer-base { margin-top: 32px; padding-top: 16px; border-top: 1px solid var(--color-line-soft); }
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

  protected readonly skeletonRows = [1, 2, 3, 4];

  protected readonly footnoteReport = computed(() => (this.state.error() ? undefined : this.state.report()));

  protected readonly dataAsOfLabel = computed(() => {
    const report = this.state.report();
    return report?.dataAsOf ? formatCalendarDay(report.dataAsOf, report.account.timezone) : null;
  });

  protected isEmptyAccount(report: ActivityHealthReport): boolean {
    return report.locations.length === 0 && report.summary.baseline.weeksUsed === 0;
  }
}
