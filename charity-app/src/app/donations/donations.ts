import { Component, ChangeDetectorRef, OnInit, Optional } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { Api } from '../api';
import { PaginationComponent } from '../pagination/pagination';

@Component({
  selector: 'app-donations',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, PaginationComponent],
  templateUrl: './donations.html',
  styleUrls: ['./donations.css']
})
export class DonationsComponent implements OnInit {
  viewMode: 'list' | 'form' = 'list';
  donations: any[] = [];
  inventory: any[] = [];
  selectedFilter: 'الكل' | 'نقدي' | 'عيني' | 'خدمة' = 'الكل';
  searchTerm: string = '';
  successMessage: string = '';

  currentPage: number = 1;
  pageSize: number = 5;

  newDonation: any = {
    donorName: '',
    donorPhone: '',
    donationKind: 'نقدي', // 'نقدي' | 'عيني' | 'خدمة'
    amount: null,
    cashCategory: 'زكاة مال',
    paymentMethod: 'نقداً بالخزينة',
    inventoryId: '',
    quantity: 1,
    unit: '',
    estimatedValue: null,
    date: new Date().toISOString().split('T')[0],
    notes: ''
  };

  cashCategories = [
    'زكاة مال',
    'صدقة جارية',
    'كفارة يمين',
    'فدية صيام',
    'كفالة يتيم',
    'إطعام مساكين',
    'علاج مرضى وأدوية',
    'تبرع عام للجمعية'
  ];

  paymentMethods = [
    'نقداً بالخزينة',
    'إنستاباي / تحويل بنكي',
    'فودافون كاش ومحافظ إلكترونية',
    'شيك بنكي',
    'مندوب تحصيل'
  ];

  constructor(
    private api: Api, 
    private cdr: ChangeDetectorRef, 
    @Optional() private route?: ActivatedRoute,
    @Optional() private router?: Router
  ) {}

  get isFormPage(): boolean { return this.route?.snapshot.data['formPage'] === true; }

  ngOnInit(): void {
    this.loadData();
    this.initRouteSync();
  }

  initRouteSync() {
    this.route?.queryParams.subscribe(params => {
      const mode = params['mode'];
      if (this.isFormPage || mode === 'add') {
        this.resetForm();
        this.viewMode = 'form';
      } else {
        this.viewMode = 'list';
      }
      this.cdr.detectChanges();
    });
  }

  openAdd() {
    this.resetForm();
    this.viewMode = 'form';
    if (this.router) {
      void this.router.navigate([], { relativeTo: this.route, queryParams: { mode: 'add' } });
    }
  }

  backToList() {
    this.resetForm();
    this.viewMode = 'list';
    if (this.router) {
      void this.router.navigate(['/donations'], { queryParams: {} });
    }
  }

  loadData() {
    this.api.getDonations().subscribe((data: any[]) => {
      this.donations = data;
      this.cdr.detectChanges();
    });

    this.api.getInventory().subscribe((inv: any[]) => {
      this.inventory = inv;
      this.cdr.detectChanges();
    });
  }

  get tangibleItems(): any[] {
    return this.inventory.filter(i => i.kind === 'صنف ملموس');
  }

  get serviceItems(): any[] {
    return this.inventory.filter(i => i.kind === 'خدمة');
  }

  onKindChange() {
    this.newDonation.inventoryId = '';
    this.newDonation.unit = '';
    if (this.newDonation.donationKind === 'عيني') {
      if (this.tangibleItems.length > 0 && !this.newDonation.inventoryId) {
        this.newDonation.inventoryId = this.tangibleItems[0].id;
        this.onItemSelect();
      }
    } else if (this.newDonation.donationKind === 'خدمة') {
      if (this.serviceItems.length > 0 && !this.newDonation.inventoryId) {
        this.newDonation.inventoryId = this.serviceItems[0].id;
        this.onItemSelect();
      }
    }
  }

  donationUnits = [
    'كيلو',
    'قطعة',
    'إزازة / زجاجة',
    'علبة',
    'كرتونة',
    'كيس',
    'شكارة / شوال',
    'شنطة / وجبة',
    'شريط (دواء)',
    'لتر',
    'متر',
    'طقم / باقة'
  ];

  onItemSelect() {
    const item = this.inventory.find(i => String(i.id) === String(this.newDonation.inventoryId));
    if (item) {
      this.newDonation.unit = item.unit || 'كيلو';
      if (item.unit && !this.donationUnits.includes(item.unit)) {
        this.donationUnits.unshift(item.unit);
      }
    }
  }

  getSelectedItemName(): string {
    const item = this.inventory.find(i => String(i.id) === String(this.newDonation.inventoryId));
    return item ? item.itemName : 'غير محدد';
  }

  getSelectedItemStock(): number {
    const item = this.inventory.find(i => String(i.id) === String(this.newDonation.inventoryId));
    return item ? Number(item.quantity || 0) : 0;
  }

  get totalCash(): number {
    return this.donations
      .filter(d => (d.donationKind === 'نقدي' || !d.donationKind) && d.amount)
      .reduce((sum, d) => sum + Number(d.amount), 0);
  }

  get inKindCount(): number {
    return this.donations.filter(d => d.donationKind === 'عيني').length;
  }

  get serviceCount(): number {
    return this.donations.filter(d => d.donationKind === 'خدمة').length;
  }

  get filteredDonations(): any[] {
    return this.donations.filter(d => {
      const kind = d.donationKind || 'نقدي';
      const matchFilter = this.selectedFilter === 'الكل' || kind === this.selectedFilter;
      const term = this.searchTerm.trim().toLowerCase();
      const matchSearch = !term ||
        (d.donorName && d.donorName.toLowerCase().includes(term)) ||
        (d.type && d.type.toLowerCase().includes(term)) ||
        (d.itemName && d.itemName.toLowerCase().includes(term));
      return matchFilter && matchSearch;
    });
  }

  get paginatedDonations(): any[] {
    const start = (this.currentPage - 1) * this.pageSize;
    return this.filteredDonations.slice(start, start + this.pageSize);
  }

  onSubmit() {
    if (!this.newDonation.donorName) return;

    const payload: any = { ...this.newDonation };

    if (payload.donationKind === 'نقدي') {
      if (!Number.isFinite(Number(payload.amount)) || Number(payload.amount) <= 0) return;
      payload.amount = Number(payload.amount);
      payload.type = payload.cashCategory;
    } else if (payload.donationKind === 'عيني') {
      if (!payload.inventoryId || !Number.isFinite(Number(payload.quantity)) || Number(payload.quantity) <= 0) return;
      const item = this.inventory.find(i => String(i.id) === String(payload.inventoryId));
      payload.itemName = item ? item.itemName : 'صنف ملموس';
      payload.unit = item ? item.unit : '';
      payload.quantity = Number(payload.quantity);
      payload.type = 'تبرع عيني (' + payload.itemName + ')';
      payload.amount = payload.estimatedValue ? Number(payload.estimatedValue) : 0;
    } else if (payload.donationKind === 'خدمة') {
      if (!payload.inventoryId || !Number.isFinite(Number(payload.quantity)) || Number(payload.quantity) <= 0) return;
      const item = this.inventory.find(i => String(i.id) === String(payload.inventoryId));
      payload.itemName = item ? item.itemName : 'خدمة';
      payload.unit = item ? item.unit : '';
      payload.quantity = Number(payload.quantity);
      payload.type = 'رعاية وتبرع بخدمة (' + payload.itemName + ')';
      payload.amount = payload.estimatedValue ? Number(payload.estimatedValue) : 0;
    }

    this.api.addDonation(payload).subscribe(() => {
      if (payload.donationKind === 'عيني') {
        this.successMessage = `✅ تم استلام التبرع بنجاح وزيادة رصيد (${payload.itemName}) في المخزن بمقدار (${payload.quantity} ${payload.unit})!`;
      } else if (payload.donationKind === 'نقدي') {
        this.successMessage = `✅ تم استلام التبرع النقدي بقيمة (${payload.amount} ج.م) بنجاح وإضافته للخزينة!`;
      } else {
        this.successMessage = `✅ تم تسجيل التبرع بالخدمة (${payload.itemName}) بنجاح!`;
      }

      this.loadData();
      this.backToList();

      setTimeout(() => {
        this.successMessage = '';
        this.cdr.detectChanges();
      }, 6000);
    });
  }

  resetForm() {
    this.newDonation = {
      donorName: '',
      donorPhone: '',
      donationKind: 'نقدي',
      amount: null,
      cashCategory: 'زكاة مال',
      paymentMethod: 'نقداً بالخزينة',
      inventoryId: '',
      quantity: 1,
      unit: '',
      estimatedValue: null,
      date: new Date().toISOString().split('T')[0],
      notes: ''
    };
    this.cdr.detectChanges();
  }
}
