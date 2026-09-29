import { A11yModule } from '@angular/cdk/a11y';
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
} from '@angular/core';
import { MatCalendar, MatCalendarCellClassFunction, DateFilterFn } from '@angular/material/datepicker';
import { format, isMonday, isWithinInterval, parseISO } from 'date-fns';
import { WeekRange } from '../../../core/models';
import { formatSelectableWeeks, formatWeekRange } from '../week';
import { Icon } from './icon';

const ISO_DATE_FORMAT = 'yyyy-MM-dd';
const SELECTED_WEEK_CLASS = 'week-picker-selected-week';

let nextDialogId = 0;

@Component({
  selector: 'app-week-picker',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [A11yModule, CdkConnectedOverlay, CdkOverlayOrigin, Icon, MatCalendar],
  template: `
    <button
      #trigger
      type="button"
      class="trigger"
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
      <div class="week-picker-dialog" role="dialog" aria-label="Choose week" [id]="dialogId" cdkTrapFocus>
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
          <button type="button" class="latest-week" [disabled]="isLatestWeekSelected()" (click)="chooseLatestWeek()">Latest week</button>
        </div>
      </div>
    </ng-template>
  `,
  styles: `
    :host { display: block; position: relative; }
    .trigger {
      width: var(--week-picker-trigger-width); height: 44px; padding: 0 14px; display: flex; align-items: center; justify-content: space-between; gap: 10px;
      border: 1px solid var(--color-control-border); background: var(--color-surface); color: var(--color-ink); cursor: pointer; font: inherit;
    }
    .trigger[aria-expanded='true'] { border-color: var(--color-ink); box-shadow: inset 0 0 0 1px var(--color-ink); }
    .trigger:disabled { background: var(--color-disabled-fill); border-color: var(--color-disabled-border); color: var(--color-ink-2); cursor: not-allowed; }
    .trigger-label { display: flex; align-items: center; gap: 10px; font-size: 15px; font-weight: 600; font-variant-numeric: tabular-nums; white-space: nowrap; }
    .chevron-icon { color: var(--color-ink-2); }
    .trigger[aria-expanded='true'] .chevron-icon { color: var(--color-ink); }
    .trigger:disabled .calendar-icon { color: var(--color-disabled-ink); }
    .trigger:disabled .chevron-icon { color: var(--color-disabled-chevron); }
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
  protected readonly selectedSunday = computed(() => parseISO(this.week().end));
  protected readonly earliestMonday = computed(() => parseISO(this.earliestWeek()));
  protected readonly latestCompleteMonday = computed(() => parseISO(this.latestCompleteWeek()));

  protected readonly isSelectableMonday: DateFilterFn<Date> = (day) =>
    day !== null && isMonday(day) && isWithinInterval(day, { start: this.earliestMonday(), end: this.latestCompleteMonday() });

  protected readonly selectedWeekClass: MatCalendarCellClassFunction<Date> = (day, view) =>
    view === 'month' && !isMonday(day) && isWithinInterval(day, { start: this.selectedMonday(), end: this.selectedSunday() })
      ? SELECTED_WEEK_CLASS
      : '';

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
