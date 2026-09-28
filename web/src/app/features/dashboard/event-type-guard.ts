import { EVENT_TYPES, EventType } from '../../core/models';

export function isEventType(candidate: string | null): candidate is EventType {
  return (EVENT_TYPES as readonly (string | null)[]).includes(candidate);
}
