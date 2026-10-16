import { of } from 'rxjs';
import { DistributionsComponent } from './distributions';

describe('Distributions screen', () => {
  it.each([-1, 0, NaN, Infinity])('rejects invalid quantity %s before writing', quantity => {
    const api: any = { addDistribution: vi.fn(() => of({})) };
    const page = new DistributionsComponent(api, { detectChanges: () => {} } as any);
    page.inventory = [{ id: 'rice', charityId: 'c1', kind: 'صنف ملموس', quantity: 10 }];
    page.beneficiaries = [{ id: 'b1', name: 'أسرة اختبار' }];
    page.newDist = { beneficiaryId: 'b1', inventoryId: 'rice', quantity };
    page.onSubmit();
    expect(api.addDistribution).not.toHaveBeenCalled();
    expect(page.errorMessage).toBeTruthy();
  });

  it('rejects a stale inventory selection', () => {
    const api: any = { addDistribution: vi.fn(() => of({})) };
    const page = new DistributionsComponent(api, { detectChanges: () => {} } as any);
    page.newDist = { beneficiaryId: 'b1', inventoryId: 'missing', quantity: 1 };
    page.onSubmit();
    expect(api.addDistribution).not.toHaveBeenCalled();
  });
});
