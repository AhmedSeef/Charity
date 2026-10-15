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
  allNetworkDistributions: any[] = [];
  beneficiaries: any[] = [];
  inventory: any[] = [];
  allInventory: any[] = [];
  
  viewScope: 'my_charity' | 'all_network' = 'my_charity';
  activeCharity: any = null;

  currentPage: number = 1;
  pageSize: number = 5;

  get displayedDistributions(): any[] {
    return this.viewScope === 'my_charity' ? this.distributions : this.allNetworkDistributions;
  }

  get paginatedDistributions(): any[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.displayedDistributions.slice(start, start + this.pageSize);
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

  constructor(public api: Api, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.activeCharity = this.api.getActiveCharity();
    this.loadData();
  }

  loadData() {
    this.activeCharity = this.api.getActiveCharity();

    // 1. Current charity distributions (isolated for internal inventory auditing)
    this.api.getDistributions(true).subscribe((data: any) => { 
      this.distributions = data; 
      this.cdr.detectChanges(); 
    });

    // 2. All network distributions (cross-charity view)
    this.api.getAllDistributions().subscribe((data: any) => {
      this.allNetworkDistributions = data;
      this.cdr.detectChanges();
    });

    // 3. Shared Beneficiaries
    this.api.getBeneficiaries().subscribe((data: any) => { 
      this.beneficiaries = data; 
      this.cdr.detectChanges(); 
    });

    // 4. Current charity inventory (isolated stock)
    this.api.getInventory().subscribe((data: any) => { 
      this.inventory = data; 
      this.cdr.detectChanges(); 
    });

    // 5. All inventory for resolving names in all-network view
    this.api.getAllInventory().subscribe((data: any) => {
      this.allInventory = data;
      this.cdr.detectChanges();
    });
  }

  setScope(scope: 'my_charity' | 'all_network') {
    this.viewScope = scope;
    this.currentPage = 1;
    this.cdr.detectChanges();
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
    return (this.allInventory || []).find(x => String(x.id) === String(id)) || 
           (this.inventory || []).find(x => String(x.id) === String(id));
  }

  getItemName(d: any) {
    if (d.itemName) {
      return `${d.itemName} ${d.unit ? '(' + d.unit + ')' : ''}`;
    }
    const i = this.getItem(d.inventoryId);
    return i ? `${i.itemName} (${i.unit})` : 'مساعدة معتمدة';
  }

  onSubmit() {
    this.errorMessage = '';
    this.successMessage = '';

    if (!this.newDist.beneficiaryId || !this.newDist.inventoryId || !this.newDist.quantity) {
      this.errorMessage = 'يرجى اختيار المستفيد، والصنف أو الخدمة من مخزن الجمعية، وتحديد الكمية.';
      return;
    }

    if (this.isStockInsufficient) {
      const item = this.selectedItem;
      this.errorMessage = `⚠️ عذراً، الكمية المطلوبة (${this.newDist.quantity}) أكبر من الرصيد المتوفر بمخزن ${this.activeCharity?.name} (${item.quantity} ${item.unit})!`;
      return;
    }

    const payload = { ...this.newDist, quantity: Number(this.newDist.quantity) };
    const item = this.selectedItem;

    this.api.addDistribution(payload).subscribe(() => {
      if (item && item.kind === 'صنف ملموس') {
        this.successMessage = `✅ تم تسجيل حركة الصرف لحساب (${this.activeCharity?.name})، وخصم (${payload.quantity} ${item.unit}) من رصيد المخزن الداخلي بنجاح!`;
      } else {
        this.successMessage = `✅ تم تقديم الخدمة باسم (${this.activeCharity?.name}) وتوثيقها في ملف المستفيد بنجاح!`;
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
