import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Api } from '../api';

interface AuditLog {
  id: number;
  occurredAt: string;
  username: string;
  actorName: string;
  action: string;
  entityType: string;
  entityId: string;
  entityName: string;
}

@Component({
  selector: 'app-audit-logs',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './audit-logs.html',
  styleUrls: ['./audit-logs.css']
})
export class AuditLogsComponent implements OnInit {
  filters = { name: '', action: '', entityType: '', from: '', to: '' };
  rows: AuditLog[] = [];
  page = 1;
  readonly pageSize = 25;
  totalCount = 0;
  loading = false;
  error = '';

  readonly actions = [
    { value: 'Create', label: 'إضافة' },
    { value: 'Update', label: 'تعديل' },
    { value: 'Delete', label: 'حذف' }
  ];
  readonly entityTypes = [
    { value: 'Beneficiary', label: 'ملفات المستفيدين' },
    { value: 'BeneficiaryFamilyMember', label: 'أفراد الأسرة' },
    { value: 'Distribution', label: 'حركات الصرف' },
    { value: 'Donation', label: 'التبرعات' },
    { value: 'InventoryItem', label: 'الأصناف والمخزون' },
    { value: 'ExpenseCategory', label: 'بنود المصروفات' },
    { value: 'StaffMember', label: 'فريق العمل' },
    { value: 'ApplicationUser', label: 'حسابات المستخدمين' },
    { value: 'Village', label: 'القرى' },
    { value: 'VillageArea', label: 'المناطق' },
    { value: 'AddressRecord', label: 'العناوين' },
    { value: 'SuspensionReason', label: 'أسباب الإيقاف' },
    { value: 'SeparationReason', label: 'أسباب الانفصال' },
    { value: 'CharitySettings', label: 'إعدادات الجمعية' }
  ];

  constructor(private api: Api) {}

  ngOnInit(): void { this.load(); }

  search(): void { this.page = 1; this.load(); }

  clear(): void {
    this.filters = { name: '', action: '', entityType: '', from: '', to: '' };
    this.search();
  }

  changePage(next: number): void {
    if (next < 1 || next > this.totalPages || next === this.page) return;
    this.page = next;
    this.load();
  }

  get totalPages(): number { return Math.max(1, Math.ceil(this.totalCount / this.pageSize)); }

  labelAction(value: string): string { return this.actions.find(x => x.value === value)?.label || value; }
  labelEntity(value: string): string { return this.entityTypes.find(x => x.value === value)?.label || value; }

  load(): void {
    this.loading = true;
    this.error = '';
    this.api.getAuditLogs({ ...this.filters, page: this.page, pageSize: this.pageSize }).subscribe({
      next: response => {
        this.rows = response.items || [];
        this.totalCount = response.totalCount || 0;
        this.loading = false;
      },
      error: err => {
        this.rows = [];
        this.loading = false;
        this.error = err.error?.message || 'تعذر تحميل سجل العمليات.';
      }
    });
  }
}
