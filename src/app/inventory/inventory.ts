import { Component, ChangeDetectorRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Api } from '../api';

@Component({
  selector: 'app-inventory',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './inventory.html',
  styleUrls: ['./inventory.css']
})
export class InventoryComponent implements OnInit {
  inventory: any[] = [];
  selectedTypeFilter: string = 'الكل';
  searchTerm: string = '';
  successMessage: string = '';

  // Stock Adjustment Modal
  adjustModalOpen: boolean = false;
  selectedStockItem: any = null;
  adjustedQuantity: number = 0;
  adjustmentReason: string = 'جرد مخزني دوري';

  constructor(private api: Api, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.loadData();
  }

  loadData() {
    this.api.getInventory().subscribe((data: any[]) => {
      // Focus on tangible items with actual warehouse stock
      this.inventory = data;
      this.cdr.detectChanges();
    });
  }

  get tangibleItems(): any[] {
    return this.inventory.filter(i => i.kind === 'صنف ملموس');
  }

  get totalUnitsInWarehouse(): number {
    return this.tangibleItems.reduce((sum, item) => sum + Number(item.quantity || 0), 0);
  }

  get lowStockCount(): number {
    return this.tangibleItems.filter(i => Number(i.quantity) < Number(i.minAlert || 10)).length;
  }

  get availableCategories(): string[] {
    const cats = new Set<string>();
    this.tangibleItems.forEach(i => { if (i.type) cats.add(i.type); });
    return Array.from(cats);
  }

  get filteredTangibleItems(): any[] {
    return this.tangibleItems.filter(item => {
      const matchType = this.selectedTypeFilter === 'الكل' || item.type === this.selectedTypeFilter;
      const term = this.searchTerm.trim().toLowerCase();
      const matchSearch = !term ||
        (item.itemName && item.itemName.toLowerCase().includes(term)) ||
        (item.code && item.code.toLowerCase().includes(term)) ||
        (item.type && item.type.toLowerCase().includes(term));
      return matchType && matchSearch;
    });
  }

  openAdjustModal(item: any, event: Event) {
    event.stopPropagation();
    this.selectedStockItem = { ...item };
    this.adjustedQuantity = Number(item.quantity || 0);
    this.adjustmentReason = 'جرد مخزني دوري وتصحيح رصيد';
    this.adjustModalOpen = true;
    this.cdr.detectChanges();
  }

  closeAdjustModal() {
    this.adjustModalOpen = false;
    this.selectedStockItem = null;
    this.cdr.detectChanges();
  }

  saveStockAdjustment() {
    if (!this.selectedStockItem) return;
    const oldQty = this.selectedStockItem.quantity;
    const newQty = Number(this.adjustedQuantity);

    const updatedItem = {
      ...this.selectedStockItem,
      quantity: newQty,
      notes: this.adjustmentReason ? `${this.selectedStockItem.notes || ''} [تعديل رصيد: من ${oldQty} إلى ${newQty} - ${this.adjustmentReason}]` : this.selectedStockItem.notes
    };

    this.api.updateInventory(updatedItem).subscribe(() => {
      this.successMessage = `✅ تم تحديث رصيد (${updatedItem.itemName}) بالمخزن إلى (${newQty} ${updatedItem.unit}) بنجاح!`;
      this.closeAdjustModal();
      this.loadData();
      setTimeout(() => { this.successMessage = ''; this.cdr.detectChanges(); }, 4000);
    });
  }
}
