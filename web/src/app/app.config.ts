import { provideHttpClient } from '@angular/common/http';
import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { MAT_DATE_LOCALE, MatDateFormats } from '@angular/material/core';
import { MAT_DATE_FNS_FORMATS, provideDateFnsAdapter } from '@angular/material-date-fns-adapter';
import { provideRouter } from '@angular/router';
import { enUS } from 'date-fns/locale';
import { AccountsApi, HttpAccountsApi } from './core/api/accounts.api';
import { ActivityHealthApi, HttpActivityHealthApi } from './core/api/activity-health.api';
import { routes } from './app.routes';

const dateFormatsWithFullMonthLabel: MatDateFormats = {
  ...MAT_DATE_FNS_FORMATS,
  display: { ...MAT_DATE_FNS_FORMATS.display, monthYearLabel: 'LLLL yyyy' },
};

const enUSWithMondayWeekStart = { ...enUS, options: { ...enUS.options, weekStartsOn: 1 as const } };

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(),
    provideDateFnsAdapter(dateFormatsWithFullMonthLabel),
    { provide: MAT_DATE_LOCALE, useValue: enUSWithMondayWeekStart },
    { provide: ActivityHealthApi, useClass: HttpActivityHealthApi },
    { provide: AccountsApi, useClass: HttpAccountsApi },
  ],
};
