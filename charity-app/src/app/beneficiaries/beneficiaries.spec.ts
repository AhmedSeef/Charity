import { BeneficiariesComponent } from './beneficiaries';
import { EMPTY, of } from 'rxjs';

function form() {
  const api = { addBeneficiary: vi.fn((_data: any) => EMPTY), updateBeneficiary: vi.fn((_data: any) => EMPTY), addFamilyMember: vi.fn((_id: any, _data: any) => EMPTY),
    getBeneficiarySharedHistory: vi.fn((_id: string): any => EMPTY), getAllDistributions: vi.fn() };
  const page = new BeneficiariesComponent(api as any, { detectChanges: () => {} } as any, {} as any, {} as any,
    { getCurrentUser: () => ({ role: 'CharityManager', charityId: 'c1' }), hasPermission: () => true } as any);
  page.currentBen.name = 'QA identity';
  page.currentBen.nationalId = '28501011234567';
  return { page, api };
}

describe('Beneficiaries screen', () => {
  it('only permits editing files owned by the authenticated charity', () => {
    const { page } = form();
    expect(page.canEditBeneficiary({ registeredByCharityId: 'c1' })).toBe(true);
    expect(page.canEditBeneficiary({ registeredByCharityId: 'c2' })).toBe(false);
    expect(page.canEditBeneficiary({ registeredByCharityId: null })).toBe(false);
  });
  it('loads all charities aid only for the beneficiary profile being opened', () => {
    const { page, api: rawApi } = form();
    const api: any = rawApi;
    page.beneficiaries = [{ id: 'ahmed', registeredByCharityId: 'c2' }];
    (page as any).route = { queryParams: of({ mode: 'detail', id: 'ahmed' }) };
    api.getBeneficiarySharedHistory.mockReturnValue(of({ aidHistory: [
      { id: 'aid-a', beneficiaryId: 'ahmed', charityId: 'c1' },
      { id: 'aid-b', beneficiaryId: 'ahmed', charityId: 'c2' }
    ] }));
    page.initRouteSync();
    expect(api.getBeneficiarySharedHistory).toHaveBeenCalledWith('ahmed');
    expect(page.distributions.map(d => d.id)).toEqual(['aid-a', 'aid-b']);
    expect(api.getAllDistributions).not.toHaveBeenCalled();
    page.ngOnDestroy();
  });
  it('starts with a new family form and no selected record', () => {
    const { page } = form();
    expect(page.viewMode).toBe('list');
    expect(page.currentBen.familyCount).toBe(1);
    expect(page.currentBen.familyMembers).toEqual([]);
  });
  it('cascades governorate to district, village, then area and clears stale children', () => {
    const { page, api: rawApi } = form();
    const api: any = rawApi;
    api.getDistricts = vi.fn(() => of([{ id: 'd1', governorateId: 'g1', name: 'مركز أول' }]));
    api.getVillages = vi.fn(() => of([{ id: 'v1', districtId: 'd1', name: 'قرية أولى' }]));
    api.getVillageAreas = vi.fn(() => of([{ id: 'a1', villageId: 'v1', name: 'منطقة أولى' }]));
    page.governorates = [{ id: 'g1', name: 'محافظة أولى' }];
    page.currentBen.governorateId = 'g1';
    page.currentBen.districtId = 'old';
    page.currentBen.villageId = 'old-village';
    page.currentBen.villageAreaId = 'old-area';
    page.onGovernorateChange();
    expect(api.getDistricts).toHaveBeenCalledWith('g1');
    expect(page.filteredDistricts.map(x => x.id)).toEqual(['d1']);
    expect(page.currentBen.districtId).toBe('');
    page.currentBen.districtId = 'd1';
    page.onDistrictChange();
    expect(api.getVillages).toHaveBeenCalledWith('d1');
    expect(page.filteredVillages.map(x => x.id)).toEqual(['v1']);
    page.currentBen.villageId = 'v1';
    page.onVillageChange();
    expect(api.getVillageAreas).toHaveBeenCalledWith('v1');
    expect(page.filteredVillageAreas.map(x => x.id)).toEqual(['a1']);
  });

  for (const edit of [false, true]) {
    for (const name of ['', '   ', '\t\n']) {
      it(`rejects a blank name before ${edit ? 'updating' : 'creating'}`, () => {
        const { page, api } = form();
        page.currentBen.id = edit ? 'existing' : null;
        page.currentBen.name = name;
        page.onSubmit();
        expect(page.errorMessage).toContain('اسم');
        expect(api.addBeneficiary).not.toHaveBeenCalled();
        expect(api.updateBeneficiary).not.toHaveBeenCalled();
      });
    }
    it(`rejects negative income before ${edit ? 'updating' : 'creating'}`, () => {
      const { page, api } = form();
      page.currentBen.id = edit ? 'existing' : null;
      page.currentBen.monthlyIncome = -0.01;
      page.onSubmit();
      expect(page.errorMessage).toContain('الدخل');
      expect(api.addBeneficiary).not.toHaveBeenCalled();
      expect(api.updateBeneficiary).not.toHaveBeenCalled();
    });
    for (const income of [0, 0.01, 1234.56, null]) {
      it(`accepts income ${income} when ${edit ? 'updating' : 'creating'}`, () => {
        const { page, api } = form();
        page.currentBen.id = edit ? 'existing' : null;
        page.currentBen.name = '  Trimmed name  ';
        page.currentBen.monthlyIncome = income;
        page.onSubmit();
        expect(edit ? api.updateBeneficiary : api.addBeneficiary).toHaveBeenCalledOnce();
        expect(page.currentBen.name).toBe('Trimmed name');
        expect(page.currentBen.monthlyIncome).toBe(income);
      });
    }
    it(`rejects an invalid head before ${edit ? 'updating' : 'creating'} via API`, () => {
      const { page, api } = form();
      page.currentBen.id = edit ? 'existing' : null;
      page.currentBen.nationalId = 'abc';
      page.onSubmit();
      expect(page.errorMessage).toContain('14');
      expect(api.addBeneficiary).not.toHaveBeenCalled();
      expect(api.updateBeneficiary).not.toHaveBeenCalled();
    });
    it(`rejects an invalid dependent before ${edit ? 'updating' : 'creating'} via API`, () => {
      const { page, api } = form();
      page.currentBen.id = edit ? 'existing' : null;
      page.currentBen.familyMembers = [{ name: 'Invalid child', nationalId: '39901010112345' }];
      page.onSubmit();
      expect(page.errorMessage).toContain('المستقبل');
      expect(api.addBeneficiary).not.toHaveBeenCalled();
      expect(api.updateBeneficiary).not.toHaveBeenCalled();
    });
  }
  it('normalizes an Arabic head and dependent before submitting', () => {
    const { page, api } = form();
    page.currentBen.nationalId = '٢٨٥٠١٠١١٢٣٤٥٦٧';
    page.currentBen.familyMembers = [{ name: 'Child', nationalId: '٣٠٠٠٢٢٩٠١١٢٣٤٥' }];
    page.onSubmit();
    expect(api.addBeneficiary).toHaveBeenCalledOnce();
    expect(api.addBeneficiary.mock.calls[0][0].nationalId).toBe('28501011234567');
    expect(api.addBeneficiary.mock.calls[0][0].familyMembers[0].nationalId).toBe('30002290112345');
  });
  it('rejects an invalid inline new head before adding a member', () => {
    const { page, api } = form();
    page.selectedBen = { id: 'existing' };
    page.newMemberData = { name: 'Invalid head', nationalId: '28501010512345' };
    page.saveAndSelectNewMember();
    expect(page.errorMessage).toContain('محافظة');
    expect(api.addFamilyMember).not.toHaveBeenCalled();
  });
  it('rejects negative dependent income even when hasIncome is false', () => {
    const { page, api } = form();
    page.currentBen.familyMembers = [{ name: 'Child', nationalId: '30002290112345', hasIncome: false, incomeAmount: -1 }];
    page.onSubmit();
    expect(page.errorMessage).toContain('دخل فرد الأسرة');
    expect(api.addBeneficiary).not.toHaveBeenCalled();
  });
  it('rejects a whitespace-only dependent name', () => {
    const { page, api } = form();
    page.currentBen.familyMembers = [{ name: '  ', nationalId: '30002290112345', incomeAmount: 0 }];
    page.onSubmit();
    expect(page.errorMessage).toContain('اسم فرد الأسرة');
    expect(api.addBeneficiary).not.toHaveBeenCalled();
  });
});
