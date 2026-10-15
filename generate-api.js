const fs = require('fs');
const dbContent = fs.readFileSync('d:/Charity/charity-app/db.json', 'utf8');

const apiTs = `import { Injectable } from '@angular/core';
import { Observable, of, BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class Api {
  private db: any;
  private activeCharitySubject: BehaviorSubject<string>;
  public activeCharity$: Observable<string>;

  constructor() {
    const defaultData = ${dbContent};
    const localDb = localStorage.getItem('charity_db');
    
    if (localDb) {
      try {
        this.db = JSON.parse(localDb);
      } catch (e) {
        this.db = null;
      }
    }

    // Auto-migration if local database doesn't have the multi-tenant charities structure
    if (!this.db || !this.db.charities || !Array.isArray(this.db.charities) || this.db.charities.length === 0) {
      this.db = defaultData;
      this.save();
    } else {
      // Ensure all collections have charityId tags
      let modified = false;

      // Ensure default charities exist
      if (!this.db.charities.some((c: any) => c.id === 'c2')) {
        this.db.charities = defaultData.charities;
        modified = true;
      }

      // Check users
      if (!this.db.users || !this.db.users.some((u: any) => u.username === 'amal')) {
        this.db.users = defaultData.users;
        modified = true;
      }

      // Check inventory has items for c1, c2, c3
      if (this.db.inventory && Array.isArray(this.db.inventory)) {
        this.db.inventory.forEach((i: any) => {
          if (!i.charityId) {
            i.charityId = 'c1';
            modified = true;
          }
        });
        const hasC2Inv = this.db.inventory.some((i: any) => i.charityId === 'c2');
        if (!hasC2Inv) {
          const extraInv = defaultData.inventory.filter((i: any) => i.charityId !== 'c1');
          this.db.inventory.push(...extraInv);
          modified = true;
        }
      }

      // Check donations has items for c1, c2, c3
      if (this.db.donations && Array.isArray(this.db.donations)) {
        this.db.donations.forEach((d: any) => {
          if (!d.charityId) {
            d.charityId = 'c1';
            modified = true;
          }
        });
        const hasC2Don = this.db.donations.some((d: any) => d.charityId === 'c2');
        if (!hasC2Don) {
          const extraDon = defaultData.donations.filter((d: any) => d.charityId !== 'c1');
          this.db.donations.push(...extraDon);
          modified = true;
        }
      }

      // Check distributions has items for c1, c2, c3
      if (this.db.distributions && Array.isArray(this.db.distributions)) {
        this.db.distributions.forEach((dist: any) => {
          if (!dist.charityId) {
            dist.charityId = 'c1';
            dist.charityName = 'جمعية أبواب الخير';
            modified = true;
          }
        });
        const hasAmalDist = this.db.distributions.some((d: any) => d.charityId === 'c2');
        if (!hasAmalDist) {
          const extraDist = defaultData.distributions.filter((d: any) => d.charityId !== 'c1');
          this.db.distributions.push(...extraDist);
          modified = true;
        }
      }

      // Check staff has items for c1, c2, c3
      if (this.db.staff && Array.isArray(this.db.staff)) {
        this.db.staff.forEach((s: any) => {
          if (!s.charityId) {
            s.charityId = 'c1';
            modified = true;
          }
        });
        const hasC2Staff = this.db.staff.some((s: any) => s.charityId === 'c2');
        if (!hasC2Staff) {
          const extraStaff = defaultData.staff.filter((s: any) => s.charityId !== 'c1');
          this.db.staff.push(...extraStaff);
          modified = true;
        }
      }

      // Check settings
      if (!this.db.settings || !Array.isArray(this.db.settings) || !this.db.settings.some((s: any) => s.charityId === 'c2')) {
        this.db.settings = defaultData.settings;
        modified = true;
      }

      if (modified) {
        this.save();
      }
    }

    const savedActiveId = localStorage.getItem('active_charity_id') || this.db.activeCharityId || 'c1';
    this.activeCharitySubject = new BehaviorSubject<string>(savedActiveId);
    this.activeCharity$ = this.activeCharitySubject.asObservable();
  }

  private save() {
    try {
      localStorage.setItem('charity_db', JSON.stringify(this.db));
    } catch (e) {
      console.error('Error saving charity_db to localStorage', e);
    }
  }

  // ==========================================
  // Charities & Multi-Tenancy Management
  // ==========================================
  getCharities(): Observable<any[]> {
    return of(this.db.charities || []);
  }

  getActiveCharityId(): string {
    return this.activeCharitySubject.value;
  }

  getActiveCharity(): any {
    const id = this.getActiveCharityId();
    const found = (this.db.charities || []).find((c: any) => c.id === id);
    return found || (this.db.charities && this.db.charities[0]) || { id: 'c1', name: 'جمعية أبواب الخير', color: '#004d40', icon: '🏢' };
  }

  setActiveCharityId(id: string): void {
    const charity = (this.db.charities || []).find((c: any) => c.id === id);
    if (charity) {
      this.db.activeCharityId = id;
      localStorage.setItem('active_charity_id', id);
      this.save();
      this.activeCharitySubject.next(id);
    }
  }

  // ==========================================
  // 1. Beneficiaries (SHARED NETWORK RESOURCE)
  // ==========================================
  getBeneficiaries(): Observable<any[]> {
    return of(this.db.beneficiaries || []);
  }

  addBeneficiary(data: any): Observable<any> {
    data.id = 'b_' + Date.now().toString();
    const active = this.getActiveCharity();
    if (!data.registeredByCharityId) {
      data.registeredByCharityId = active.id;
      data.registeredByCharityName = active.name;
    }
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

  // ==========================================
  // 2. Distributions (CROSS-CHARITY AID RECORD)
  // ==========================================
  // Returns distributions issued by current charity by default, or all if onlyCurrentCharity is false
  getDistributions(onlyCurrentCharity: boolean = true): Observable<any[]> {
    const list = this.db.distributions || [];
    if (!onlyCurrentCharity) {
      return of(list);
    }
    const currentId = this.getActiveCharityId();
    return of(list.filter((d: any) => d.charityId === currentId));
  }

  getAllDistributions(): Observable<any[]> {
    return of(this.db.distributions || []);
  }

  // Returns all distributions for a specific beneficiary across ALL charities (The Quantum Leap feature)
  getDistributionsForBeneficiary(benId: string): Observable<any[]> {
    const allDist = this.db.distributions || [];
    const benDist = allDist.filter((d: any) => String(d.beneficiaryId) === String(benId));
    
    // Enrich with item names and units
    const enriched = benDist.map((dist: any) => {
      let resolvedItem = (this.db.inventory || []).find((i: any) => String(i.id) === String(dist.inventoryId));
      return {
        ...dist,
        itemName: dist.itemName || (resolvedItem ? resolvedItem.itemName : 'مساعدة معتمدة'),
        unit: dist.unit || (resolvedItem ? resolvedItem.unit : ''),
        kind: dist.kind || (resolvedItem ? resolvedItem.kind : 'صنف ملموس'),
        charityName: dist.charityName || 'جمعية شقيقة'
      };
    });

    return of(enriched);
  }

  addDistribution(data: any): Observable<any> {
    data.id = 'dist_' + Date.now().toString();
    if (!data.date) data.date = new Date().toISOString().split('T')[0];

    const active = this.getActiveCharity();
    data.charityId = active.id;
    data.charityName = active.name;

    // Deduct stock strictly from THIS charity's inventory if tangible
    if (data.inventoryId && data.quantity) {
      const item = (this.db.inventory || []).find((i: any) => 
        String(i.id) === String(data.inventoryId) && i.charityId === active.id
      );
      if (item) {
        if (!data.itemName) data.itemName = item.itemName;
        if (!data.unit) data.unit = item.unit;
        if (!data.kind) data.kind = item.kind;
        if (item.kind !== 'خدمة') {
          item.quantity = Math.max(0, Number(item.quantity || 0) - Number(data.quantity));
        }
      }
    }

    this.db.distributions.unshift(data);
    this.save();
    return of(data);
  }

  // ==========================================
  // 3. Inventory & Catalog (TENANT ISOLATED)
  // ==========================================
  getInventory(charityId?: string): Observable<any[]> {
    const targetCharityId = charityId || this.getActiveCharityId();
    const list = (this.db.inventory || []).filter((i: any) => i.charityId === targetCharityId);
    return of(list);
  }

  getAllInventory(): Observable<any[]> {
    return of(this.db.inventory || []);
  }

  addInventory(data: any): Observable<any> {
    data.id = 'i_' + Date.now().toString();
    data.charityId = this.getActiveCharityId();
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
    this.db.inventory = (this.db.inventory || []).filter((x: any) => String(x.id) !== String(id));
    this.save();
    return of(true);
  }

  // ==========================================
  // 4. Donations (TENANT ISOLATED)
  // ==========================================
  getDonations(charityId?: string): Observable<any[]> {
    const targetCharityId = charityId || this.getActiveCharityId();
    const list = (this.db.donations || []).filter((d: any) => d.charityId === targetCharityId);
    return of(list);
  }

  addDonation(data: any): Observable<any> {
    data.id = 'd_' + Date.now().toString();
    data.charityId = this.getActiveCharityId();
    if (!data.date) data.date = new Date().toISOString().split('T')[0];

    // If donation is in-kind (tangible item), auto increase inventory of THIS charity!
    if (data.donationKind === 'عيني' && data.inventoryId && data.quantity) {
      const item = (this.db.inventory || []).find((i: any) => 
        String(i.id) === String(data.inventoryId) && i.charityId === data.charityId
      );
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

  // ==========================================
  // 5. Staff & HR (TENANT ISOLATED)
  // ==========================================
  getStaff(charityId?: string): Observable<any[]> {
    const targetCharityId = charityId || this.getActiveCharityId();
    const list = (this.db.staff || []).filter((s: any) => s.charityId === targetCharityId);
    return of(list);
  }

  addStaff(data: any): Observable<any> {
    data.id = 's_' + Date.now().toString();
    data.charityId = this.getActiveCharityId();
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
    this.db.staff = (this.db.staff || []).filter((x: any) => String(x.id) !== String(id));
    this.save();
    return of(true);
  }

  // ==========================================
  // 6. Settings (TENANT ISOLATED)
  // ==========================================
  getSettings(charityId?: string): Observable<any> {
    const targetCharityId = charityId || this.getActiveCharityId();
    const list = this.db.settings || [];
    const found = list.find((s: any) => s.charityId === targetCharityId);
    if (found) {
      return of(found);
    }
    // Fallback to active charity data
    const active = this.getActiveCharity();
    return of({
      charityId: active.id,
      charityName: active.name,
      logo: active.logo || '',
      phone: active.phone || '',
      details: active.roleDesc || ''
    });
  }

  updateSettings(data: any): Observable<any> {
    const activeId = this.getActiveCharityId();
    data.charityId = activeId;
    if (!this.db.settings) this.db.settings = [];
    const idx = this.db.settings.findIndex((s: any) => s.charityId === activeId);
    if (idx > -1) {
      this.db.settings[idx] = data;
    } else {
      this.db.settings.push(data);
    }

    // Also update charity entry in charities list
    const cIdx = (this.db.charities || []).findIndex((c: any) => c.id === activeId);
    if (cIdx > -1) {
      if (data.charityName) this.db.charities[cIdx].name = data.charityName;
      if (data.logo !== undefined) this.db.charities[cIdx].logo = data.logo;
      if (data.phone) this.db.charities[cIdx].phone = data.phone;
    }

    this.save();
    return of(data);
  }
}
`;

fs.writeFileSync('d:/Charity/charity-app/src/app/api.ts', apiTs, 'utf8');
console.log('Multi-tenant api.ts successfully written!');
