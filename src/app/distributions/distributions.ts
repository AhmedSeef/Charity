import { Component, ChangeDetectorRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Api } from '../api';
import { PaginationComponent } from '../pagination/pagination';

@Component({
  selector: 'app-distributions',
  standalone: true,
  imports: [CommonModule, FormsModule, PaginationComponent],
  templateUrl: './distributions.html',
  styleUrls: ['./distributions.css']
})
export class DistributionsComponent implements OnInit {
  distributions: any[] = [];
  beneficiaries: any[] = [];
  inventory: any[] = [];
  
  currentPage: number = 1;
  pageSize: number = 5;

  get paginatedDistributions(): any[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.distributions.slice(start, start + this.pageSize);
  }

  newDist: any = { 
    beneficiaryId: '', 
    inventoryId: '', 
    quantity: 1, 
    date: new Date().toISOString().split('T')[0], 
    notes: '' 
  };

  errorMessage: string = '';
  successMessage: string = '';

  constructor(private api: Api, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.loadData();
  }

  loadData() {
    this.api.getDistributions().subscribe((data: any) => { 
      this.distributions = data; 
      this.cdr.detectChanges(); 
    });
    this.api.getBeneficiaries().subscribe((data: any) => { 
      this.beneficiaries = data; 
      this.cdr.detectChanges(); 
    });
    this.api.getInventory().subscribe((data: any) => { 
      this.inventory = data; 
      this.cdr.detectChanges(); 
    });
  }

  get selectedItem(): any {
    return this.inventory.find(x => String(x.id) === String(this.newDist.inventoryId));
  }

  get isStockInsufficient(): boolean {
    const item = this.selectedItem;
    if (item && item.kind === 'صنف ملموس') {
      return Number(this.newDist.quantity) > Number(item.quantity);
    }
    return false;
  }

  getBenName(id: string) {
    const b = this.beneficiaries.find(x => String(x.id) === String(id));
    return b ? b.name : 'غير معروف';
  }

  getItem(id: string) {
    return this.inventory.find(x => String(x.id) === String(id));
  }

  getItemName(id: string) {
    const i = this.getItem(id);
    return i ? `${i.itemName} (${i.unit})` : 'غير معروف';
  }

  onSubmit() {
    this.errorMessage = '';
    this.successMessage = '';

    if (!this.newDist.beneficiaryId || !this.newDist.inventoryId || !this.newDist.quantity) {
      this.errorMessage = 'يرجى اختيار المستفيد، والصنف أو الخدمة، وتحديد الكمية.';
      return;
    }

    if (this.isStockInsufficient) {
      const item = this.selectedItem;
      this.errorMessage = `⚠️ عذراً، الكمية المطلوبة (${this.newDist.quantity}) أكبر من الرصيد المتوفر بالمخزن (${item.quantity} ${item.unit})!`;
      return;
    }

    const payload = { ...this.newDist, quantity: Number(this.newDist.quantity) };
    const item = this.selectedItem;

    this.api.addDistribution(payload).subscribe(() => {
      if (item && item.kind === 'صنف ملموس') {
        this.successMessage = `✅ تم تسجيل حركة الصرف بنجاح، وخصم (${payload.quantity} ${item.unit}) من رصيد المخزن!`;
      } else {
        this.successMessage = `✅ تم تقديم الخدمة وتسجيلها في ملف المستفيد بنجاح!`;
      }

      this.loadData();
      this.newDist = { 
        beneficiaryId: '', 
        inventoryId: '', 
        quantity: 1, 
        date: new Date().toISOString().split('T')[0], 
        notes: '' 
      };

      setTimeout(() => {
        this.successMessage = '';
        this.cdr.detectChanges();
      }, 5000);
    });
  }
}
