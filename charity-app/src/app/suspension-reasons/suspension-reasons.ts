import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Api } from '../api';

@Component({
  selector: 'app-suspension-reasons',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './suspension-reasons.html',
  styleUrls: ['./suspension-reasons.css']
})
export class SuspensionReasonsComponent implements OnInit {
  reasons: any[] = [];
  newReason: any = {
    title: '',
    description: ''
  };

  successMessage: string = '';
  errorMessage: string = '';
  searchTerm: string = '';

  constructor(private api: Api, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.loadReasons();
  }

  loadReasons(): void {
    this.api.getSuspensionReasons().subscribe({
      next: (data: any[]) => {
        this.reasons = data;
        this.cdr.detectChanges();
      },
      error: (err: any) => {
        this.errorMessage = err.message || 'فشل في تحميل قائمة أسباب الإيقاف.';
        this.cdr.detectChanges();
      }
    });
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
      this.errorMessage = 'يرجى إدخال عنوان أو مسمى سبب الإيقاف.';
      return;
    }

    this.errorMessage = '';
    this.api.addSuspensionReason(this.newReason).subscribe({
      next: (created: any) => {
        this.successMessage = `✅ تم إضافة سبب الإيقاف (${created.title}) بنجاح!`;
        this.newReason = { title: '', description: '' };
        this.loadReasons();
        setTimeout(() => {
          this.successMessage = '';
          this.cdr.detectChanges();
        }, 4000);
      },
      error: (err: any) => {
        this.errorMessage = err.message || 'فشل في إضافة سبب الإيقاف.';
        this.cdr.detectChanges();
      }
    });
  }

  deleteReason(id: string, title: string): void {
    if (confirm(`هل أنت متأكد من حذف سبب الإيقاف "${title}"؟`)) {
      this.api.deleteSuspensionReason(id).subscribe({
        next: () => {
          this.successMessage = `تم حذف سبب الإيقاف بنجاح.`;
          this.loadReasons();
          setTimeout(() => {
            this.successMessage = '';
            this.cdr.detectChanges();
          }, 3000);
        },
        error: (err: any) => {
          this.errorMessage = err.message || 'فشل في حذف سبب الإيقاف.';
          this.cdr.detectChanges();
        }
      });
    }
  }
}
