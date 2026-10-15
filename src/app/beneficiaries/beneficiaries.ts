import { Component, ChangeDetectorRef, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { Api } from '../api';
import { PaginationComponent } from '../pagination/pagination';

@Component({
  selector: 'app-beneficiaries',
  standalone: true,
  imports: [CommonModule, FormsModule, PaginationComponent],
  templateUrl: './beneficiaries.html',
  styleUrls: ['./beneficiaries.css']
})
export class BeneficiariesComponent implements OnInit, OnDestroy {
  // Navigation / Views: 'list' | 'detail' | 'form'
  viewMode: 'list' | 'detail' | 'form' = 'list';
  formMode: 'add' | 'edit' = 'add';
  selectedBen: any = null;

  beneficiaries: any[] = [];
  distributions: any[] = [];
  inventory: any[] = [];

  // Search & Filtering
  searchTerm: string = '';
  selectedCategoryFilter: string = 'الكل';

  // Pagination
  currentPage: number = 1;
  pageSize: number = 5;

  // Alerts
  successMessage: string = '';
  errorMessage: string = '';

  currentBen: any = { 
    id: null,
    nationalId: '', 
    name: '', 
    phone: '',
    category: 'أرامل وأيتام', 
    maritalStatus: 'متزوج',
    familyCount: 1,
    address: '', 
    housing: 'إيجار جديد',
    healthIssues: '',
    researchNotes: '',
    status: 'مستحق فعال' 
  };

  categories = ['أرامل وأيتام', 'مرضى ومسنين', 'غارمين', 'حالات طوارئ', 'طالب علم'];
  maritalStatuses = ['متزوج', 'أرمل', 'مطلق', 'أعزب'];
  housingTypes = ['إيجار جديد', 'إيجار قديم', 'ملك', 'سكن مشترك', 'سكن متهالك/يحتاج ترميم'];

  private querySub?: Subscription;

  constructor(
    private api: Api, 
    private cdr: ChangeDetectorRef,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadData();
  }

  ngOnDestroy(): void {
    if (this.querySub) {
      this.querySub.unsubscribe();
    }
  }

  loadData() {
    this.api.getBeneficiaries().subscribe((data: any[]) => { 
      this.beneficiaries = data; 
      this.initRouteSync();
      this.cdr.detectChanges(); 
    });
    // Load ALL distributions across the federated network for the beneficiary profile
    this.api.getAllDistributions().subscribe((data: any[]) => { 
      this.distributions = data; 
      this.cdr.detectChanges(); 
    });
    this.api.getAllInventory().subscribe((data: any[]) => { 
      this.inventory = data; 
      this.cdr.detectChanges(); 
    });
  }

  initRouteSync() {
    if (this.querySub) return;
    this.querySub = this.route.queryParams.subscribe(params => {
      const mode = params['mode'];
      const id = params['id'];

      if (mode === 'detail' && id) {
        const found = this.beneficiaries.find(b => String(b.id) === String(id));
        if (found) {
          this.selectedBen = found;
          this.viewMode = 'detail';
        } else {
          this.viewMode = 'list';
        }
      } else if (mode === 'edit' && id) {
        const found = this.beneficiaries.find(b => String(b.id) === String(id));
        if (found) {
          if (!this.canEditBeneficiary(found)) {
            this.errorMessage = `🔒 غير مصرح: لا تملك صلاحية تعديل هذا المستفيد! الملف مسجل ومملوك لـ (${found.registeredByCharityName || 'الجمعية الأصلية'}). صلاحيتكم تقتصر على الاطلاع وسجل المساعدات وإضافة حركات صرف جديدة.`;
            this.router.navigate([], { relativeTo: this.route, queryParams: { mode: 'detail', id: found.id } });
            return;
          }
          this.currentBen = { ...found };
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
        this.selectedBen = null;
      }
      this.cdr.detectChanges();
    });
  }

  // Authorization helper: ONLY the registering charity has edit permissions
  canEditBeneficiary(b: any): boolean {
    if (!b) return false;
    const activeId = this.api.getActiveCharityId();
    return !b.registeredByCharityId || b.registeredByCharityId === activeId;
  }

  // View switchers
  openDetail(b: any) {
    this.router.navigate([], { relativeTo: this.route, queryParams: { mode: 'detail', id: b.id } });
  }

  openAdd() {
    this.router.navigate([], { relativeTo: this.route, queryParams: { mode: 'add' } });
  }

  openEdit(b: any, event?: Event) {
    if (event) event.stopPropagation();
    if (!this.canEditBeneficiary(b)) {
      this.errorMessage = `🔒 عذراً، التعديل مقصور قانونياً على الجمعية التي أنشأت الملف أول مرة (${b.registeredByCharityName || 'الجمعية المسجلة'}).`;
      this.cdr.detectChanges();
      setTimeout(() => { this.errorMessage = ''; this.cdr.detectChanges(); }, 5000);
      return;
    }
    this.router.navigate([], { relativeTo: this.route, queryParams: { mode: 'edit', id: b.id } });
  }

  backToList() {
    this.router.navigate([], { relativeTo: this.route, queryParams: {} });
  }

  // Distribution History for selected beneficiary across ALL charities in the network
  getBeneficiaryDistributions(benId: string): any[] {
    const activeId = this.api.getActiveCharityId();
    return (this.distributions || [])
      .filter(dist => String(dist.beneficiaryId) === String(benId))
      .map(dist => {
        const item = (this.inventory || []).find(i => String(i.id) === String(dist.inventoryId));
        return {
          ...dist,
          kind: dist.kind || (item ? (item.kind || 'صنف ملموس') : 'صنف ملموس'),
          itemName: dist.itemName || (item ? item.itemName : 'مساعدة معتمدة'),
          unit: dist.unit || (item ? (item.unit || '') : ''),
          charityName: dist.charityName || 'جمعية شقيقة',
          isFromCurrentCharity: dist.charityId === activeId
        };
      });
  }

  // Returns aid received from other partner charities (The Quantum Leap feature)
  getOtherCharitiesAid(benId: string): any[] {
    const activeId = this.api.getActiveCharityId();
    return this.getBeneficiaryDistributions(benId).filter(d => d.charityId && d.charityId !== activeId);
  }

  // Filter & Search
  get filteredBeneficiaries(): any[] {
    return this.beneficiaries.filter(b => {
      const matchCat = this.selectedCategoryFilter === 'الكل' || b.category === this.selectedCategoryFilter;
      const term = this.searchTerm.trim().toLowerCase();
      const matchSearch = !term ||
        (b.name && b.name.toLowerCase().includes(term)) ||
        (b.nationalId && b.nationalId.includes(term)) ||
        (b.phone && b.phone.includes(term)) ||
        (b.address && b.address.toLowerCase().includes(term));
      return matchCat && matchSearch;
    });
  }

  get paginatedBeneficiaries(): any[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.filteredBeneficiaries.slice(start, start + this.pageSize);
  }

  resetForm() {
    this.currentBen = { 
      id: null,
      nationalId: '', 
      name: '', 
      phone: '', 
      category: 'أرامل وأيتام', 
      maritalStatus: 'متزوج',
      familyCount: 1, 
      address: '', 
      housing: 'إيجار جديد', 
      healthIssues: '', 
      researchNotes: '', 
      status: 'مستحق فعال' 
    };
  }

  onSubmit() {
    if (!this.currentBen.name || !this.currentBen.nationalId) {
      this.errorMessage = 'يرجى إدخال اسم المستفيد والرقم القومي بشكل صحيح.';
      return;
    }

    this.errorMessage = '';

    if (this.currentBen.id) {
      try {
        this.api.updateBeneficiary(this.currentBen).subscribe({
          next: () => {
            this.successMessage = `✅ تم تحديث بيانات المستفيد (${this.currentBen.name}) بنجاح!`;
            this.api.getBeneficiaries().subscribe((data: any[]) => {
              this.beneficiaries = data;
              this.openDetail(this.currentBen);
              this.cdr.detectChanges();
            });
            setTimeout(() => { this.successMessage = ''; this.cdr.detectChanges(); }, 4000);
          },
          error: (err: any) => {
            this.errorMessage = err.message || '⚠️ غير مصرح بتعديل هذا الملف.';
            this.cdr.detectChanges();
          }
        });
      } catch (e: any) {
        this.errorMessage = e.message || '⚠️ غير مصرح بتعديل هذا الملف.';
        this.cdr.detectChanges();
      }
    } else {
      this.api.addBeneficiary(this.currentBen).subscribe((added: any) => {
        this.successMessage = `✅ تم تسجيل وفتح ملف الحالة الجديد (${added.name}) بنجاح!`;
        this.api.getBeneficiaries().subscribe((data: any[]) => {
          this.beneficiaries = data;
          this.openDetail(added);
          this.cdr.detectChanges();
        });
        setTimeout(() => { this.successMessage = ''; this.cdr.detectChanges(); }, 4000);
      });
    }
  }

  printProfile() {
    window.print();
  }
}
