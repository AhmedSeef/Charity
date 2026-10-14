import { Injectable } from '@angular/core';
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
      this.db = {
  "beneficiaries": [
    {
      "id": "b1",
      "nationalId": "28501011234567",
      "name": "أحمد محمود سالم",
      "category": "أرامل وأيتام",
      "address": "الجيزة، العمرانية",
      "status": "مستحق فعال",
      "phone": "غير مسجل",
      "maritalStatus": "غير محدد",
      "familyCount": 0,
      "housing": "غير مسجل",
      "healthIssues": "لا يوجد",
      "researchNotes": ""
    },
    {
      "id": "b2",
      "nationalId": "27005057654321",
      "name": "سعاد إبراهيم محمد",
      "category": "مرضى ومسنين",
      "address": "القاهرة، المطرية",
      "status": "مستحق فعال",
      "phone": "غير مسجل",
      "maritalStatus": "غير محدد",
      "familyCount": 0,
      "housing": "غير مسجل",
      "healthIssues": "لا يوجد",
      "researchNotes": ""
    },
    {
      "id": "b3",
      "nationalId": "29012345678901",
      "name": "محمود حسن علي",
      "category": "غارمين",
      "address": "المعادي، القاهرة",
      "status": "مستحق فعال",
      "phone": "غير مسجل",
      "maritalStatus": "غير محدد",
      "familyCount": 0,
      "housing": "غير مسجل",
      "healthIssues": "لا يوجد",
      "researchNotes": ""
    },
    {
      "id": "b4",
      "nationalId": "28009091112223",
      "name": "فاطمة عبد الرحمن",
      "category": "حالات طوارئ",
      "address": "الإسكندرية، العصافرة",
      "status": "مستحق مؤقت",
      "phone": "غير مسجل",
      "maritalStatus": "غير محدد",
      "familyCount": 0,
      "housing": "غير مسجل",
      "healthIssues": "لا يوجد",
      "researchNotes": ""
    },
    {
      "id": "b5",
      "nationalId": "26504043334445",
      "name": "عماد الدين صبحي",
      "category": "مرضى ومسنين",
      "address": "القليوبية، بنها",
      "status": "مستحق فعال",
      "phone": "غير مسجل",
      "maritalStatus": "غير محدد",
      "familyCount": 0,
      "housing": "غير مسجل",
      "healthIssues": "لا يوجد",
      "researchNotes": ""
    },
    {
      "id": "b6",
      "nationalId": "29505051234567",
      "name": "عائلة محمود المنسي",
      "category": "حالات طوارئ",
      "address": "الإسكندرية",
      "status": "مستحق",
      "phone": "غير مسجل",
      "maritalStatus": "غير محدد",
      "familyCount": 0,
      "housing": "غير مسجل",
      "healthIssues": "لا يوجد",
      "researchNotes": ""
    },
    {
      "id": "b7",
      "nationalId": "28811223344556",
      "name": "هبة كمال الدين",
      "category": "أرامل وأيتام",
      "address": "القاهرة، السيدة زينب",
      "status": "مستحق فعال",
      "phone": "غير مسجل",
      "maritalStatus": "غير محدد",
      "familyCount": 0,
      "housing": "غير مسجل",
      "healthIssues": "لا يوجد",
      "researchNotes": ""
    }
  ],
  "donations": [
    {
      "id": "d_inkind_1",
      "donorName": "م. حسام الدين",
      "donorPhone": "01011223344",
      "donationKind": "عيني",
      "inventoryId": "i1",
      "itemName": "أرز مصري",
      "quantity": 20,
      "unit": "كيلو",
      "amount": 600,
      "type": "تبرع عيني (أغذية للمخزن)",
      "date": "2026-10-12",
      "notes": "تم توريد 20 كيلو أرز إلى المخزن"
    },
    {
      "id": "d_service_1",
      "donorName": "د. شريف عيون",
      "donorPhone": "01122334455",
      "donationKind": "خدمة",
      "inventoryId": "s1",
      "itemName": "كشف طبي تخصصي (باطنة/عيون)",
      "quantity": 10,
      "unit": "كشف",
      "amount": 0,
      "type": "رعاية وتبرع بكشوفات طبية",
      "date": "2026-10-13",
      "notes": "كشوفات مجانية لمرضى الجمعية"
    },
    {
      "id": "d1",
      "donorName": "فاعل خير",
      "amount": 5000,
      "type": "زكاة مال",
      "date": "2026-09-15",
      "donationKind": "نقدي"
    },
    {
      "id": "d2",
      "donorName": "محمد علي",
      "amount": 1000,
      "type": "صدقة جارية",
      "date": "2026-10-05",
      "donationKind": "نقدي"
    },
    {
      "id": "d3",
      "donorName": "شركة الأمل للتجارة",
      "amount": 15000,
      "type": "زكاة مال",
      "date": "2026-10-08",
      "donationKind": "نقدي"
    },
    {
      "id": "d4",
      "donorName": "سارة أحمد",
      "amount": 300,
      "type": "كفارة يمين",
      "date": "2026-10-09",
      "donationKind": "نقدي"
    },
    {
      "id": "d5",
      "donorName": "فاعل خير",
      "amount": 2500,
      "type": "كفالة يتيم",
      "date": "2026-10-10",
      "donationKind": "نقدي"
    },
    {
      "id": "d6",
      "donorName": "الحاج إبراهيم الدسوقي",
      "amount": 10000,
      "type": "صدقة جارية",
      "date": "2026-10-10",
      "donationKind": "نقدي"
    }
  ],
  "inventory": [
    {
      "id": "i1",
      "itemName": "أرز مصري",
      "quantity": 450,
      "unit": "كيلو",
      "type": "أغذية جافة",
      "kind": "صنف ملموس",
      "code": "ITM-001"
    },
    {
      "id": "i2",
      "itemName": "سكر أبيض",
      "quantity": 300,
      "unit": "كيلو",
      "type": "أغذية جافة",
      "kind": "صنف ملموس",
      "code": "ITM-002"
    },
    {
      "id": "i3",
      "itemName": "لحوم مفرومة",
      "quantity": 120,
      "unit": "كيلو",
      "type": "لحوم ومجمدات",
      "kind": "صنف ملموس",
      "code": "ITM-003"
    },
    {
      "id": "i4",
      "itemName": "أدوية سكر (ميكستارد)",
      "quantity": 45,
      "unit": "علبة",
      "type": "طبي",
      "kind": "صنف ملموس",
      "code": "ITM-004"
    },
    {
      "id": "i5",
      "itemName": "أكفان شرعية",
      "quantity": 10,
      "unit": "قطعة",
      "type": "متنوع",
      "kind": "صنف ملموس",
      "code": "ITM-005"
    },
    {
      "id": "i6",
      "itemName": "كرسي متحرك",
      "quantity": 5,
      "unit": "قطعة",
      "type": "طبي",
      "kind": "صنف ملموس",
      "code": "ITM-006"
    },
    {
      "id": "i7",
      "itemName": "زيت طعام",
      "quantity": 95,
      "unit": "زجاجة",
      "type": "أغذية جافة",
      "kind": "صنف ملموس",
      "code": "ITM-007"
    },
    {
      "id": "i8",
      "itemName": "بطاطين شتوية",
      "quantity": 150,
      "unit": "قطعة",
      "type": "متنوع",
      "kind": "صنف ملموس",
      "code": "ITM-008"
    },
    {
      "id": "i9",
      "itemName": "شنط رمضان/إطعام",
      "quantity": 50,
      "unit": "كرتونة",
      "type": "أغذية جافة",
      "kind": "صنف ملموس",
      "code": "ITM-009"
    },
    {
      "id": "s1",
      "itemName": "كشف طبي تخصصي (باطنة/عيون)",
      "kind": "خدمة",
      "quantity": 0,
      "unit": "كشف",
      "type": "خدمات طبية",
      "code": "SRV-010"
    },
    {
      "id": "s2",
      "itemName": "عملية جراحية (مياه بيضاء)",
      "kind": "خدمة",
      "quantity": 0,
      "unit": "عملية",
      "type": "خدمات طبية",
      "code": "SRV-011"
    },
    {
      "id": "s3",
      "itemName": "تجهيز عرائس (أجهزة كهربائية)",
      "kind": "خدمة",
      "quantity": 0,
      "unit": "حالة",
      "type": "خدمات اجتماعية",
      "code": "SRV-012"
    },
    {
      "id": "s4",
      "itemName": "ترميم سقف وبناء منزل",
      "kind": "خدمة",
      "quantity": 0,
      "unit": "منزل",
      "type": "خدمات إيواء وتعمير",
      "code": "SRV-013"
    },
    {
      "id": "s5",
      "itemName": "سداد ديون غارم/غارمة",
      "kind": "خدمة",
      "quantity": 0,
      "unit": "حالة",
      "type": "خدمات اجتماعية",
      "code": "SRV-014"
    }
  ],
  "distributions": [
    {
      "id": "dist1",
      "beneficiaryId": "b1",
      "inventoryId": "i1",
      "quantity": 5,
      "date": "2026-10-01",
      "notes": "شنطة إطعام شهرية"
    },
    {
      "id": "dist2",
      "beneficiaryId": "b1",
      "inventoryId": "i7",
      "quantity": 2,
      "date": "2026-10-01",
      "notes": "شنطة إطعام شهرية"
    },
    {
      "id": "dist3",
      "beneficiaryId": "b2",
      "inventoryId": "i4",
      "quantity": 1,
      "date": "2026-10-03",
      "notes": "روشتة علاج مزمن"
    },
    {
      "id": "dist4",
      "beneficiaryId": "b4",
      "inventoryId": "i8",
      "quantity": 3,
      "date": "2026-10-08",
      "notes": "مساعدة شتاء طارئة"
    },
    {
      "id": "dist5",
      "beneficiaryId": "b5",
      "inventoryId": "i6",
      "quantity": 1,
      "date": "2026-10-09",
      "notes": "استلام عهدة (كرسي متحرك)"
    },
    {
      "id": "dist6",
      "beneficiaryId": "b3",
      "inventoryId": "i3",
      "quantity": 2,
      "date": "2026-10-10",
      "notes": "توزيع لحوم أسبوعي"
    },
    {
      "id": "dist7",
      "beneficiaryId": "b7",
      "inventoryId": "i9",
      "quantity": 1,
      "date": "2026-10-10",
      "notes": "كفالة أرملة"
    },
    {
      "id": "dist8",
      "beneficiaryId": "b6",
      "inventoryId": "i7",
      "quantity": 3,
      "date": "2026-10-10",
      "notes": "تم التسليم يدويا"
    }
  ],
  "settings": [
    {
      "id": "1",
      "charityName": "جمعية أبواب الخير الشهيرة",
      "logo": "",
      "details": "جمعية خيرية مشهرة برقم 1234، تهدف لخدمة الأسر المحتاجة وتوزيع التبرعات.",
      "phone": "01012345678"
    }
  ],
  "$schema": "./node_modules/json-server/schema.json",
  "users": [
    {
      "id": "1",
      "username": "admin",
      "password": "admin",
      "role": "Administrator"
    }
  ],
  "staff": [
    {
      "id": "s1",
      "nationalId": "29000000000000",
      "name": "أحمد المتطوع",
      "phone": "01000000000",
      "type": "متطوع",
      "role": "باحث اجتماعي",
      "salary": null,
      "joinDate": "2026-01-01"
    }
  ]
};
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
}
