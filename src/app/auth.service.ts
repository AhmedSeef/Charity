import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { tap } from 'rxjs/operators';
import { Api } from './api';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  constructor(private router: Router, private api: Api) {}

  login(user: string, pass: string): Observable<any> {
    const localDb = localStorage.getItem('charity_db');
    let users = [];
    if (localDb) {
      try {
        const db = JSON.parse(localDb);
        users = db.users || [];
      } catch (e) {
        users = [];
      }
    }

    const matchedUser = users.find((u: any) => u.username === user && u.password === pass);
    
    return of(matchedUser ? [matchedUser] : []).pipe(
      tap((result: any) => {
        if (result && result.length > 0) {
          const userObj = result[0];
          localStorage.setItem('currentUser', JSON.stringify(userObj));
          if (userObj.charityId) {
            this.api.setActiveCharityId(userObj.charityId);
          }
        }
      })
    );
  }

  logout() {
    localStorage.removeItem('currentUser');
    this.router.navigate(['/login']).then(() => window.location.reload());
  }

  isLoggedIn(): boolean {
    return !!localStorage.getItem('currentUser');
  }

  getCurrentUser() {
    const user = localStorage.getItem('currentUser');
    return user ? JSON.parse(user) : null;
  }
}
