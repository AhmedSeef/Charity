import { Component, ChangeDetectorRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Api } from '../api';
import { AuthService } from '../auth.service';
import { PaginationComponent } from '../pagination/pagination';

@Component({
  selector: 'app-distributions',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, PaginationComponent],
  templateUrl: './distributions.html',
  styleUrls: ['./distributions.css']
})
export class DistributionsComponent implements OnInit {
  distributions: any[] = [];
  beneficiaries: any[] = [];
  inventory: any[] = [];
  activeCharity: any = null;

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

  constructor(public api: Api, private cdr: ChangeDetectorRef, public auth: AuthService) {}

  ngOnInit(): void {
    this.activeCharity = this.api.getActiveCharity();
    this.loadData();
  }

  loadData() {
    this.activeCharity = this.api.getActiveCharity();
    const isPlatformAdmin = this.auth.getCurrentUser()?.role === 'Admin';

    // Current charity distributions or whole network for platform Admin
    this.api.getDistributions(!isPlatformAdmin).subscribe((data: any) => { 
      this.distributions = data; 
      this.cdr.detectChanges(); 
    });

    // 2. Shared Beneficiaries (from the shared network)
    this.api.getBeneficiaries().subscribe((data: any) => { 
      this.beneficiaries = data; 
      this.cdr.detectChanges(); 
    });

    // 3. Current charity inventory (isolated stock)
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

    if (this.auth.getCurrentUser()?.role === 'Admin') {
      this.errorMessage = '🔒 غير مصرح لمدير المنصة تسجيل حركات صرف؛ الصرف مقصور على مسؤولي الجمعيات.';
      return;
    }

    if (!this.newDist.beneficiaryId || !this.newDist.inventoryId ||
        !Number.isFinite(Number(this.newDist.quantity)) || Number(this.newDist.quantity) <= 0) {
      this.errorMessage = 'يرجى اختيار المستفيد، والصنف أو الخدمة، وتحديد الكمية.';
      return;
    }

    if (!this.selectedItem) {
      this.errorMessage = 'الصنف أو الخدمة المختارة غير موجودة في مخزن جمعيتكم.';
      return;
    }

    const selectedBen = this.beneficiaries.find(b => String(b.id) === String(this.newDist.beneficiaryId));
    if (selectedBen && (selectedBen.isSuspended || selectedBen.status === 'موقوف')) {
      this.errorMessage = `⛔ عذراً، لا يمكن تسجيل أي صرف للمستفيد (${selectedBen.name}) لأن الملف موقوف حالياً وحظر الصرف نشط (السبب: ${selectedBen.suspensionReasonTitle || 'موقوف'}).`;
      return;
    }

    if (this.isStockInsufficient) {
      const item = this.selectedItem;
      this.errorMessage = `⚠️ عذراً، الكمية المطلوبة (${this.newDist.quantity}) أكبر من الرصيد المتوفر بمخزن ${this.activeCharity?.name} (${item.quantity} ${item.unit})!`;
      return;
    }

    const payload = { ...this.newDist, quantity: Number(this.newDist.quantity) };
    const item = this.selectedItem;

    this.api.addDistribution(payload).subscribe({
      next: () => {
        if (item && item.kind === 'صنف ملموس') {
          this.successMessage = `✅ تم تسجيل حركة الصرف بنجاح لحساب (${this.activeCharity?.name})، وخصم (${payload.quantity} ${item.unit}) من رصيد المخزن!`;
        } else {
          this.successMessage = `✅ تم توثيق تقديم الخدمة بنجاح في ملف المستفيد بالشبكة الموحدة!`;
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
      },
      error: (err: any) => {
        this.errorMessage = err.message || '⚠️ فشل في تسجيل حركة الصرف.';
        this.cdr.detectChanges();
      }
    });
  }
}
