import { Component, OnInit, ChangeDetectorRef, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { Subscription } from 'rxjs';
import { Api } from '../api';

@Component({
  selector: 'app-bi-analytics',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './bi-analytics.html',
  styleUrls: ['./bi-analytics.css']
})
export class BiAnalyticsComponent implements OnInit, OnDestroy {
  activeCharity: any = null;
  biData: any = null;
  isLoading: boolean = true;

  private charitySub?: Subscription;

  constructor(public api: Api, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.activeCharity = this.api.getActiveCharity();
    this.loadAnalytics();

    this.charitySub = this.api.activeCharity$.subscribe(() => {
      this.activeCharity = this.api.getActiveCharity();
      this.loadAnalytics();
    });
  }

  ngOnDestroy(): void {
    if (this.charitySub) {
      this.charitySub.unsubscribe();
    }
  }

  loadAnalytics(): void {
    this.isLoading = true;
    this.api.getBiAnalytics().subscribe({
      next: (data: any) => {
        this.biData = data;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.isLoading = false;
        this.cdr.detectChanges();
      }
    });
  }

  printReport(): void {
    window.print();
  }

  // Helper calculation for SVG bar chart scaling
  getMaxMonthlyDonation(): number {
    if (!this.biData?.monthlyTrends || this.biData.monthlyTrends.length === 0) return 1000;
    const max = Math.max(...this.biData.monthlyTrends.map((m: any) => m.donationsTotal || 0));
    return max > 0 ? max : 1000;
  }

  getMaxMonthlyDist(): number {
    if (!this.biData?.monthlyTrends || this.biData.monthlyTrends.length === 0) return 10;
    const max = Math.max(...this.biData.monthlyTrends.map((m: any) => m.distributionsCount || 0));
    return max > 0 ? max : 10;
  }

  getUnsafeRoofCount(): number {
    const list = this.biData?.roofTypeBreakdown || this.biData?.RoofTypeBreakdown || [];
    return list
      .filter((r: any) => {
        const type = (r.roofType || r.RoofType || '');
        return type.includes('قش') || type.includes('جريد') || type.includes('مكشوف') || type.includes('صاج');
      })
      .reduce((sum: number, r: any) => sum + (r.count || r.Count || 0), 0);
  }
}
