import { provideHttpClient } from '@angular/common/http';
import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { AccountsApi, HttpAccountsApi } from './core/api/accounts.api';
import { ActivityHealthApi, HttpActivityHealthApi } from './core/api/activity-health.api';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(),
    { provide: ActivityHealthApi, useClass: HttpActivityHealthApi },
    { provide: AccountsApi, useClass: HttpAccountsApi },
  ],
};
