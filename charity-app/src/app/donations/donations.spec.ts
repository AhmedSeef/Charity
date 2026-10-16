import { of } from 'rxjs';
import { DonationsComponent } from './donations';

describe('Donations screen', () => {
  it('rejects a nonfinite cash amount before an API write', () => {
    const api: any = { addDonation: vi.fn(() => of({})), getDonations: () => of([]), getInventory: () => of([]) };
    const page = new DonationsComponent(api, { detectChanges: () => {} } as any);
    page.newDonation.donorName = 'متبرع';
    page.newDonation.amount = 'NaN';
    page.onSubmit();
    expect(api.addDonation).not.toHaveBeenCalled();
  });

  it('chooses a matching catalog item when donation kind changes', () => {
    const page = new DonationsComponent({} as any, { detectChanges: () => {} } as any);
    page.inventory = [{ id: 'food', kind: 'صنف ملموس', unit: 'كيلو' }, { id: 'check', kind: 'خدمة', unit: 'كشف' }];
    page.newDonation.inventoryId = 'food';
    page.newDonation.donationKind = 'خدمة';
    page.onKindChange();
    expect(page.newDonation.inventoryId).toBe('check');
  });
});
