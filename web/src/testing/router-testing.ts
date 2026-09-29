import { TestBed } from '@angular/core/testing';
import { NavigationEnd, Params, Router } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';

export interface RecordedNavigation {
  url: string;
  replaceUrl: boolean;
}

export function recordNavigations(router: Router): RecordedNavigation[] {
  const navigations: RecordedNavigation[] = [];
  router.events.subscribe((event) => {
    if (event instanceof NavigationEnd) {
      navigations.push({ url: event.urlAfterRedirects, replaceUrl: router.lastSuccessfulNavigation()?.extras.replaceUrl === true });
    }
  });
  return navigations;
}

export function queryParamsOf(url: string): Params {
  return TestBed.inject(Router).parseUrl(url).queryParams;
}

export function currentQueryParams(): Params {
  const router = TestBed.inject(Router);
  return queryParamsOf(router.url);
}

export async function settle(harness?: RouterTestingHarness): Promise<void> {
  for (let round = 0; round < 12; round++) {
    await new Promise<void>((resolve) => setTimeout(resolve, 0));
    TestBed.tick();
    harness?.detectChanges();
  }
}
