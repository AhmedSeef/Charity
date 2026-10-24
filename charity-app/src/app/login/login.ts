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
    <div dir="rtl" style="display: flex; justify-content: center; align-items: center; min-height: 100vh; background: linear-gradient(135deg, #004d40 0%, #1565c0 100%); padding: 20px; box-sizing: border-box;">
      <div style="background-color: white; padding: 35px 30px; border-radius: 12px; box-shadow: 0 8px 30px rgba(0,0,0,0.25); width: 100%; max-width: 420px; box-sizing: border-box; text-align: center;">
        
        <div style="font-size: 38px; margin-bottom: 5px;">🌐</div>
        <h2 style="color: #004d40; margin: 0 0 5px 0; font-size: 22px;">شبكة الجمعيات الخيرية</h2>
        <p style="color: #666; font-size: 13px; margin: 0 0 20px 0;">نظام إدارة المستفيدين الموحد ومنع ازدواجية الصرف</p>

        <form (ngSubmit)="onLogin()" style="display: flex; flex-direction: column; gap: 12px; text-align: right;">
          <div>
            <label style="font-size: 12px; color: #555; display: block; margin-bottom: 3px;">اسم المستخدم</label>
            <input type="text" [(ngModel)]="username" name="username" placeholder="اسم المستخدم" required 
                   style="width: 100%; padding: 10px 12px; border: 1px solid #ccc; border-radius: 6px; box-sizing: border-box; font-size: 14px;">
          </div>
          
          <div>
            <label style="font-size: 12px; color: #555; display: block; margin-bottom: 3px;">كلمة المرور</label>
            <input type="password" [(ngModel)]="password" name="password" placeholder="كلمة المرور" required 
                   style="width: 100%; padding: 10px 12px; border: 1px solid #ccc; border-radius: 6px; box-sizing: border-box; font-size: 14px;">
          </div>
          
          <div *ngIf="errorMsg" style="color: #c62828; font-size: 13px; font-weight: bold;">{{ errorMsg }}</div>
          
          <button type="submit" style="padding: 12px; background-color: #004d40; color: white; border: none; border-radius: 6px; cursor: pointer; font-size: 15px; font-weight: bold; margin-top: 5px;">
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
    this.errorMsg = '';
    this.auth.login(this.username, this.password).subscribe((users: any) => {
      if (users && users.length > 0) {
        const destination = this.auth.getCurrentUser()?.role === 'Admin' ? '/dashboard' : (this.auth.hasPermission('beneficiaries.view') ? '/beneficiaries' : '/home');
        this.router.navigate([destination]).then(() => window.location.reload());
      } else {
        this.errorMsg = 'اسم المستخدم أو كلمة المرور غير صحيحة';
        this.cdr.detectChanges();
      }
    });
  }
}
