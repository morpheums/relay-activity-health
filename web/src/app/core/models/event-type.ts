export const EVENT_TYPES = ['all', 'call_received', 'lead_created', 'appointment_set'] as const;

export type EventType = (typeof EVENT_TYPES)[number];
