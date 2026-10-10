import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, of } from 'rxjs';
import { tap } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  constructor(private router: Router) {}

  login(user: string, pass: string): Observable<any> {
    const localDb = localStorage.getItem('charity_db');
    let users = [];
    if (localDb) {
      const db = JSON.parse(localDb);
      users = db.users || [];
    }

    const matchedUser = users.find((u: any) => u.username === user && u.password === pass);
    
    return of(matchedUser ? [matchedUser] : []).pipe(
      tap((result: any) => {
        if (result && result.length > 0) {
          localStorage.setItem('currentUser', JSON.stringify(result[0]));
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
