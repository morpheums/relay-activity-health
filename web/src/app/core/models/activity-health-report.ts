import { Account } from './account';
import { EventType } from './event-type';
import { LocationHealth } from './location-health';
import { SeriesHealth } from './series-health';
import { WeekRange } from './week-range';

export interface ActivityHealthReport {
  account: Account;
  eventType: EventType;
  week: WeekRange;
  dataAsOf: string | null;
  latestCompleteWeek: string;
  earliestWeek: string;
  baselineWeeks: number;
  minimumEligibleWeeks: number;
  summary: SeriesHealth;
  locations: LocationHealth[];
}
