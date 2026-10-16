import { BeneficiariesComponent } from './beneficiaries';

describe('Beneficiaries screen', () => {
  it('starts with a new family form and no selected record', () => {
    const page = new BeneficiariesComponent({} as any, {} as any, {} as any, { detectChanges: () => {} } as any);
    expect(page.viewMode).toBe('list');
    expect(page.currentBen.familyCount).toBe(1);
    expect(page.currentBen.familyMembers).toEqual([]);
  });
});
