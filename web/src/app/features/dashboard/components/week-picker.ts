import { CdkTrapFocus } from '@angular/cdk/a11y';
import { CdkConnectedOverlay, CdkOverlayOrigin, ConnectedPosition } from '@angular/cdk/overlay';
import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  Injector,
  afterNextRender,
  computed,
  inject,
  input,
  output,
  signal,
  viewChild,
  ViewEncapsulation,
} from '@angular/core';
import { MatCalendar, MatCalendarCellClassFunction, DateFilterFn } from '@angular/material/datepicker';
import { format, isMonday, isSameWeek, parseISO } from 'date-fns';
import { WeekRange } from '../../../core/models';
import { formatSelectableWeeks, formatWeekRange } from '../week';
import { Icon } from './icon';

const ISO_DATE_FORMAT = 'yyyy-MM-dd';
const SELECTED_WEEK_CLASS = 'week-picker-selected-week';

let nextDialogId = 0;

@Component({
  selector: 'app-week-picker',
  changeDetection: ChangeDetectionStrategy.OnPush,
  encapsulation: ViewEncapsulation.None,
  imports: [CdkTrapFocus, CdkConnectedOverlay, CdkOverlayOrigin, Icon, MatCalendar],
  template: `
    <button
      #trigger
      type="button"
      class="trigger control"
      cdkOverlayOrigin
      #triggerOrigin="cdkOverlayOrigin"
      aria-haspopup="dialog"
      [attr.aria-expanded]="isOpen()"
      [attr.aria-controls]="isOpen() ? dialogId : null"
      [attr.aria-label]="triggerName()"
      [disabled]="isDisabled()"
      (click)="toggle()"
    >
      <span class="trigger-label">
        <app-icon class="calendar-icon" name="calendar" [size]="18" />
        <span aria-live="polite">{{ weekLabel() }}</span>
      </span>
      <app-icon class="chevron-icon" [name]="isOpen() ? 'chevron-up' : 'chevron-down'" />
    </button>

    <ng-template
      cdkConnectedOverlay
      [cdkConnectedOverlayOrigin]="triggerOrigin"
      [cdkConnectedOverlayOpen]="isOpen()"
      [cdkConnectedOverlayPositions]="popoverPositions"
      (attach)="focusSelectedWeek()"
      (detach)="close()"
      (overlayOutsideClick)="closeOnOutsideClick($event)"
    >
      <div class="week-picker-dialog card" role="dialog" aria-label="Choose week" [id]="dialogId" cdkTrapFocus>
        <mat-calendar
          [startAt]="selectedMonday()"
          [selected]="selectedMonday()"
          [minDate]="earliestMonday()"
          [maxDate]="latestCompleteMonday()"
          [dateFilter]="isSelectableMonday"
          [dateClass]="selectedWeekClass"
          (_userSelection)="choose($event.value)"
        />
        <div class="helper">
          <p>Weeks run Monday to Sunday.</p>
          <p>{{ selectableWeeksLabel() }}</p>
        </div>
        <div class="footer">
          <button type="button" class="latest-week control" [disabled]="isLatestWeekSelected()" (click)="chooseLatestWeek()">Latest week</button>
        </div>
      </div>
    </ng-template>
  `,
  styles: `
    app-week-picker { display: block; position: relative; }
    app-week-picker .trigger {
      width: var(--week-picker-trigger-width); height: var(--control-height); padding: 0 14px; display: flex; align-items: center; justify-content: space-between; gap: 10px;
    }
    app-week-picker .trigger[aria-expanded='true'] { border-color: var(--color-ink); box-shadow: inset 0 0 0 1px var(--color-ink); }
    app-week-picker .trigger:disabled { color: var(--color-ink-2); }
    app-week-picker .trigger-label { display: flex; align-items: center; gap: 10px; font-size: 15px; font-weight: 600; font-variant-numeric: tabular-nums; white-space: nowrap; }
    app-week-picker .chevron-icon { color: var(--color-ink-2); }
    app-week-picker .trigger[aria-expanded='true'] .chevron-icon { color: var(--color-ink); }
    app-week-picker .trigger:disabled .calendar-icon { color: var(--color-disabled-ink); }
    app-week-picker .trigger:disabled .chevron-icon { color: var(--color-disabled-chevron); }

    .week-picker-dialog {
      width: var(--week-picker-dialog-width); box-sizing: border-box; padding: 12px 20px 18px;
      box-shadow: var(--popover-shadow);
      font-variant-numeric: tabular-nums; animation: week-picker-open 120ms ease-out;
    }
    .week-picker-dialog .mat-calendar-header { padding: 0; }
    .week-picker-dialog .mat-calendar-controls { margin: 0; }
    .week-picker-dialog .mat-calendar-content { padding: 6px 0 0; }
    .week-picker-dialog .mat-calendar-table-header th:first-child { color: var(--color-ink); font-weight: 600; }
    /* MatCalendarHeader upper-cases monthYearLabel in code, so the case is restored here. */
    .week-picker-dialog .mat-calendar-period-button .mdc-button__label > span { display: inline-block; text-transform: lowercase; }
    .week-picker-dialog .mat-calendar-period-button .mdc-button__label > span::first-letter { text-transform: uppercase; }
    .week-picker-dialog tr:has(> .mat-calendar-body-label[colspan='7']) { display: none; }
    .week-picker-dialog .mat-calendar-body-cell:not(.mat-calendar-body-disabled) > .mat-calendar-body-cell-content:not(.mat-calendar-body-selected) {
      background: var(--color-disabled-fill); font-weight: 600;
    }
    .week-picker-dialog .mat-calendar-body-selected { font-weight: 600; }
    .week-picker-dialog .mat-calendar-body-cell:focus-visible > .mat-calendar-body-cell-content { outline: 2px solid var(--color-ink); outline-offset: -2px; }
    .week-picker-dialog .mat-calendar-body-cell-container:has(.mat-calendar-body-selected) {
      background: linear-gradient(to right, transparent 50%, var(--color-fill-muted) 50%);
    }
    .week-picker-dialog .mat-calendar-body-cell-container:has(.week-picker-selected-week) { background: var(--color-fill-muted); }
    .week-picker-dialog .mat-calendar-body-cell-container:has(.week-picker-selected-week):nth-child(7) {
      background: linear-gradient(to left, transparent 50%, var(--color-fill-muted) 50%);
    }
    .week-picker-dialog .week-picker-selected-week > .mat-calendar-body-cell-content { color: var(--color-ink-2); }
    .week-picker-dialog .mat-calendar-body-cell-container:nth-child(7) .week-picker-selected-week > .mat-calendar-body-cell-content {
      background: var(--color-fill-muted);
    }
    .week-picker-dialog .helper {
      margin-top: 12px; padding-top: 12px; border-top: 1px solid var(--color-line-soft); font-size: 13px; line-height: 18px; color: var(--color-ink-2);
    }
    .week-picker-dialog .helper p { margin: 0; }
    .week-picker-dialog .helper p + p { margin-top: 2px; }
    .week-picker-dialog .footer { margin-top: 14px; display: flex; }
    .week-picker-dialog .latest-week {
      height: var(--control-height); padding: 0 16px; border-radius: var(--radius-control); font-size: 14px; font-weight: 500; white-space: nowrap;
    }
    @keyframes week-picker-open { from { opacity: 0; transform: scale(0.97); } }
    @media (prefers-reduced-motion: reduce) { .week-picker-dialog { animation: none; } }
  `,
})
export class WeekPicker {
  readonly week = input.required<WeekRange>();
  readonly earliestWeek = input.required<string>();
  readonly latestCompleteWeek = input.required<string>();

  readonly weekSelected = output<string>();

  private readonly injector = inject(Injector);
  private readonly trigger = viewChild.required<ElementRef<HTMLButtonElement>>('trigger');
  private readonly calendar = viewChild(MatCalendar);

  protected readonly dialogId = `week-picker-dialog-${nextDialogId++}`;
  protected readonly popoverPositions: ConnectedPosition[] = [
    { originX: 'start', originY: 'bottom', overlayX: 'start', overlayY: 'top', offsetY: 8 },
    { originX: 'start', originY: 'top', overlayX: 'start', overlayY: 'bottom', offsetY: -8 },
  ];

  protected readonly isOpen = signal(false);
  protected readonly isDisabled = computed(() => this.earliestWeek() === this.latestCompleteWeek());
  protected readonly weekLabel = computed(() => formatWeekRange(this.week().start, this.week().end));
  protected readonly triggerName = computed(() => `${this.weekLabel()}, choose week`);
  protected readonly selectableWeeksLabel = computed(() => formatSelectableWeeks(this.earliestWeek(), this.latestCompleteWeek()));
  protected readonly isLatestWeekSelected = computed(() => this.week().start === this.latestCompleteWeek());
  protected readonly selectedMonday = computed(() => parseISO(this.week().start));
  protected readonly earliestMonday = computed(() => parseISO(this.earliestWeek()));
  protected readonly latestCompleteMonday = computed(() => parseISO(this.latestCompleteWeek()));

  protected readonly isSelectableMonday: DateFilterFn<Date> = (day) => day !== null && isMonday(day);

  protected readonly selectedWeekClass: MatCalendarCellClassFunction<Date> = (day, view) =>
    view === 'month' && isSameWeek(day, this.selectedMonday(), { weekStartsOn: 1 }) && !isMonday(day) ? SELECTED_WEEK_CLASS : '';

  protected toggle(): void {
    if (this.isOpen()) {
      this.close();
    } else {
      this.isOpen.set(true);
    }
  }

  protected close(): void {
    if (!this.isOpen()) {
      return;
    }
    this.isOpen.set(false);
    this.trigger().nativeElement.focus();
  }

  protected closeOnOutsideClick(event: MouseEvent): void {
    if (!(event.target instanceof Node && this.trigger().nativeElement.contains(event.target))) {
      this.close();
    }
  }

  protected focusSelectedWeek(): void {
    afterNextRender(() => this.calendar()?.focusActiveCell(), { injector: this.injector });
  }

  protected chooseLatestWeek(): void {
    this.close();
    this.weekSelected.emit(this.latestCompleteWeek());
  }

  protected choose(day: Date | null): void {
    if (day === null) {
      return;
    }
    const chosenWeek = format(day, ISO_DATE_FORMAT);
    this.close();
    if (chosenWeek !== this.week().start) {
      this.weekSelected.emit(chosenWeek);
    }
  }
}
