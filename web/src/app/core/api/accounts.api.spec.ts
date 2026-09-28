import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Account } from '../models';
import { seedAccounts } from '../../../testing/activity-health-fixtures';
import { AccountsApi, HttpAccountsApi } from './accounts.api';

describe('HttpAccountsApi', () => {
  let accountsApi: AccountsApi;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), { provide: AccountsApi, useClass: HttpAccountsApi }],
    });
    accountsApi = TestBed.inject(AccountsApi);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('sends GET /api/accounts as a relative URL without query params', () => {
    accountsApi.listAccounts().subscribe();

    const request = httpTesting.expectOne((candidate) => candidate.urlWithParams === '/api/accounts');
    expect(request.request.method).toBe('GET');
    request.flush([]);
  });

  it('emits the accounts from the response body unchanged, including account 20', () => {
    const accountsInResponse = seedAccounts();
    let receivedAccounts: Account[] | undefined;

    accountsApi.listAccounts().subscribe((accounts) => (receivedAccounts = accounts));
    httpTesting.expectOne('/api/accounts').flush(accountsInResponse);

    expect(receivedAccounts).toEqual(accountsInResponse);
    expect(receivedAccounts?.some((account) => account.id === 20 && account.name === 'Quiet Harbor Spa')).toBe(true);
  });
});
