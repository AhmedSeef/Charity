import { Component, ChangeDetectorRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Api } from '../api';

@Component({
  selector: 'app-distributions',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './distributions.html',
  styleUrls: ['./distributions.css']
})
export class DistributionsComponent implements OnInit {
  distributions: any[] = [];
  beneficiaries: any[] = [];
  inventory: any[] = [];
  
  newDist: any = { beneficiaryId: '', inventoryId: '', quantity: 1, date: new Date().toISOString().split('T')[0], notes: '' };

  constructor(private api: Api, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.loadData();
  }

  loadData() {
    this.api.getDistributions().subscribe((data: any) => { this.distributions = data; this.cdr.detectChanges(); });
    this.api.getBeneficiaries().subscribe((data: any) => { this.beneficiaries = data; this.cdr.detectChanges(); });
    this.api.getInventory().subscribe((data: any) => { this.inventory = data; this.cdr.detectChanges(); });
  }

  getBenName(id: string) {
    const b = this.beneficiaries.find(x => String(x.id) === String(id));
    return b ? b.name : 'غير معروف';
  }

  getItemName(id: string) {
    const i = this.inventory.find(x => String(x.id) === String(id));
    return i ? `${i.itemName} (${i.unit})` : 'غير معروف';
  }

  onSubmit() {
    if (!this.newDist.beneficiaryId || !this.newDist.inventoryId || !this.newDist.quantity) return;
    this.api.addDistribution(this.newDist).subscribe(() => {
      this.loadData();
      this.newDist = { beneficiaryId: '', inventoryId: '', quantity: 1, date: new Date().toISOString().split('T')[0], notes: '' };
    });
  }
}
