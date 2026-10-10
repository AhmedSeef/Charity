import { Component, ChangeDetectorRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Api } from '../api';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.css']
})
export class DashboardComponent implements OnInit {
  beneficiariesCount = 0;
  donationsCount = 0;
  inventoryCount = 0;

  constructor(private api: Api, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.api.getBeneficiaries().subscribe((data: any) => { this.beneficiariesCount = data.length; this.cdr.detectChanges(); });
    this.api.getDonations().subscribe((data: any) => { this.donationsCount = data.length; this.cdr.detectChanges(); });
    this.api.getInventory().subscribe((data: any) => { this.inventoryCount = data.length; this.cdr.detectChanges(); });
  }
}
