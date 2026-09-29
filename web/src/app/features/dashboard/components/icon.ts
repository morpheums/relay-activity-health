import { ChangeDetectionStrategy, Component, input } from '@angular/core';

export type IconName =
  | 'calendar'
  | 'chevron-down'
  | 'chevron-up'
  | 'within-range'
  | 'not-enough-history'
  | 'alert'
  | 'retry'
  | 'empty-inbox'
  | 'loading'
  | 'history'
  | 'copies'
  | 'info'
  | 'clock';

@Component({
  selector: 'app-icon',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { 'aria-hidden': 'true' },
  template: `
    <svg
      [attr.width]="size()"
      [attr.height]="size()"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      [attr.stroke-width]="strokeWidth()"
      stroke-linecap="round"
      stroke-linejoin="round"
      focusable="false"
    >
      @switch (name()) {
        @case ('calendar') {
          <rect x="3.5" y="5" width="17" height="15.5" rx="2" />
          <path d="M3.5 10h17M8 3v4M16 3v4" />
        }
        @case ('chevron-down') {
          <path d="M6 9l6 6 6-6" />
        }
        @case ('chevron-up') {
          <path d="M18 15l-6-6-6 6" />
        }
        @case ('within-range') {
          <circle cx="12" cy="12" r="8.5" />
          <path d="M8.5 12.2l2.4 2.4 4.6-4.9" />
        }
        @case ('not-enough-history') {
          <circle cx="12" cy="12" r="8.5" stroke-dasharray="3 3" />
          <path d="M12 8v4l2.5 1.5" />
        }
        @case ('alert') {
          <circle cx="12" cy="12" r="8.5" />
          <path d="M12 7.5v5.5M12 16.5v.01" />
        }
        @case ('retry') {
          <path d="M20 11a8 8 0 1 0-2.3 5.7" />
          <path d="M20 4v7h-7" />
        }
        @case ('empty-inbox') {
          <path d="M3.5 13.5l2.6-7.1A2 2 0 0 1 8 5h8a2 2 0 0 1 1.9 1.4l2.6 7.1" />
          <path d="M3.5 13.5V18a1.5 1.5 0 0 0 1.5 1.5h14a1.5 1.5 0 0 0 1.5-1.5v-4.5h-5l-1.5 2h-4l-1.5-2z" />
        }
        @case ('loading') {
          <circle cx="12" cy="12" r="8.5" stroke="var(--color-line)" />
          <path d="M12 3.5a8.5 8.5 0 0 1 8.5 8.5" />
        }
        @case ('history') {
          <path d="M4 12a8 8 0 1 0 2.3-5.7L4 8.5" />
          <path d="M4 4v4.5h4.5" />
          <path d="M12 8v4l2.5 1.5" />
        }
        @case ('copies') {
          <rect x="9" y="9" width="11" height="11" rx="2" />
          <path d="M15 9V6a2 2 0 0 0-2-2H6a2 2 0 0 0-2 2v7a2 2 0 0 0 2 2h3" />
        }
        @case ('info') {
          <circle cx="12" cy="12" r="8.5" />
          <path d="M12 11v5.5M12 7.5v.01" />
        }
        @case ('clock') {
          <circle cx="12" cy="12" r="8.5" />
          <path d="M12 7.5V12l3 2" />
        }
      }
    </svg>
  `,
  styles: `
    :host { display: inline-flex; flex-shrink: 0; }
  `,
})
export class Icon {
  readonly name = input.required<IconName>();
  readonly size = input(16);
  readonly strokeWidth = input(1.75);
}
