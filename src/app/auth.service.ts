import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  constructor(private http: HttpClient, private router: Router) {}

  login(user: string, pass: string): Observable<any> {
    return this.http.get(`http://localhost:3000/users?username=${user}&password=${pass}`).pipe(
      tap((users: any) => {
        if (users && users.length > 0) {
          localStorage.setItem('currentUser', JSON.stringify(users[0]));
          // Hard reload or simple navigate
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
