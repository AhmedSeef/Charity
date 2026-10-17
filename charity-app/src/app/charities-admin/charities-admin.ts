import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Api } from '../api';

@Component({
  selector: 'app-charities-admin',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section dir="rtl" style="max-width: 900px; margin: auto; color: #18332f">
      <h2>🏢 إدارة وتسجيل الجمعيات الشريكة</h2>
      <p>أضف جمعية وحدد نطاقها الجغرافي (المحافظة والمركز) والمستخدم الأساسي الذي يدير بياناتها.</p>
      
      <form (ngSubmit)="create()" #form="ngForm" style="display: grid; grid-template-columns: repeat(auto-fit, minmax(260px, 1fr)); gap: 14px; background: white; padding: 24px; border-radius: 12px; box-shadow: 0 2px 8px rgba(0,0,0,0.06);">
        <label>اسم الجمعية *
          <input name="name" [(ngModel)]="draft.name" required maxlength="200" placeholder="مثال: جمعية الأمل الخيرية">
        </label>
        
        <label>رقم الترخيص / الإشهار
          <input name="license" [(ngModel)]="draft.license" maxlength="100" placeholder="مشهرة برقم ...">
        </label>
        
        <label>الهاتف
          <input name="phone" [(ngModel)]="draft.phone" maxlength="50" placeholder="010XXXXXXXX">
        </label>

        <!-- النطاق الجغرافي -->
        <label>المحافظة *
          <select name="governorateId" [(ngModel)]="draft.governorateId" (change)="onGovernorateChange()" required style="padding: 10px; border: 1px solid #becdc8; border-radius: 6px; font: inherit;">
            <option value="">-- اختر المحافظة --</option>
            <option *ngFor="let g of governorates" [value]="g.id">{{ g.name }}</option>
          </select>
        </label>

        <label>المركز / الحي *
          <select name="districtId" [(ngModel)]="draft.districtId" (change)="onDistrictChange()" required style="padding: 10px; border: 1px solid #becdc8; border-radius: 6px; font: inherit;">
            <option value="">-- اختر المركز أو الحي --</option>
            <option *ngFor="let d of filteredDistricts" [value]="d.id">{{ d.name }}</option>
          </select>
        </label>

        <!-- العنوان التفصيلي مع الأوتو كومبليت الذكي -->
        <div style="grid-column: 1 / -1; position: relative;">
          <label style="display: block; margin-bottom: 5px; font-weight: 600;">العنوان التفصيلي / المقر (مع الإكمال التلقائي الذكي)
            <input name="address" [(ngModel)]="draft.address" (input)="onAddressInput()" (blur)="hideAutocompleteLater()" maxlength="300" placeholder="اكتب القرية أو الشارع (مثال: الجزيرة الخضراء، شارع المحكمة...)" autocomplete="off">
          </label>
          
          <!-- قائمة اقتراحات الأوتو كومبليت -->
          <div *ngIf="showAddressSuggestions && addressSuggestions.length > 0" 
               style="position: absolute; top: 100%; right: 0; left: 0; background: white; border: 1px solid #00695c; border-radius: 8px; box-shadow: 0 4px 14px rgba(0,0,0,0.15); z-index: 100; max-height: 220px; overflow-y: auto;">
            <div *ngFor="let item of addressSuggestions" 
                 (mousedown)="selectAddressSuggestion(item)"
                 style="padding: 10px 14px; border-bottom: 1px solid #f0f0f0; cursor: pointer; display: flex; justify-content: space-between; align-items: center;"
                 onmouseover="this.style.background='#e0f2f1'" onmouseout="this.style.background='white'">
              <div>
                <strong>📍 {{ item.addressText }}</strong>
                <span *ngIf="item.districtName" style="color: #666; font-size: 12px; margin-right: 8px;">({{ item.districtName }} - {{ item.governorateName }})</span>
              </div>
              <span *ngIf="item.usageCount > 1" style="font-size: 11px; background: #e0f2f1; color: #004d40; padding: 2px 6px; border-radius: 4px;">شائع ({{ item.usageCount }})</span>
            </div>
          </div>
        </div>

        <label style="grid-column: 1 / -1">مجال العمل والأنشطة
          <input name="roleDesc" [(ngModel)]="draft.roleDesc" maxlength="500" placeholder="مثال: رعاية أسر، إطعام، كفالات أيتام، إعانات علاجية...">
        </label>

        <h4 style="grid-column: 1 / -1; margin: 10px 0 0 0; color: #004d40; border-top: 1px dashed #ccc; padding-top: 12px;">بيانات حساب مدير الجمعية</h4>
        <label>اسم المستخدم الأساسي *
          <input name="managerUsername" [(ngModel)]="draft.managerUsername" required minlength="3" autocomplete="off" placeholder="اسم الدخول للنظام">
        </label>
        <label>اسم المسؤول *
          <input name="managerFullName" [(ngModel)]="draft.managerFullName" required maxlength="200" placeholder="الاسم ثلاثي">
        </label>
        <label>كلمة مرور المسؤول *
          <input name="managerPassword" [(ngModel)]="draft.managerPassword" required minlength="6" type="password" autocomplete="new-password">
        </label>

        <div style="grid-column: 1 / -1; color: #b71c1c; font-weight: bold;" *ngIf="error">{{ error }}</div>
        <button type="submit" [disabled]="form.invalid || saving" style="background: #004d40; color: white; border: 0; border-radius: 8px; padding: 12px; cursor: pointer; font-size: 15px; font-weight: bold;">{{ saving ? 'جارٍ الحفظ...' : '➕ حفظ وإضافة الجمعية' }}</button>
      </form>

      <p *ngIf="success" style="color: #08723c; font-weight: bold; background: #e8f5e9; padding: 10px; border-radius: 6px; margin-top: 12px;">{{ success }}</p>

      <h3 style="margin-top: 30px;">🏢 الجمعيات المسجلة بالشبكة</h3>
      <div *ngFor="let charity of charities" style="background: white; padding: 16px; margin: 10px 0; border-radius: 10px; display: flex; justify-content: space-between; gap: 12px; align-items: center; box-shadow: 0 1px 4px rgba(0,0,0,0.05);">
        <div>
          <div style="font-size: 16px; font-weight: bold; color: #004d40;">{{ charity.icon || '🏢' }} {{ charity.name }}</div>
          <div style="font-size: 13px; color: #555; margin-top: 4px;">
            <span *ngIf="charity.governorateName" style="background: #e0f2f1; color: #004d40; padding: 2px 8px; border-radius: 4px; font-weight: bold; margin-left: 6px;">
              📍 {{ charity.governorateName }} - {{ charity.districtName || 'المركز الرئيسي' }}
            </span>
            <span>{{ charity.address }}</span>
            <span *ngIf="charity.license" style="color: #777; margin-right: 10px;">({{ charity.license }})</span>
          </div>
        </div>
        <button type="button" (click)="selectedCharityId = charity.id; error = ''; success = ''" style="background: #eceff1; border: 1px solid #cfd8dc; padding: 8px 14px; border-radius: 6px; cursor: pointer;">إضافة مستخدم إضافي</button>
      </div>

      <form *ngIf="selectedCharityId" (ngSubmit)="createManager()" #managerForm="ngForm" style="display: grid; gap: 10px; background: white; padding: 20px; border-radius: 10px; margin-top: 16px; border: 2px solid #004d40;">
        <h3 style="margin: 0; color: #004d40;">إضافة مستخدم مسؤول للجمعية المختارة</h3>
        <label>الاسم الكامل *<input name="fullName" [(ngModel)]="managerDraft.fullName" required></label>
        <label>اسم المستخدم *<input name="username" [(ngModel)]="managerDraft.username" required minlength="3"></label>
        <label>كلمة المرور *<input name="password" [(ngModel)]="managerDraft.password" required minlength="6" type="password" autocomplete="new-password"></label>
        <div style="display: flex; gap: 10px;">
          <button type="submit" [disabled]="managerForm.invalid || saving" style="background: #004d40; color: white; border: 0; border-radius: 6px; padding: 10px 18px; cursor: pointer;">حفظ المستخدم</button>
          <button type="button" (click)="selectedCharityId = ''" style="background: #eee; border: 0; border-radius: 6px; padding: 10px 18px; cursor: pointer;">إلغاء</button>
        </div>
      </form>
    </section>
  `,
  styles: [`
    label { display: flex; flex-direction: column; gap: 5px; font-weight: 600; font-size: 13px; }
    input { padding: 10px; border: 1px solid #becdc8; border-radius: 6px; font: inherit; }
    button:disabled { opacity: .5; cursor: not-allowed; }
  `]
})
export class CharitiesAdminComponent implements OnInit {
  charities: any[] = [];
  governorates: any[] = [];
  allDistricts: any[] = [];
  filteredDistricts: any[] = [];
  
  // Autocomplete
  addressSuggestions: any[] = [];
  showAddressSuggestions = false;
  private autocompleteTimer: any;

  draft = this.emptyDraft();
  saving = false;
  error = '';
  success = '';
  selectedCharityId = '';
  managerDraft = { fullName: '', username: '', password: '' };

  constructor(private api: Api, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    this.reload();
    this.loadGeographicLookups();
  }

  private emptyDraft() {
    return {
      name: '',
      license: '',
      phone: '',
      address: '',
      roleDesc: '',
      governorateId: 'gov_kfs',
      governorateName: 'كفر الشيخ',
      districtId: 'dist_motobas',
      districtName: 'مركز مطوبس',
      managerUsername: '',
      managerFullName: '',
      managerPassword: ''
    };
  }

  loadGeographicLookups() {
    this.api.getGovernorates().subscribe({
      next: govs => {
        this.governorates = govs;
        this.cdr.detectChanges();
      }
    });

    this.api.getDistricts().subscribe({
      next: dists => {
        this.allDistricts = dists;
        this.filterDistrictsByGov();
        this.cdr.detectChanges();
      }
    });
  }

  onGovernorateChange() {
    const selGov = this.governorates.find(g => g.id === this.draft.governorateId);
    this.draft.governorateName = selGov ? selGov.name : '';
    this.filterDistrictsByGov();
    this.draft.districtId = this.filteredDistricts.length > 0 ? this.filteredDistricts[0].id : '';
    this.onDistrictChange();
  }

  onDistrictChange() {
    const selDist = this.allDistricts.find(d => d.id === this.draft.districtId);
    this.draft.districtName = selDist ? selDist.name : '';
  }

  private filterDistrictsByGov() {
    if (this.draft.governorateId) {
      this.filteredDistricts = this.allDistricts.filter(d => d.governorateId === this.draft.governorateId);
    } else {
      this.filteredDistricts = [...this.allDistricts];
    }
  }

  onAddressInput() {
    if (this.autocompleteTimer) clearTimeout(this.autocompleteTimer);
    const query = this.draft.address?.trim() || '';

    this.autocompleteTimer = setTimeout(() => {
      this.api.autocompleteAddresses(query, this.draft.governorateId, this.draft.districtId, 8).subscribe({
        next: results => {
          this.addressSuggestions = results || [];
          this.showAddressSuggestions = this.addressSuggestions.length > 0;
          this.cdr.detectChanges();
        }
      });
    }, 200);
  }

  selectAddressSuggestion(item: any) {
    this.draft.address = item.addressText;
    this.showAddressSuggestions = false;
    this.cdr.detectChanges();
  }

  hideAutocompleteLater() {
    setTimeout(() => {
      this.showAddressSuggestions = false;
      this.cdr.detectChanges();
    }, 250);
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

    // Set governorate and district names if missing
    if (!this.draft.governorateName && this.draft.governorateId) {
      const g = this.governorates.find(x => x.id === this.draft.governorateId);
      if (g) this.draft.governorateName = g.name;
    }
    if (!this.draft.districtName && this.draft.districtId) {
      const d = this.allDistricts.find(x => x.id === this.draft.districtId);
      if (d) this.draft.districtName = d.name;
    }

    this.api.createCharity(this.draft).subscribe({
      next: result => {
        this.success = `تمت إضافة ${result.name} بنجاح بنطاق (${this.draft.governorateName} - ${this.draft.districtName}) وحساب المستخدم ${this.draft.managerUsername}.`;
        this.draft = this.emptyDraft();
        this.filterDistrictsByGov();
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
        this.error = err.error?.message || 'تعذر إنشاء المستخدم.';
        this.saving = false;
        this.cdr.detectChanges();
      }
    });
  }
}
