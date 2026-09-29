import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
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
});
