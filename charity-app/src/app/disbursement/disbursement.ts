import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { Api } from '../api';
import { AuthService } from '../auth.service';
import { isTodayOrEarlier, todayInCairo } from '../business-date';

@Component({
  selector: 'app-disbursement',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './disbursement.html',
  styleUrls: ['./disbursement.css']
})
export class DisbursementComponent implements OnInit {
  beneficiaries: any[] = [];
  inventory: any[] = [];
  expenseCategories: any[] = [];
  activeCharity: any = null;

  // نمط البحث
  searchMode: 'byBeneficiary' | 'byFamily' = 'byBeneficiary';
  searchQuery: string = '';
  searchResults: any[] = [];

  // الملف المختار
  selectedBeneficiary: any = null;

  // المستلم الفعلي للمساعدة
  selectedMemberRecipient: any = null; // فرد الأسرة المختار إن وُجد
  isRecipientHead: boolean = true;       // هل المستلم هو رب الأسرة الأساسي

  // بيانات حركة الصرف
  distributionData = {
    inventoryItemId: '',
    kind: 'صنف ملموس',
    expenseCategoryId: '',
    expenseCategoryName: '',
    itemName: '',
    quantity: 1,
    unit: 'وحدة',
    amount: null as number | null,
    date: todayInCairo(),
    notes: ''
  };

  // سجل المساعدات السابقة للحالة لمنع الازدواجية
  recentAidHistory: any[] = [];
  isLoadingAidHistory: boolean = false;

  // رسائل وتأكيدات
  isSubmitting: boolean = false;
  successMessage: string = '';
  errorMessage: string = '';

  // نافذة إيصال وسند الاستلام
  showReceiptModal: boolean = false;
  lastReceiptData: any = null;

  constructor(
    public api: Api,
    public auth: AuthService,
    private cdr: ChangeDetectorRef,
    private router: Router
  ) {}

  todayInCairo(): string {
    return todayInCairo();
  }

  ngOnInit(): void {
    this.activeCharity = this.api.getActiveCharity();
    this.loadInitialData();
  }

  loadInitialData(): void {
    this.api.getBeneficiaries().subscribe({
      next: (data) => {
        this.beneficiaries = data || [];
        this.cdr.detectChanges();
      }
    });

    this.api.getInventory().subscribe({
      next: (data) => {
        this.inventory = data || [];
        this.cdr.detectChanges();
      }
    });

    this.api.getExpenseCategories().subscribe({
      next: (data) => {
        this.expenseCategories = (data || []).filter((c: any) => c.isActive !== false);
        this.cdr.detectChanges();
      }
    });
  }

  // ==========================================
  // محرك البحث الذكي (باسم الملف أو بالمستفيد)
  // ==========================================
  setSearchMode(mode: 'byBeneficiary' | 'byFamily'): void {
    this.searchMode = mode;
    this.searchQuery = '';
    this.searchResults = [];
    this.cdr.detectChanges();
  }

  onSearchInput(): void {
    const q = (this.searchQuery || '').trim().toLowerCase();
    if (q.length < 2) {
      this.searchResults = [];
      return;
    }

    const results: any[] = [];

    for (const b of this.beneficiaries) {
      const matchFamilyName = b.familyName && b.familyName.toLowerCase().includes(q);
      const matchFamilyCode = b.familyCode && b.familyCode.toLowerCase().includes(q);
      const matchHeadName = b.name && b.name.toLowerCase().includes(q);
      const matchHeadNid = b.nationalId && b.nationalId.includes(q);

      // مطابقة أفراد الأسرة المقيمين
      const matchingMembers = (b.familyMembers || []).filter((m: any) =>
        (m.name && m.name.toLowerCase().includes(q)) ||
        (m.nationalId && m.nationalId.includes(q))
      );

      if (this.searchMode === 'byFamily') {
        if (matchFamilyName || matchFamilyCode || matchHeadName) {
          results.push({
            type: 'family',
            beneficiary: b,
            matchedMember: null,
            title: b.familyName || `عائلة ${b.name}`,
            subtitle: `كود الملف: ${b.familyCode || '—'} | رب الأسرة الحالي: ${b.name} (${b.nationalId})`,
            badge: 'ملف أسرة'
          });
        }
      } else {
        // نمط البحث بالمستفيد المباشر (رب الأسرة أو أحد أفرادها)
        if (matchHeadName || matchHeadNid) {
          results.push({
            type: 'head',
            beneficiary: b,
            matchedMember: null,
            title: b.name,
            subtitle: `صاحب الملف (رب الأسرة) | بطاقة: ${b.nationalId} | كود: ${b.familyCode || '—'}`,
            badge: 'رب الأسرة'
          });
        }

        for (const m of matchingMembers) {
          results.push({
            type: 'member',
            beneficiary: b,
            matchedMember: m,
            title: m.name,
            subtitle: `فرد أسرة (${m.relationship}) | بطاقة: ${m.nationalId} | تابع لـ: ${b.familyName || b.name}`,
            badge: m.relationship || 'فرد أسرة'
          });
        }

        // في حال بحث بكود العائلة في نمط المستفيد
        if (results.length === 0 && (matchFamilyName || matchFamilyCode)) {
          results.push({
            type: 'family',
            beneficiary: b,
            matchedMember: null,
            title: b.familyName || `عائلة ${b.name}`,
            subtitle: `كود: ${b.familyCode || '—'} | رب الأسرة: ${b.name}`,
            badge: 'ملف أسرة'
          });
        }
      }
    }

    this.searchResults = results.slice(0, 12);
  }

  selectSearchResult(result: any): void {
    this.selectedBeneficiary = result.beneficiary;
    this.searchResults = [];
    this.searchQuery = '';

    // إذا تم البحث واختيار فرد معين من الأسرة، نحدده مباشرة كمستلم فعلي
    if (result.matchedMember) {
      this.selectRecipientMember(result.matchedMember);
    } else {
      this.selectRecipientHead();
    }

    this.loadBeneficiaryAidHistory(this.selectedBeneficiary.id);
    this.cdr.detectChanges();
  }

  clearSelectedBeneficiary(): void {
    this.selectedBeneficiary = null;
    this.selectedMemberRecipient = null;
    this.isRecipientHead = true;
    this.recentAidHistory = [];
    this.distributionData.inventoryItemId = '';
    this.distributionData.itemName = '';
    this.distributionData.amount = null;
    this.distributionData.quantity = 1;
    this.cdr.detectChanges();
  }

  // ==========================================
  // اختيار المستلم الفعلي للمساعدة
  // ==========================================
  selectRecipientHead(): void {
    this.isRecipientHead = true;
    this.selectedMemberRecipient = null;
    this.cdr.detectChanges();
  }

  selectRecipientMember(m: any): void {
    this.isRecipientHead = false;
    this.selectedMemberRecipient = m;
    this.cdr.detectChanges();
  }

  get currentRecipientName(): string {
    if (this.selectedMemberRecipient) {
      return this.selectedMemberRecipient.name;
    }
    return this.selectedBeneficiary ? this.selectedBeneficiary.name : '';
  }

  get currentRecipientNationalId(): string {
    if (this.selectedMemberRecipient) {
      return this.selectedMemberRecipient.nationalId;
    }
    return this.selectedBeneficiary ? this.selectedBeneficiary.nationalId : '';
  }

  get currentRecipientRelationship(): string {
    if (this.selectedMemberRecipient) {
      return this.selectedMemberRecipient.relationship || 'فرد أسرة';
    }
    return 'صاحب الملف الأساسي (رب الأسرة)';
  }

  // أفراد الأسرة المؤهلين للاستلام المقيمين (باستثناء المتوفين)
  get eligibleFamilyMembers(): any[] {
    if (!this.selectedBeneficiary || !this.selectedBeneficiary.familyMembers) return [];
    return this.selectedBeneficiary.familyMembers.filter((m: any) => 
      !m.relationship?.includes('متوفى') && 
      !m.supportType?.includes('متوفى') &&
      !m.isSeparated
    );
  }

  // ==========================================
  // فحص المخزن والصنف
  // ==========================================
  onInventoryItemChange(): void {
    const item = this.inventory.find(i => String(i.id) === String(this.distributionData.inventoryItemId));
    if (item) {
      this.distributionData.itemName = item.itemName;
      this.distributionData.kind = item.kind || 'صنف ملموس';
      this.distributionData.unit = item.unit || 'وحدة';
      if (item.kind === 'خدمة') {
        this.distributionData.quantity = 1;
      }
    } else {
      this.distributionData.itemName = '';
      this.distributionData.unit = 'وحدة';
    }
    this.cdr.detectChanges();
  }

  onExpenseCategoryChange(): void {
    const cat = this.expenseCategories.find(c => String(c.id) === String(this.distributionData.expenseCategoryId));
    if (cat) {
      this.distributionData.expenseCategoryName = cat.name;
      this.distributionData.itemName = cat.name;
      this.distributionData.unit = 'جنيه';
    } else {
      this.distributionData.expenseCategoryName = '';
    }
    this.cdr.detectChanges();
  }

  get selectedInventoryItem(): any {
    return this.inventory.find(i => String(i.id) === String(this.distributionData.inventoryItemId));
  }

  get isStockInsufficient(): boolean {
    const item = this.selectedInventoryItem;
    if (item && item.kind === 'صنف ملموس') {
      return Number(this.distributionData.quantity) > Number(item.quantity);
    }
    return false;
  }

  // ==========================================
  // سجل المساعدات السابقة للحالة
  // ==========================================
  loadBeneficiaryAidHistory(beneficiaryId: string): void {
    this.isLoadingAidHistory = true;
    this.api.getDistributionsForBeneficiary(beneficiaryId).subscribe({
      next: (hist) => {
        this.recentAidHistory = hist || [];
        this.isLoadingAidHistory = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.isLoadingAidHistory = false;
        this.cdr.detectChanges();
      }
    });
  }

  // ==========================================
  // اعتماد وتنفيذ حركة الصرف
  // ==========================================
  submitDisbursement(): void {
    this.errorMessage = '';
    this.successMessage = '';

    if (!this.selectedBeneficiary) {
      this.errorMessage = 'يرجى البحث واختيار ملف الأسرة أولاً قبل الصرف.';
      return;
    }

    if (this.selectedBeneficiary.isSuspended || this.selectedBeneficiary.status === 'موقوف') {
      this.errorMessage = `⛔ محظور الصرف: ملف (${this.selectedBeneficiary.name}) موقوف حالياً (${this.selectedBeneficiary.suspensionReasonTitle || 'حظر صرف'}). يرجى إعادة تفعيله أولاً.`;
      return;
    }

    const recName = this.currentRecipientName;
    const recNid = this.currentRecipientNationalId;

    if (!recName) {
      this.errorMessage = 'يرجى تحديد المستلم الفعلي للمساعدة بالاسم.';
      return;
    }

    if (!isTodayOrEarlier(this.distributionData.date)) {
      this.errorMessage = 'تاريخ الصرف يجب أن يكون صحيحًا ولا يتجاوز اليوم.';
      return;
    }

    if (this.distributionData.kind === 'إعانة نقدية') {
      if (!this.distributionData.expenseCategoryId) {
        this.errorMessage = 'يرجى اختيار نوع وبند المصروف للإعانة النقدية.';
        return;
      }
      if (!this.distributionData.amount || Number(this.distributionData.amount) <= 0) {
        this.errorMessage = 'يرجى تحديد المبلغ النقدي المصروف (أكبر من صفر).';
        return;
      }
    } else if (!this.distributionData.inventoryItemId) {
      this.errorMessage = 'يرجى اختيار الصنف أو المساعدة المراد صرفها من المخزن.';
      return;
    }

    this.isSubmitting = true;

    const payload = {
      beneficiaryId: this.selectedBeneficiary.id,
      inventoryItemId: this.distributionData.inventoryItemId || null,
      itemName: this.distributionData.kind === 'إعانة نقدية' 
        ? (this.distributionData.expenseCategoryName || 'إعانة نقدية')
        : (this.selectedInventoryItem?.itemName || this.distributionData.itemName),
      kind: this.distributionData.kind,
      quantity: Number(this.distributionData.quantity),
      unit: this.distributionData.unit,
      amount: this.distributionData.amount,
      expenseCategoryId: this.distributionData.kind === 'إعانة نقدية' ? this.distributionData.expenseCategoryId : null,
      expenseCategoryName: this.distributionData.kind === 'إعانة نقدية' ? this.distributionData.expenseCategoryName : null,
      date: this.distributionData.date,
      notes: this.distributionData.notes,
      receivedByMemberId: (!this.isRecipientHead && this.selectedMemberRecipient)
        ? this.selectedMemberRecipient.id
        : null,
      receivedByName: recName,
      receivedByNationalId: recNid,
      receivedByRelationship: this.currentRecipientRelationship
    };

    this.api.addDistribution(payload).subscribe({
      next: (created: any) => {
        this.isSubmitting = false;
        this.successMessage = `✅ تم بنجاح صرف (${payload.kind === 'إعانة نقدية' ? (payload.amount + ' ج.م - ' + payload.itemName) : (payload.quantity + ' ' + payload.unit + ' - ' + payload.itemName)})، وتسليمها للمستلم الفعلي: (${recName}) [${this.currentRecipientRelationship}].`;
        if (created.warnings?.length) {
          this.successMessage += ' ⚠️ تم التسجيل بعجز في الرصيد. الرصيد المتبقي: ' +
            (created.remainingCashBalance ?? created.remainingStock);
        }

        // إعداد بيانات سند الاستلام
        this.lastReceiptData = {
          ...created,
          beneficiary: this.selectedBeneficiary,
          recipientName: recName,
          recipientNationalId: recNid,
          recipientRelationship: this.currentRecipientRelationship,
          itemName: payload.itemName,
          quantity: payload.quantity,
          unit: payload.unit,
          amount: payload.amount,
          charityName: this.activeCharity?.name || 'الجمعية المانحة',
          date: payload.date,
          notes: payload.notes
        };

        this.showReceiptModal = true;
        this.loadBeneficiaryAidHistory(this.selectedBeneficiary.id);

        // إعادة تعيين بيانات الصنف مع بقاء الأسرة
        this.distributionData.inventoryItemId = '';
        this.distributionData.expenseCategoryId = '';
        this.distributionData.expenseCategoryName = '';
        this.distributionData.itemName = '';
        this.distributionData.amount = null;
        this.distributionData.quantity = 1;
        this.distributionData.notes = '';

        // تحديث رصيد المخزن
        this.api.getInventory().subscribe(data => this.inventory = data || []);

        this.cdr.detectChanges();
      },
      error: (err: any) => {
        this.isSubmitting = false;
        this.errorMessage = err.error?.message || err.message || 'فشل في تسجيل حركة الصرف.';
        this.cdr.detectChanges();
      }
    });
  }

  printReceipt(): void {
    window.print();
  }

  closeReceiptModal(): void {
    this.showReceiptModal = false;
    this.cdr.detectChanges();
  }
}
