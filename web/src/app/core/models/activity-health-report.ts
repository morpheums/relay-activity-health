import { Account } from './account';
import { AccountHealthSummary } from './account-health-summary';
import { EventType } from './event-type';
import { LocationHealth } from './location-health';
import { WeekRange } from './week-range';

export interface ActivityHealthReport {
  account: Account;
  eventType: EventType;
  week: WeekRange;
  dataAsOf: string;
  latestCompleteWeek: string;
  earliestWeek: string;
  baselineWeeks: number;
  minimumEligibleWeeks: number;
  summary: AccountHealthSummary;
  locations: LocationHealth[];
}
