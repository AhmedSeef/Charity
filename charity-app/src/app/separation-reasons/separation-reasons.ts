import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Api } from '../api';

@Component({
  selector: 'app-separation-reasons',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './separation-reasons.html',
  styleUrls: ['./separation-reasons.css']
})
export class SeparationReasonsComponent implements OnInit {
  viewMode: 'list' | 'form' = 'list';

  reasons: any[] = [];
  newReason: any = {
    title: '',
    description: ''
  };

  successMessage: string = '';
  errorMessage: string = '';
  searchTerm: string = '';
  isSaving: boolean = false;

  constructor(private api: Api, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.loadReasons();
  }

  loadReasons(): void {
    this.api.getSeparationReasons().subscribe({
      next: (data: any[]) => {
        this.reasons = data || [];
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        this.errorMessage = err.error?.message || err.message || 'فشل في تحميل قائمة أسباب الانفصال.';
        this.cdr.detectChanges();
      }
    });
  }

  switchView(mode: 'list' | 'form'): void {
    this.viewMode = mode;
    this.errorMessage = '';
    this.successMessage = '';
    if (mode === 'form') {
      this.newReason = { title: '', description: '' };
    }
    this.cdr.detectChanges();
  }

  get filteredReasons(): any[] {
    const term = this.searchTerm.trim().toLowerCase();
    if (!term) return this.reasons;
    return this.reasons.filter(r => 
      (r.title && r.title.toLowerCase().includes(term)) ||
      (r.description && r.description.toLowerCase().includes(term))
    );
  }

  addReason(): void {
    if (!this.newReason.title || !this.newReason.title.trim()) {
      this.errorMessage = 'يرجى إدخال عنوان أو مسمى سبب الانفصال.';
      return;
    }

    this.errorMessage = '';
    this.isSaving = true;

    this.api.addSeparationReason(this.newReason).subscribe({
      next: (created: any) => {
        this.isSaving = false;
        this.successMessage = `✅ تم إضافة سبب الانفصال (${created.title}) بنجاح!`;
        this.newReason = { title: '', description: '' };
        this.switchView('list');
        this.loadReasons();
      },
      error: (err: any) => {
        this.isSaving = false;
        this.errorMessage = err.error?.message || err.message || 'فشل في إضافة سبب الانفصال.';
        this.cdr.detectChanges();
      }
    });
  }

  deleteReason(id: string, title: string): void {
    const target = this.reasons.find(r => String(r.id) === String(id));
    if (target && target.linkedOperationsCount > 0) {
      this.errorMessage = `⛔ محظور الحذف: لا يمكن حذف سبب الانفصال (${title}) لأنه تم استخدامه في (${target.linkedOperationsCount}) عملية انفصال سابقة! يُسمح فقط بحذف الأسباب التي لم تُنفذ عليها أي عملية لحماية وتدقيق السجلات.`;
      this.cdr.detectChanges();
      setTimeout(() => { this.errorMessage = ''; this.cdr.detectChanges(); }, 6000);
      return;
    }

    if (confirm(`هل أنت متأكد من حذف سبب الانفصال "${title}"؟`)) {
      this.api.deleteSeparationReason(id).subscribe({
        next: () => {
          this.successMessage = `✅ تم حذف سبب الانفصال بنجاح.`;
          this.loadReasons();
          setTimeout(() => {
            this.successMessage = '';
            this.cdr.detectChanges();
          }, 3000);
        },
        error: (err: any) => {
          this.errorMessage = err.error?.message || err.message || 'فشل في حذف سبب الانفصال.';
          this.cdr.detectChanges();
        }
      });
    }
  }
}
