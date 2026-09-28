import { EventType, SeriesHealth } from '../../core/models';

const ACTIVITY_NOUNS: Record<EventType, { singular: string; plural: string }> = {
  all: { singular: 'inbound event', plural: 'inbound events' },
  call_received: { singular: 'call', plural: 'calls' },
  lead_created: { singular: 'lead', plural: 'leads' },
  appointment_set: { singular: 'appointment', plural: 'appointments' },
};

export const EVENT_TYPE_LABELS: Record<EventType, string> = {
  all: 'All activity',
  call_received: 'Calls',
  lead_created: 'Leads',
  appointment_set: 'Appointments',
};

export function activityCount(count: number, eventType: EventType): string {
  const nouns = ACTIVITY_NOUNS[eventType];
  return `${count} ${count === 1 ? nouns.singular : nouns.plural}`;
}

export function usualRange(series: SeriesHealth): string | null {
  const { low, high } = series.baseline;
  return series.status === 'insufficient_data' || low === null || high === null ? null : `${low}–${high}`;
}

export function statusLabel(series: SeriesHealth, minimumEligibleWeeks: number): string {
  switch (series.status) {
    case 'above':
      return '▲ Higher than usual';
    case 'below':
      return '▼ Lower than usual';
    case 'normal':
      return 'Within usual range';
    case 'insufficient_data':
      return `Not enough history yet (${series.baseline.weeksUsed} of ${minimumEligibleWeeks} weeks needed)`;
  }
}
