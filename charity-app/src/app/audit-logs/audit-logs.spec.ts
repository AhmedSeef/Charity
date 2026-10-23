import { of } from 'rxjs';
import { AuditLogsComponent } from './audit-logs';

describe('Charity audit log', () => {
  it('sends actor, action, type and date filters and keeps server pagination', () => {
    const api: any = { getAuditLogs: vi.fn(() => of({ items: [{ id: 1 }], totalCount: 52 })) };
    const page = new AuditLogsComponent(api);
    page.filters = { name: 'أحمد', action: 'Delete', entityType: 'Beneficiary', from: '2026-10-01', to: '2026-10-23' };
    page.search();

    expect(api.getAuditLogs).toHaveBeenCalledWith({ ...page.filters, page: 1, pageSize: 25 });
    expect(page.totalPages).toBe(3);
    page.changePage(2);
    expect(api.getAuditLogs).toHaveBeenLastCalledWith({ ...page.filters, page: 2, pageSize: 25 });
  });
});
