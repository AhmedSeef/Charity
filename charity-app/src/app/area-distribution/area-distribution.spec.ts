import { NEVER } from 'rxjs';
import { AreaDistributionComponent } from './area-distribution';

describe('Area distribution filtering', () => {
  const families = Array.from({ length: 11 }, (_, i) => ({
    id: `family-${i + 1}`,
    villageAreaName: i < 2 ? 'المنشر' : 'السوق',
    familyCount: 4
  }));

  function page(api: any = {}): AreaDistributionComponent {
    const component = new AreaDistributionComponent(api, {} as any, { detectChanges: () => {} } as any);
    component.beneficiaries = families;
    return component;
  }

  it('replaces an earlier selection and previews only the two families in the selected area', () => {
    const component = page();
    component.selectAllFiltered();
    expect(component.selectedCount).toBe(11);

    component.setAreaSearch('المنشر');
    expect(component.filteredBeneficiaries).toHaveLength(2);
    expect(component.selectedCount).toBe(2);
    expect(component.totalRequiredQuantity).toBe(2);
    component.previewRunSheet();
    expect(component.runSheetData.beneficiaries.map((b: any) => b.id)).toEqual(['family-1', 'family-2']);

    component.setAreaSearch('السوق');
    expect(component.selectedCount).toBe(9);
    component.setAreaSearch('المنشر');
    expect(component.selectedCount).toBe(2);
  });

  it('sends only visible selected families, even if stale IDs remain in the selection set', () => {
    const api = { createBulkDistribution: vi.fn((_payload: any) => NEVER) };
    const component = page(api);
    component.selectedBeneficiaryIds = new Set(families.map(f => f.id));
    component.areaSearchTerm = 'المنشر';
    component.inventoryItems = [{ id: 'stock', itemName: 'كرتونة', unit: 'كرتونة', quantity: 50 }];
    component.selectedInventoryItemId = 'stock';
    const originalConfirm = globalThis.confirm;
    globalThis.confirm = vi.fn(() => true);
    try {
      component.executeBulkDisbursement();
      expect(api.createBulkDistribution).toHaveBeenCalledOnce();
      const payload = api.createBulkDistribution.mock.calls[0][0];
      expect(payload.beneficiaryIds).toEqual(['family-1', 'family-2']);
      expect(payload.targetAreaFilter).toBe('المنشر');
    } finally {
      globalThis.confirm = originalConfirm;
    }
  });
});
