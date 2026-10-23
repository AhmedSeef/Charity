import { Component, OnInit, ChangeDetectorRef, Optional } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { Api } from '../api';
import { AuthService } from '../auth.service';
import { isTodayOrEarlier, todayInCairo } from '../business-date';

@Component({
  selector: 'app-area-distribution',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './area-distribution.html',
  styleUrls: ['./area-distribution.css']
})
export class AreaDistributionComponent implements OnInit {
  // بيانات أساسية
  beneficiaries: any[] = [];
  inventoryItems: any[] = [];
  volunteers: any[] = [];
  expenseCategories: any[] = [];
  governorates: any[] = [];
  districts: any[] = [];
  villages: any[] = [];
  villageAreas: any[] = [];
  activeCharity: any = null;

  isLoading: boolean = false;
  isSubmitting: boolean = false;
  errorMessage: string = '';
  successMessage: string = '';

  // تصفية وبحث المنطقة الجغرافية
  areaSearchTerm: string = '';
  selectedGovernorateId: string = '';
  selectedDistrictId: string = '';
  selectedPriorityTier: string = '';
  onlyActive: boolean = true;

  // إدارة التحديد المتعدد للأسر
  selectedBeneficiaryIds: Set<string> = new Set<string>();

  // خيارات نموذج الصرف الجماعي
  aidType: 'tangible' | 'cash' = 'tangible';
  selectedInventoryItemId: string = '';
  quantityPerBeneficiary: number = 1;
  cashAmountPerBeneficiary: number = 500;
  selectedExpenseCategoryId: string = '';
  volunteerName: string = '';
  volunteerPhone: string = '';
  distributionDate: string = todayInCairo();
  distributionNotes: string = '';

  // كشف خط السير الميداني للمتطوع (Run-Sheet)
  showRunSheetModal: boolean = false;
  runSheetData: any = null;
  isCommittedSheet: boolean = false;

  constructor(
    public api: Api,
    public auth: AuthService,
    private cdr: ChangeDetectorRef,
    @Optional() private route?: ActivatedRoute
  ) {}

  todayInCairo(): string {
    return todayInCairo();
  }

  ngOnInit(): void {
    this.activeCharity = this.api.getActiveCharity();
    this.loadInitialData();

    if (this.route) {
      this.route.queryParams.subscribe(params => {
        if (params['area']) {
          this.setAreaSearch(params['area']);
        }
      });
    }
  }

  loadInitialData(): void {
    this.isLoading = true;
    this.errorMessage = '';

    // 1. تحميل المستفيدين
    this.api.getBeneficiaries().subscribe({
      next: (bens) => {
        this.beneficiaries = bens || [];
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || err.message || 'فشل في تحميل بيانات المستفيدين.';
        this.cdr.detectChanges();
      }
    });

    // 2. تحميل المخزن الحالي
    this.api.getInventory().subscribe({
      next: (items) => {
        this.inventoryItems = items || [];
        if (this.inventoryItems.length > 0) {
          this.selectedInventoryItemId = this.inventoryItems[0].id;
        }
        this.cdr.detectChanges();
      }
    });

    // 3. تحميل المتطوعين وفريق العمل
    this.api.getStaff().subscribe({
      next: (staff) => {
        this.volunteers = staff || [];
        this.cdr.detectChanges();
      }
    });

    // 4. تحميل بنود المصروفات (في حال اختيار مساعدة نقدية)
    this.api.getExpenseCategories().subscribe({
      next: (cats) => {
        this.expenseCategories = (cats || []).filter(c => c.isActive !== false);
        if (this.expenseCategories.length > 0) {
          this.selectedExpenseCategoryId = this.expenseCategories[0].id;
        }
        this.cdr.detectChanges();
      }
    });

    // 5. تحميل المحافظات والمراكز
    this.api.getGovernorates().subscribe({
      next: (govs) => {
        this.governorates = govs || [];
        this.cdr.detectChanges();
      }
    });

    // 6. تحميل القرى والمناطق الميدانية (مركز مطوبس ومنية المرشد ومناطقها)
    this.api.getVillages().subscribe({
      next: (vils) => {
        this.villages = vils || [];
        this.cdr.detectChanges();
      }
    });

    this.api.getVillageAreas().subscribe({
      next: (areas) => {
        this.villageAreas = areas || [];
        this.cdr.detectChanges();
      }
    });
  }

  onGovernorateChange(): void {
    this.deselectAll();
    this.selectedDistrictId = '';
    if (this.selectedGovernorateId) {
      this.api.getDistricts(this.selectedGovernorateId).subscribe({
        next: (dists) => {
          this.districts = dists || [];
          this.cdr.detectChanges();
        }
      });
    } else {
      this.districts = [];
    }
  }

  onFilterChange(): void {
    this.deselectAll();
  }

  // ==========================================
  // الفلاتر واستخراج المناطق الشائعة (Quick Chips)
  // ==========================================
  get filteredBeneficiaries(): any[] {
    const term = (this.areaSearchTerm || '').trim().toLowerCase();
    const govId = this.selectedGovernorateId;
    const distId = this.selectedDistrictId;
    const tier = this.selectedPriorityTier;

    return this.beneficiaries.filter(b => {
      // استبعاد الموقوفين إذا كان الخيار مفعلاً
      if (this.onlyActive && (b.isSuspended || b.status === 'موقوف')) {
        return false;
      }

      // مطابقة المحافظة والمركز
      if (govId && b.governorateId !== govId) return false;
      if (distId && b.districtId !== distId) return false;

      // مطابقة فئة الأولوية
      if (tier && b.priorityTier !== tier) return false;

      // مطابقة البحث بالمنطقة أو القرية أو العنوان أو الاسم
      if (term) {
        const inVillageArea = (b.villageAreaName || '').toLowerCase().includes(term);
        const inVillage = (b.villageName || '').toLowerCase().includes(term) || (b.villageOrDistrict || '').toLowerCase().includes(term);
        const inAddress = (b.address || '').toLowerCase().includes(term);
        const inDistrict = (b.districtName || '').toLowerCase().includes(term);
        const inGov = (b.governorateName || '').toLowerCase().includes(term);
        if (!inVillageArea && !inVillage && !inAddress && !inDistrict && !inGov) {
          return false;
        }
      }

      return true;
    });
  }

  // استخراج قائمة المناطق والقرى الشائعة كأزرار سريعة بنقرة واحدة (أبرزها مناطق منية المرشد)
  get quickAreaChips(): string[] {
    const defaultPriority = ['المنشر', 'السوق', 'الجسر', 'المضرب', 'الترب', 'منية المرشد'];
    const areas = new Set<string>(defaultPriority);

    for (const a of this.villageAreas) {
      if (a.name) areas.add(a.name.trim());
    }

    for (const b of this.beneficiaries) {
      if (b.villageAreaName && b.villageAreaName.trim().length > 1) {
        areas.add(b.villageAreaName.trim());
      }
      if (b.villageOrDistrict && b.villageOrDistrict.trim().length > 1) {
        areas.add(b.villageOrDistrict.trim());
      }
    }

    return Array.from(areas).slice(0, 12);
  }

  setAreaSearch(area: string): void {
    this.areaSearchTerm = area;
    this.selectAllFiltered();
    this.cdr.detectChanges();
  }

  clearSearch(): void {
    this.deselectAll();
    this.areaSearchTerm = '';
    this.selectedGovernorateId = '';
    this.selectedDistrictId = '';
    this.selectedPriorityTier = '';
    this.districts = [];
    this.cdr.detectChanges();
  }

  // ==========================================
  // منطق التحديد المتعدد
  // ==========================================
  isSelected(id: string): boolean {
    return this.selectedBeneficiaryIds.has(id);
  }

  toggleSelection(id: string): void {
    if (!this.filteredBeneficiaries.some(b => b.id === id)) return;
    if (this.selectedBeneficiaryIds.has(id)) {
      this.selectedBeneficiaryIds.delete(id);
    } else {
      this.selectedBeneficiaryIds.add(id);
    }
    this.cdr.detectChanges();
  }

  get isAllFilteredSelected(): boolean {
    const list = this.filteredBeneficiaries;
    if (list.length === 0) return false;
    return list.every(b => this.selectedBeneficiaryIds.has(b.id));
  }

  toggleSelectAllFiltered(): void {
    if (this.isAllFilteredSelected) {
      this.deselectAll();
    } else {
      this.selectAllFiltered();
    }
  }

  selectAllFiltered(): void {
    this.selectedBeneficiaryIds = new Set(this.filteredBeneficiaries.map(b => b.id));
    this.cdr.detectChanges();
  }

  deselectAll(): void {
    this.selectedBeneficiaryIds.clear();
    this.cdr.detectChanges();
  }

  get selectedBeneficiariesList(): any[] {
    return this.filteredBeneficiaries.filter(b => this.selectedBeneficiaryIds.has(b.id));
  }

  get selectedCount(): number {
    return this.selectedBeneficiariesList.length;
  }

  get totalFamilyMembersCount(): number {
    return this.selectedBeneficiariesList.reduce((acc, b) => acc + (b.familyCount || 1), 0);
  }

  // ==========================================
  // احتساب المخزن والكميات
  // ==========================================
  get selectedItemObject(): any {
    if (this.aidType !== 'tangible') return null;
    return this.inventoryItems.find(i => i.id === this.selectedInventoryItemId) || null;
  }

  get totalRequiredQuantity(): number {
    return (this.quantityPerBeneficiary || 0) * this.selectedCount;
  }

  get totalRequiredCash(): number {
    return (this.cashAmountPerBeneficiary || 0) * this.selectedCount;
  }

  get isStockInsufficient(): boolean {
    if (this.aidType !== 'tangible') return false;
    const item = this.selectedItemObject;
    if (!item || item.kind === 'خدمة') return false;
    return item.quantity < this.totalRequiredQuantity;
  }

  onVolunteerSelect(event: any): void {
    const name = event.target.value;
    const found = this.volunteers.find(v => v.name === name);
    if (found) {
      this.volunteerPhone = found.phone || '';
    }
  }

  // ==========================================
  // اعتماد وتنفيذ الصرف الجماعي
  // ==========================================
  executeBulkDisbursement(): void {
    this.errorMessage = '';
    this.successMessage = '';

    const selectedFamilies = this.selectedBeneficiariesList;
    if (selectedFamilies.length === 0) {
      this.errorMessage = 'يرجى تحديد الأسر المستفيدة أولاً من الجدول قبل تنفيذ الصرف.';
      return;
    }

    if (this.aidType === 'tangible') {
      if (!this.selectedInventoryItemId) {
        this.errorMessage = 'يرجى اختيار صنف المساعدة من المخزن.';
        return;
      }
      if (this.quantityPerBeneficiary <= 0) {
        this.errorMessage = 'الكمية لكل أسرة يجب أن تكون أكبر من صفر.';
        return;
      }
    } else {
      if (!this.selectedExpenseCategoryId) {
        this.errorMessage = 'يرجى اختيار بند المصروف للمساعدة النقدية.';
        return;
      }
      if (this.cashAmountPerBeneficiary <= 0) {
        this.errorMessage = 'المبلغ النقدي لكل أسرة يجب أن يكون أكبر من صفر.';
        return;
      }
    }

    const volName = (this.volunteerName || '').trim() || 'مندوب التوزيع الميداني';
    const targetArea = (this.areaSearchTerm || '').trim() || 'منطقة التوزيع الميداني';
    if (!isTodayOrEarlier(this.distributionDate)) {
      this.errorMessage = 'تاريخ خط السير يجب أن يكون صحيحًا ولا يتجاوز اليوم.';
      return;
    }

    const confirmed = confirm(`هل أنت متأكد من تنفيذ الصرف الجماعي لعدد (${selectedFamilies.length}) أسرة في (${targetArea}) وتسليم العهدة للمتطوع/المندوب (${volName})؟`);
    if (!confirmed) return;

    this.isSubmitting = true;

    const payload: any = {
      beneficiaryIds: selectedFamilies.map(b => b.id),
      targetAreaFilter: this.areaSearchTerm.trim(),
      targetGovernorateId: this.selectedGovernorateId || null,
      targetDistrictId: this.selectedDistrictId || null,
      targetPriorityTier: this.selectedPriorityTier || null,
      date: this.distributionDate,
      notes: this.distributionNotes?.trim() || `توزيع ميداني جماعي - منطقة: ${targetArea} - المتطوع: ${volName}`,
      volunteerOrWorkerName: volName,
      volunteerPhone: this.volunteerPhone?.trim(),
      targetAreaName: targetArea
    };

    if (this.aidType === 'tangible') {
      const item = this.selectedItemObject;
      payload.inventoryItemId = item.id;
      payload.itemName = item.itemName;
      payload.kind = item.kind || 'صنف ملموس';
      payload.quantityPerBeneficiary = Number(this.quantityPerBeneficiary);
      payload.unit = item.unit;
    } else {
      const cat = this.expenseCategories.find(c => c.id === this.selectedExpenseCategoryId);
      payload.kind = 'إعانة نقدية';
      payload.amountPerBeneficiary = Number(this.cashAmountPerBeneficiary);
      payload.expenseCategoryId = cat?.id;
      payload.expenseCategoryName = cat?.name;
      payload.itemName = cat?.name || 'مساعدة نقدية';
    }

    this.api.createBulkDistribution(payload).subscribe({
      next: (res: any) => {
        this.isSubmitting = false;
        this.successMessage = `✅ تم بنجاح تنفيذ الصرف الجماعي لعدد (${res.successCount}) أسرة، وتجهيز كشف خط السير للمتطوع (${volName})!`;
        if (res.warnings?.length) {
          this.successMessage += ' ⚠️ تم التسجيل بعجز في الرصيد. الرصيد المتبقي: ' +
            (res.remainingCashBalance ?? res.remainingStock);
        }

        // إعداد بيانات كشف خط السير المعتمد للطباعة
        const successfulIds = new Set((res.createdDistributions || []).map((d: any) => d.beneficiaryId));
        this.runSheetData = {
          ...res,
          beneficiaries: selectedFamilies.filter(b => successfulIds.has(b.id)),
          itemDescription: this.aidType === 'tangible' 
            ? `${this.quantityPerBeneficiary} ${payload.unit} ${payload.itemName} (إجمالي: ${res.totalQuantityDisbursed} ${payload.unit})`
            : `${this.cashAmountPerBeneficiary} ج.م ${payload.itemName} (إجمالي: ${res.totalAmountDisbursed} ج.م)`,
          charityName: this.activeCharity?.name || 'جمعية الرعاية المشتركة',
          charityPhone: this.activeCharity?.phone || '',
          charityAddress: this.activeCharity?.address || ''
        };
        this.isCommittedSheet = true;
        this.showRunSheetModal = true;

        // تحديث المخزن والأرصدة
        this.loadInitialData();
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        this.isSubmitting = false;
        this.errorMessage = err.error?.message || err.message || 'فشل في تنفيذ الصرف الجماعي.';
        this.cdr.detectChanges();
      }
    });
  }

  // ==========================================
  // معاينة وطباعة كشف خط السير (Run-Sheet)
  // ==========================================
  previewRunSheet(): void {
    const selectedFamilies = this.selectedBeneficiariesList;
    if (selectedFamilies.length === 0) {
      this.errorMessage = 'يرجى تحديد أسر خط السير أولاً لمعاينة الكشف.';
      return;
    }

    const volName = (this.volunteerName || '').trim() || 'مندوب التوزيع الميداني';
    const targetArea = (this.areaSearchTerm || '').trim() || 'منطقة التوزيع';
    const item = this.selectedItemObject;

    this.runSheetData = {
      targetAreaName: targetArea,
      volunteerOrWorkerName: volName,
      volunteerPhone: this.volunteerPhone,
      date: this.distributionDate,
      successCount: selectedFamilies.length,
      totalQuantityDisbursed: this.totalRequiredQuantity,
      totalAmountDisbursed: this.totalRequiredCash,
      itemDescription: this.aidType === 'tangible' 
        ? `${this.quantityPerBeneficiary} ${item?.unit || 'وحدة'} ${item?.itemName || 'طعام / كرتونة مواد غذائية'} (إجمالي: ${this.totalRequiredQuantity} ${item?.unit || 'وحدة'})`
        : `${this.cashAmountPerBeneficiary} ج.م (إجمالي: ${this.totalRequiredCash} ج.م)`,
      beneficiaries: selectedFamilies,
      charityName: this.activeCharity?.name || 'الجمعية الخيرية',
      charityPhone: this.activeCharity?.phone || '',
      charityAddress: this.activeCharity?.address || ''
    };
    this.isCommittedSheet = false;
    this.showRunSheetModal = true;
    this.cdr.detectChanges();
  }

  closeRunSheet(): void {
    this.showRunSheetModal = false;
    this.cdr.detectChanges();
  }

  printRunSheet(): void {
    window.print();
  }
}
