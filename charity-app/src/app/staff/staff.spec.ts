import { of } from 'rxjs';
import { StaffComponent } from './staff';

describe('Staff entry', () => {
  it('blocks a negative salary and malformed date before sending', () => {
    const api: any = { addStaff: vi.fn(() => of({})) };
    const page = new StaffComponent(api, {} as any, { detectChanges: () => {} } as any, {} as any, {} as any);
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

  it('lets a charity manager reset a staff password only after matching confirmation', () => {
    const api: any = { resetStaffUserPassword: vi.fn(() => of(void 0)) };
    const auth: any = { getCurrentUser: () => ({ role: 'CharityManager' }) };
    const page = new StaffComponent(api, auth, { detectChanges: () => {} } as any, {} as any, {} as any);
    page.openPasswordResetModal({ id: 'staff-1', fullName: 'موظف', username: 'worker' });
    page.newStaffPassword = 'NewPass123';
    page.confirmStaffPassword = 'different';
    page.resetStaffPassword();
    expect(api.resetStaffUserPassword).not.toHaveBeenCalled();
    expect(page.passwordResetError).toContain('غير مطابق');

    page.confirmStaffPassword = 'NewPass123';
    page.resetStaffPassword();
    expect(api.resetStaffUserPassword).toHaveBeenCalledWith('staff-1', 'NewPass123');
    expect(page.showPasswordModal).toBe(false);
    expect(page.newStaffPassword).toBe('');
  });
});
