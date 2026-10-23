import { of } from 'rxjs';
import { ExpenseCategoriesComponent } from './expense-categories';

describe('Expense reports', () => {
  it('uses the same date range for category totals, monthly report and recipient details', () => {
    const api: any = {
      getExpenseCategories: vi.fn(() => of([{ id: 'medical', totalDisbursedAmount: 1200, disbursementsCount: 1, beneficiaryIds: ['family-1'] }])),
      getMonthlyExpenseMovements: vi.fn(() => of([{ monthYear: '2026-10', totalAmount: 1200, disbursementsCount: 1, beneficiaryIds: ['family-1'] }])),
      getExpenseCategoryBeneficiaries: vi.fn(() => of([{ beneficiaryId: 'family-1', amount: 1200 }]))
    };
    const page = new ExpenseCategoriesComponent(api, {} as any, { detectChanges: () => {} } as any);
    page.dateFrom = '2026-10-20';
    page.dateTo = '2026-10-21';
    page.applyDateRange();

    expect(api.getExpenseCategories).toHaveBeenCalledWith(undefined, '2026-10-20', '2026-10-21');
    expect(api.getMonthlyExpenseMovements).toHaveBeenCalledWith(undefined, '2026-10-20', '2026-10-21');
    expect(page.totalDisbursedAll).toBe(1200);
    page.openDrilldown({ id: 'medical' });
    expect(api.getExpenseCategoryBeneficiaries).toHaveBeenCalledWith('medical', undefined, '2026-10-20', '2026-10-21');
  });

  it('rejects reversed dates without requesting a report', () => {
    const api: any = { getExpenseCategories: vi.fn(), getMonthlyExpenseMovements: vi.fn() };
    const page = new ExpenseCategoriesComponent(api, {} as any, { detectChanges: () => {} } as any);
    page.dateFrom = '2026-10-22';
    page.dateTo = '2026-10-20';
    page.applyDateRange();
    expect(api.getExpenseCategories).not.toHaveBeenCalled();
    expect(api.getMonthlyExpenseMovements).not.toHaveBeenCalled();
    expect(page.errorMessage).toContain('البداية');
  });
});
