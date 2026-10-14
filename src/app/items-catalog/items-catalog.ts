import { Component, ChangeDetectorRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Api } from '../api';

@Component({
  selector: 'app-items-catalog',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './items-catalog.html',
  styleUrls: ['./items-catalog.css']
})
export class ItemsCatalogComponent implements OnInit {
  items: any[] = [];
  selectedFilter: 'الكل' | 'صنف ملموس' | 'خدمة' = 'الكل';
  searchTerm: string = '';
  successMessage: string = '';

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

  constructor(private api: Api, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.generateNewCode();
    this.loadData();
  }

  loadData() {
    this.api.getInventory().subscribe((data: any[]) => {
      this.items = data;
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

  selectItem(item: any) {
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
    window.scrollTo({ top: 0, behavior: 'smooth' });
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
        minAlert: Number(this.itemForm.minAlert || 10),
        notes: this.itemForm.notes
      };

      this.api.updateInventory(payload).subscribe(() => {
        this.successMessage = `✅ تم تحديث بيانات (${payload.itemName}) بنجاح!`;
        this.loadData();
        this.resetForm();
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
        minAlert: Number(this.itemForm.minAlert || 10),
        notes: this.itemForm.notes
      };

      this.api.addInventory(payload).subscribe(() => {
        this.successMessage = `✅ تم تسجيل وتعريف (${payload.itemName}) بنجاح وأصبح جاهزاً للاستخدام في التبرعات والمخازن والصرف!`;
        this.loadData();
        this.resetForm();
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
