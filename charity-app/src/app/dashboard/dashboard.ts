import { Component, ChangeDetectorRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Api } from '../api';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css']
})
export class DashboardComponent implements OnInit {
  beneficiariesCount = 0;
  totalCashDonations = 0;
  inKindDonationsCount = 0;
  tangibleItemsCount = 0;
  servicesCount = 0;
  distributionsCount = 0;
  staffCount = 0;
  
  // Federation Stats
  charitiesCount = 0;
  totalNetworkDistributionsCount = 0;
  activeCharity: any = null;
  networkStats: any = null;

  beneficiaries: any[] = [];
  inventory: any[] = [];
  recentDonations: any[] = [];
  recentDistributions: any[] = [];

  constructor(public api: Api, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.activeCharity = this.api.getActiveCharity();
    this.api.getDashboardStats().subscribe((stats: any) => {
      this.networkStats = stats;
      this.cdr.detectChanges();
    });

    // 1. Beneficiaries (Shared across network)
    this.api.getBeneficiaries().subscribe((data: any[]) => { 
      this.beneficiaries = data;
      this.beneficiariesCount = data.length; 
      this.cdr.detectChanges(); 
    });

    // 2. Donations (Isolated for active charity)
    this.api.getDonations().subscribe((data: any[]) => { 
      this.recentDonations = data.slice(0, 5);
      this.totalCashDonations = data
        .filter(d => (d.donationKind === 'نقدي' || !d.donationKind) && d.amount)
        .reduce((sum, d) => sum + Number(d.amount), 0);
      this.inKindDonationsCount = data.filter(d => d.donationKind === 'عيني').length;
      this.cdr.detectChanges(); 
    });

    // 3. Inventory (Isolated for active charity)
    this.api.getInventory().subscribe((data: any[]) => { 
      this.inventory = data;
      this.tangibleItemsCount = data.filter(i => i.kind === 'صنف ملموس').length;
      this.servicesCount = data.filter(i => i.kind === 'خدمة').length;
      this.cdr.detectChanges(); 
    });

    // 4. Distributions (Isolated for active charity)
    this.api.getDistributions(true).subscribe((data: any[]) => { 
      this.distributionsCount = data.length;
      this.recentDistributions = data.slice(0, 5);
      this.cdr.detectChanges(); 
    });

    // 5. Staff (Isolated for active charity)
    this.api.getStaff().subscribe((data: any[]) => { 
      this.staffCount = data.length;
      this.cdr.detectChanges(); 
    });

    // 6. Federation statistics
    this.api.getCharities().subscribe((list: any[]) => {
      this.charitiesCount = list.length;
      this.cdr.detectChanges();
    });

    this.api.getAllDistributions().subscribe((allDist: any[]) => {
      this.totalNetworkDistributionsCount = allDist.length;
      this.cdr.detectChanges();
    });
  }

  getBenName(id: string): string {
    const b = this.beneficiaries.find(x => String(x.id) === String(id));
    return b ? b.name : 'غير معروف';
  }

  getItemName(id: string): string {
    const i = this.inventory.find(x => String(x.id) === String(id));
    return i ? `${i.itemName} (${i.unit})` : 'غير معروف';
  }
}
