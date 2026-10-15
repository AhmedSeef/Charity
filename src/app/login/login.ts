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

        <!-- أزرار الدخول السريع للجمعيات -->
        <div style="background-color: #f5f5f5; border: 1px solid #e0e0e0; border-radius: 8px; padding: 12px; margin-bottom: 20px; text-align: right;">
          <div style="font-size: 12px; font-weight: bold; color: #333; margin-bottom: 8px;">🚀 تجربة الدخول المباشر بحساب الجمعية:</div>
          <div style="display: flex; flex-direction: column; gap: 6px;">
            <button type="button" (click)="quickLogin('admin', 'admin')" 
                    style="background: #e8f5e9; border: 1px solid #c8e6c9; color: #1b5e20; padding: 8px 12px; border-radius: 6px; cursor: pointer; font-size: 12px; font-weight: bold; text-align: right; display: flex; justify-content: space-between;">
              <span>🏢 جمعية أبواب الخير</span>
              <span style="font-size: 11px; opacity: 0.8;">admin / admin</span>
            </button>
            <button type="button" (click)="quickLogin('amal', 'amal')" 
                    style="background: #e3f2fd; border: 1px solid #bbdefb; color: #0d47a1; padding: 8px 12px; border-radius: 6px; cursor: pointer; font-size: 12px; font-weight: bold; text-align: right; display: flex; justify-content: space-between;">
              <span>🤝 جمعية الأمل لرعاية الأيتام</span>
              <span style="font-size: 11px; opacity: 0.8;">amal / amal</span>
            </button>
            <button type="button" (click)="quickLogin('sedaka', 'sedaka')" 
                    style="background: #f3e5f5; border: 1px solid #e1bee7; color: #4a148c; padding: 8px 12px; border-radius: 6px; cursor: pointer; font-size: 12px; font-weight: bold; text-align: right; display: flex; justify-content: space-between;">
              <span>🏥 جمعية رسالة الصداقة</span>
              <span style="font-size: 11px; opacity: 0.8;">sedaka / sedaka</span>
            </button>
          </div>
        </div>

        <div style="position: relative; text-align: center; margin: 15px 0;">
          <hr style="border: 0; border-top: 1px solid #ddd;">
          <span style="position: absolute; top: -10px; background: white; padding: 0 10px; color: #888; font-size: 12px; left: 50%; transform: translateX(-50%);">أو الدخول اليدوي</span>
        </div>
        
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

  quickLogin(user: string, pass: string) {
    this.username = user;
    this.password = pass;
    this.onLogin();
  }

  onLogin() {
    this.errorMsg = '';
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
