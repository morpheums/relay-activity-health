import { Baseline } from './baseline';
import { HealthStatus } from './health-status';

export interface LocationHealth {
  location: string;
  count: number;
  baseline: Baseline;
  status: HealthStatus;
  deviation: number | null;
}
