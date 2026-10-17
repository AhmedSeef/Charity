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

  // الفلترة والبحث بالسجل
  searchFilter: string = '';
  currentPage: number = 1;
  pageSize: number = 10;

  // نافذة عرض التفاصيل الكاملة لحركة الصرف والمستلم
  selectedDistForDetails: any = null;

  errorMessage: string = '';
  successMessage: string = '';

  constructor(
    public api: Api, 
    private cdr: ChangeDetectorRef, 
    public auth: AuthService
  ) {}

  ngOnInit(): void {
    this.activeCharity = this.api.getActiveCharity();
    this.loadData();
  }

  loadData(): void {
    this.activeCharity = this.api.getActiveCharity();
    const isPlatformAdmin = this.auth.getCurrentUser()?.role === 'Admin';

    // حركات الصرف الحالية أو لكل الشبكة للأدمن
    this.api.getDistributions(!isPlatformAdmin).subscribe({
      next: (data: any) => { 
        this.distributions = data || []; 
        this.cdr.detectChanges(); 
      }
    });

    // قاعدة المستفيدين المشتركة
    this.api.getBeneficiaries().subscribe({
      next: (data: any) => { 
        this.beneficiaries = data || []; 
        this.cdr.detectChanges(); 
      }
    });

    // مخزن الجمعية
    this.api.getInventory().subscribe({
      next: (data: any) => { 
        this.inventory = data || []; 
        this.cdr.detectChanges(); 
      }
    });
  }

  get filteredDistributions(): any[] {
    let list = this.distributions;
    if (this.searchFilter.trim()) {
      const q = this.searchFilter.trim().toLowerCase();
      list = list.filter(d => 
        (d.beneficiaryName && d.beneficiaryName.toLowerCase().includes(q)) ||
        (d.receivedByName && d.receivedByName.toLowerCase().includes(q)) ||
        (d.receivedByNationalId && d.receivedByNationalId.includes(q)) ||
        (d.receivedByRelationship && d.receivedByRelationship.toLowerCase().includes(q)) ||
        (d.itemName && d.itemName.toLowerCase().includes(q)) ||
        (d.notes && d.notes.toLowerCase().includes(q)) ||
        (d.date && d.date.includes(q))
      );
    }
    return list;
  }

  get paginatedDistributions(): any[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.filteredDistributions.slice(start, start + this.pageSize);
  }

  getBeneficiary(id: string): any {
    return this.beneficiaries.find(x => String(x.id) === String(id));
  }

  getBenName(id: string): string {
    const b = this.getBeneficiary(id);
    return b ? b.name : 'غير معروف';
  }

  getBenFamilyCode(id: string): string {
    const b = this.getBeneficiary(id);
    return b?.familyCode || '—';
  }

  getItem(id: string): any {
    return this.inventory.find(x => String(x.id) === String(id));
  }

  getItemName(d: any): string {
    if (d.itemName) {
      return `${d.itemName} ${d.unit ? '(' + d.unit + ')' : ''}`;
    }
    const i = this.getItem(d.inventoryItemId || d.inventoryId);
    return i ? `${i.itemName} (${i.unit})` : 'مساعدة معتمدة';
  }

  // ==========================================
  // نافذة إظهار تفاصيل المستلم وسند الاستلام
  // ==========================================
  openDetails(d: any): void {
    const b = this.getBeneficiary(d.beneficiaryId);
    this.selectedDistForDetails = {
      ...d,
      beneficiary: b,
      familyCode: b?.familyCode || '—',
      familyName: b?.familyName || b?.name || '—',
      headName: b?.name || d.beneficiaryName,
      headNationalId: b?.nationalId || '—',
      recipientName: d.receivedByName || b?.name || d.beneficiaryName,
      recipientNationalId: d.receivedByNationalId || b?.nationalId || '—',
      recipientRelationship: d.receivedByRelationship || 'صاحب الملف الأساسي (رب الأسرة)'
    };
    this.cdr.detectChanges();
  }

  closeDetails(): void {
    this.selectedDistForDetails = null;
    this.cdr.detectChanges();
  }

  printReceipt(): void {
    window.print();
  }
}
