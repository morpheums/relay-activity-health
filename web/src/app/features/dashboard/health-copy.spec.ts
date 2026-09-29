import { EventType, SeriesHealth } from '../../core/models';
import { withRange, withoutEnoughHistory } from '../../../testing/activity-health-fixtures';
import { activityCount, statusLabel, usualRange } from './health-copy';

describe('health copy', () => {
  it.each<{ count: number; eventType: EventType; expected: string }>([
    { count: 26, eventType: 'all', expected: '26 inbound events' },
    { count: 1, eventType: 'all', expected: '1 inbound event' },
    { count: 16, eventType: 'call_received', expected: '16 calls' },
    { count: 1, eventType: 'call_received', expected: '1 call' },
    { count: 8, eventType: 'lead_created', expected: '8 leads' },
    { count: 1, eventType: 'lead_created', expected: '1 lead' },
    { count: 2, eventType: 'appointment_set', expected: '2 appointments' },
    { count: 1, eventType: 'appointment_set', expected: '1 appointment' },
  ])('activityCount($count, $eventType) reads "$expected"', ({ count, eventType, expected }) => {
    expect(activityCount(count, eventType)).toBe(expected);
  });

  it('usualRange prints low and high exactly as given, joined by an en dash', () => {
    expect(usualRange(withRange(26, 27, 25, 26, 'normal', 0))).toBe('25–26');
  });

  it('usualRange is null for insufficient history', () => {
    expect(usualRange(withoutEnoughHistory(8, 3))).toBeNull();
  });

  it.each<{ series: SeriesHealth; expected: string }>([
    { series: withRange(880, 70, 39, 101, 'above', 4), expected: '▲ Higher than usual' },
    { series: withRange(2, 7.5, 3, 12, 'below', -2.16), expected: '▼ Lower than usual' },
    { series: withRange(26, 27, 18, 38, 'normal', -0.19), expected: 'Within usual range' },
    { series: withoutEnoughHistory(8, 3), expected: 'Not enough history yet (3 of 6 weeks needed)' },
  ])('statusLabel for $series.status reads "$expected"', ({ series, expected }) => {
    expect(statusLabel(series, 6)).toBe(expected);
  });
});
