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
    villageOrDistrict: '',
    priorityTier: 'أ - أولوية قصوى ومعدم',
    roofType: 'خرساني مسلح',
    wallType: 'طوب أحمر',
    waterSource: 'عداد مياه رسمي',
    sanitationType: 'شبكة صرف عمومية',
    electricitySource: 'عداد رسمي (كارت/قديم)',
    appliancesSummary: '',
    hasDebts: false,
    totalDebtAmount: null,
    debtCreditor: '',
    isThreatenedWithImprisonment: false,
    debtNotes: '',
    isCandidateForMicroProject: false,
    suggestedMicroProject: '',
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

  priorityTiers = [
    'أ - أولوية قصوى ومعدم',
    'ب - متوسط الاحتياج',
    'ج - طوارئ وظرف مؤقت'
  ];
  roofTypes = [
    'خرساني مسلح',
    'خشب وألواح',
    'جريد وقش وطين',
    'صاج زنك',
    'بدون سقف / مكشوف'
  ];
  wallTypes = [
    'طوب أحمر',
    'طوب لبن / طين',
    'دبش / حجر',
    'صاج وألواح',
    'بدون جدران كاملة'
  ];
  waterSources = [
    'عداد مياه رسمي',
    'طلمبة حبشية / مياه جوفية',
    'وصلة مياه من الجيران',
    'ملء من فناطيس / سبيل خارجي',
    'بدون مصدر مياه'
  ];
  sanitationTypes = [
    'شبكة صرف عمومية',
    'بيارة / طرنش',
    'صرف على مصرف زراعي',
    'بدون دورة مياه / محروم'
  ];
  electricitySources = [
    'عداد رسمي (كارت/قديم)',
    'ممارسة / وصلة',
    'وصلة من الجيران',
    'بدون كهرباء'
  ];
  educationStages = [
    'منتظم بالتعليم',
    'تعليم أساسي (ابتدائي)',
    'إعدادي',
    'ثانوي / فني',
    'جامعي',
    'أمي / بدون دراسة',
    'دون سن التعليم'
  ];

  // إيقاف الملف وحظر الصرف
  suspensionReasons: any[] = [];
  showSuspendModal: boolean = false;
  targetBeneficiaryToSuspend: any = null;
  selectedSuspensionReasonTitle: string = '';
  suspensionNotes: string = '';

  // فصل فرد من الأسرة
  showSeparateModal: boolean = false;
  targetMemberToSeparate: any = null;
  separateReason: string = 'استقلال بمسكن خاص وزواج';
  createNewProfileOnSeparate: boolean = true;

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
    this.api.getSuspensionReasons().subscribe((data: any[]) => {
      this.suspensionReasons = data;
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
      educationStage: 'منتظم بالتعليم',
      isSchoolDropout: false,
      isChildLabor: false,
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
      villageOrDistrict: '',
      priorityTier: 'أ - أولوية قصوى ومعدم',
      roofType: 'خرساني مسلح',
      wallType: 'طوب أحمر',
      waterSource: 'عداد مياه رسمي',
      sanitationType: 'شبكة صرف عمومية',
      electricitySource: 'عداد رسمي (كارت/قديم)',
      appliancesSummary: '',
      hasDebts: false,
      totalDebtAmount: null,
      debtCreditor: '',
      isThreatenedWithImprisonment: false,
      debtNotes: '',
      isCandidateForMicroProject: false,
      suggestedMicroProject: '',
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

  // ==========================================
  // إيقاف الملف وحظر الصرف
  // ==========================================
  openSuspendModal(b: any, event?: Event) {
    if (event) event.stopPropagation();
    if (!this.canEditBeneficiary(b)) {
      this.errorMessage = `🔒 غير مصرح: إيقاف الملف مقصور على الجمعية المسجلة له (${b.registeredByCharityName || 'الجمعية الأصلية'}).`;
      this.cdr.detectChanges();
      setTimeout(() => { this.errorMessage = ''; this.cdr.detectChanges(); }, 4000);
      return;
    }
    this.targetBeneficiaryToSuspend = b;
    this.selectedSuspensionReasonTitle = this.suspensionReasons.length > 0 ? this.suspensionReasons[0].title : 'وفاة صاحب الحالة';
    this.suspensionNotes = '';
    this.showSuspendModal = true;
    this.cdr.detectChanges();
  }

  closeSuspendModal() {
    this.showSuspendModal = false;
    this.targetBeneficiaryToSuspend = null;
    this.cdr.detectChanges();
  }

  confirmSuspend() {
    if (!this.targetBeneficiaryToSuspend) return;
    if (!this.selectedSuspensionReasonTitle) {
      this.errorMessage = 'يرجى اختيار أو تحديد سبب الإيقاف.';
      return;
    }

    const matchedReason = this.suspensionReasons.find(r => r.title === this.selectedSuspensionReasonTitle);

    this.api.suspendBeneficiary(
      this.targetBeneficiaryToSuspend.id,
      this.selectedSuspensionReasonTitle,
      matchedReason?.id,
      this.suspensionNotes
    ).subscribe({
      next: (updated: any) => {
        this.successMessage = `⛔ تم إيقاف ملف الحالة (${updated.name}) بنجاح، ولن يتمكن أي مستخدم من صرف أي مساعدة له.`;
        this.closeSuspendModal();
        this.loadData();
        if (this.selectedBen && String(this.selectedBen.id) === String(updated.id)) {
          this.selectedBen = updated;
        }
        setTimeout(() => { this.successMessage = ''; this.cdr.detectChanges(); }, 5000);
      },
      error: (err: any) => {
        this.errorMessage = err.message || 'فشل في إيقاف الملف.';
        this.cdr.detectChanges();
      }
    });
  }

  reactivateBeneficiary(b: any, event?: Event) {
    if (event) event.stopPropagation();
    if (!this.canEditBeneficiary(b)) {
      this.errorMessage = `🔒 غير مصرح: إعادة تفعيل الملف مقصورة على الجمعية المسجلة له (${b.registeredByCharityName || 'الجمعية الأصلية'}).`;
      this.cdr.detectChanges();
      setTimeout(() => { this.errorMessage = ''; this.cdr.detectChanges(); }, 4000);
      return;
    }

    if (confirm(`هل أنت متأكد من رغبتك في إعادة تفعيل ملف الحالة (${b.name}) والسماح بالصرف له مجدداً؟`)) {
      this.api.reactivateBeneficiary(b.id).subscribe({
        next: (updated: any) => {
          this.successMessage = `✅ تم إعادة تفعيل ملف المستفيد (${updated.name}) بنجاح، وأصبح متاحاً للصرف مجدداً.`;
          this.loadData();
          if (this.selectedBen && String(this.selectedBen.id) === String(updated.id)) {
            this.selectedBen = updated;
          }
          setTimeout(() => { this.successMessage = ''; this.cdr.detectChanges(); }, 5000);
        },
        error: (err: any) => {
          this.errorMessage = err.message || 'فشل في إعادة تفعيل الملف.';
          this.cdr.detectChanges();
        }
      });
    }
  }

  // ==========================================
  // فصل فرد من ملف الأسرة لاستقلاله بملف جديد
  // ==========================================
  openSeparateModal(m: any) {
    if (!this.canEditBeneficiary(this.selectedBen)) {
      this.errorMessage = `🔒 غير مصرح: إدارة وفصل أفراد الأسرة مقصورة على الجمعية المسجلة للملف (${this.selectedBen?.registeredByCharityName || 'الجمعية الأصلية'}).`;
      this.cdr.detectChanges();
      setTimeout(() => { this.errorMessage = ''; this.cdr.detectChanges(); }, 4000);
      return;
    }
    this.targetMemberToSeparate = m;
    this.separateReason = 'استقلال بمسكن خاص وزواج';
    this.createNewProfileOnSeparate = true;
    this.showSeparateModal = true;
    this.cdr.detectChanges();
  }

  closeSeparateModal() {
    this.showSeparateModal = false;
    this.targetMemberToSeparate = null;
    this.cdr.detectChanges();
  }

  confirmSeparateFamilyMember() {
    if (!this.selectedBen || !this.targetMemberToSeparate) return;

    this.api.separateFamilyMember(
      this.selectedBen.id,
      this.targetMemberToSeparate.id,
      this.createNewProfileOnSeparate,
      this.separateReason
    ).subscribe({
      next: (res: any) => {
        const actionDesc = this.createNewProfileOnSeparate 
          ? `وفُتح له ملف حالة جديد ومستقل بنجاح (برقم قومي: ${this.targetMemberToSeparate.nationalId})!`
          : 'وفك قيد رقمه القومي ليصبح متاحاً للتسجيل المستقل لاحقاً.';
        this.successMessage = `✅ تم فصل فرد الأسرة (${this.targetMemberToSeparate.name}) من هذا الملف، ${actionDesc}`;
        this.closeSeparateModal();
        this.loadData();
        if (res.updatedOriginalBeneficiary) {
          this.selectedBen = res.updatedOriginalBeneficiary;
        }
        setTimeout(() => { this.successMessage = ''; this.cdr.detectChanges(); }, 6000);
      },
      error: (err: any) => {
        this.errorMessage = err.message || 'فشل في فصل فرد الأسرة.';
        this.cdr.detectChanges();
      }
    });
  }

  printProfile() {
    window.print();
  }
}
