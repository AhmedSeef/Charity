import { Injectable } from '@angular/core';
import { Observable, of, BehaviorSubject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class Api {
  private db: any;
  private activeCharitySubject: BehaviorSubject<string>;
  public activeCharity$: Observable<string>;

  constructor() {
    const defaultData = {
  "charities": [
    {
      "id": "c1",
      "name": "جمعية أبواب الخير",
      "roleDesc": "رعاية عامة وإطعام ومساعدات للأسر الأكثر احتياجاً",
      "logo": "",
      "color": "#004d40",
      "icon": "🏢",
      "phone": "01012345678",
      "address": "الجيزة، العمرانية",
      "license": "مشهرة برقم 1234 لسنة 2015"
    },
    {
      "id": "c2",
      "name": "جمعية الأمل لرعاية الأيتام",
      "roleDesc": "كفالة الأيتام والإعانات النقدية وتجهيز العرائس",
      "logo": "",
      "color": "#1565c0",
      "icon": "🤝",
      "phone": "01122334455",
      "address": "القاهرة، المعادي",
      "license": "مشهرة برقم 5678 لسنة 2018"
    },
    {
      "id": "c3",
      "name": "جمعية رسالة الصداقة",
      "roleDesc": "رعاية طبية وقوافل علاجية وكراسي متحركة",
      "logo": "",
      "color": "#6a1b9a",
      "icon": "🏥",
      "phone": "01233445566",
      "address": "الإسكندرية، سموحة",
      "license": "مشهرة برقم 9012 لسنة 2020"
    }
  ],
  "activeCharityId": "c1",
  "users": [
    {
      "id": "u1",
      "username": "admin",
      "password": "admin",
      "role": "مدير النظام",
      "charityId": "c1",
      "charityName": "جمعية أبواب الخير"
    },
    {
      "id": "u2",
      "username": "abwab",
      "password": "1234",
      "role": "مدير الجمعية",
      "charityId": "c1",
      "charityName": "جمعية أبواب الخير"
    },
    {
      "id": "u3",
      "username": "amal",
      "password": "amal",
      "role": "مدير الجمعية",
      "charityId": "c2",
      "charityName": "جمعية الأمل لرعاية الأيتام"
    },
    {
      "id": "u4",
      "username": "sedaka",
      "password": "sedaka",
      "role": "مدير الجمعية",
      "charityId": "c3",
      "charityName": "جمعية رسالة الصداقة"
    }
  ],
  "beneficiaries": [
    {
      "id": "b1",
      "nationalId": "28501011234567",
      "name": "أحمد محمود سالم",
      "category": "أرامل وأيتام",
      "address": "الجيزة، العمرانية",
      "status": "مستحق فعال",
      "phone": "01098765432",
      "maritalStatus": "متزوج",
      "familyCount": 4,
      "housing": "إيجار قديم",
      "healthIssues": "ضعف إبصار ومشاكل بالرمد",
      "researchNotes": "أسرة تعول 3 أطفال، عائل الأسرة باليومية وبحاجة لمساعدات غذائية ونقدية دورية.",
      "incomeSource": "عامل يومية بالمعمار",
      "monthlyIncome": 2800,
      "hasLandOrProperty": false,
      "landOrPropertyDetails": "",
      "hasGovernmentPension": true,
      "governmentPensionDetails": "معاش تكافل وكرامة 750 ج.م شهرياً",
      "hasChronicDisease": true,
      "chronicDiseaseDetails": "ضعف إبصار ومشاكل بالرمد وارتفاع ضغط الدم",
      "needsPeriodicMedication": true,
      "periodicMedicationDetails": "قطرات ترطيب وحفظ ضغط العين وعلاج ضغط",
      "medicationFrequency": "شهرياً",
      "familyMembers": [
        {
          "id": "fm_b1_1",
          "nationalId": "28804041234568",
          "name": "مريم كمال السيد",
          "relationship": "زوجة",
          "memberType": "معال",
          "hasIncome": false,
          "incomeAmount": null,
          "incomeSource": "",
          "supportType": "ربة منزل",
          "healthStatus": "جيدة",
          "notes": "ربة منزل ترعى الأطفال"
        },
        {
          "id": "fm_b1_2",
          "nationalId": "31502021234569",
          "name": "يوسف أحمد محمود",
          "relationship": "ابن",
          "memberType": "معال",
          "hasIncome": false,
          "incomeAmount": null,
          "incomeSource": "",
          "supportType": "طالب",
          "healthStatus": "جيدة",
          "notes": "طالب بالمرحلة الابتدائية"
        },
        {
          "id": "fm_b1_3",
          "nationalId": "31806061234560",
          "name": "نور أحمد محمود",
          "relationship": "ابنة",
          "memberType": "معال",
          "hasIncome": false,
          "incomeAmount": null,
          "incomeSource": "",
          "supportType": "طفل",
          "healthStatus": "جيدة",
          "notes": "طفلة في سن الحضانة"
        }
      ],
      "registeredByCharityId": "c1",
      "registeredByCharityName": "جمعية أبواب الخير"
    },
    {
      "id": "b2",
      "nationalId": "27005057654321",
      "name": "سعاد إبراهيم محمد",
      "category": "مرضى ومسنين",
      "address": "القاهرة، المطرية",
      "status": "مستحق فعال",
      "phone": "01187654321",
      "maritalStatus": "أرمل",
      "familyCount": 2,
      "housing": "سكن مشترك",
      "healthIssues": "مريضة سكر وتحتاج أدوية شهرية وكرسي متحرك",
      "researchNotes": "مسنة تعيش مع ابنتها، لا يوجد دخل ثابت سوى معاش تكافل وكرامة.",
      "incomeSource": "لا يوجد عمل (مسنة)",
      "monthlyIncome": 0,
      "hasLandOrProperty": false,
      "landOrPropertyDetails": "",
      "hasGovernmentPension": true,
      "governmentPensionDetails": "معاش ضمان اجتماعي وتكافل 600 ج.م",
      "hasChronicDisease": true,
      "chronicDiseaseDetails": "مرض السكري المتقدم والتهاب المفاصل وعجز حركي",
      "needsPeriodicMedication": true,
      "periodicMedicationDetails": "أنسولين ومسكنات وأدوية ضبط السكر",
      "medicationFrequency": "شهرياً",
      "familyMembers": [
        {
          "id": "fm_b2_1",
          "nationalId": "29801017654322",
          "name": "هناء محمود إسماعيل",
          "relationship": "ابنة",
          "memberType": "معيل",
          "hasIncome": true,
          "incomeAmount": 1500,
          "incomeSource": "خياطة وتطريز منزلي",
          "supportType": "أخرى",
          "healthStatus": "سليمة",
          "notes": "ابنتها المعيلة وتساعد في مصاريف البيت والأدوية"
        }
      ],
      "registeredByCharityId": "c1",
      "registeredByCharityName": "جمعية أبواب الخير"
    },
    {
      "id": "b3",
      "nationalId": "29012345678901",
      "name": "محمود حسن علي",
      "category": "غارمين",
      "address": "المعادي، القاهرة",
      "status": "مستحق فعال",
      "phone": "01234567890",
      "maritalStatus": "متزوج",
      "familyCount": 5,
      "housing": "إيجار جديد",
      "healthIssues": "سليم طبياً",
      "researchNotes": "عليه ديون متراكمة بسبب جهاز ابنته الكبرى وإيصالات أمانة.",
      "registeredByCharityId": "c2",
      "registeredByCharityName": "جمعية الأمل لرعاية الأيتام"
    },
    {
      "id": "b4",
      "nationalId": "28009091112223",
      "name": "فاطمة عبد الرحمن",
      "category": "حالات طوارئ",
      "address": "الإسكندرية، العصافرة",
      "status": "مستحق مؤقت",
      "phone": "01511223344",
      "maritalStatus": "مطلق",
      "familyCount": 3,
      "housing": "سكن متهالك/يحتاج ترميم",
      "healthIssues": "روماتويد والتهاب مفاصل",
      "researchNotes": "تحتاج مساعدة شتاء وبطاطين وأدوات مدرسية لأطفالها.",
      "registeredByCharityId": "c3",
      "registeredByCharityName": "جمعية رسالة الصداقة"
    },
    {
      "id": "b5",
      "nationalId": "26504043334445",
      "name": "عماد الدين صبحي",
      "category": "مرضى ومسنين",
      "address": "القليوبية، بنها",
      "status": "مستحق فعال",
      "phone": "01022334455",
      "maritalStatus": "متزوج",
      "familyCount": 2,
      "housing": "ملك قديم",
      "healthIssues": "بتر جزئي بالقدم ويحتاج كرسي متحرك",
      "researchNotes": "يحتاج رعاية صحية ودواء دوري.",
      "registeredByCharityId": "c3",
      "registeredByCharityName": "جمعية رسالة الصداقة"
    },
    {
      "id": "b6",
      "nationalId": "29505051234567",
      "name": "عائلة محمود المنسي",
      "category": "حالات طوارئ",
      "address": "الإسكندرية، المنتزه",
      "status": "مستحق",
      "phone": "01288997766",
      "maritalStatus": "متزوج",
      "familyCount": 6,
      "housing": "إيجار جديد",
      "healthIssues": "أطفال في سن المدارس",
      "researchNotes": "تعطل عائل الأسرة عن العمل في ورشة.",
      "registeredByCharityId": "c1",
      "registeredByCharityName": "جمعية أبواب الخير"
    },
    {
      "id": "b7",
      "nationalId": "28811223344556",
      "name": "هبة ككمال الدين",
      "category": "أرامل وأيتام",
      "address": "القاهرة، السيدة زينب",
      "status": "مستحق فعال",
      "phone": "01033445566",
      "maritalStatus": "أرمل",
      "familyCount": 3,
      "housing": "إيجار قديم",
      "healthIssues": "لا يوجد",
      "researchNotes": "أرملة تعول يتيمين ومقيدة في برامج كفالة الأيتام.",
      "registeredByCharityId": "c2",
      "registeredByCharityName": "جمعية الأمل لرعاية الأيتام"
    }
  ],
  "inventory": [
    {
      "id": "i1",
      "charityId": "c1",
      "itemName": "أرز مصري ممتاز",
      "quantity": 450,
      "unit": "كيلو",
      "type": "أغذية جافة",
      "kind": "صنف ملموس",
      "code": "ITM-001"
    },
    {
      "id": "i2",
      "charityId": "c1",
      "itemName": "سكر أبيض نقي",
      "quantity": 300,
      "unit": "كيلو",
      "type": "أغذية جافة",
      "kind": "صنف ملموس",
      "code": "ITM-002"
    },
    {
      "id": "i3",
      "charityId": "c1",
      "itemName": "لحوم بلدية مجمدة",
      "quantity": 120,
      "unit": "كيلو",
      "type": "لحوم ومجمدات",
      "kind": "صنف ملموس",
      "code": "ITM-003"
    },
    {
      "id": "i7",
      "charityId": "c1",
      "itemName": "زيت طعام ممتاز",
      "quantity": 95,
      "unit": "زجاجة",
      "type": "أغذية جافة",
      "kind": "صنف ملموس",
      "code": "ITM-007"
    },
    {
      "id": "i9",
      "charityId": "c1",
      "itemName": "كرتونة إطعام وشنط رمضان",
      "quantity": 50,
      "unit": "كرتونة",
      "type": "أغذية جافة",
      "kind": "صنف ملموس",
      "code": "ITM-009"
    },
    {
      "id": "s_abwab_1",
      "charityId": "c1",
      "itemName": "سداد ديون غارم/غارمة",
      "kind": "خدمة",
      "quantity": 0,
      "unit": "حالة",
      "type": "خدمات اجتماعية",
      "code": "SRV-014"
    },
    {
      "id": "i_amal_1",
      "charityId": "c2",
      "itemName": "بطاطين شتوية مبطنة",
      "quantity": 180,
      "unit": "قطعة",
      "type": "ملابس وبطاطين",
      "kind": "صنف ملموس",
      "code": "ITM-201"
    },
    {
      "id": "i_amal_2",
      "charityId": "c2",
      "itemName": "شنطة أدوات مدرسية للأيتام",
      "quantity": 75,
      "unit": "شنطة",
      "type": "متنوع",
      "kind": "صنف ملموس",
      "code": "ITM-202"
    },
    {
      "id": "i_amal_3",
      "charityId": "c2",
      "itemName": "أجهزة كهربائية (تجهيز عرائس)",
      "quantity": 12,
      "unit": "طقم",
      "type": "متنوع",
      "kind": "صنف ملموس",
      "code": "ITM-203"
    },
    {
      "id": "s_amal_1",
      "charityId": "c2",
      "itemName": "إعانة مالية نقدية عاجلة",
      "kind": "خدمة",
      "quantity": 0,
      "unit": "دفعة نقدية",
      "type": "خدمات اجتماعية وتجهيز",
      "code": "SRV-201"
    },
    {
      "id": "s_amal_2",
      "charityId": "c2",
      "itemName": "كفالة يتيم شهرية",
      "kind": "خدمة",
      "quantity": 0,
      "unit": "كفالة",
      "type": "خدمات تعليمية وكفالات",
      "code": "SRV-202"
    },
    {
      "id": "i_sedaka_1",
      "charityId": "c3",
      "itemName": "كرسي متحرك لكبار السن والمرضى",
      "quantity": 8,
      "unit": "قطعة",
      "type": "أجهزة تعويضية وكراسي",
      "kind": "صنف ملموس",
      "code": "ITM-301"
    },
    {
      "id": "i_sedaka_2",
      "charityId": "c3",
      "itemName": "أدوية سكر وضغط (ميكستارد/كونكور)",
      "quantity": 60,
      "unit": "علبة",
      "type": "أدوية ومستلزمات طبية",
      "kind": "صنف ملموس",
      "code": "ITM-302"
    },
    {
      "id": "s_sedaka_1",
      "charityId": "c3",
      "itemName": "كشف رمد وعيون ونظارة طبية",
      "kind": "خدمة",
      "quantity": 0,
      "unit": "كشف ونظارة",
      "type": "خدمات طبية وعلاجية",
      "code": "SRV-301"
    },
    {
      "id": "s_sedaka_2",
      "charityId": "c3",
      "itemName": "كشف باطنة وقافلة علاجية",
      "kind": "خدمة",
      "quantity": 0,
      "unit": "كشف",
      "type": "خدمات طبية وعلاجية",
      "code": "SRV-302"
    }
  ],
  "donations": [
    {
      "id": "d1",
      "charityId": "c1",
      "donorName": "فاعل خير",
      "amount": 5000,
      "type": "زكاة مال",
      "date": "2026-09-15",
      "donationKind": "نقدي"
    },
    {
      "id": "d2",
      "charityId": "c1",
      "donorName": "محمد علي",
      "amount": 1000,
      "type": "صدقة جارية",
      "date": "2026-10-05",
      "donationKind": "نقدي"
    },
    {
      "id": "d_inkind_1",
      "charityId": "c1",
      "donorName": "م. حسام الدين",
      "donorPhone": "01011223344",
      "donationKind": "عيني",
      "inventoryId": "i1",
      "itemName": "أرز مصري ممتاز",
      "quantity": 50,
      "unit": "كيلو",
      "amount": 1500,
      "type": "تبرع عيني (أغذية للمخزن)",
      "date": "2026-10-12",
      "notes": "تم توريد 50 كيلو أرز إلى مخزن الجمعية"
    },
    {
      "id": "d_amal_1",
      "charityId": "c2",
      "donorName": "شركة الأمل للتجارة والاستثمار",
      "donorPhone": "01099887766",
      "donationKind": "نقدي",
      "amount": 25000,
      "type": "كفالات أيتام وزكاة مال",
      "date": "2026-10-02",
      "notes": "مخصصة للمساعدات المالية العاجلة للأسر المتعففة"
    },
    {
      "id": "d_amal_2",
      "charityId": "c2",
      "donorName": "أهل الخير بالمعادي",
      "donorPhone": "01144556677",
      "donationKind": "عيني",
      "inventoryId": "i_amal_1",
      "itemName": "بطاطين شتوية مبطنة",
      "quantity": 40,
      "unit": "قطعة",
      "amount": 8000,
      "type": "كساء شتاء",
      "date": "2026-10-04",
      "notes": "بطاطين لمشروع دفء الشتاء للأيتام"
    },
    {
      "id": "d_sedaka_1",
      "charityId": "c3",
      "donorName": "د. أحمد خليل - استشاري جراحة",
      "donorPhone": "01222334455",
      "donationKind": "نقدي",
      "amount": 12000,
      "type": "دعم القوافل الطبية والأدوية",
      "date": "2026-10-06",
      "notes": "لشراء أدوية سكر وضغط وكراسي متحركة"
    },
    {
      "id": "d_sedaka_2",
      "charityId": "c3",
      "donorName": "مركز النور للبصريات",
      "donorPhone": "01055667788",
      "donationKind": "خدمة",
      "inventoryId": "s_sedaka_1",
      "itemName": "كشف رمد وعيون ونظارة طبية",
      "quantity": 15,
      "unit": "كشف ونظارة",
      "amount": 0,
      "type": "رعاية طبية خيرية",
      "date": "2026-10-08",
      "notes": "15 كشف ونظارة طبية مجانية لمرضى الشبكة"
    }
  ],
  "distributions": [
    {
      "id": "dist1",
      "charityId": "c1",
      "charityName": "جمعية أبواب الخير",
      "beneficiaryId": "b1",
      "inventoryId": "i1",
      "itemName": "أرز مصري ممتاز",
      "kind": "صنف ملموس",
      "quantity": 5,
      "unit": "كيلو",
      "amount": 150,
      "date": "2026-10-01",
      "notes": "صرف دعم تمويني شهري للأسر المتعففة"
    },
    {
      "id": "dist_amal_user_example",
      "charityId": "c2",
      "charityName": "جمعية الأمل لرعاية الأيتام",
      "beneficiaryId": "b1",
      "inventoryId": "s_amal_1",
      "itemName": "إعانة مالية نقدية عاجلة",
      "kind": "إعانة نقدية",
      "quantity": 1,
      "unit": "دفعة نقدية",
      "amount": 1000,
      "date": "2026-10-05",
      "notes": "تم صرف 1,000 ج.م إعانة نقدية طارئة نظراً لمرض عائل الأسرة"
    },
    {
      "id": "dist_sedaka_medical",
      "charityId": "c3",
      "charityName": "جمعية رسالة الصداقة",
      "beneficiaryId": "b1",
      "inventoryId": "s_sedaka_1",
      "itemName": "كشف رمد وعيون ونظارة طبية",
      "kind": "خدمة",
      "quantity": 1,
      "unit": "كشف ونظارة",
      "amount": 350,
      "date": "2026-10-09",
      "notes": "كشف بالقافلة الطبية وتسليم نظارة قراءة"
    },
    {
      "id": "dist2",
      "charityId": "c1",
      "charityName": "جمعية أبواب الخير",
      "beneficiaryId": "b1",
      "inventoryId": "i7",
      "itemName": "زيت طعام ممتاز",
      "kind": "صنف ملموس",
      "quantity": 2,
      "unit": "زجاجة",
      "amount": 120,
      "date": "2026-10-01",
      "notes": "معونة إطعام شهرية"
    },
    {
      "id": "dist3",
      "charityId": "c1",
      "charityName": "جمعية أبواب الخير",
      "beneficiaryId": "b2",
      "inventoryId": "i1",
      "itemName": "أرز مصري ممتاز",
      "kind": "صنف ملموس",
      "quantity": 10,
      "unit": "كيلو",
      "amount": 300,
      "date": "2026-10-03",
      "notes": "إعانة كبار السن"
    },
    {
      "id": "dist_sedaka_suad",
      "charityId": "c3",
      "charityName": "جمعية رسالة الصداقة",
      "beneficiaryId": "b2",
      "inventoryId": "i_sedaka_1",
      "itemName": "كرسي متحرك لكبار السن والمرضى",
      "kind": "صنف ملموس",
      "quantity": 1,
      "unit": "قطعة",
      "amount": 2500,
      "date": "2026-10-10",
      "notes": "تسليم كرسي متحرك كعهدة مستديمة للحالة"
    },
    {
      "id": "dist_amal_fatma",
      "charityId": "c2",
      "charityName": "جمعية الأمل لرعاية الأيتام",
      "beneficiaryId": "b4",
      "inventoryId": "i_amal_1",
      "itemName": "بطاطين شتوية مبطنة",
      "kind": "صنف ملموس",
      "quantity": 3,
      "unit": "قطعة",
      "amount": 600,
      "date": "2026-10-08",
      "notes": "مساعدة شتاء وكسوة أيتام"
    }
  ],
  "staff": [
    {
      "id": "s1",
      "charityId": "c1",
      "nationalId": "29000000000000",
      "name": "أحمد المتطوع",
      "phone": "01000000000",
      "type": "متطوع",
      "role": "باحث اجتماعي ميداني",
      "salary": null,
      "joinDate": "2026-01-01",
      "address": "الجيزة، فيصل",
      "tasks": "إجراء البحوث الميدانية للحالات وزيارة المنازل والتأكد من الاستحقاق."
    },
    {
      "id": "s2",
      "charityId": "c1",
      "nationalId": "28811223344551",
      "name": "م. طارق عبد الحميد",
      "phone": "01122334411",
      "type": "موظف براتب",
      "role": "أمين المخازن ومسؤول اللوجستيات",
      "salary": 5500,
      "joinDate": "2025-06-01",
      "address": "القاهرة، المعادي",
      "tasks": "إدارة الوارد والمنصرف من المخزن، والجرد الدوري، وإعداد تقارير الأرصدة."
    },
    {
      "id": "s_amal_1",
      "charityId": "c2",
      "nationalId": "28509091122334",
      "name": "د. سمير عبد الله",
      "phone": "01099881122",
      "type": "موظف براتب",
      "role": "مدير دار الأيتام والبرامج التنموية",
      "salary": 7000,
      "joinDate": "2024-03-01",
      "address": "المعادي، دجلة",
      "tasks": "الإشراف على دور رعاية الأيتام، وبرامج الكفالة الشهرية والتأهيل التعليمي."
    },
    {
      "id": "s_amal_2",
      "charityId": "c2",
      "nationalId": "29603034455667",
      "name": "رانيا كمال الدين",
      "phone": "01155667788",
      "type": "متطوع",
      "role": "منسقة كفالات الأيتام",
      "salary": null,
      "joinDate": "2026-02-15",
      "address": "حلوان، القاهرة",
      "tasks": "التواصل مع الكفلاء والمتبرعين ومتابعة تقارير التحصيل والزيارات."
    },
    {
      "id": "s_sedaka_1",
      "charityId": "c3",
      "nationalId": "28104047788990",
      "name": "د. شريف عيون",
      "phone": "01233441122",
      "type": "متطوع",
      "role": "طبيب منسق القوافل الطبية",
      "salary": null,
      "joinDate": "2025-01-10",
      "address": "الإسكندرية، رشدي",
      "tasks": "تنظيم القوافل العلاجية، فحص الحالات، والتنسيق مع المراكز الطبية الشريكة."
    }
  ],
  "settings": [
    {
      "id": "1",
      "charityId": "c1",
      "charityName": "جمعية أبواب الخير",
      "logo": "",
      "details": "جمعية خيرية مشهرة برقم 1234، تهدف لخدمة الأسر المحتاجة وتوزيع الإطعام والتبرعات.",
      "phone": "01012345678"
    },
    {
      "id": "2",
      "charityId": "c2",
      "charityName": "جمعية الأمل لرعاية الأيتام",
      "logo": "",
      "details": "جمعية رائدة في كفالة الأيتام والرعاية التنموية والإعانات النقدية مشهرة برقم 5678.",
      "phone": "01122334455"
    },
    {
      "id": "3",
      "charityId": "c3",
      "charityName": "جمعية رسالة الصداقة",
      "logo": "",
      "details": "جمعية متخصصة في الخدمات الطبية والعلاجية وتوفير الأجهزة التعويضية مشهرة برقم 9012.",
      "phone": "01233445566"
    }
  ]
};
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

  private validateNationalIdUniqueness(data: any, currentId?: string): void {
    const benNatId = (data.nationalId || '').trim();
    if (!benNatId) {
      throw new Error('الرقم القومي لصاحب الملف مطلوب!');
    }

    // 1. فحص عدم تكرار الرقم القومي مع أي مستفيد آخر
    const conflictBen = (this.db.beneficiaries || []).find(
      (b: any) => b.nationalId === benNatId && String(b.id) !== String(currentId)
    );
    if (conflictBen) {
      throw new Error(`الرقم القومي (${benNatId}) مسجل مسبقاً لمستفيد آخر باسم: ${conflictBen.name}!`);
    }

    // 2. فحص عدم تسجيل الرقم القومي لصاحب الملف كفرد أسرة في ملف آخر
    for (const b of this.db.beneficiaries || []) {
      if (String(b.id) === String(currentId)) continue;
      if (b.familyMembers && Array.isArray(b.familyMembers)) {
        const conflictFm = b.familyMembers.find((m: any) => (m.nationalId || '').trim() === benNatId);
        if (conflictFm) {
          throw new Error(`الرقم القومي (${benNatId}) مسجل مسبقاً كفرد أسرة تابع لملف (${b.name}) باسم: ${conflictFm.name}!`);
        }
      }
    }

    // 3. فحص أفراد الأسرة
    if (data.familyMembers && Array.isArray(data.familyMembers)) {
      const seenFm = new Set<string>();
      for (const m of data.familyMembers) {
        const mNatId = (m.nationalId || '').trim();
        if (!mNatId) {
          throw new Error(`الرقم القومي لفرد الأسرة (${m.name || 'بدون اسم'}) مطلوب!`);
        }
        if (mNatId === benNatId) {
          throw new Error(`لا يمكن أن يتطابق الرقم القومي لفرد الأسرة (${m.name}) مع الرقم القومي لصاحب الملف!`);
        }
        if (seenFm.has(mNatId)) {
          throw new Error(`الرقم القومي (${mNatId}) مكرر أكثر من مرة بين أفراد الأسرة!`);
        }
        seenFm.add(mNatId);

        // فحص عدم تسجيل فرد الأسرة كصاحب ملف مستقل مسبقاً
        const benExists = (this.db.beneficiaries || []).find(
          (b: any) => b.nationalId === mNatId && String(b.id) !== String(currentId)
        );
        if (benExists) {
          throw new Error(`الرقم القومي لفرد الأسرة (${m.name} - ${mNatId}) مسجل مسبقاً كصاحب ملف مستقل باسم: ${benExists.name}!`);
        }

        // فحص عدم تسجيل فرد الأسرة في أسرة أخرى
        for (const b of this.db.beneficiaries || []) {
          if (String(b.id) === String(currentId)) continue;
          if (b.familyMembers && Array.isArray(b.familyMembers)) {
            const fmExists = b.familyMembers.find((item: any) => (item.nationalId || '').trim() === mNatId);
            if (fmExists) {
              throw new Error(`الرقم القومي (${mNatId}) لفرد الأسرة (${m.name}) مسجل مسبقاً في ملف أسرة (${b.name})!`);
            }
          }
        }
      }
    }
  }

  addBeneficiary(data: any): Observable<any> {
    this.validateNationalIdUniqueness(data);
    data.id = 'b_' + Date.now().toString();
    const active = this.getActiveCharity();
    if (!data.registeredByCharityId) {
      data.registeredByCharityId = active.id;
      data.registeredByCharityName = active.name;
    }
    if (!data.familyMembers) data.familyMembers = [];
    this.db.beneficiaries.unshift(data);
    this.save();
    return of(data);
  }

  updateBeneficiary(data: any): Observable<any> {
    const idx = this.db.beneficiaries.findIndex((x: any) => String(x.id) === String(data.id));
    if (idx > -1) {
      const existing = this.db.beneficiaries[idx];
      const activeId = this.getActiveCharityId();

      // Authorization & Stewardship Rule: ONLY the registering charity can edit!
      if (existing.registeredByCharityId && existing.registeredByCharityId !== activeId) {
        const errorMsg = 'غير مصرح: لا تملك صلاحية تعديل هذا المستفيد! التعديل مقصور قانونياً على الجمعية التي سجلت الملف أول مرة (' + (existing.registeredByCharityName || 'الجمعية الأصلية') + ').';
        console.error(errorMsg);
        throw new Error(errorMsg);
      }

      this.validateNationalIdUniqueness(data, existing.id);

      this.db.beneficiaries[idx] = {
        ...data,
        registeredByCharityId: existing.registeredByCharityId,
        registeredByCharityName: existing.registeredByCharityName
      };
      this.save();
      return of(this.db.beneficiaries[idx]);
    }
    throw new Error('المستفيد غير موجود');
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
