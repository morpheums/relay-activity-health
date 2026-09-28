const ISO_DATE_PATTERN = /^\d{4}-\d{2}-\d{2}$/;
const MILLISECONDS_PER_DAY = 24 * 60 * 60 * 1000;
const DAYS_PER_WEEK = 7;
const MONDAY = 1;

interface CalendarDayParts {
  weekday: string;
  month: string;
  day: string;
  year: string;
}

function utcMidnightOf(isoDate: string): Date {
  return new Date(`${isoDate}T00:00:00Z`);
}

function isoDateOf(utcMidnight: Date): string {
  return utcMidnight.toISOString().slice(0, 10);
}

export function isIsoMonday(candidate: string | null): candidate is string {
  if (candidate === null || !ISO_DATE_PATTERN.test(candidate)) {
    return false;
  }
  const parsed = utcMidnightOf(candidate);
  return !Number.isNaN(parsed.getTime()) && isoDateOf(parsed) === candidate && parsed.getUTCDay() === MONDAY;
}

export function addWeeks(weekStart: string, weekCount: number): string {
  return isoDateOf(new Date(utcMidnightOf(weekStart).getTime() + weekCount * DAYS_PER_WEEK * MILLISECONDS_PER_DAY));
}

export function sundayOfWeek(weekStart: string): string {
  return isoDateOf(new Date(utcMidnightOf(weekStart).getTime() + (DAYS_PER_WEEK - 1) * MILLISECONDS_PER_DAY));
}

function calendarDayParts(instant: Date, timeZone: string): CalendarDayParts {
  const parts = new Intl.DateTimeFormat('en-US', { weekday: 'short', month: 'short', day: 'numeric', year: 'numeric', timeZone }).formatToParts(instant);
  const partValue = (type: Intl.DateTimeFormatPartTypes): string => parts.find((part) => part.type === type)?.value ?? '';
  return { weekday: partValue('weekday'), month: partValue('month'), day: partValue('day'), year: partValue('year') };
}

function dayWithoutYear(parts: CalendarDayParts): string {
  return `${parts.weekday} ${parts.month} ${parts.day}`;
}

function dayWithYear(parts: CalendarDayParts): string {
  return `${dayWithoutYear(parts)}, ${parts.year}`;
}

export function formatWeekRange(weekStart: string, weekEnd: string): string {
  const startParts = calendarDayParts(utcMidnightOf(weekStart), 'UTC');
  const endParts = calendarDayParts(utcMidnightOf(weekEnd), 'UTC');
  const startLabel = startParts.year === endParts.year ? dayWithoutYear(startParts) : dayWithYear(startParts);
  return `${startLabel} – ${dayWithYear(endParts)}`;
}

export function formatCalendarDay(instant: string, timeZone: string): string | null {
  const parsed = new Date(instant);
  return Number.isNaN(parsed.getTime()) ? null : dayWithYear(calendarDayParts(parsed, timeZone));
}
