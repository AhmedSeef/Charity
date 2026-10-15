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
    status: 'مستحق فعال',
    incomeSource: '',
    monthlyIncome: null,
    hasLandOrProperty: false,
    landOrPropertyDetails: '',
    hasGovernmentPension: false,
    governmentPensionDetails: '',
    hasChronicDisease: false,
    chronicDiseaseDetails: '',
    needsPeriodicMedication: false,
    periodicMedicationDetails: '',
    medicationFrequency: '',
    familyMembers: []
  };

  categories = ['أرامل وأيتام', 'مرضى ومسنين', 'غارمين', 'حالات طوارئ', 'طالب علم'];
  maritalStatuses = ['متزوج', 'أرمل', 'مطلق', 'أعزب'];
  housingTypes = ['إيجار جديد', 'إيجار قديم', 'ملك', 'سكن مشترك', 'سكن متهالك/يحتاج ترميم'];
  relationships = ['زوج/زوجة', 'ابن', 'ابنة', 'والد', 'والدة', 'أخ/أخت', 'حفيد', 'جد/جدة', 'أخرى'];
  memberTypes = ['معال', 'معيل'];
  supportTypes = ['طالب', 'مريض', 'طفل رضيع', 'مسن', 'عاطل عن العمل', 'من ذوي الهمم', 'ربة منزل', 'أخرى'];
  medicationFrequencies = ['أسبوعياً', 'كل أسبوعين', 'شهرياً', 'كل شهرين', 'كل 3 أشهر', 'حسب الحاجة / عند اللزوم'];

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
          this.currentBen = { 
            ...found, 
            familyMembers: (found.familyMembers || []).map((m: any) => ({ ...m })) 
          };
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

  addFamilyMember() {
    if (!this.currentBen.familyMembers) {
      this.currentBen.familyMembers = [];
    }
    this.currentBen.familyMembers.push({
      id: 'fm_' + Date.now().toString() + '_' + Math.floor(Math.random() * 1000),
      nationalId: '',
      name: '',
      relationship: 'ابن',
      memberType: 'معال',
      hasIncome: false,
      incomeAmount: null,
      incomeSource: '',
      supportType: 'طالب',
      healthStatus: '',
      notes: ''
    });
    this.syncFamilyCount();
  }

  removeFamilyMember(index: number) {
    if (this.currentBen.familyMembers && this.currentBen.familyMembers.length > index) {
      this.currentBen.familyMembers.splice(index, 1);
      this.syncFamilyCount();
    }
  }

  syncFamilyCount() {
    const membersCount = this.currentBen.familyMembers ? this.currentBen.familyMembers.length : 0;
    this.currentBen.familyCount = membersCount + 1;
  }

  get dependentsCount(): number {
    const list = (this.viewMode === 'detail' ? this.selectedBen?.familyMembers : this.currentBen?.familyMembers) || [];
    return list.filter((m: any) => m.memberType === 'معال').length;
  }

  get supportersCount(): number {
    const list = (this.viewMode === 'detail' ? this.selectedBen?.familyMembers : this.currentBen?.familyMembers) || [];
    return list.filter((m: any) => m.memberType === 'معيل').length;
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
      status: 'مستحق فعال',
      incomeSource: '',
      monthlyIncome: null,
      hasLandOrProperty: false,
      landOrPropertyDetails: '',
      hasGovernmentPension: false,
      governmentPensionDetails: '',
      hasChronicDisease: false,
      chronicDiseaseDetails: '',
      needsPeriodicMedication: false,
      periodicMedicationDetails: '',
      medicationFrequency: '',
      familyMembers: []
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
            this.errorMessage = err.message || '⚠️ حدث خطأ أثناء التعديل.';
            this.cdr.detectChanges();
          }
        });
      } catch (e: any) {
        this.errorMessage = e.message || '⚠️ حدث خطأ أثناء التعديل.';
        this.cdr.detectChanges();
      }
    } else {
      try {
        this.api.addBeneficiary(this.currentBen).subscribe({
          next: (added: any) => {
            this.successMessage = `✅ تم تسجيل وفتح ملف الحالة الجديد (${added.name}) بنجاح!`;
            this.api.getBeneficiaries().subscribe((data: any[]) => {
              this.beneficiaries = data;
              this.openDetail(added);
              this.cdr.detectChanges();
            });
            setTimeout(() => { this.successMessage = ''; this.cdr.detectChanges(); }, 4000);
          },
          error: (err: any) => {
            this.errorMessage = err.message || '⚠️ حدث خطأ أثناء الحفظ.';
            this.cdr.detectChanges();
          }
        });
      } catch (e: any) {
        this.errorMessage = e.message || '⚠️ حدث خطأ أثناء الحفظ.';
        this.cdr.detectChanges();
      }
    }
  }

  printProfile() {
    window.print();
  }
}
