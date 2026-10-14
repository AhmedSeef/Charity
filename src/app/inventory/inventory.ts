import { Component, ChangeDetectorRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Api } from '../api';

@Component({
  selector: 'app-inventory',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './inventory.html',
  styleUrls: ['./inventory.css']
})
export class InventoryComponent implements OnInit {
  inventory: any[] = [];
  selectedFilter: 'الكل' | 'صنف ملموس' | 'خدمة' = 'الكل';
  searchTerm: string = '';

  newItem: any = {
    id: null,
    itemName: '',
    kind: 'صنف ملموس', // 'صنف ملموس' | 'خدمة'
    quantity: 0,
    unit: 'كيلو',
    type: 'أغذية جافة',
    notes: ''
  };

  tangibleUnits = ['كيلو', 'كرتونة', 'علبة', 'زجاجة', 'قطعة', 'شريط (دواء)', 'شنطة / وجبة', 'متر'];
  serviceUnits = ['خدمة / استشارة', 'كشف طبي', 'عملية جراحية', 'حالة / مساعدة', 'جلسة علاجية', 'منزل / سقف', 'منحة / كفالة'];

  tangibleTypes = ['أغذية جافة', 'لحوم ومجمدات', 'أدوية ومستلزمات طبية', 'أجهزة تعويضية وكراسي', 'ملابس وبطاطين', 'أكفان وتجهيز متوفين', 'متنوع'];
  serviceTypes = ['خدمات طبية وعلاجية', 'خدمات اجتماعية وتجهيز', 'خدمات إيواء وتعمير', 'خدمات تعليمية وكفالات', 'سداد ديون وغارمين'];

  constructor(private api: Api, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.loadData();
  }

  loadData() {
    this.api.getInventory().subscribe((data: any[]) => {
      this.inventory = data;
      this.cdr.detectChanges();
    });
  }

  onKindChange() {
    if (this.newItem.kind === 'صنف ملموس') {
      this.newItem.unit = 'كيلو';
      this.newItem.type = 'أغذية جافة';
    } else {
      this.newItem.quantity = 0;
      this.newItem.unit = 'كشف طبي';
      this.newItem.type = 'خدمات طبية وعلاجية';
    }
  }

  get filteredInventory(): any[] {
    return this.inventory.filter(item => {
      const matchFilter = this.selectedFilter === 'الكل' || item.kind === this.selectedFilter;
      const term = this.searchTerm.trim().toLowerCase();
      const matchSearch = !term || 
        (item.itemName && item.itemName.toLowerCase().includes(term)) ||
        (item.type && item.type.toLowerCase().includes(term)) ||
        (item.unit && item.unit.toLowerCase().includes(term));
      return matchFilter && matchSearch;
    });
  }

  get tangibleCount(): number {
    return this.inventory.filter(i => i.kind === 'صنف ملموس').length;
  }

  get serviceCount(): number {
    return this.inventory.filter(i => i.kind === 'خدمة').length;
  }

  get lowStockCount(): number {
    return this.inventory.filter(i => i.kind === 'صنف ملموس' && Number(i.quantity) < 10).length;
  }

  selectItem(item: any) {
    this.newItem = { ...item };
    this.cdr.detectChanges();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  resetForm() {
    this.newItem = {
      id: null,
      itemName: '',
      kind: 'صنف ملموس',
      quantity: 0,
      unit: 'كيلو',
      type: 'أغذية جافة',
      notes: ''
    };
    this.cdr.detectChanges();
  }

  onSubmit() {
    if (!this.newItem.itemName) return;

    if (this.newItem.id) {
      this.api.updateInventory(this.newItem).subscribe(() => {
        this.loadData();
        this.resetForm();
      });
    } else {
      this.api.addInventory(this.newItem).subscribe(() => {
        this.loadData();
        this.resetForm();
      });
    }
  }

  deleteItem(id: string, event: Event) {
    event.stopPropagation();
    if (confirm('هل أنت متأكد من رغبتك في حذف هذا الصنف من الدليل؟')) {
      this.api.deleteInventory(id).subscribe(() => {
        this.loadData();
        if (this.newItem.id === id) {
          this.resetForm();
        }
      });
    }
  }
}
