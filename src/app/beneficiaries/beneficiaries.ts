import { Component, ChangeDetectorRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Api } from '../api';

@Component({
  selector: 'app-beneficiaries',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './beneficiaries.html',
  styleUrls: ['./beneficiaries.css']
})
export class BeneficiariesComponent implements OnInit {
  beneficiaries: any[] = [];
  distributions: any[] = [];
  inventory: any[] = [];
  
  historyModalOpen = false;
  selectedHistory: any[] = [];
  selectedBenName = '';
  
  newBen: any = { 
    nationalId: '', 
    name: '', 
    phone: '',
    category: 'أرامل وأيتام', 
    maritalStatus: 'متزوج',
    familyCount: 1,
    address: '', 
    housing: 'إيجار جديد',
    healthIssues: '',
    researchNotes: '',
    status: 'مستحق فعال' 
  };

  categories = ['أرامل وأيتام', 'مرضى ومسنين', 'غارمين', 'حالات طوارئ', 'طالب علم'];
  maritalStatuses = ['متزوج', 'أرمل', 'مطلق', 'أعزب'];
  housingTypes = ['إيجار جديد', 'إيجار قديم', 'ملك', 'سكن مشترك', 'سكن متهالك/يحتاج ترميم'];

  constructor(private api: Api, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.loadData();
  }

  loadData() {
    this.api.getBeneficiaries().subscribe((data: any) => { this.beneficiaries = data; this.cdr.detectChanges(); });
    this.api.getDistributions().subscribe((data: any) => { this.distributions = data; this.cdr.detectChanges(); });
    this.api.getInventory().subscribe((data: any) => { this.inventory = data; this.cdr.detectChanges(); });
  }

  selectBeneficiary(b: any) {
    this.newBen = { ...b };
    this.cdr.detectChanges();
    // Scroll to top for better UX
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  resetForm() {
    this.newBen = { 
      nationalId: '', name: '', phone: '', category: 'أرامل وأيتام', maritalStatus: 'متزوج',
      familyCount: 1, address: '', housing: 'إيجار جديد', healthIssues: '', researchNotes: '', status: 'مستحق فعال' 
    };
    this.cdr.detectChanges();
  }

  onSubmit() {
    if (!this.newBen.name || !this.newBen.nationalId) return;
    
    if (this.newBen.id) {
      this.api.updateBeneficiary(this.newBen).subscribe(() => {
        this.loadData();
        this.resetForm();
      });
    } else {
      this.api.addBeneficiary(this.newBen).subscribe(() => {
        this.loadData();
        this.resetForm();
      });
    }
  }

  openHistory(b: any, event: Event) {
    event.stopPropagation();
    this.selectedBenName = b.name;
    this.selectedHistory = this.distributions
      .filter(dist => String(dist.beneficiaryId) === String(b.id))
      .map(dist => {
        const item = this.inventory.find(i => String(i.id) === String(dist.inventoryId));
        return {
          ...dist,
          itemName: item ? `${item.itemName} (${item.unit})` : 'غير معروف'
        };
      });
    this.historyModalOpen = true;
    this.cdr.detectChanges();
  }

  closeHistory() {
    this.historyModalOpen = false;
    this.cdr.detectChanges();
  }
}
