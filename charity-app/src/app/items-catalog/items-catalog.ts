import { Component, ChangeDetectorRef, OnInit, Optional } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Api } from '../api';
import { PaginationComponent } from '../pagination/pagination';

@Component({
  selector: 'app-items-catalog',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, PaginationComponent],
  templateUrl: './items-catalog.html',
  styleUrls: ['./items-catalog.css']
})
export class ItemsCatalogComponent implements OnInit {
  viewMode: 'list' | 'form' = 'list';
  items: any[] = [];
  selectedFilter: 'الكل' | 'صنف ملموس' | 'خدمة' = 'الكل';
  searchTerm: string = '';
  successMessage: string = '';

  currentPage: number = 1;
  pageSize: number = 5;

  itemForm: any = {
    id: null,
    code: '',
    itemName: '',
    kind: 'صنف ملموس', // 'صنف ملموس' | 'خدمة'
    unit: 'كيلو',
    type: 'أغذية جافة',
    initialStock: 0,
    minAlert: 10,
    notes: ''
  };

  tangibleUnits = ['كيلو', 'قطعة', 'إزازة / زجاجة', 'علبة', 'كرتونة', 'كيس', 'شكارة / شوال', 'شنطة / وجبة', 'شريط (دواء)', 'لتر', 'متر', 'طقم'];
  serviceUnits = ['كشف طبي', 'عملية جراحية', 'حالة / مساعدة', 'جلسة علاجية', 'منزل / سقف', 'منحة / كفالة', 'خدمة / استشارة'];

  tangibleTypes = ['أغذية جافة', 'لحوم ومجمدات', 'أدوية ومستلزمات طبية', 'أجهزة تعويضية وكراسي', 'ملابس وبطاطين', 'أكفان وتجهيز متوفين', 'متنوع'];
  serviceTypes = ['خدمات طبية وعلاجية', 'خدمات اجتماعية وتجهيز', 'خدمات إيواء وتعمير', 'خدمات تعليمية وكفالات', 'سداد ديون وغارمين'];

  constructor(private api: Api, private cdr: ChangeDetectorRef,
    @Optional() private route?: ActivatedRoute, @Optional() private router?: Router) {}

  get isFormPage(): boolean { return this.route?.snapshot.data['formPage'] === true; }

  ngOnInit(): void {
    this.generateNewCode();
    this.loadData();
    this.initRouteSync();
  }

  initRouteSync() {
    this.route?.queryParams.subscribe(params => {
      const mode = params['mode'];
      const id = params['id'] || params['edit'];
      if (this.isFormPage || mode === 'add') {
        if (!this.itemForm.id) this.resetForm();
        this.viewMode = 'form';
      } else if (mode === 'edit' && id) {
        const item = this.items.find(i => String(i.id) === String(id));
        if (item) this.populateItem(item);
        this.viewMode = 'form';
      } else if (!this.isFormPage) {
        this.viewMode = 'list';
      }
      this.cdr.detectChanges();
    });
  }

  openAdd() {
    this.resetForm();
    this.viewMode = 'form';
    if (this.router) {
      void this.router.navigate([], { relativeTo: this.route, queryParams: { mode: 'add' } });
    }
  }

  openEdit(item: any, event?: Event) {
    if (event) event.stopPropagation();
    this.populateItem(item);
    this.viewMode = 'form';
    if (this.router) {
      void this.router.navigate([], { relativeTo: this.route, queryParams: { mode: 'edit', id: item.id } });
    }
  }

  backToList() {
    this.resetForm();
    this.viewMode = 'list';
    if (this.router) {
      void this.router.navigate(['/items-catalog'], { queryParams: {} });
    }
  }

  loadData() {
    this.api.getInventory().subscribe((data: any[]) => {
      this.items = data;
      const editId = this.route?.snapshot.queryParamMap.get('edit') || this.route?.snapshot.queryParamMap.get('id');
      if (editId) {
        const item = data.find(i => String(i.id) === String(editId));
        if (item) {
          this.populateItem(item);
          this.viewMode = 'form';
        }
      }
      this.cdr.detectChanges();
    });
  }

  generateNewCode() {
    if (this.itemForm.id) return;
    const prefix = this.itemForm.kind === 'خدمة' ? 'SRV' : 'ITM';
    const rand = Math.floor(100 + Math.random() * 900);
    this.itemForm.code = `${prefix}-${rand}`;
  }

  onKindChange() {
    if (this.itemForm.kind === 'صنف ملموس') {
      this.itemForm.unit = 'كيلو';
      this.itemForm.type = 'أغذية جافة';
    } else {
      this.itemForm.unit = 'كشف طبي';
      this.itemForm.type = 'خدمات طبية وعلاجية';
      this.itemForm.initialStock = 0;
    }
    if (!this.itemForm.id) {
      this.generateNewCode();
    }
  }

  get tangibleCount(): number {
    return this.items.filter(i => i.kind === 'صنف ملموس').length;
  }

  get serviceCount(): number {
    return this.items.filter(i => i.kind === 'خدمة').length;
  }

  get filteredItems(): any[] {
    return this.items.filter(item => {
      const matchFilter = this.selectedFilter === 'الكل' || item.kind === this.selectedFilter;
      const term = this.searchTerm.trim().toLowerCase();
      const matchSearch = !term ||
        (item.itemName && item.itemName.toLowerCase().includes(term)) ||
        (item.code && item.code.toLowerCase().includes(term)) ||
        (item.type && item.type.toLowerCase().includes(term)) ||
        (item.unit && item.unit.toLowerCase().includes(term));
      return matchFilter && matchSearch;
    });
  }

  get paginatedItems(): any[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.filteredItems.slice(start, start + this.pageSize);
  }

  selectItem(item: any) {
    if (!this.isFormPage && this.router) {
      void this.router.navigate(['/items-catalog/new'], { queryParams: { edit: item.id } });
      return;
    }
    this.populateItem(item);
  }

  private populateItem(item: any) {
    this.itemForm = {
      id: item.id,
      code: item.code || '',
      itemName: item.itemName,
      kind: item.kind || 'صنف ملموس',
      unit: item.unit || 'كيلو',
      type: item.type || 'عام',
      initialStock: item.quantity || 0,
      minAlert: item.minAlert || 10,
      notes: item.notes || ''
    };
    this.cdr.detectChanges();
  }

  resetForm() {
    this.itemForm = {
      id: null,
      code: '',
      itemName: '',
      kind: 'صنف ملموس',
      unit: 'كيلو',
      type: 'أغذية جافة',
      initialStock: 0,
      minAlert: 10,
      notes: ''
    };
    this.generateNewCode();
    this.cdr.detectChanges();
  }

  onSubmit() {
    if (!this.itemForm.itemName) return;

    if (this.itemForm.id) {
      const payload = {
        id: this.itemForm.id,
        code: this.itemForm.code,
        itemName: this.itemForm.itemName,
        kind: this.itemForm.kind,
        unit: this.itemForm.unit,
        type: this.itemForm.type,
        minAlert: Number(this.itemForm.minAlert ?? 10),
        notes: this.itemForm.notes
      };

      this.api.updateInventory(payload).subscribe(() => {
        this.successMessage = `✅ تم تحديث بيانات (${payload.itemName}) بنجاح!`;
        this.loadData();
        this.backToList();
        setTimeout(() => { this.successMessage = ''; this.cdr.detectChanges(); }, 4000);
      });
    } else {
      const payload = {
        code: this.itemForm.code,
        itemName: this.itemForm.itemName,
        kind: this.itemForm.kind,
        unit: this.itemForm.unit,
        type: this.itemForm.type,
        quantity: this.itemForm.kind === 'صنف ملموس' ? Number(this.itemForm.initialStock || 0) : 0,
        minAlert: Number(this.itemForm.minAlert ?? 10),
        notes: this.itemForm.notes
      };

      this.api.addInventory(payload).subscribe(() => {
        this.successMessage = `✅ تم تسجيل وتعريف (${payload.itemName}) بنجاح وأصبح جاهزاً للاستخدام في التبرعات والمخازن والصرف!`;
        this.loadData();
        this.backToList();
        setTimeout(() => { this.successMessage = ''; this.cdr.detectChanges(); }, 4000);
      });
    }
  }

  deleteItem(id: string, event: Event) {
    event.stopPropagation();
    if (confirm('هل أنت متأكد من رغبتك في حذف هذا الصنف من الدليل التعريفي؟')) {
      this.api.deleteInventory(id).subscribe(() => {
        this.loadData();
        if (this.itemForm.id === id) {
          this.resetForm();
        }
      });
    }
  }
}
