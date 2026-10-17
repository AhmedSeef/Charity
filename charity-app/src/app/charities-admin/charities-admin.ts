import { ChangeDetectorRef, Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Api } from '../api';

@Component({
  selector: 'app-charities-admin',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section dir="rtl" style="max-width: 850px; margin: auto; color: #18332f">
      <h2>🏢 إدارة الجمعيات</h2>
      <p>أضف جمعية وحساب المستخدم الأساسي الذي يدير بياناتها فقط. يمكن إضافة مستخدم أساسي لجمعية موجودة من القائمة أدناه.</p>
      <form (ngSubmit)="create()" #form="ngForm" style="display: grid; grid-template-columns: repeat(auto-fit, minmax(240px, 1fr)); gap: 14px; background: white; padding: 24px; border-radius: 12px">
        <label>اسم الجمعية *<input name="name" [(ngModel)]="draft.name" required maxlength="200"></label>
        <label>رقم الترخيص<input name="license" [(ngModel)]="draft.license" maxlength="100"></label>
        <label>الهاتف<input name="phone" [(ngModel)]="draft.phone" maxlength="50"></label>
        <label>العنوان<input name="address" [(ngModel)]="draft.address" maxlength="300"></label>
        <label style="grid-column: 1 / -1">مجال العمل<input name="roleDesc" [(ngModel)]="draft.roleDesc" maxlength="500"></label>
        <label>اسم المستخدم الأساسي *<input name="managerUsername" [(ngModel)]="draft.managerUsername" required minlength="3" autocomplete="off"></label>
        <label>اسم المسؤول *<input name="managerFullName" [(ngModel)]="draft.managerFullName" required maxlength="200"></label>
        <label>كلمة مرور المسؤول *<input name="managerPassword" [(ngModel)]="draft.managerPassword" required minlength="6" type="password" autocomplete="new-password"></label>
        <div style="grid-column: 1 / -1; color: #b71c1c" *ngIf="error">{{ error }}</div>
        <button type="submit" [disabled]="form.invalid || saving" style="background: #004d40; color: white; border: 0; border-radius: 8px; padding: 12px; cursor: pointer">{{ saving ? 'جارٍ الحفظ...' : 'إضافة الجمعية' }}</button>
      </form>
      <p *ngIf="success" style="color: #08723c">{{ success }}</p>
      <h3>الجمعيات المسجلة</h3>
      <div *ngFor="let charity of charities" style="background: white; padding: 12px; margin: 8px 0; border-radius: 8px; display: flex; justify-content: space-between; gap: 10px; align-items: center">
        <span>{{ charity.icon }} {{ charity.name }} — {{ charity.address }}</span>
        <button type="button" (click)="selectedCharityId = charity.id; error = ''; success = ''">إضافة مستخدم أساسي</button>
      </div>
      <form *ngIf="selectedCharityId" (ngSubmit)="createManager()" #managerForm="ngForm" style="display: grid; gap: 10px; background: white; padding: 20px; border-radius: 10px; margin-top: 16px">
        <h3>المستخدم الأساسي للجمعية المختارة</h3>
        <label>الاسم الكامل *<input name="fullName" [(ngModel)]="managerDraft.fullName" required></label>
        <label>اسم المستخدم *<input name="username" [(ngModel)]="managerDraft.username" required minlength="3"></label>
        <label>كلمة المرور *<input name="password" [(ngModel)]="managerDraft.password" required minlength="6" type="password" autocomplete="new-password"></label>
        <button type="submit" [disabled]="managerForm.invalid || saving">حفظ المستخدم الأساسي</button>
      </form>
    </section>
  `,
  styles: [`label { display: flex; flex-direction: column; gap: 5px; font-weight: 600; } input { padding: 10px; border: 1px solid #becdc8; border-radius: 6px; font: inherit; } button:disabled { opacity: .5; cursor: not-allowed; }`]
})
export class CharitiesAdminComponent {
  charities: any[] = [];
  draft = this.emptyDraft();
  saving = false;
  error = '';
  success = '';
  selectedCharityId = '';
  managerDraft = { fullName: '', username: '', password: '' };

  constructor(private api: Api, private cdr: ChangeDetectorRef) { this.reload(); }

  private emptyDraft() {
    return { name: '', license: '', phone: '', address: '', roleDesc: '', managerUsername: '', managerFullName: '', managerPassword: '' };
  }

  private reload() {
    this.api.getCharities().subscribe(items => {
      this.charities = items;
      this.cdr.detectChanges();
    });
  }

  create() {
    this.saving = true;
    this.error = '';
    this.success = '';
    this.api.createCharity(this.draft).subscribe({
      next: result => {
        this.success = `تمت إضافة ${result.name} وحساب المستخدم الأساسي ${this.draft.managerUsername}.`;
        this.draft = this.emptyDraft();
        this.saving = false;
        this.cdr.detectChanges();
        this.reload();
      },
      error: err => {
        this.error = err.error?.message || 'تعذر إضافة الجمعية. تحقق من البيانات وحاول مرة أخرى.';
        this.saving = false;
        this.cdr.detectChanges();
      }
    });
  }

  createManager() {
    if (!this.selectedCharityId) return;
    this.saving = true;
    this.error = '';
    this.success = '';
    this.api.createCharityManager(this.selectedCharityId, this.managerDraft).subscribe({
      next: () => {
        this.success = `تم إنشاء المستخدم ${this.managerDraft.username} للجمعية.`;
        this.managerDraft = { fullName: '', username: '', password: '' };
        this.selectedCharityId = '';
        this.saving = false;
        this.cdr.detectChanges();
      },
      error: err => {
        this.error = err.error?.message || 'تعذر إنشاء المستخدم الأساسي.';
        this.saving = false;
        this.cdr.detectChanges();
      }
    });
  }
}
