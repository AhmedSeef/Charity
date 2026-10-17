import { DistributionsComponent } from './distributions';

describe('Distributions screen', () => {
  it('filters the existing distribution list by item', () => {
    const page = new DistributionsComponent({} as any, { detectChanges: () => {} } as any, {} as any);
    page.distributions = [
      { id: '1', receivedByName: 'أحمد', itemName: 'أرز' },
      { id: '2', receivedByName: 'منى', itemName: 'بطانية' }
    ];
    page.searchFilter = 'بطانية';
    expect(page.filteredDistributions.map(d => d.id)).toEqual(['2']);
  });
});
