import { isTodayOrEarlier, todayInCairo } from './business-date';

describe('Cairo business date', () => {
  it('uses the Cairo calendar day near the UTC date boundary', () => {
    expect(todayInCairo(new Date('2026-10-22T22:30:00Z'))).toBe('2026-10-23');
  });

  it('accepts today and earlier dates but rejects invalid and future dates', () => {
    expect(isTodayOrEarlier('2026-10-23', '2026-10-23')).toBe(true);
    expect(isTodayOrEarlier('2020-01-01', '2026-10-23')).toBe(true);
    expect(isTodayOrEarlier('2026-10-24', '2026-10-23')).toBe(false);
    expect(isTodayOrEarlier('2026-02-30', '2026-10-23')).toBe(false);
    expect(isTodayOrEarlier('wrong', '2026-10-23')).toBe(false);
  });
});
