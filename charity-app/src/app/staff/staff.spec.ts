import { of } from 'rxjs';
import { StaffComponent } from './staff';

describe('Staff entry', () => {
  it('blocks a negative salary and malformed date before sending', () => {
    const api: any = { addStaff: vi.fn(() => of({})) };
    const page = new StaffComponent(api, { detectChanges: () => {} } as any, {} as any, {} as any);
    page.newStaff = {
      name: 'موظف', nationalId: '28501010112345', phone: '01012345678',
      type: 'موظف براتب', role: 'محاسب', salary: -100, joinDate: page.todayInCairo()
    };
    page.onSubmit();
    expect(api.addStaff).not.toHaveBeenCalled();
    page.newStaff.salary = 0;
    page.newStaff.joinDate = 'wrong';
    page.onSubmit();
    expect(api.addStaff).not.toHaveBeenCalled();
  });
});
