import { Component, OnInit, ChangeDetectorRef, Optional } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Api } from '../api';
import { AuthService } from '../auth.service';

@Component({
  selector: 'app-expense-categories',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './expense-categories.html',
  styleUrls: ['./expense-categories.css']
})
export class ExpenseCategoriesComponent implements OnInit {
  viewMode: 'list' | 'form' = 'list';

  // تبويبات العرض في القائمة
  activeTab: 'categories' | 'monthly' = 'categories';

  // تصفية بالشهر
  selectedMonth: string = ''; // '' تعني كل الشهور / الإجمالي التاريخي
  reportPeriod: 'all' | 'month' | 'range' = 'all';
  dateFrom = '';
  dateTo = '';
  selectedYear: number | null = null;
  reportYears = Array.from({ length: 10 }, (_, index) => new Date().getFullYear() - index);
  monthlyMovements: any[] = [];
  isLoadingMonthly: boolean = false;

  categories: any[] = [];
  isLoading: boolean = false;
  searchTerm: string = '';
  activeCharity: any = null;

  successMessage: string = '';
  errorMessage: string = '';

  // نموذج الإضافة والتعديل
  isEditing: boolean = false;
  isSaving: boolean = false;
  categoryForm = {
    id: null as string | null,
    name: '',
    code: '',
    description: '',
    isActive: true
  };

  // نافذة كشف الحالات ("من أخذ من هذا البند")
  showDrilldownModal: boolean = false;
  selectedCategory: any = null;
  drilldownRecipients: any[] = [];
  isLoadingRecipients: boolean = false;
  drilldownSearchTerm: string = '';

  constructor(
    public api: Api,
    public auth: AuthService,
    private cdr: ChangeDetectorRef,
    @Optional() private route?: ActivatedRoute,
    @Optional() private router?: Router
  ) {}

  get isFormPage(): boolean {
    return this.route?.snapshot.data['formPage'] === true;
  }

  get isCharityManager(): boolean {
    return this.auth.getCurrentUser()?.role !== 'Admin';
  }

  ngOnInit(): void {
    this.activeCharity = this.api.getActiveCharity();
    this.loadCategories();
    this.loadMonthlyMovements();
    this.initRouteSync();
  }

  initRouteSync(): void {
    this.route?.data.subscribe(data => {
      if (data && data['formPage'] === true) {
        this.openCreateForm();
      }
    });

    this.route?.queryParams.subscribe(params => {
      if (params && params['mode'] === 'add') {
        this.openCreateForm();
      }
    });
  }

  loadCategories(): void {
    if (!this.validateReportPeriod()) return;
    this.isLoading = true;
    this.api.getExpenseCategories(this.reportPeriod === 'month' ? this.selectedMonth || undefined : undefined,
      this.reportPeriod === 'range' ? this.dateFrom : undefined,
      this.reportPeriod === 'range' ? this.dateTo : undefined).subscribe({
      next: (data: any[]) => {
        this.categories = data || [];
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || err.message || 'فشل في تحميل بنود المصروفات.';
        this.cdr.detectChanges();
      }
    });
  }

  loadMonthlyMovements(): void {
    if (!this.validateReportPeriod()) return;
    this.isLoadingMonthly = true;
    this.api.getMonthlyExpenseMovements(this.reportPeriod === 'range' ? undefined : this.selectedYear || undefined,
      this.reportPeriod === 'range' ? this.dateFrom : undefined,
      this.reportPeriod === 'range' ? this.dateTo : undefined).subscribe({
      next: (data: any[]) => {
        this.monthlyMovements = data || [];
        this.isLoadingMonthly = false;
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        this.isLoadingMonthly = false;
        this.cdr.detectChanges();
      }
    });
  }

  onMonthFilterChange(month: string): void {
    this.selectedMonth = month;
    this.reportPeriod = month ? 'month' : 'all';
    this.loadCategories();
  }

  filterByMonthFromMovement(monthYear: string): void {
    this.selectedMonth = monthYear;
    this.reportPeriod = 'month';
    this.activeTab = 'categories';
    this.loadCategories();
  }

  resetMonthFilter(): void {
    this.selectedMonth = '';
    this.reportPeriod = 'all';
    this.loadCategories();
  }

  setReportPeriod(period: 'all' | 'month' | 'range'): void {
    this.reportPeriod = period;
    this.errorMessage = '';
    if (period === 'all') this.selectedMonth = '';
    if (period !== 'range' || this.validateReportPeriod()) {
      this.loadCategories();
      this.loadMonthlyMovements();
    }
  }

  applyDateRange(): void {
    this.reportPeriod = 'range';
    if (this.validateReportPeriod()) {
      this.loadCategories();
      this.loadMonthlyMovements();
    }
  }

  private validateReportPeriod(): boolean {
    if (this.reportPeriod !== 'range') return true;
    if (!/^\d{4}-\d{2}-\d{2}$/.test(this.dateFrom) || !/^\d{4}-\d{2}-\d{2}$/.test(this.dateTo) || this.dateFrom > this.dateTo) {
      this.errorMessage = 'حدد تاريخ بداية ونهاية صحيحين، ويجب ألا تكون البداية بعد النهاية.';
      return false;
    }
    this.errorMessage = '';
    return true;
  }

  get reportPeriodLabel(): string {
    if (this.reportPeriod === 'range') return `من ${this.dateFrom} إلى ${this.dateTo}`;
    if (this.reportPeriod === 'month') return this.getSelectedMonthLabel();
    return 'كل الفترات';
  }

  onYearChange(): void {
    this.reportPeriod = 'all';
    this.loadMonthlyMovements();
    this.loadCategories();
  }

  getSelectedMonthLabel(): string {
    if (!this.selectedMonth) return 'كل الشهور (الإجمالي التاريخي)';
    const found = this.monthlyMovements.find(m => m.monthYear === this.selectedMonth);
    if (found) return found.monthLabel;
    
    // Fallback format YYYY-MM
    const parts = this.selectedMonth.split('-');
    if (parts.length === 2) {
      return `شهر ${parseInt(parts[1], 10)}/${parts[0]}`;
    }
    return this.selectedMonth;
  }

  // ==========================================
  // التنقل بين شاشة القائمة وشاشة الإضافة/التعديل
  // ==========================================
  switchView(mode: 'list' | 'form'): void {
    this.viewMode = mode;
    this.errorMessage = '';
    this.successMessage = '';
    this.cdr.detectChanges();
  }

  openCreateForm(): void {
    const nextNumber = this.categories.length + 1;
    const formattedCode = `EXP-${nextNumber.toString().padStart(3, '0')}`;

    this.isEditing = false;
    this.categoryForm = {
      id: null,
      name: '',
      code: formattedCode,
      description: '',
      isActive: true
    };
    this.switchView('form');
  }

  openEditForm(cat: any): void {
    this.isEditing = true;
    this.categoryForm = {
      id: cat.id,
      name: cat.name,
      code: cat.code,
      description: cat.description || '',
      isActive: cat.isActive !== false
    };
    this.switchView('form');
  }

  saveCategory(): void {
    this.errorMessage = '';
    this.successMessage = '';

    const name = this.categoryForm.name?.trim();
    if (!name) {
      this.errorMessage = 'يرجى إدخال اسم أو مسمى بند المصروفات.';
      return;
    }

    this.isSaving = true;

    if (this.isEditing && this.categoryForm.id) {
      this.api.updateExpenseCategory(this.categoryForm.id, {
        name: name,
        code: this.categoryForm.code?.trim(),
        description: this.categoryForm.description?.trim(),
        isActive: this.categoryForm.isActive
      }).subscribe({
        next: (res: any) => {
          this.isSaving = false;
          this.successMessage = `✅ تم تحديث بند المصروفات (${res.name}) بنجاح.`;
          this.switchView('list');
          this.loadCategories();
        },
        error: (err: any) => {
          this.isSaving = false;
          this.errorMessage = err.error?.message || err.message || 'فشل في تحديث بند المصروفات.';
          this.cdr.detectChanges();
        }
      });
    } else {
      this.api.addExpenseCategory({
        name: name,
        code: this.categoryForm.code?.trim(),
        description: this.categoryForm.description?.trim()
      }).subscribe({
        next: (res: any) => {
          this.isSaving = false;
          this.successMessage = `✅ تم بنجاح إضافة بند المصروفات الجديد: (${res.name}).`;
          this.switchView('list');
          this.loadCategories();
        },
        error: (err: any) => {
          this.isSaving = false;
          this.errorMessage = err.error?.message || err.message || 'فشل في إضافة بند المصروفات.';
          this.cdr.detectChanges();
        }
      });
    }
  }

  deleteCategory(cat: any): void {
    this.errorMessage = '';
    this.successMessage = '';

    // التحقق الصارم في الواجهة قبل حتى الاتصال بالخادم
    if (!cat.canDelete || cat.disbursementsCount > 0) {
      this.errorMessage = `⛔ محظور الحذف: لا يمكن حذف بند المصروفات (${cat.name}) لوجود (${cat.disbursementsCount}) حركة صرف سابقة مسجلة عليه بإجمالي مبلغ (${cat.totalDisbursedAmount} ج.م)!`;
      window.scrollTo({ top: 0, behavior: 'smooth' });
      return;
    }

    const confirmed = confirm(`هل أنت متأكد من حذف بند المصروفات: (${cat.name})؟`);
    if (!confirmed) return;

    this.api.deleteExpenseCategory(cat.id).subscribe({
      next: () => {
        this.successMessage = `✅ تم حذف بند المصروفات (${cat.name}) بنجاح.`;
        this.loadCategories();
      },
      error: (err: any) => {
        this.errorMessage = err.error?.message || err.message || 'فشل في حذف بند المصروفات.';
        this.cdr.detectChanges();
      }
    });
  }

  // ==========================================
  // نافذة كشف الحالات: "من أخذ من هذا البند"
  // ==========================================
  openDrilldown(cat: any): void {
    this.selectedCategory = cat;
    this.showDrilldownModal = true;
    this.isLoadingRecipients = true;
    this.drilldownRecipients = [];
    this.drilldownSearchTerm = '';
    this.cdr.detectChanges();

    this.api.getExpenseCategoryBeneficiaries(cat.id,
      this.reportPeriod === 'month' ? this.selectedMonth || undefined : undefined,
      this.reportPeriod === 'range' ? this.dateFrom : undefined,
      this.reportPeriod === 'range' ? this.dateTo : undefined).subscribe({
      next: (data: any[]) => {
        this.drilldownRecipients = data || [];
        this.isLoadingRecipients = false;
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        this.isLoadingRecipients = false;
        this.errorMessage = err.error?.message || err.message || 'فشل في استرجاع سجل المستفيدين من هذا البند.';
        this.cdr.detectChanges();
      }
    });
  }

  closeDrilldown(): void {
    this.showDrilldownModal = false;
    this.selectedCategory = null;
    this.drilldownRecipients = [];
    this.drilldownSearchTerm = '';
    this.cdr.detectChanges();
  }

  printDrilldown(): void {
    window.print();
  }

  // ==========================================
  // الفلاتر والإحصائيات
  // ==========================================
  get filteredCategories(): any[] {
    const term = (this.searchTerm || '').trim().toLowerCase();
    if (!term) return this.categories;

    return this.categories.filter(c =>
      (c.name && c.name.toLowerCase().includes(term)) ||
      (c.code && c.code.toLowerCase().includes(term)) ||
      (c.description && c.description.toLowerCase().includes(term))
    );
  }

  get totalDisbursedAll(): number {
    return this.categories.reduce((acc, c) => acc + (Number(c.totalDisbursedAmount) || 0), 0);
  }

  get totalDisbursementsCount(): number {
    return this.categories.reduce((acc, c) => acc + (Number(c.disbursementsCount) || 0), 0);
  }

  get totalBeneficiariesCount(): number {
    return new Set(this.categories.flatMap(c => c.beneficiaryIds || [])).size;
  }

  get monthlyYearTotalAmount(): number {
    return this.monthlyMovements.reduce((acc, m) => acc + (Number(m.totalAmount) || 0), 0);
  }

  get monthlyYearTotalDisbursements(): number {
    return this.monthlyMovements.reduce((acc, m) => acc + (Number(m.disbursementsCount) || 0), 0);
  }

  get monthlyYearTotalBeneficiaries(): number {
    return new Set(this.monthlyMovements.flatMap(m => m.beneficiaryIds || [])).size;
  }

  get filteredRecipients(): any[] {
    const term = (this.drilldownSearchTerm || '').trim().toLowerCase();
    if (!term) return this.drilldownRecipients;

    return this.drilldownRecipients.filter(r =>
      (r.beneficiaryName && r.beneficiaryName.toLowerCase().includes(term)) ||
      (r.familyCode && r.familyCode.toLowerCase().includes(term)) ||
      (r.nationalId && r.nationalId.includes(term)) ||
      (r.receivedByName && r.receivedByName.toLowerCase().includes(term)) ||
      (r.notes && r.notes.toLowerCase().includes(term))
    );
  }
}
