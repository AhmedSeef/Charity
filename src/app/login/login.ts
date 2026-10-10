import { Component, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div dir="rtl" style="display: flex; justify-content: center; align-items: center; height: 100vh; background-color: #004d40;">
      <div style="background-color: white; padding: 40px; border-radius: 10px; box-shadow: 0 4px 15px rgba(0,0,0,0.2); width: 350px; text-align: center;">
        <h2 style="color: #004d40; margin-bottom: 20px;">تسجيل الدخول</h2>
        <div style="background-color: #e0f2f1; color: #00695c; padding: 10px; border-radius: 5px; margin-bottom: 20px; font-size: 14px;">
          للدخول المبدئي:<br>اسم المستخدم: <strong>admin</strong><br>كلمة المرور: <strong>admin</strong>
        </div>
        
        <form (ngSubmit)="onLogin()" style="display: flex; flex-direction: column; gap: 15px;">
          <input type="text" [(ngModel)]="username" name="username" placeholder="اسم المستخدم" required style="padding: 12px; border: 1px solid #ccc; border-radius: 5px;">
          <input type="password" [(ngModel)]="password" name="password" placeholder="كلمة المرور" required style="padding: 12px; border: 1px solid #ccc; border-radius: 5px;">
          
          <div *ngIf="errorMsg" style="color: red; font-size: 14px;">{{ errorMsg }}</div>
          
          <button type="submit" style="padding: 12px; background-color: #004d40; color: white; border: none; border-radius: 5px; cursor: pointer; font-size: 16px; font-weight: bold; margin-top: 10px;">
            دخول للنظام
          </button>
        </form>
      </div>
    </div>
  `
})
export class LoginComponent {
  username = '';
  password = '';
  errorMsg = '';

  constructor(private auth: AuthService, private router: Router, private cdr: ChangeDetectorRef) {}

  onLogin() {
    this.auth.login(this.username, this.password).subscribe((users: any) => {
      if (users && users.length > 0) {
        this.router.navigate(['/dashboard']).then(() => window.location.reload());
      } else {
        this.errorMsg = 'اسم المستخدم أو كلمة المرور غير صحيحة';
        this.cdr.detectChanges();
      }
    });
  }
}
