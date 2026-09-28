import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { Account } from '../models';

export abstract class AccountsApi {
  abstract listAccounts(): Observable<Account[]>;
}

@Injectable()
export class HttpAccountsApi extends AccountsApi {
  private readonly http = inject(HttpClient);

  listAccounts(): Observable<Account[]> {
    return this.http.get<Account[]>('/api/accounts');
  }
}
