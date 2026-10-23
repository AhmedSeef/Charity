import { Component, ChangeDetectorRef, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Subscription } from 'rxjs';
import { Api } from '../api';
import { AuthService } from '../auth.service';
import { PaginationComponent } from '../pagination/pagination';
import { nationalIdError, normalizeNationalId } from '../egyptian-national-id';

@Component({
  selector: 'app-beneficiaries',
  standalone: true,
  imports: [CommonModule, FormsModule, PaginationComponent, RouterModule],
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

  // Geographic Lookups & Autocomplete
  governorates: any[] = [];
  allDistricts: any[] = [];
  filteredDistricts: any[] = [];
  allVillages: any[] = [];
  filteredVillages: any[] = [];
  allVillageAreas: any[] = [];
  filteredVillageAreas: any[] = [];
  selectedGovFilter: string = 'الكل';
  selectedDistrictFilter: string = 'الكل';
  filterDistrictsList: any[] = [];
  addressSuggestions: any[] = [];
  showAddressSuggestions: boolean = false;
  private addressAutocompleteTimer: any;

  currentBen: any = { 
    id: null,
    familyCode: '',
    familyName: '',
    nationalId: '', 
    name: '', 
    phone: '',
    category: 'أرامل وأيتام', 
    maritalStatus: 'متزوج',
    familyCount: 1,
    governorateId: 'gov_kfs',
    governorateName: 'كفر الشيخ',
    districtId: 'dist_motobas',
    districtName: 'مركز مطوبس',
    villageId: 'vil_monyat_morshid',
    villageName: 'منية المرشد',
    villageAreaId: 'area_manshar',
    villageAreaName: 'المنشر',
    addressId: null,
    address: '', 
    housing: 'إيجار جديد',
    villageOrDistrict: 'المنشر - منية المرشد',
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
  separationReasons: any[] = [];
  selectedSeparationReasonId: string = '';
  createNewProfileOnSeparate: boolean = true;

  // نقل ولاية الملف (تعيين عائل جديد للأسرة)
  showTransferHeadModal: boolean = false;
  selectedNewHeadMemberId: string = '';
  formerHeadAction: 'KeepAsFamilyMember' | 'RemoveFromFamily' = 'KeepAsFamilyMember';
  keepReasons = [
    'عجز كلي أو مرض مزمن مقعد ♿ (العائل حي ومقيم كمعال)',
    'كبر سن وعدم قدرة على السعي أو العمل 🧓',
    'ظروف صحية تمنعه من الخروج والمتابعة 🩺',
    'تنازل اختياري للزوجة أو الابن لإدارة شؤون الأسرة 🤝',
    'سفر مؤقت للعمل مع بقاء مسؤوليته بالأسرة 🧳',
    'سبب آخر (مع بقائه كفرد معال بالأسرة)'
  ];
  removeReasons = [
    'وفاة رب الأسرة السابق 🕊️ (رحمه الله)',
    'انفصال وطلاق وهجر الأسرة بشكل دائم 🚪',
    'سفر دائم وانقطاع الصلة بالأسرة تماماً ✈️',
    'خروج نهائي واستقلال عن الأسرة',
    'سبب آخر (مع استبعاده من أفراد الأسرة)'
  ];
  transferHeadReason: string = 'عجز كلي أو مرض مزمن مقعد ♿ (العائل حي ومقيم كمعال)';
  formerHeadRelationship: string = 'زوج (عائل سابق / مريض معال)';
  formerHeadIsAlive: boolean = true;
  formerHeadSupportType: string = 'مريض / عاجز عن العمل';
  newFamilyNameInput: string = '';
  transferHeadNotes: string = '';
  showAdvancedTransferOptions: boolean = false;
  showAddMemberInline: boolean = false;
  isAddingMember: boolean = false;
  newMemberData: any = {
    name: '',
    nationalId: '',
    relationship: 'زوجة',
    memberType: 'معال',
    supportType: 'ربة منزل',
    educationStage: 'منتظم بالتعليم',
    notes: ''
  };
  get currentTransferReasons(): string[] {
    return this.formerHeadAction === 'KeepAsFamilyMember' ? this.keepReasons : this.removeReasons;
  }

  private querySub?: Subscription;

  constructor(
    private api: Api, 
    private cdr: ChangeDetectorRef,
    private route: ActivatedRoute,
    private router: Router,
    public auth: AuthService
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
    if (this.auth.getCurrentUser()?.role === 'Admin' || this.auth.hasPermission('inventory.view') || this.auth.hasPermission('inventory.manage')) {
      this.api.getAllInventory().subscribe({
        next: (data: any[]) => { 
          this.inventory = data; 
          this.cdr.detectChanges(); 
        },
        error: () => {
          this.inventory = [];
        }
      });
    }
    this.api.getSuspensionReasons().subscribe((data: any[]) => {
      this.suspensionReasons = data;
      this.cdr.detectChanges();
    });
    this.loadGeographicLookups();
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
          this.loadSharedHistory(found.id);
        } else {
          this.viewMode = 'list';
        }
      } else if (mode === 'edit' && id) {
        const found = this.beneficiaries.find(b => String(b.id) === String(id));
        if (found) {
          if (!this.canEditBeneficiary(found)) {
            this.errorMessage = this.auth.getCurrentUser()?.role === 'Admin'
              ? '🔒 صلاحية مدير المنصة تقتصر على الاطلاع والبحث؛ التعديل مقصور على الجمعيات المسجلة.'
              : `🔒 غير مصرح: لا تملك صلاحية تعديل هذا المستفيد! الملف مسجل ومملوك لـ (${found.registeredByCharityName || 'الجمعية الأصلية'}).`;
            this.router.navigate([], { relativeTo: this.route, queryParams: { mode: 'detail', id: found.id } });
            return;
          }
          this.currentBen = { 
            ...found, 
            familyMembers: (found.familyMembers || []).map((m: any) => ({ ...m })) 
          };
          this.filterDistrictsByGov();
          this.formMode = 'edit';
          this.viewMode = 'form';
        } else {
          this.viewMode = 'list';
        }
      } else if (mode === 'add') {
        if (this.auth.getCurrentUser()?.role === 'Admin') {
          this.errorMessage = '🔒 غير مصرح لمدير المنصة إضافة مستفيدين، الإضافة مقصورة على مسؤولي الجمعيات.';
          this.router.navigate([], { relativeTo: this.route, queryParams: {} });
          return;
        }
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

  // Authorization helper: ONLY the registering charity has edit permissions (Admin is strictly read-only)
  canEditBeneficiary(b: any): boolean {
    if (!b) return false;
    if (this.auth.getCurrentUser()?.role === 'Admin') return false;
    if (!this.auth.hasPermission('beneficiaries.edit')) return false;
    const activeId = this.auth.getCurrentUser()?.charityId;
    return !!activeId && b.registeredByCharityId === activeId;
  }

  canDeleteBeneficiary(b: any): boolean {
    if (!b) return false;
    if (this.auth.getCurrentUser()?.role === 'Admin') return false;
    if (!this.auth.hasPermission('beneficiaries.delete')) return false;
    const activeId = this.auth.getCurrentUser()?.charityId;
    return !!activeId && b.registeredByCharityId === activeId;
  }

  private loadSharedHistory(id: string) {
    this.distributions = [];
    this.api.getBeneficiarySharedHistory(id).subscribe({
      next: summary => {
        if (this.selectedBen?.id !== id || this.viewMode !== 'detail') return;
        this.distributions = summary.aidHistory || [];
        this.cdr.detectChanges();
      },
      error: () => {
        if (this.selectedBen?.id !== id || this.viewMode !== 'detail') return;
        this.errorMessage = 'تعذر تحميل سجل مساعدات المستفيد.';
        this.cdr.detectChanges();
      }
    });
  }

  // View switchers
  openDetail(b: any) {
    this.router.navigate([], { relativeTo: this.route, queryParams: { mode: 'detail', id: b.id } });
  }

  openAdd() {
    if (this.auth.getCurrentUser()?.role === 'Admin') {
      this.errorMessage = '🔒 غير مصرح لمدير المنصة إضافة مستفيدين، الإضافة مقصورة على مسؤولي الجمعيات.';
      this.cdr.detectChanges();
      return;
    }
    if (!this.auth.hasPermission('beneficiaries.create')) {
      this.errorMessage = '🔒 ليس لديك صلاحية فتح ملفات حالات جديدة.';
      this.cdr.detectChanges();
      return;
    }
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
      const matchGov = this.selectedGovFilter === 'الكل' || b.governorateId === this.selectedGovFilter;
      const matchDist = this.selectedDistrictFilter === 'الكل' || b.districtId === this.selectedDistrictFilter;
      const term = this.searchTerm.trim().toLowerCase();
      const matchSearch = !term ||
        (b.name && b.name.toLowerCase().includes(term)) ||
        (b.nationalId && b.nationalId.includes(term)) ||
        (b.familyCode && b.familyCode.toLowerCase().includes(term)) ||
        (b.familyName && b.familyName.toLowerCase().includes(term)) ||
        (b.phone && b.phone.includes(term)) ||
        (b.address && b.address.toLowerCase().includes(term)) ||
        (b.villageOrDistrict && b.villageOrDistrict.toLowerCase().includes(term)) ||
        (b.governorateName && b.governorateName.toLowerCase().includes(term)) ||
        (b.districtName && b.districtName.toLowerCase().includes(term)) ||
        (b.familyMembers && b.familyMembers.some((m: any) =>
          (m.name && m.name.toLowerCase().includes(term)) ||
          (m.nationalId && m.nationalId.includes(term))
        ));
      return matchCat && matchGov && matchDist && matchSearch;
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
      familyCode: '',
      familyName: '',
      nationalId: '', 
      name: '', 
      phone: '', 
      category: 'أرامل وأيتام', 
      maritalStatus: 'متزوج',
      familyCount: 1, 
      headHistories: [],
      governorateId: 'gov_kfs',
      governorateName: 'كفر الشيخ',
      districtId: 'dist_motobas',
      districtName: 'مركز مطوبس',
      villageId: 'vil_monyat_morshid',
      villageName: 'منية المرشد',
      villageAreaId: 'area_manshar',
      villageAreaName: 'المنشر',
      addressId: null,
      address: '', 
      housing: 'إيجار جديد',
      villageOrDistrict: 'المنشر - منية المرشد',
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

    // جلب كود العائلة التلقائي الموحد من السيرفر
    this.api.getNextFamilyCode().subscribe({
      next: (res) => {
        if (this.formMode === 'add' || !this.currentBen?.id) {
          this.currentBen.familyCode = res.familyCode;
          this.cdr.detectChanges();
        }
      },
      error: () => {
        if (this.formMode === 'add' || !this.currentBen?.id) {
          this.currentBen.familyCode = 'يولّد تلقائياً (FAM-26-XXXX)';
          this.cdr.detectChanges();
        }
      }
    });
  }

  onSubmit() {
    if (this.auth.getCurrentUser()?.role === 'Admin') {
      this.errorMessage = '🔒 غير مصرح لمدير المنصة إضافة أو تعديل مستفيدين.';
      this.cdr.detectChanges();
      return;
    }

    const headName = (this.currentBen.name || '').trim();
    if (!headName) {
      this.errorMessage = 'اسم المستفيد مطلوب ولا يمكن أن يكون مسافات فقط.';
      return;
    }
    this.currentBen.name = headName;
    if (this.invalidIncome(this.currentBen.monthlyIncome)) {
      this.errorMessage = 'الدخل الشهري يجب أن يكون صفراً أو أكثر، ولا يقبل قيمة سالبة.';
      return;
    }
    if (!Number.isInteger(Number(this.currentBen.familyCount)) || Number(this.currentBen.familyCount) < 1) {
      this.errorMessage = 'عدد أفراد الأسرة يجب أن يكون واحداً على الأقل.';
      return;
    }

    const headError = nationalIdError(this.currentBen.nationalId);
    if (headError) {
      this.errorMessage = `الرقم القومي لصاحب الملف: ${headError}`;
      return;
    }
    this.currentBen.nationalId = normalizeNationalId(this.currentBen.nationalId);
    for (const member of this.currentBen.familyMembers || []) {
      const memberName = (member.name || '').trim();
      if (!memberName) {
        this.errorMessage = 'اسم فرد الأسرة مطلوب ولا يمكن أن يكون مسافات فقط.';
        return;
      }
      if (this.invalidIncome(member.incomeAmount)) {
        this.errorMessage = `دخل فرد الأسرة (${memberName}) يجب أن يكون صفراً أو أكثر.`;
        return;
      }
      member.name = memberName;
      const memberError = nationalIdError(member.nationalId);
      if (memberError) {
        this.errorMessage = `الرقم القومي لفرد الأسرة (${member.name || 'بدون اسم'}): ${memberError}`;
        return;
      }
      member.nationalId = normalizeNationalId(member.nationalId);
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
    this.createNewProfileOnSeparate = true;

    // جلب أسباب الانفصال المعتمدة ديناميكياً
    this.api.getSeparationReasons().subscribe({
      next: (data) => {
        this.separationReasons = data || [];
        if (this.separationReasons.length > 0) {
          this.selectedSeparationReasonId = this.separationReasons[0].id;
          this.separateReason = this.separationReasons[0].title;
        } else {
          this.separateReason = 'استقلال بمسكن خاص وزواج';
        }
        this.cdr.detectChanges();
      }
    });

    this.showSeparateModal = true;
    this.cdr.detectChanges();
  }

  onSeparationReasonChange() {
    const found = this.separationReasons.find(r => String(r.id) === String(this.selectedSeparationReasonId));
    if (found) {
      this.separateReason = found.title;
    }
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
      this.separateReason,
      this.selectedSeparationReasonId
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

  // ==========================================
  // نقل ولاية الملف (تعيين عائل جديد للأسرة)
  // ==========================================
  openTransferHeadModal() {
    if (!this.canEditBeneficiary(this.selectedBen)) {
      this.errorMessage = `🔒 غير مصرح: نقل ولاية الملف مقصورة على الجمعية المسجلة للملف (${this.selectedBen?.registeredByCharityName || 'الجمعية الأصلية'}).`;
      this.cdr.detectChanges();
      setTimeout(() => { this.errorMessage = ''; this.cdr.detectChanges(); }, 4000);
      return;
    }
    this.formerHeadAction = 'KeepAsFamilyMember';
    const eligibleMembers = this.getEligibleNewHeads();
    this.selectedNewHeadMemberId = eligibleMembers.length > 0 ? eligibleMembers[0].id : '';
    this.transferHeadReason = this.keepReasons[0];
    this.transferHeadNotes = '';
    this.showAdvancedTransferOptions = false;
    this.showAddMemberInline = eligibleMembers.length === 0;
    this.resetNewMemberData();
    this.updateFormerHeadDetails(eligibleMembers.length > 0 ? eligibleMembers[0] : null);
    this.showTransferHeadModal = true;
    this.cdr.detectChanges();
  }

  closeTransferHeadModal() {
    this.showTransferHeadModal = false;
    this.showAddMemberInline = false;
    this.cdr.detectChanges();
  }

  setFormerHeadAction(action: 'KeepAsFamilyMember' | 'RemoveFromFamily') {
    this.formerHeadAction = action;
    this.transferHeadReason = action === 'KeepAsFamilyMember' ? this.keepReasons[0] : this.removeReasons[0];
    this.updateFormerHeadDetails();
    this.cdr.detectChanges();
  }

  getEligibleNewHeads(): any[] {
    if (!this.selectedBen || !this.selectedBen.familyMembers) return [];
    return this.selectedBen.familyMembers.filter((m: any) => 
      !m.relationship?.includes('متوفى') && 
      !m.supportType?.includes('متوفى') && 
      !m.isSeparated
    );
  }

  resetNewMemberData() {
    this.newMemberData = {
      name: '',
      nationalId: '',
      relationship: 'زوجة',
      memberType: 'معال',
      supportType: 'ربة منزل',
      educationStage: 'منتظم بالتعليم',
      notes: ''
    };
  }

  toggleAddMemberInline() {
    this.showAddMemberInline = !this.showAddMemberInline;
    if (this.showAddMemberInline) {
      this.resetNewMemberData();
    }
    this.cdr.detectChanges();
  }

  onTransferReasonChange() {
    this.updateFormerHeadDetails();
    this.cdr.detectChanges();
  }

  updateFormerHeadDetails(member?: any) {
    member = member || this.getSelectedNewHeadMember();
    const isDeath = this.formerHeadAction === 'RemoveFromFamily' && this.transferHeadReason?.includes('وفاة');
    this.formerHeadIsAlive = !isDeath;

    if (this.formerHeadAction === 'KeepAsFamilyMember') {
      this.formerHeadSupportType = 'مريض / عاجز عن العمل';
      const originalFamilyName = (this.selectedBen?.familyName || `عائلة ${this.selectedBen?.name || ''}`).replace('المرحوم ', '').trim();
      this.newFamilyNameInput = originalFamilyName;

      if (member?.relationship?.includes('زوج')) {
        this.formerHeadRelationship = 'زوج (عائل سابق / مريض معال)';
      } else if (member?.relationship?.includes('ابن') || member?.relationship?.includes('ابنة')) {
        this.formerHeadRelationship = 'والد (عائل سابق / كبير سن)';
      } else {
        this.formerHeadRelationship = 'عائل سابق (حي ومعال بالأسرة)';
      }
    } else {
      // RemoveFromFamily (حذف / استبعاد العائل السابق)
      if (isDeath) {
        this.formerHeadRelationship = 'رب الأسرة السابق (متوفى)';
        this.formerHeadSupportType = 'متوفى';
        this.newFamilyNameInput = `عائلة المرحوم ${this.selectedBen?.name || ''}`.trim();
      } else {
        this.formerHeadRelationship = 'عائل سابق (منفصل / خارج الأسرة)';
        this.formerHeadSupportType = 'منفصل / خارج الأسرة';
        this.newFamilyNameInput = (this.selectedBen?.familyName || `عائلة ${this.selectedBen?.name || ''}`).replace('المرحوم ', '').trim();
      }
    }
  }

  selectNewHeadMember(member: any) {
    if (!member) return;
    this.selectedNewHeadMemberId = member.id;
    this.updateFormerHeadDetails(member);
    this.cdr.detectChanges();
  }

  private invalidIncome(value: any): boolean {
    return value != null && value !== '' && (!Number.isFinite(Number(value)) || Number(value) < 0);
  }

  saveAndSelectNewMember() {
    if (!this.selectedBen) return;
    const name = (this.newMemberData.name || '').trim();
    const nid = normalizeNationalId(this.newMemberData.nationalId);

    if (!name) {
      this.errorMessage = 'يرجى إدخال اسم فرد الأسرة بالكامل.';
      this.cdr.detectChanges();
      return;
    }
    if (this.invalidIncome(this.newMemberData.incomeAmount)) {
      this.errorMessage = 'دخل فرد الأسرة يجب أن يكون صفراً أو أكثر.';
      this.cdr.detectChanges();
      return;
    }
    this.newMemberData.name = name;

    const identityError = nationalIdError(nid);
    if (identityError) {
      this.errorMessage = identityError;
      this.cdr.detectChanges();
      return;
    }
    this.newMemberData.nationalId = nid;

    this.isAddingMember = true;
    this.errorMessage = '';

    this.api.addFamilyMember(this.selectedBen.id, this.newMemberData).subscribe({
      next: (createdMember: any) => {
        this.isAddingMember = false;
        if (!this.selectedBen.familyMembers) {
          this.selectedBen.familyMembers = [];
        }
        this.selectedBen.familyMembers.push(createdMember);
        this.selectNewHeadMember(createdMember);
        this.showAddMemberInline = false;
        this.resetNewMemberData();
        this.successMessage = `✅ تمت إضافة (${createdMember.name}) للأسرة بنجاح وتم اختياره تلقائياً كعائل جديد للملف.`;
        this.cdr.detectChanges();
        setTimeout(() => { this.successMessage = ''; this.cdr.detectChanges(); }, 4500);
      },
      error: (err: any) => {
        this.isAddingMember = false;
        this.errorMessage = err.error?.message || err.message || 'فشل في إضافة فرد الأسرة.';
        this.cdr.detectChanges();
      }
    });
  }

  getSelectedNewHeadMember(): any {
    if (!this.selectedBen || !this.selectedBen.familyMembers) return null;
    return this.selectedBen.familyMembers.find((m: any) => String(m.id) === String(this.selectedNewHeadMemberId));
  }

  confirmTransferHeadship() {
    if (!this.selectedBen || !this.selectedNewHeadMemberId) {
      this.errorMessage = 'يرجى اختيار العائل الجديد من بين أفراد الأسرة المسجلين.';
      this.cdr.detectChanges();
      return;
    }

    const chosenMember = this.getSelectedNewHeadMember();

    this.api.transferHeadship(this.selectedBen.id, {
      newHeadFamilyMemberId: this.selectedNewHeadMemberId,
      formerHeadAction: this.formerHeadAction,
      reason: this.transferHeadReason,
      formerHeadRelationship: this.formerHeadRelationship,
      formerHeadSupportType: this.formerHeadSupportType,
      newFamilyName: this.newFamilyNameInput,
      notes: this.transferHeadNotes
    }).subscribe({
      next: (updated: any) => {
        const actionNote = this.formerHeadAction === 'KeepAsFamilyMember'
          ? 'مع إبقاء العائل السابق بالأسرة كفرد معال حي ومقيم مع كامل إمكانية التحكم فيه ومتابعته.'
          : 'مع استبعاد العائل السابق وتوثيق سبب الخروج بالسجل التاريخي المعتمد للأسرة.';
        this.successMessage = `👑 تم بنجاح نقل ولاية الملف وتعيين (${chosenMember?.name || 'العائل الجديد'}) كرب أسرة، ${actionNote}`;
        this.closeTransferHeadModal();
        this.loadData();
        this.selectedBen = updated;
        setTimeout(() => { this.successMessage = ''; this.cdr.detectChanges(); }, 6000);
      },
      error: (err: any) => {
        this.errorMessage = err.message || 'فشل في نقل ولاية الملف.';
        this.cdr.detectChanges();
      }
    });
  }

  // ==========================================
  // حذف لين وأرشفة للمستفيد (لا يوجد حذف نهائي للداتا التاريخية)
  // ==========================================
  deleteBeneficiary(b: any, event?: Event) {
    if (event) event.stopPropagation();
    if (!this.canEditBeneficiary(b)) {
      this.errorMessage = `🔒 غير مصرح: أرشفة أو إيقاف الملف مقصور على الجمعية المسجلة له (${b.registeredByCharityName || 'الجمعية الأصلية'}).`;
      this.cdr.detectChanges();
      setTimeout(() => { this.errorMessage = ''; this.cdr.detectChanges(); }, 4000);
      return;
    }

    if (confirm(`⚠️ تنبيه نظام الحوكمة:\nلا يوجد حذف نهائي للداتا التاريخية.\nهل ترغب في أرشفة وإيقاف ملف (${b.name}) مع حفظ كامل سجل المساعدات والتاريخ الأسري في قاعدة البيانات؟`)) {
      this.api.deleteBeneficiary(b.id).subscribe({
        next: () => {
          this.successMessage = `📦 تم أرشفة وإيقاف ملف الحالة بنجاح، وحفظ كافة البيانات وحركات الصرف التاريخية في سجلات النظام.`;
          this.loadData();
          if (this.selectedBen && String(this.selectedBen.id) === String(b.id)) {
            this.viewMode = 'list';
            this.selectedBen = null;
          }
          setTimeout(() => { this.successMessage = ''; this.cdr.detectChanges(); }, 5000);
        },
        error: (err: any) => {
          this.errorMessage = err.message || 'فشل في أرشفة الملف.';
          this.cdr.detectChanges();
        }
      });
    }
  }

  printProfile() {
    window.print();
  }

  // ==========================================
  // Geographic Lookups & Smart Address Autocomplete
  // ==========================================
  loadGeographicLookups() {
    this.api.getGovernorates().subscribe({
      next: govs => {
        this.governorates = govs || [];
        this.cdr.detectChanges();
      }
    });

    this.api.getDistricts().subscribe({
      next: dists => {
        this.allDistricts = dists || [];
        this.filterDistrictsByGov();
        this.cdr.detectChanges();
      }
    });

    this.api.getVillages().subscribe({
      next: vils => {
        this.allVillages = vils || [];
        this.filterVillagesByDistrict();
        this.cdr.detectChanges();
      }
    });

    this.api.getVillageAreas().subscribe({
      next: areas => {
        this.allVillageAreas = areas || [];
        this.filterAreasByVillage();
        this.cdr.detectChanges();
      }
    });
  }

  onGovernorateChange() {
    const g = this.governorates.find(x => x.id === this.currentBen.governorateId);
    this.currentBen.governorateName = g ? g.name : '';
    this.filterDistrictsByGov();
    this.currentBen.districtId = this.filteredDistricts.length > 0 ? this.filteredDistricts[0].id : '';
    this.onDistrictChange();
  }

  onDistrictChange() {
    const d = this.allDistricts.find(x => x.id === this.currentBen.districtId);
    this.currentBen.districtName = d ? d.name : '';
    this.filterVillagesByDistrict();
    if (this.filteredVillages.length > 0) {
      // تفضيل منية المرشد إن وجدت
      const monyat = this.filteredVillages.find(v => v.id === 'vil_monyat_morshid' || v.name.includes('منية المرشد'));
      this.currentBen.villageId = monyat ? monyat.id : this.filteredVillages[0].id;
    } else {
      this.currentBen.villageId = '';
    }
    this.onVillageChange();
  }

  onVillageChange() {
    const v = this.allVillages.find(x => x.id === this.currentBen.villageId);
    this.currentBen.villageName = v ? v.name : '';
    this.filterAreasByVillage();
    if (this.filteredVillageAreas.length > 0) {
      // تفضيل المنشر إن وجد
      const manshar = this.filteredVillageAreas.find(a => a.id === 'area_manshar' || a.name.includes('المنشر'));
      this.currentBen.villageAreaId = manshar ? manshar.id : this.filteredVillageAreas[0].id;
    } else {
      this.currentBen.villageAreaId = '';
    }
    this.onVillageAreaChange();
  }

  onVillageAreaChange() {
    const a = this.allVillageAreas.find(x => x.id === this.currentBen.villageAreaId);
    this.currentBen.villageAreaName = a ? a.name : '';
    if (this.currentBen.villageName && this.currentBen.villageAreaName) {
      this.currentBen.villageOrDistrict = `${this.currentBen.villageAreaName} - ${this.currentBen.villageName}`;
    } else if (this.currentBen.villageName) {
      this.currentBen.villageOrDistrict = this.currentBen.villageName;
    }
  }

  filterVillagesByDistrict() {
    if (this.currentBen.districtId) {
      this.filteredVillages = this.allVillages.filter(v => v.districtId === this.currentBen.districtId);
    } else {
      this.filteredVillages = [...this.allVillages];
    }
  }

  filterAreasByVillage() {
    if (this.currentBen.villageId) {
      this.filteredVillageAreas = this.allVillageAreas.filter(a => a.villageId === this.currentBen.villageId);
    } else {
      this.filteredVillageAreas = [...this.allVillageAreas];
    }
  }

  filterDistrictsByGov() {
    if (this.currentBen.governorateId) {
      this.filteredDistricts = this.allDistricts.filter(d => d.governorateId === this.currentBen.governorateId);
    } else {
      this.filteredDistricts = [...this.allDistricts];
    }
  }

  onFilterGovChange() {
    if (this.selectedGovFilter !== 'الكل') {
      this.filterDistrictsList = this.allDistricts.filter(d => d.governorateId === this.selectedGovFilter);
    } else {
      this.filterDistrictsList = [];
      this.selectedDistrictFilter = 'الكل';
    }
    this.currentPage = 1;
  }

  onAddressInput() {
    if (this.addressAutocompleteTimer) clearTimeout(this.addressAutocompleteTimer);
    const query = this.currentBen.address?.trim() || '';

    this.addressAutocompleteTimer = setTimeout(() => {
      this.api.autocompleteAddresses(query, this.currentBen.governorateId, this.currentBen.districtId, 8).subscribe({
        next: (res: any[]) => {
          this.addressSuggestions = res || [];
          this.showAddressSuggestions = this.addressSuggestions.length > 0;
          this.cdr.detectChanges();
        }
      });
    }, 200);
  }

  selectAddressSuggestion(item: any) {
    this.currentBen.address = item.addressText;
    this.currentBen.addressId = item.id;
    if (item.addressText && !this.currentBen.villageOrDistrict) {
      this.currentBen.villageOrDistrict = item.addressText;
    }
    this.showAddressSuggestions = false;
    this.cdr.detectChanges();
  }

  hideAddressSuggestions() {
    setTimeout(() => {
      this.showAddressSuggestions = false;
      this.cdr.detectChanges();
    }, 250);
  }
}
