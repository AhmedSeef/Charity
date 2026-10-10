import { Component, ChangeDetectorRef, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Api } from '../api';

@Component({
  selector: 'app-donations',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './donations.html',
  styleUrls: ['./donations.css']
})
export class DonationsComponent implements OnInit {
  donations: any[] = [];
  newDonation: any = { donorName: '', amount: '', type: 'زكاة مال (نقدي)', date: new Date().toISOString().split('T')[0] };
  types = ['زكاة مال (نقدي)', 'صدقة جارية (نقدي)', 'كفالة يتيم (نقدي)', 'تبرع عيني (مواد غذائية)', 'تبرع عيني (أجهزة طبية/متنوع)'];

  constructor(private api: Api, private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.loadData();
  }

  loadData() {
    this.api.getDonations().subscribe((data: any) => { this.donations = data; this.cdr.detectChanges(); });
  }

  onSubmit() {
    if (!this.newDonation.donorName || !this.newDonation.amount) return;
    this.api.addDonation(this.newDonation).subscribe(() => {
      this.loadData();
      this.newDonation = { donorName: '', amount: '', type: 'زكاة مال (نقدي)', date: new Date().toISOString().split('T')[0] };
    });
  }
}
