import { of } from 'rxjs';
import { DashboardComponent } from './dashboard';

describe('Dashboard tenant statistics', () => {
  it('shows shared network counts and tenant cash, stock and aid', () => {
    const api: any = {
      getActiveCharity: () => ({ id: 'c1' }),
      getDashboardStats: () => of({ totalPartnerCharitiesCount: 3, totalCashDonations: 500 }),
      getBeneficiaries: () => of([{ id: 'b1' }, { id: 'b2' }]),
      getDonations: () => of([{ donationKind: 'نقدي', amount: 100 }, { donationKind: 'عيني', amount: 900 }]),
      getInventory: () => of([{ kind: 'صنف ملموس' }, { kind: 'خدمة' }]),
      getDistributions: () => of([{ id: 'd1' }]), getStaff: () => of([{ id: 's1' }]),
      getCharities: () => of([{ id: 'c1' }, { id: 'c2' }]),
      getAllDistributions: () => of([{ id: 'd1' }, { id: 'd2' }])
    };
    const dashboard = new DashboardComponent(api, { detectChanges: () => {} } as any);
    dashboard.ngOnInit();
    expect(dashboard.beneficiariesCount).toBe(2);
    expect(dashboard.charitiesCount).toBe(2);
    expect(dashboard.totalCashDonations).toBe(100);
    expect(dashboard.tangibleItemsCount).toBe(1);
    expect(dashboard.distributionsCount).toBe(1);
    expect(dashboard.totalNetworkDistributionsCount).toBe(2);
    expect(dashboard.networkStats.totalCashDonations).toBe(500);
  });
});
