import { DisbursementComponent } from './disbursement';

describe('Disbursement entry', () => {
  it('blocks future dates before sending', () => {
    const api: any = { addDistribution: vi.fn() };
    const page = new DisbursementComponent(api, {} as any, { detectChanges: () => {} } as any, {} as any);
    page.selectedBeneficiary = { id: 'family', name: 'أسرة', nationalId: '28501010112345' };
    page.distributionData.date = '2099-01-01';
    page.submitDisbursement();
    expect(api.addDistribution).not.toHaveBeenCalled();
    expect(page.errorMessage).toContain('تاريخ الصرف');
  });
});
