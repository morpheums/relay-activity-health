import { Baseline } from './baseline';
import { HealthStatus } from './health-status';

export interface AccountHealthSummary {
  count: number;
  baseline: Baseline;
  status: HealthStatus;
  deviation: number | null;
}
