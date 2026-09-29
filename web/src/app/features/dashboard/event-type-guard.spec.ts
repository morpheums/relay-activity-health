import { EVENT_TYPES } from '../../core/models';
import { isEventType } from './event-type-guard';

describe('isEventType', () => {
  it('accepts every EVENT_TYPES value', () => {
    expect(EVENT_TYPES.every((eventType) => isEventType(eventType))).toBe(true);
  });

  it.each(['ALL', 'Call_Received', 'foo', '', null])('rejects %s', (candidate) => {
    expect(isEventType(candidate)).toBe(false);
  });
});
