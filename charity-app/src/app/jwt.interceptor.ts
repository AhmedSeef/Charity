import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const token = localStorage.getItem('token');
  if (token) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }
  return next(req).pipe(catchError((error: HttpErrorResponse) => {
    if (token && error.status === 401 && !req.url.includes('/auth/login')) {
      localStorage.removeItem('token');
      localStorage.removeItem('currentUser');
      void router.navigateByUrl('/login');
    }
    return throwError(() => error);
  }));
};
