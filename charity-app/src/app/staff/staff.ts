import { Component, ChangeDetectorRef, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { Api } from '../api';
import { AuthService } from '../auth.service';
import { PaginationComponent } from '../pagination/pagination';
import { nationalIdError, normalizeNationalId } from '../egyptian-national-id';
import { isTodayOrEarlier, todayInCairo } from '../business-date';

@Component({
  selector: 'app-staff',
  standalone: true,
  imports: [CommonModule, FormsModule, PaginationComponent],
  templateUrl: './staff.html',
  styleUrls: ['./staff.css']
})
export class StaffComponent implements OnInit, OnDestroy {
  // Navigation: 'list' | 'detail' | 'form'
  viewMode: 'list' | 'detail' | 'form' = 'list';
  formMode: 'add' | 'edit' = 'add';
  selectedStaff: any = null;

  // Tabs: 'registry' (HR directory) | 'accounts' (User logins & permissions)
  activeTab: 'registry' | 'accounts' = 'registry';

  staffList: any[] = [];
  staffUsersList: any[] = [];

  // Search & Filter
  searchTerm: string = '';
  selectedTypeFilter: string = 'الكل';

  // Pagination
  currentPage: number = 1;
  pageSize: number = 5;

  // Alerts
  successMessage: string = '';
  errorMessage: string = '';

  // Staff User Account Modal State
  showUserModal: boolean = false;
  isEditUser: boolean = false;
  showPasswordModal = false;
  passwordTarget: any = null;
  newStaffPassword = '';
  confirmStaffPassword = '';
  passwordResetError = '';
  isResettingPassword = false;
  userForm: {
    id?: string;
    username: string;
    password?: string;
    fullName: string;
    roleTemplate: string;
    permissions: string[];
    staffMemberId?: string;
  } = {
    username: '',
    password: '',
    fullName: '',
    roleTemplate: 'SocialWorker',
    permissions: ['beneficiaries.view', 'beneficiaries.view_full', 'beneficiaries.create', 'beneficiaries.edit'],
    staffMemberId: ''
  };

  roleTemplates = [
    { id: 'SocialWorker', name: '📋 باحث اجتماعي ميداني', desc: 'استعراض وبحث كامل، فتح وتعديل ملفات الحالات، بدون حذف وبدون صرف' },
    { id: 'DataEntry', name: '⌨️ مدخل بيانات واستقبال', desc: 'عرض وإضافة الحالات الأساسية، حجب الديون والأمراض الحساسة، بدون حذف' },
    { id: 'DisbursementOfficer', name: '🤝 مسؤول مساعدات وصرف', desc: 'تنفيذ الصرف الفردي والتوزيع الميداني وعرض أرصدة المخازن' },
    { id: 'Storekeeper', name: '📦 أمين مخزن وتوريدات', desc: 'استعراض وإدارة المخازن والأصناف وجرد وتعديل الأرصدة' },
    { id: 'Accountant', name: '💰 محاسب ومسؤول تبرعات', desc: 'تسجيل واستعراض التبرعات، بنود ومصروفات الجمعية' },
    { id: 'Custom', name: '🛠️ مخصص (تحديد يدوي)', desc: 'تحديد صلاحيات مفصلة بحرية تامة' }
  ];

  templatePermissionsMap: Record<string, string[]> = {
    SocialWorker: ['beneficiaries.view', 'beneficiaries.view_full', 'beneficiaries.create', 'beneficiaries.edit'],
    DataEntry: ['beneficiaries.view', 'beneficiaries.create'],
    DisbursementOfficer: ['beneficiaries.view', 'disbursement.execute', 'disbursement.view', 'inventory.view'],
    Storekeeper: ['inventory.view', 'inventory.manage', 'disbursement.view'],
    Accountant: ['donations.view', 'donations.create', 'expenses.manage', 'disbursement.view'],
    Custom: []
  };

  permissionGroups = [
    {
      name: '👨‍👩‍👧‍👦 إدارة المستفيدين',
      items: [
        { code: 'beneficiaries.view', label: 'عرض سجل المستفيدين والبحث الأساسي' },
        { code: 'beneficiaries.view_full', label: 'الاطلاع على البيانات الحساسة (الديون، الأمراض، البحث الاجتماعي)' },
        { code: 'beneficiaries.create', label: 'فتح ملفات حالات جديدة' },
        { code: 'beneficiaries.edit', label: 'تعديل وتحديث بيانات المستفيدين' },
        { code: 'beneficiaries.delete', label: 'أرشفة وحذف ملفات المستفيدين' },
      ]
    },
    {
      name: '📤 الصرف والمساعدات',
      items: [
        { code: 'disbursement.execute', label: 'تنفيذ صرف المساعدات (فردي وجماعي)' },
        { code: 'disbursement.view', label: 'استعراض سجلات وتقارير الصرف' },
      ]
    },
    {
      name: '📦 المخازن والأصناف',
      items: [
        { code: 'inventory.view', label: 'استعراض أرصدة المخازن' },
        { code: 'inventory.manage', label: 'إدارة وتعديل المخزون والأصناف والجرد' },
      ]
    },
    {
      name: '💰 التبرعات والمصروفات',
      items: [
        { code: 'donations.view', label: 'استعراض سجل التبرعات' },
        { code: 'donations.create', label: 'استلام وتسجيل التبرعات وإصدار إيصالات' },
        { code: 'expenses.manage', label: 'إدارة بنود ومصروفات الجمعية' },
      ]
    },
    {
      name: '👥 الإدارة وفريق العمل',
      items: [
        { code: 'staff.manage', label: 'إدارة فريق العمل وتفويض صلاحيات المستخدمين' },
      ]
    }
  ];

  newStaff: any = { 
    id: null,
    nationalId: '', 
    name: '', 
    phone: '', 
    address: '',
    type: 'متطوع', 
    role: 'باحث اجتماعي', 
    salary: null, 
    joinDate: todayInCairo(),
    tasks: '',
    status: 'نشط على رأس العمل'
  };

  types = ['متطوع', 'موظف براتب'];
  roles = [
    'باحث اجتماعي ميداني', 
    'إداري ومسؤول مشروعات', 
    'أمين مخزن ومسؤول توريدات', 
    'مندوب تحصيل تبرعات', 
    'طبيب متطوع / رعاية صحية', 
    'سائق ومسؤول توزيع', 
    'منسق قوافل ومتطوعين',
    'محاسب ومسؤول مالي'
  ];

  private querySub?: Subscription;

  todayInCairo(): string {
    return todayInCairo();
  }

  constructor(
    public api: Api, 
    public auth: AuthService,
    private cdr: ChangeDetectorRef,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadData();
    this.loadStaffUsers();
  }

  ngOnDestroy(): void {
    if (this.querySub) {
      this.querySub.unsubscribe();
    }
  }

  loadData() {
    this.api.getStaff().subscribe((data: any[]) => { 
      this.staffList = data; 
      this.initRouteSync();
      this.cdr.detectChanges(); 
    });
  }

  initRouteSync() {
    if (this.querySub) return;
    this.querySub = this.route.queryParams.subscribe(params => {
      const mode = params['mode'];
      const id = params['id'];

      if (mode === 'detail' && id) {
        const found = this.staffList.find(s => String(s.id) === String(id));
        if (found) {
          this.selectedStaff = found;
          this.viewMode = 'detail';
        } else {
          this.viewMode = 'list';
        }
      } else if (mode === 'edit' && id) {
        const found = this.staffList.find(s => String(s.id) === String(id));
        if (found) {
          this.newStaff = { ...found };
          this.formMode = 'edit';
          this.viewMode = 'form';
        } else {
          this.viewMode = 'list';
        }
      } else if (mode === 'add') {
        this.resetForm();
        this.formMode = 'add';
        this.viewMode = 'form';
      } else {
        this.viewMode = 'list';
        this.selectedStaff = null;
      }
      this.cdr.detectChanges();
    });
  }

  // Navigation helpers
  openDetail(s: any) {
    this.router.navigate([], { relativeTo: this.route, queryParams: { mode: 'detail', id: s.id } });
  }

  openAdd() {
    this.router.navigate([], { relativeTo: this.route, queryParams: { mode: 'add' } });
  }

  openEdit(s: any, event?: Event) {
    if (event) event.stopPropagation();
    this.router.navigate([], { relativeTo: this.route, queryParams: { mode: 'edit', id: s.id } });
  }

  backToList() {
    this.router.navigate([], { relativeTo: this.route, queryParams: {} });
  }

  onTypeChange() {
    if (this.newStaff.type === 'متطوع') {
      this.newStaff.salary = null;
    }
  }

  get volunteerCount(): number {
    return this.staffList.filter(s => s.type === 'متطوع').length;
  }

  get employeeCount(): number {
    return this.staffList.filter(s => s.type === 'موظف براتب').length;
  }

  get filteredStaff(): any[] {
    return this.staffList.filter(s => {
      const matchType = this.selectedTypeFilter === 'الكل' || s.type === this.selectedTypeFilter;
      const term = this.searchTerm.trim().toLowerCase();
      const matchSearch = !term ||
        (s.name && s.name.toLowerCase().includes(term)) ||
        (s.nationalId && s.nationalId.includes(term)) ||
        (s.phone && s.phone.includes(term)) ||
        (s.role && s.role.toLowerCase().includes(term));
      return matchType && matchSearch;
    });
  }

  get paginatedStaff(): any[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.filteredStaff.slice(start, start + this.pageSize);
  }

  resetForm() {
    this.newStaff = { 
      id: null,
      nationalId: '', 
      name: '', 
      phone: '', 
      address: '',
      type: 'متطوع', 
      role: 'باحث اجتماعي ميداني', 
      salary: null, 
      joinDate: todayInCairo(),
      tasks: '',
      status: 'نشط على رأس العمل'
    };
  }

  onSubmit() {
    this.newStaff.name = (this.newStaff.name || '').trim();
    this.newStaff.nationalId = normalizeNationalId(this.newStaff.nationalId);
    const idError = nationalIdError(this.newStaff.nationalId);
    if (!this.newStaff.name || idError) {
      this.errorMessage = idError || 'اسم عضو فريق العمل مطلوب.';
      return;
    }
    this.newStaff.phone = (this.newStaff.phone || '').trim();
    if (!/^01[0125][0-9]{8}$/.test(this.newStaff.phone)) {
      this.errorMessage = 'رقم الهاتف يجب أن يكون رقم محمول مصريًا صحيحًا من 11 رقمًا.';
      return;
    }
    if (!this.newStaff.role || !isTodayOrEarlier(this.newStaff.joinDate)) {
      this.errorMessage = 'راجع المسمى الوظيفي وتاريخ الانضمام؛ لا يجوز تاريخ مستقبلي.';
      return;
    }
    if (this.newStaff.type === 'موظف براتب' &&
        (this.newStaff.salary === null || this.newStaff.salary === '' ||
         !Number.isFinite(Number(this.newStaff.salary)) || Number(this.newStaff.salary) < 0)) {
      this.errorMessage = 'راتب الموظف يجب أن يكون صفرًا أو أكثر.';
      return;
    }
    if (this.newStaff.type === 'متطوع') {
      this.newStaff.salary = null;
    } else if (this.newStaff.type !== 'موظف براتب') {
      this.errorMessage = 'نوع عضو فريق العمل غير صحيح.';
      return;
    }

    this.errorMessage = '';

    if (this.newStaff.id) {
      this.api.updateStaff(this.newStaff).subscribe({ next: () => {
        this.successMessage = `✅ تم تحديث بيانات (${this.newStaff.name}) بنجاح!`;
        this.api.getStaff().subscribe((data: any[]) => {
          this.staffList = data;
          this.openDetail(this.newStaff);
          this.cdr.detectChanges();
        });
        setTimeout(() => { this.successMessage = ''; this.cdr.detectChanges(); }, 4000);
      }, error: (err: any) => { this.errorMessage = err.error?.message || 'فشل تحديث بيانات عضو فريق العمل.'; this.cdr.detectChanges(); } });
    } else {
      this.api.addStaff(this.newStaff).subscribe({ next: (added: any) => {
        this.successMessage = `✅ تم إضافة (${added.name}) إلى فريق العمل بنجاح!`;
        this.api.getStaff().subscribe((data: any[]) => {
          this.staffList = data;
          this.openDetail(added);
          this.cdr.detectChanges();
        });
        setTimeout(() => { this.successMessage = ''; this.cdr.detectChanges(); }, 4000);
      }, error: (err: any) => { this.errorMessage = err.error?.message || 'فشل إضافة عضو فريق العمل.'; this.cdr.detectChanges(); } });
    }
  }

  deleteStaff(id: string, event: Event) {
    event.stopPropagation();
    if (confirm('هل أنت متأكد من رغبتك في حذف هذا العضو من سجلات الموارد البشرية؟')) {
      this.api.deleteStaff(id).subscribe(() => {
        this.loadData();
        if (this.selectedStaff && this.selectedStaff.id === id) {
          this.backToList();
        }
      });
    }
  }

  printStaffProfile() {
    window.print();
  }

  // ==========================================
  // Staff User Accounts & Permissions
  // ==========================================
  loadStaffUsers() {
    this.api.getStaffUsers().subscribe({
      next: (users: any[]) => {
        this.staffUsersList = users || [];
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        console.error('Error fetching staff users:', err);
      }
    });
  }

  switchTab(tab: 'registry' | 'accounts') {
    this.activeTab = tab;
    if (tab === 'accounts') {
      this.loadStaffUsers();
    }
    this.cdr.detectChanges();
  }

  onStaffMemberSelect(staffId: string) {
    if (!staffId) return;
    const member = this.staffList.find(s => s.id === staffId);
    if (member) {
      this.userForm.fullName = member.name;
      // Auto suggest username based on romanized name or role
      if (!this.userForm.username) {
        this.userForm.username = 'staff_' + Math.floor(100 + Math.random() * 900);
      }
      // Suggest role template
      if (member.role?.includes('باحث')) {
        this.userForm.roleTemplate = 'SocialWorker';
      } else if (member.role?.includes('مخزن') || member.role?.includes('توريد')) {
        this.userForm.roleTemplate = 'Storekeeper';
      } else if (member.role?.includes('توزيع') || member.role?.includes('سائق')) {
        this.userForm.roleTemplate = 'DisbursementOfficer';
      } else if (member.role?.includes('محاسب') || member.role?.includes('مالي') || member.role?.includes('تحصيل')) {
        this.userForm.roleTemplate = 'Accountant';
      } else {
        this.userForm.roleTemplate = 'DataEntry';
      }
      this.onRoleTemplateChange();
    }
  }

  onRoleTemplateChange() {
    if (this.userForm.roleTemplate !== 'Custom' && this.templatePermissionsMap[this.userForm.roleTemplate]) {
      this.userForm.permissions = [...this.templatePermissionsMap[this.userForm.roleTemplate]];
    }
    this.cdr.detectChanges();
  }

  isPermChecked(code: string): boolean {
    return this.userForm.permissions.includes(code);
  }

  togglePerm(code: string) {
    if (this.isPermChecked(code)) {
      this.userForm.permissions = this.userForm.permissions.filter(p => p !== code);
    } else {
      this.userForm.permissions.push(code);
    }

    // Check if matches a preset template
    const match = Object.keys(this.templatePermissionsMap).find(t => 
      t !== 'Custom' &&
      this.templatePermissionsMap[t].length === this.userForm.permissions.length &&
      this.templatePermissionsMap[t].every(p => this.userForm.permissions.includes(p))
    );
    this.userForm.roleTemplate = match || 'Custom';
    this.cdr.detectChanges();
  }

  openCreateUserModal(staffMember?: any) {
    this.isEditUser = false;
    this.userForm = {
      username: '',
      password: '',
      fullName: staffMember?.name || '',
      roleTemplate: 'SocialWorker',
      permissions: [...this.templatePermissionsMap['SocialWorker']],
      staffMemberId: staffMember?.id || ''
    };
    if (staffMember) {
      this.onStaffMemberSelect(staffMember.id);
    }
    this.showUserModal = true;
    this.cdr.detectChanges();
  }

  openEditUserModal(user: any) {
    this.isEditUser = true;
    this.userForm = {
      id: user.id,
      username: user.username,
      password: '',
      fullName: user.fullName,
      roleTemplate: user.roleTemplate || 'Custom',
      permissions: [...(user.permissions || [])]
    };
    this.showUserModal = true;
    this.cdr.detectChanges();
  }

  closeUserModal() {
    this.showUserModal = false;
    this.cdr.detectChanges();
  }

  get canResetStaffPasswords(): boolean {
    return this.auth.getCurrentUser()?.role === 'CharityManager';
  }

  openPasswordResetModal(user: any): void {
    if (!this.canResetStaffPasswords) return;
    this.passwordTarget = user;
    this.newStaffPassword = '';
    this.confirmStaffPassword = '';
    this.passwordResetError = '';
    this.showPasswordModal = true;
    this.cdr.detectChanges();
  }

  closePasswordResetModal(): void {
    if (this.isResettingPassword) return;
    this.showPasswordModal = false;
    this.passwordTarget = null;
    this.newStaffPassword = '';
    this.confirmStaffPassword = '';
    this.passwordResetError = '';
    this.cdr.detectChanges();
  }

  resetStaffPassword(): void {
    if (!this.passwordTarget || this.isResettingPassword) return;
    if (this.newStaffPassword.trim().length < 4) {
      this.passwordResetError = 'كلمة المرور الجديدة يجب ألا تقل عن 4 خانات.';
      return;
    }
    if (this.newStaffPassword !== this.confirmStaffPassword) {
      this.passwordResetError = 'تأكيد كلمة المرور غير مطابق.';
      return;
    }

    this.passwordResetError = '';
    this.isResettingPassword = true;
    const fullName = this.passwordTarget.fullName;
    this.api.resetStaffUserPassword(this.passwordTarget.id, this.newStaffPassword).subscribe({
      next: () => {
        this.isResettingPassword = false;
        this.closePasswordResetModal();
        this.successMessage = `تم تعيين كلمة مرور جديدة للموظف (${fullName}).`;
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        this.isResettingPassword = false;
        this.passwordResetError = err.error?.message || 'تعذر إعادة تعيين كلمة المرور.';
        this.cdr.detectChanges();
      }
    });
  }

  saveUserAccount() {
    if (!this.userForm.fullName || !this.userForm.fullName.trim()) {
      alert('يرجى إدخال اسم الموظف الكامل.');
      return;
    }

    if (!this.isEditUser) {
      if (!this.userForm.username || this.userForm.username.trim().length < 3) {
        alert('اسم المستخدم يجب ألا يقل عن 3 أحرف.');
        return;
      }
      if (!this.userForm.password || this.userForm.password.length < 4) {
        alert('كلمة المرور يجب ألا تقل عن 4 خانات.');
        return;
      }

      this.api.createStaffUser(this.userForm).subscribe({
        next: (created: any) => {
          this.successMessage = `✅ تم إنشاء حساب المستخدم للموظف (${created.fullName}) بنجاح!`;
          this.closeUserModal();
          this.loadStaffUsers();
          setTimeout(() => { this.successMessage = ''; this.cdr.detectChanges(); }, 4000);
        },
        error: (err: any) => {
          alert(err.error?.message || 'تعذر إنشاء حساب المستخدم.');
        }
      });
    } else {
      this.api.updateStaffUserPermissions(this.userForm.id!, {
        roleTemplate: this.userForm.roleTemplate,
        permissions: this.userForm.permissions
      }).subscribe({
        next: (updated: any) => {
          this.successMessage = `✅ تم تحديث صلاحيات الحساب (${updated.fullName}) بنجاح!`;
          this.closeUserModal();
          this.loadStaffUsers();
          setTimeout(() => { this.successMessage = ''; this.cdr.detectChanges(); }, 4000);
        },
        error: (err: any) => {
          alert(err.error?.message || 'تعذر تحديث الصلاحيات.');
        }
      });
    }
  }

  deleteStaffUserAccount(user: any) {
    if (confirm(`هل أنت متأكد من رغبتك في حذف حساب تسجيل الدخول للموظف (${user.fullName})؟ لن يتمكن من تسجيل الدخول بعد الآن.`)) {
      this.api.deleteStaffUser(user.id).subscribe({
        next: () => {
          this.successMessage = `✅ تم حذف حساب المستخدم بنجاح.`;
          this.loadStaffUsers();
          setTimeout(() => { this.successMessage = ''; this.cdr.detectChanges(); }, 4000);
        },
        error: (err: any) => {
          alert(err.error?.message || 'تعذر حذف الحساب.');
        }
      });
    }
  }

  getRoleTemplateBadge(template: string): { label: string; bg: string; color: string } {
    switch (template) {
      case 'SocialWorker':
        return { label: '📋 باحث اجتماعي', bg: '#e8f5e9', color: '#1b5e20' };
      case 'DataEntry':
        return { label: '⌨️ مدخل بيانات واستقبال', bg: '#e0f7fa', color: '#006064' };
      case 'DisbursementOfficer':
        return { label: '🤝 مسؤول صرف وتوزيع', bg: '#fff3e0', color: '#e65100' };
      case 'Storekeeper':
        return { label: '📦 أمين مخزن وتوريدات', bg: '#f3e5f5', color: '#4a148c' };
      case 'Accountant':
        return { label: '💰 محاسب ومسؤول تبرعات', bg: '#e8eaf6', color: '#1a237e' };
      default:
        return { label: '🛠️ صلاحيات مخصصة', bg: '#ede7f6', color: '#512da8' };
    }
  }

  getPermissionLabel(code: string): string {
    for (const group of this.permissionGroups) {
      const found = group.items.find(i => i.code === code);
      if (found) return found.label;
    }
    return code;
  }
}
