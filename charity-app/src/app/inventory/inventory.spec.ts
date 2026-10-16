import { of } from 'rxjs';
import { InventoryComponent } from './inventory';

describe('Inventory screen', () => {
  it('counts only tangible stock and low-stock items', () => {
    const page = new InventoryComponent({} as any, { detectChanges: () => {} } as any);
    page.inventory = [
      { kind: 'صنف ملموس', quantity: 3, minAlert: 5 },
      { kind: 'صنف ملموس', quantity: 10, minAlert: 5 },
      { kind: 'خدمة', quantity: 100, minAlert: 5 }
    ];
    expect(page.totalUnitsInWarehouse).toBe(13);
    expect(page.lowStockCount).toBe(1);
  });

  it('rejects a negative stock adjustment before an API write', () => {
    const api: any = { adjustStock: vi.fn(() => of({})), getInventory: () => of([]) };
    const page = new InventoryComponent(api, { detectChanges: () => {} } as any);
    page.selectedStockItem = { id: 'i1', quantity: 10 };
    page.adjustedQuantity = -2;
    page.saveStockAdjustment();
    expect(api.adjustStock).not.toHaveBeenCalled();
  });
});
