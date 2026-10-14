import { Component, ChangeDetectorRef, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { Api } from '../api';
import { PaginationComponent } from '../pagination/pagination';

@Component({
  selector: 'app-staff',
  standalone: true,
  imports: [CommonModule, FormsModule, PaginationComponent],
  templateUrl: './staff.html',
  styleUrls: ['./staff.css']
})
export class StaffComponent implements OnInit, OnDestroy {
  // Navigation: 'list' | 'detail' | 'form'
  viewMode: 'list' | 'detail' | 'form' = 'list';
  formMode: 'add' | 'edit' = 'add';
  selectedStaff: any = null;

  staffList: any[] = [];
  
  // Search & Filter
  searchTerm: string = '';
  selectedTypeFilter: string = 'الكل';

  // Pagination
  currentPage: number = 1;
  pageSize: number = 5;

  // Alerts
  successMessage: string = '';
  errorMessage: string = '';

  newStaff: any = { 
    id: null,
    nationalId: '', 
    name: '', 
    phone: '', 
    address: '',
    type: 'متطوع', 
    role: 'باحث اجتماعي', 
    salary: null, 
    joinDate: new Date().toISOString().split('T')[0],
    tasks: '',
    status: 'نشط على رأس العمل'
  };

  types = ['متطوع', 'موظف براتب'];
  roles = [
    'باحث اجتماعي ميداني', 
    'إداري ومسؤول مشروعات', 
    'أمين مخزن ومسؤول توريدات', 
    'مندوب تحصيل تبرعات', 
    'طبيب متطوع / رعاية صحية', 
    'سائق ومسؤول توزيع', 
    'منسق قوافل ومتطوعين',
    'محاسب ومسؤول مالي'
  ];

  private querySub?: Subscription;

  constructor(
    private api: Api, 
    private cdr: ChangeDetectorRef,
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadData();
  }

  ngOnDestroy(): void {
    if (this.querySub) {
      this.querySub.unsubscribe();
    }
  }

  loadData() {
    this.api.getStaff().subscribe((data: any[]) => { 
      this.staffList = data; 
      this.initRouteSync();
      this.cdr.detectChanges(); 
    });
  }

  initRouteSync() {
    if (this.querySub) return;
    this.querySub = this.route.queryParams.subscribe(params => {
      const mode = params['mode'];
      const id = params['id'];

      if (mode === 'detail' && id) {
        const found = this.staffList.find(s => String(s.id) === String(id));
        if (found) {
          this.selectedStaff = found;
          this.viewMode = 'detail';
        } else {
          this.viewMode = 'list';
        }
      } else if (mode === 'edit' && id) {
        const found = this.staffList.find(s => String(s.id) === String(id));
        if (found) {
          this.newStaff = { ...found };
          this.formMode = 'edit';
          this.viewMode = 'form';
        } else {
          this.viewMode = 'list';
        }
      } else if (mode === 'add') {
        this.resetForm();
        this.formMode = 'add';
        this.viewMode = 'form';
      } else {
        this.viewMode = 'list';
        this.selectedStaff = null;
      }
      this.cdr.detectChanges();
    });
  }

  // Navigation helpers
  openDetail(s: any) {
    this.router.navigate([], { relativeTo: this.route, queryParams: { mode: 'detail', id: s.id } });
  }

  openAdd() {
    this.router.navigate([], { relativeTo: this.route, queryParams: { mode: 'add' } });
  }

  openEdit(s: any, event?: Event) {
    if (event) event.stopPropagation();
    this.router.navigate([], { relativeTo: this.route, queryParams: { mode: 'edit', id: s.id } });
  }

  backToList() {
    this.router.navigate([], { relativeTo: this.route, queryParams: {} });
  }

  onTypeChange() {
    if (this.newStaff.type === 'متطوع') {
      this.newStaff.salary = null;
    }
  }

  get volunteerCount(): number {
    return this.staffList.filter(s => s.type === 'متطوع').length;
  }

  get employeeCount(): number {
    return this.staffList.filter(s => s.type === 'موظف براتب').length;
  }

  get filteredStaff(): any[] {
    return this.staffList.filter(s => {
      const matchType = this.selectedTypeFilter === 'الكل' || s.type === this.selectedTypeFilter;
      const term = this.searchTerm.trim().toLowerCase();
      const matchSearch = !term ||
        (s.name && s.name.toLowerCase().includes(term)) ||
        (s.nationalId && s.nationalId.includes(term)) ||
        (s.phone && s.phone.includes(term)) ||
        (s.role && s.role.toLowerCase().includes(term));
      return matchType && matchSearch;
    });
  }

  get paginatedStaff(): any[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.filteredStaff.slice(start, start + this.pageSize);
  }

  resetForm() {
    this.newStaff = { 
      id: null,
      nationalId: '', 
      name: '', 
      phone: '', 
      address: '',
      type: 'متطوع', 
      role: 'باحث اجتماعي ميداني', 
      salary: null, 
      joinDate: new Date().toISOString().split('T')[0],
      tasks: '',
      status: 'نشط على رأس العمل'
    };
  }

  onSubmit() {
    if (!this.newStaff.name || !this.newStaff.nationalId) {
      this.errorMessage = 'يرجى إدخال اسم العضو والرقم القومي بشكل صحيح.';
      return;
    }

    this.errorMessage = '';

    if (this.newStaff.id) {
      this.api.updateStaff(this.newStaff).subscribe(() => {
        this.successMessage = `✅ تم تحديث بيانات (${this.newStaff.name}) بنجاح!`;
        this.api.getStaff().subscribe((data: any[]) => {
          this.staffList = data;
          this.openDetail(this.newStaff);
          this.cdr.detectChanges();
        });
        setTimeout(() => { this.successMessage = ''; this.cdr.detectChanges(); }, 4000);
      });
    } else {
      this.api.addStaff(this.newStaff).subscribe((added: any) => {
        this.successMessage = `✅ تم إضافة (${added.name}) إلى فريق العمل بنجاح!`;
        this.api.getStaff().subscribe((data: any[]) => {
          this.staffList = data;
          this.openDetail(added);
          this.cdr.detectChanges();
        });
        setTimeout(() => { this.successMessage = ''; this.cdr.detectChanges(); }, 4000);
      });
    }
  }

  deleteStaff(id: string, event: Event) {
    event.stopPropagation();
    if (confirm('هل أنت متأكد من رغبتك في حذف هذا العضو من سجلات الموارد البشرية؟')) {
      this.api.deleteStaff(id).subscribe(() => {
        this.loadData();
        if (this.selectedStaff && this.selectedStaff.id === id) {
          this.backToList();
        }
      });
    }
  }

  printStaffProfile() {
    window.print();
  }
}
