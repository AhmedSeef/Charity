import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter, Router } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { AuthService } from './auth.service';
import { routes } from './app.routes';

@Component({ standalone: true, template: 'test page' })
class TestPage {}

describe('Direct URL authorization', () => {
  let role: string;
  let loggedIn: boolean;
  let permissions: string[];
  beforeEach(() => {
    role = 'Staff'; loggedIn = true; permissions = [];
    TestBed.configureTestingModule({ providers: [
      provideRouter(routes.map(route => route.component ? { ...route, component: TestPage } : route)),
      { provide: AuthService, useValue: {
        isLoggedIn: () => loggedIn,
        getCurrentUser: () => ({ role }),
        hasPermission: (permission: string) => ['Admin', 'CharityManager'].includes(role) || permissions.includes(permission)
      } }
    ] });
  });

  it('redirects every protected direct URL to home for staff without permissions', async () => {
    const harness = await RouterTestingHarness.create();
    for (const route of routes.filter(route => route.component && !['home', 'login'].includes(route.path!))) {
      await harness.navigateByUrl('/' + route.path);
      expect(TestBed.inject(Router).url, route.path).toBe('/home');
    }
  });

  it('allows only the delegated page and blocks its creation page', async () => {
    permissions = ['inventory.view'];
    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl('/inventory');
    expect(TestBed.inject(Router).url).toBe('/inventory');
    await harness.navigateByUrl('/items-catalog/new');
    expect(TestBed.inject(Router).url).toBe('/home');
  });

  it('allows charity management but keeps platform dashboard restricted', async () => {
    role = 'CharityManager';
    const harness = await RouterTestingHarness.create();
    for (const path of ['/settings', '/suspension-reasons', '/separation-reasons', '/audit-logs']) {
      await harness.navigateByUrl(path);
      expect(TestBed.inject(Router).url).toBe(path);
    }
    await harness.navigateByUrl('/dashboard');
    expect(TestBed.inject(Router).url).toBe('/home');
  });

  it('redirects an unauthenticated direct link to login', async () => {
    loggedIn = false;
    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl('/settings');
    expect(TestBed.inject(Router).url).toBe('/login');
  });
});
