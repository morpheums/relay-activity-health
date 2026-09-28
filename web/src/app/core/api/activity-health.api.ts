import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ActivityHealthReport, EventType } from '../models';

export interface ActivityHealthRequest {
  accountId: number;
  week: string | null;
  eventType: EventType;
}

export abstract class ActivityHealthApi {
  abstract getActivityHealth(request: ActivityHealthRequest): Observable<ActivityHealthReport>;
}

@Injectable()
export class HttpActivityHealthApi extends ActivityHealthApi {
  private readonly http = inject(HttpClient);

  getActivityHealth(request: ActivityHealthRequest): Observable<ActivityHealthReport> {
    throw new Error('Not implemented');
  }
}
