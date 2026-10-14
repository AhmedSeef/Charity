const fs = require('fs');
const dbContent = fs.readFileSync('d:/Charity/charity-app/db.json', 'utf8');

const apiTs = `import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class Api {
  private db: any;

  constructor() {
    const localDb = localStorage.getItem('charity_db');
    if (localDb) {
      try {
        this.db = JSON.parse(localDb);
      } catch (e) {
        this.db = null;
      }
    }

    if (!this.db) {
      this.db = ${dbContent};
      this.save();
    } else {
      // Ensure backward compatibility migration
      let modified = false;

      if (this.db.inventory && Array.isArray(this.db.inventory)) {
        this.db.inventory.forEach((item: any, idx: number) => {
          if (!item.kind) {
            item.kind = 'صنف ملموس';
            modified = true;
          }
          if (!item.code) {
            item.code = (item.kind === 'خدمة' ? 'SRV-' : 'ITM-') + String(idx + 1).padStart(3, '0');
            modified = true;
          }
        });

        const hasServices = this.db.inventory.some((i: any) => i.kind === 'خدمة');
        if (!hasServices) {
          this.db.inventory.push(
            { id: 's1', code: 'SRV-001', itemName: 'كشف طبي تخصصي (باطنة/عيون)', kind: 'خدمة', quantity: 0, unit: 'كشف', type: 'خدمات طبية' },
            { id: 's2', code: 'SRV-002', itemName: 'عملية جراحية (مياه بيضاء)', kind: 'خدمة', quantity: 0, unit: 'عملية', type: 'خدمات طبية' },
            { id: 's3', code: 'SRV-003', itemName: 'تجهيز عرائس (أجهزة كهربائية)', kind: 'خدمة', quantity: 0, unit: 'حالة', type: 'خدمات اجتماعية' },
            { id: 's4', code: 'SRV-004', itemName: 'ترميم سقف وبناء منزل', kind: 'خدمة', quantity: 0, unit: 'منزل', type: 'خدمات إيواء وتعمير' },
            { id: 's5', code: 'SRV-005', itemName: 'سداد ديون غارم/غارمة', kind: 'خدمة', quantity: 0, unit: 'حالة', type: 'خدمات اجتماعية' }
          );
          modified = true;
        }
      }

      if (this.db.donations && Array.isArray(this.db.donations)) {
        this.db.donations.forEach((d: any) => {
          if (!d.donationKind) {
            d.donationKind = 'نقدي';
            modified = true;
          }
        });
      }

      if (modified) {
        this.save();
      }
    }
  }

  private save() {
    localStorage.setItem('charity_db', JSON.stringify(this.db));
  }

  // Beneficiaries
  getBeneficiaries(): Observable<any> { return of(this.db.beneficiaries || []); }
  addBeneficiary(data: any): Observable<any> { 
    data.id = 'b_' + Date.now().toString(); 
    this.db.beneficiaries.unshift(data); 
    this.save(); 
    return of(data); 
  }
  updateBeneficiary(data: any): Observable<any> { 
    const idx = this.db.beneficiaries.findIndex((x: any) => String(x.id) === String(data.id)); 
    if (idx > -1) {
      this.db.beneficiaries[idx] = data; 
      this.save(); 
    }
    return of(data); 
  }

  // Donations & Inventory linkage
  getDonations(): Observable<any> { return of(this.db.donations || []); }
  addDonation(data: any): Observable<any> { 
    data.id = 'd_' + Date.now().toString(); 
    if (!data.date) data.date = new Date().toISOString().split('T')[0];

    // If donation is in-kind (tangible item), auto increase inventory!
    if (data.donationKind === 'عيني' && data.inventoryId && data.quantity) {
      const item = this.db.inventory.find((i: any) => String(i.id) === String(data.inventoryId));
      if (item && item.kind !== 'خدمة') {
        item.quantity = Number(item.quantity || 0) + Number(data.quantity);
        data.itemName = item.itemName;
        data.unit = item.unit;
      }
    }
    
    this.db.donations.unshift(data); 
    this.save(); 
    return of(data); 
  }

  // Inventory & Services Catalog
  getInventory(): Observable<any> { return of(this.db.inventory || []); }
  addInventory(data: any): Observable<any> { 
    data.id = 'i_' + Date.now().toString(); 
    if (!data.kind) data.kind = 'صنف ملموس';
    if (!data.code) {
      data.code = (data.kind === 'خدمة' ? 'SRV-' : 'ITM-') + Math.floor(100 + Math.random() * 900);
    }
    if (data.kind === 'خدمة') {
      data.quantity = 0;
    } else {
      data.quantity = Number(data.quantity || 0);
    }
    this.db.inventory.unshift(data); 
    this.save(); 
    return of(data); 
  }
  updateInventory(data: any): Observable<any> { 
    const idx = this.db.inventory.findIndex((x: any) => String(x.id) === String(data.id)); 
    if (idx > -1) {
      if (data.kind !== 'خدمة') {
        data.quantity = Number(data.quantity || 0);
      } else {
        data.quantity = 0;
      }
      this.db.inventory[idx] = data; 
      this.save(); 
    }
    return of(data); 
  }
  deleteInventory(id: string): Observable<any> {
    this.db.inventory = this.db.inventory.filter((x: any) => String(x.id) !== String(id));
    this.save();
    return of(true);
  }

  // Distributions (Aid & Aid services)
  getDistributions(): Observable<any> { return of(this.db.distributions || []); }
  addDistribution(data: any): Observable<any> { 
    data.id = 'dist_' + Date.now().toString(); 
    if (!data.date) data.date = new Date().toISOString().split('T')[0];

    // If item is tangible, auto decrease inventory!
    if (data.inventoryId && data.quantity) {
      const item = this.db.inventory.find((i: any) => String(i.id) === String(data.inventoryId));
      if (item && item.kind !== 'خدمة') {
        item.quantity = Math.max(0, Number(item.quantity || 0) - Number(data.quantity));
      }
    }

    this.db.distributions.unshift(data); 
    this.save(); 
    return of(data); 
  }

  // Settings
  getSettings(): Observable<any> { return of(this.db.settings ? this.db.settings[0] : {}); }
  updateSettings(data: any): Observable<any> { this.db.settings = [data]; this.save(); return of(data); }

  // Staff
  getStaff(): Observable<any> { return of(this.db.staff || []); }
  addStaff(data: any): Observable<any> { 
    data.id = 's_' + Date.now().toString(); 
    this.db.staff.unshift(data); 
    this.save(); 
    return of(data); 
  }
  updateStaff(data: any): Observable<any> { 
    const idx = this.db.staff.findIndex((x: any) => String(x.id) === String(data.id)); 
    if (idx > -1) {
      this.db.staff[idx] = data; 
      this.save(); 
    }
    return of(data); 
  }
  deleteStaff(id: string): Observable<any> { 
    this.db.staff = this.db.staff.filter((x: any) => String(x.id) !== String(id)); 
    this.save(); 
    return of(true); 
  }
}
`;

fs.writeFileSync('d:/Charity/charity-app/src/app/api.ts', apiTs, 'utf8');
console.log('api.ts successfully written with item codes support!');
