import { Component, ChangeDetectorRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Api } from '../api';

@Component({
  selector: 'app-staff',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './staff.html',
  styleUrls: ['./staff.css']
})
export class StaffComponent implements OnInit {
  staffList: any[] = [];
  newStaff: any = { nationalId: '', name: '', phone: '', type: 'متطوع', role: 'باحث اجتماعي', salary: null, joinDate: new Date().toISOString().split('T')[0] };

  types = ['متطوع', 'موظف براتب'];
  roles = ['باحث اجتماعي', 'إداري', 'مندوب تحصيل', 'سائق', 'طبيب', 'عامل مخزن'];

  constructor(private api: Api, private cdr: ChangeDetectorRef) {}

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.api.getStaff().subscribe((data: any) => { this.staffList = data; this.cdr.detectChanges(); });
  }

  selectStaff(s: any) {
    this.newStaff = { ...s };
    this.cdr.detectChanges();
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  resetForm() {
    this.newStaff = { nationalId: '', name: '', phone: '', type: 'متطوع', role: 'باحث اجتماعي', salary: null, joinDate: new Date().toISOString().split('T')[0] };
    this.cdr.detectChanges();
  }

  onSubmit() {
    if (!this.newStaff.name || !this.newStaff.nationalId) return;
    if (this.newStaff.id) {
      this.api.updateStaff(this.newStaff).subscribe(() => { this.loadData(); this.resetForm(); });
    } else {
      this.api.addStaff(this.newStaff).subscribe(() => { this.loadData(); this.resetForm(); });
    }
  }
}
