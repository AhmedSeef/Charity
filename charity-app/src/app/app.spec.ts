import { of } from 'rxjs';
import { App } from './app';

describe('App tenant shell', () => {
  it('uses the authenticated tenant and saved settings', () => {
    const api: any = {
      setActiveCharityId: vi.fn(), getActiveCharityId: () => 'c2',
      getCharities: () => of([{ id: 'c1', name: 'الأولى' }, { id: 'c2', name: 'الثانية' }]),
      getSettings: () => of({ charityName: 'الاسم المعتمد', logo: 'logo.png' })
    };
    const auth: any = { getCurrentUser: () => ({ charityId: 'c2' }) };
    const app = new App(api, auth, { detectChanges: () => {} } as any);
    app.initUserCharity();
    expect(api.setActiveCharityId).toHaveBeenCalledWith('c2');
    expect(app.activeCharity()?.id).toBe('c2');
    expect(app.title()).toBe('الاسم المعتمد');
  });
});
