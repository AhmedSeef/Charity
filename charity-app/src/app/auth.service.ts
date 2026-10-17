import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { tap, map, catchError } from 'rxjs/operators';
import { Api } from './api';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = (typeof window !== 'undefined' && window.location.port === '4200')
    ? 'http://localhost:5072/api'
    : '/api';

  constructor(
    private router: Router, 
    private http: HttpClient, 
    private api: Api
  ) {}

  login(user: string, pass: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/auth/login`, { username: user, password: pass }).pipe(
      tap((res: any) => {
        if (res && res.token) {
          localStorage.setItem('token', res.token);
          const userObj = {
            id: res.userId,
            username: res.username,
            fullName: res.fullName,
            role: res.role,
            charityId: res.charityId,
            charityName: res.charityName
          };
          localStorage.setItem('currentUser', JSON.stringify(userObj));
          if (res.charityId) {
            this.api.setActiveCharityId(res.charityId);
          }
        }
      }),
      map((res: any) => res ? [res] : []),
      catchError((err) => {
        console.error('Backend login failed:', err);
        return of([]);
      })
    );
  }

  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('currentUser');
    this.router.navigate(['/login']).then(() => window.location.reload());
  }

  isLoggedIn(): boolean {
    return !!localStorage.getItem('token') && !!localStorage.getItem('currentUser');
  }

  getCurrentUser() {
    const user = localStorage.getItem('currentUser');
    return user ? JSON.parse(user) : null;
  }
}
