import { Component, OnInit, ChangeDetectorRef, Optional } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { Api } from '../api';
import { AuthService } from '../auth.service';

@Component({
  selector: 'app-villages-management',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './villages-management.html',
  styleUrls: ['./villages-management.css']
})
export class VillagesManagementComponent implements OnInit {
  // وضع العرض: قائمة أو نموذج إدخال مستقل
  viewMode: 'list' | 'form' = 'list';

  // التبويب النشط: القرى أو المناطق التابعة
  activeTab: 'villages' | 'areas' = 'villages';

  // البيانات الجغرافية
  governorates: any[] = [];
  districts: any[] = [];
  villages: any[] = [];
  villageAreas: any[] = [];

  // التصفية الجغرافية الافتراضية (كفر الشيخ - مركز مطوبس)
  selectedGovernorateId: string = 'gov_kfs';
  selectedDistrictId: string = 'dist_motobas';
  selectedVillageId: string = '';

  searchTerm: string = '';
  isLoading: boolean = false;
  isSaving: boolean = false;
  errorMessage: string = '';
  successMessage: string = '';

  // نموذج القرية
  isEditingVillage: boolean = false;
  villageForm = {
    id: null as string | null,
    districtId: 'dist_motobas',
    name: '',
    nameEn: '',
    isActive: true
  };

  // نموذج المنطقة التابعة للقرية
  isEditingArea: boolean = false;
  areaForm = {
    id: null as string | null,
    villageId: '',
    districtId: '',
    name: '',
    nameEn: '',
    notes: '',
    isActive: true
  };

  constructor(
    public api: Api,
    public auth: AuthService,
    private cdr: ChangeDetectorRef,
    @Optional() private route?: ActivatedRoute,
    @Optional() private router?: Router
  ) {}

  ngOnInit(): void {
    // التحقق من الرابط والتبويب المطلوب
    if (this.route) {
      this.route.data.subscribe(data => {
        if (data['formPage']) {
          this.viewMode = 'form';
        }
        if (data['tab']) {
          this.activeTab = data['tab'];
        }
      });

      this.route.queryParams.subscribe(params => {
        if (params['tab']) {
          this.activeTab = params['tab'];
        }
        if (params['villageId']) {
          this.selectedVillageId = params['villageId'];
        }
        if (params['districtId']) {
          this.selectedDistrictId = params['districtId'];
        }
      });
    }

    this.loadGovernorates();
    this.loadDistricts();
    this.loadVillages();
  }

  // ==========================================
  // تحميل البيانات
  // ==========================================

  loadGovernorates() {
    this.api.getGovernorates().subscribe({
      next: (res) => {
        this.governorates = res || [];
        this.cdr.detectChanges();
      },
      error: () => {}
    });
  }

  loadDistricts() {
    this.api.getDistricts(this.selectedGovernorateId).subscribe({
      next: (res) => {
        this.districts = res || [];
        // التأكد من اختيار مطوبس افتراضياً إن وجدت
        if (!this.selectedDistrictId && this.districts.length > 0) {
          const motobas = this.districts.find(d => d.id === 'dist_motobas');
          this.selectedDistrictId = motobas ? motobas.id : this.districts[0].id;
        }
        this.cdr.detectChanges();
      },
      error: () => {}
    });
  }

  loadVillages() {
    this.isLoading = true;
    this.api.getVillages(this.selectedDistrictId).subscribe({
      next: (res) => {
        this.villages = res || [];
        this.isLoading = false;

        // ضبط القرية الافتراضية لمنية المرشد إذا لم تكن محددة مسبقاً
        if (!this.selectedVillageId && this.villages.length > 0) {
          const monyat = this.villages.find(v => v.id === 'vil_monyat_morshid' || v.name.includes('منية المرشد'));
          this.selectedVillageId = monyat ? monyat.id : this.villages[0].id;
        }

        // إذا كان التبويب الحالي هو المناطق، يتم تحميل مناطق القرية المحددة
        if (this.activeTab === 'areas') {
          this.loadVillageAreas();
        }

        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = 'تعذر تحميل قائمة القرى: ' + (err.error?.message || err.message);
        this.cdr.detectChanges();
      }
    });
  }

  loadVillageAreas() {
    this.isLoading = true;
    this.api.getVillageAreas(this.selectedVillageId || undefined, this.selectedDistrictId || undefined).subscribe({
      next: (res) => {
        this.villageAreas = res || [];
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = 'تعذر تحميل المناطق التابعة للقرية: ' + (err.error?.message || err.message);
        this.cdr.detectChanges();
      }
    });
  }

  // ==========================================
  // تغييرات الفلاتر
  // ==========================================

  onGovernorateChange() {
    this.loadDistricts();
    this.loadVillages();
  }

  onDistrictChange() {
    this.loadVillages();
  }

  onVillageFilterChange() {
    this.loadVillageAreas();
  }

  switchTab(tab: 'villages' | 'areas') {
    this.activeTab = tab;
    this.errorMessage = '';
    this.successMessage = '';
    this.searchTerm = '';
    if (tab === 'areas') {
      this.loadVillageAreas();
    } else {
      this.loadVillages();
    }
  }

  goToVillageAreas(village: any) {
    this.selectedVillageId = village.id;
    this.switchTab('areas');
  }

  // ==========================================
  // إدارة القرى (Villages CRUD)
  // ==========================================

  openNewVillageForm() {
    this.isEditingVillage = false;
    this.villageForm = {
      id: null,
      districtId: this.selectedDistrictId || 'dist_motobas',
      name: '',
      nameEn: '',
      isActive: true
    };
    this.viewMode = 'form';
    this.activeTab = 'villages';
    this.errorMessage = '';
    this.successMessage = '';
  }

  openEditVillage(village: any) {
    this.isEditingVillage = true;
    this.villageForm = {
      id: village.id,
      districtId: village.districtId,
      name: village.name,
      nameEn: village.nameEn || '',
      isActive: village.isActive !== false
    };
    this.viewMode = 'form';
    this.activeTab = 'villages';
    this.errorMessage = '';
    this.successMessage = '';
  }

  saveVillage() {
    if (!this.villageForm.name.trim()) {
      this.errorMessage = 'يرجى كتابة اسم القرية بشكل صحيح.';
      return;
    }

    if (!this.villageForm.districtId) {
      this.errorMessage = 'يرجى اختيار المركز أو الحي التابع له القرية.';
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';
    this.successMessage = '';

    if (this.isEditingVillage && this.villageForm.id) {
      this.api.updateVillage(this.villageForm.id, {
        name: this.villageForm.name.trim(),
        nameEn: this.villageForm.nameEn.trim(),
        isActive: this.villageForm.isActive
      }).subscribe({
        next: () => {
          this.isSaving = false;
          this.successMessage = `تم تعديل بيانات قرية "${this.villageForm.name}" بنجاح.`;
          this.viewMode = 'list';
          this.loadVillages();
          this.cdr.detectChanges();
        },
        error: (err) => {
          this.isSaving = false;
          this.errorMessage = err.error?.message || err.message || 'فشل حفظ تعديل القرية.';
          this.cdr.detectChanges();
        }
      });
    } else {
      this.api.createVillage({
        districtId: this.villageForm.districtId,
        name: this.villageForm.name.trim(),
        nameEn: this.villageForm.nameEn.trim()
      }).subscribe({
        next: (created) => {
          this.isSaving = false;
          this.successMessage = `تمت إضافة قرية "${created.name}" بنجاح إلى المركز.`;
          this.viewMode = 'list';
          this.loadVillages();
          this.cdr.detectChanges();
        },
        error: (err) => {
          this.isSaving = false;
          this.errorMessage = err.error?.message || err.message || 'فشل إضافة القرية الجديدة.';
          this.cdr.detectChanges();
        }
      });
    }
  }

  deleteVillage(village: any) {
    if (village.areasCount > 0) {
      alert(`تنبيه: لا يمكن حذف قرية "${village.name}" لوجود ${village.areasCount} منطقة/حي مسجل تحتها. يرجى حذف أو نقل المناطق أولاً.`);
      return;
    }

    if (village.beneficiariesCount > 0) {
      alert(`تنبيه: لا يمكن حذف قرية "${village.name}" لوجود ${village.beneficiariesCount} أسرة مستفيدة مرتبطة بها.`);
      return;
    }

    if (!confirm(`هل أنت متأكد من حذف قرية "${village.name}" نهائياً من دليل القرى؟`)) {
      return;
    }

    this.isLoading = true;
    this.api.deleteVillage(village.id).subscribe({
      next: () => {
        this.successMessage = `تم حذف قرية "${village.name}" بنجاح.`;
        this.loadVillages();
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || err.message || 'تعذر حذف القرية.';
        this.cdr.detectChanges();
      }
    });
  }

  // ==========================================
  // إدارة المناطق والأحياء التابعة للقرية (Village Areas CRUD)
  // ==========================================

  openNewAreaForm() {
    this.isEditingArea = false;
    this.areaForm = {
      id: null,
      villageId: this.selectedVillageId || (this.villages[0]?.id || ''),
      districtId: '',
      name: '',
      nameEn: '',
      notes: '',
      isActive: true
    };
    this.viewMode = 'form';
    this.activeTab = 'areas';
    this.errorMessage = '';
    this.successMessage = '';
  }

  openEditArea(area: any) {
    this.isEditingArea = true;
    this.areaForm = {
      id: area.id,
      villageId: area.villageId,
      districtId: area.districtId || '',
      name: area.name,
      nameEn: area.nameEn || '',
      notes: area.notes || '',
      isActive: area.isActive !== false
    };
    this.viewMode = 'form';
    this.activeTab = 'areas';
    this.errorMessage = '';
    this.successMessage = '';
  }

  onAreaVillageChange() {
    if (this.areaForm.villageId) this.areaForm.districtId = '';
  }

  onAreaDistrictChange() {
    if (this.areaForm.districtId) this.areaForm.villageId = '';
  }

  saveArea() {
    if (!this.areaForm.name.trim()) {
      this.errorMessage = 'يرجى كتابة اسم المنطقة أو الحي (مثل: المنشر، السوق، الجسر، المضرب، الترب).';
      return;
    }

    if ((!this.areaForm.villageId && !this.areaForm.districtId) || (this.areaForm.villageId && this.areaForm.districtId)) {
      this.errorMessage = 'يرجى اختيار قرية أو مركز/حي واحد فقط لتكون المنطقة تابعة له.';
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';
    this.successMessage = '';

    if (this.isEditingArea && this.areaForm.id) {
      this.api.updateVillageArea(this.areaForm.id, {
        name: this.areaForm.name.trim(),
        nameEn: this.areaForm.nameEn.trim(),
        notes: this.areaForm.notes.trim(),
        isActive: this.areaForm.isActive
      }).subscribe({
        next: () => {
          this.isSaving = false;
          this.successMessage = `تم تعديل بيانات منطقة "${this.areaForm.name}" بنجاح.`;
          this.viewMode = 'list';
          this.loadVillageAreas();
          this.cdr.detectChanges();
        },
        error: (err) => {
          this.isSaving = false;
          this.errorMessage = err.error?.message || err.message || 'فشل حفظ تعديل المنطقة.';
          this.cdr.detectChanges();
        }
      });
    } else {
      this.api.createVillageArea({
        villageId: this.areaForm.villageId || null,
        districtId: this.areaForm.districtId || null,
        name: this.areaForm.name.trim(),
        nameEn: this.areaForm.nameEn.trim(),
        notes: this.areaForm.notes.trim()
      }).subscribe({
        next: (created) => {
          this.isSaving = false;
          this.successMessage = `تمت إضافة منطقة "${created.name}" بنجاح إلى القرية.`;
          this.viewMode = 'list';
          this.loadVillageAreas();
          this.cdr.detectChanges();
        },
        error: (err) => {
          this.isSaving = false;
          this.errorMessage = err.error?.message || err.message || 'فشل إضافة المنطقة الجديدة.';
          this.cdr.detectChanges();
        }
      });
    }
  }

  deleteArea(area: any) {
    if (area.beneficiariesCount > 0) {
      alert(`تنبيه: لا يمكن حذف منطقة "${area.name}" لوجود ${area.beneficiariesCount} أسرة مستفيدة مسجلة عليها.`);
      return;
    }

    if (!confirm(`هل أنت متأكد من حذف منطقة "${area.name}" نهائياً من القرية؟`)) {
      return;
    }

    this.isLoading = true;
    this.api.deleteVillageArea(area.id).subscribe({
      next: () => {
        this.successMessage = `تم حذف منطقة "${area.name}" بنجاح.`;
        this.loadVillageAreas();
      },
      error: (err) => {
        this.isLoading = false;
        this.errorMessage = err.error?.message || err.message || 'تعذر حذف المنطقة.';
        this.cdr.detectChanges();
      }
    });
  }

  cancelForm() {
    this.viewMode = 'list';
    this.errorMessage = '';
  }

  // تصفية نتائج البحث
  filteredVillages() {
    if (!this.searchTerm.trim()) return this.villages;
    const term = this.searchTerm.toLowerCase().trim();
    return this.villages.filter(v =>
      (v.name && v.name.toLowerCase().includes(term)) ||
      (v.nameEn && v.nameEn.toLowerCase().includes(term)) ||
      (v.districtName && v.districtName.toLowerCase().includes(term))
    );
  }

  filteredAreas() {
    if (!this.searchTerm.trim()) return this.villageAreas;
    const term = this.searchTerm.toLowerCase().trim();
    return this.villageAreas.filter(a =>
      (a.name && a.name.toLowerCase().includes(term)) ||
      (a.notes && a.notes.toLowerCase().includes(term)) ||
      (a.villageName && a.villageName.toLowerCase().includes(term))
    );
  }

  getSelectedVillageName(): string {
    const v = this.villages.find(x => x.id === this.selectedVillageId);
    return v ? v.name : 'القرية المحددة';
  }

  getSelectedDistrictName(): string {
    const d = this.districts.find(x => x.id === this.selectedDistrictId);
    return d ? d.name : 'مركز مطوبس';
  }
}
