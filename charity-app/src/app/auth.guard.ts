import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';
import { AuthService } from './auth.service';

export const authGuard = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  
  if (auth.isLoggedIn()) {
    return true;
  }
  
  return router.parseUrl('/login');
};

export const adminGuard = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (!auth.isLoggedIn()) return router.parseUrl('/login');
  return auth.getCurrentUser()?.role === 'Admin' ? true : router.parseUrl('/home');
};

export const charityManagerGuard = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (!auth.isLoggedIn()) return router.parseUrl('/login');
  return auth.getCurrentUser()?.role === 'CharityManager' ? true : router.parseUrl('/home');
};

export const managerOrAdminGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (!auth.isLoggedIn()) return router.parseUrl('/login');
  return ['Admin', 'CharityManager'].includes(auth.getCurrentUser()?.role) ? true : router.parseUrl('/home');
};

export const permissionGuard = (...permissions: string[]): CanActivateFn => () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (!auth.isLoggedIn()) return router.parseUrl('/login');
  return permissions.some(permission => auth.hasPermission(permission)) ? true : router.parseUrl('/home');
};
