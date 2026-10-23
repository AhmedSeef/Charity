import { nationalIdError, normalizeNationalId } from './egyptian-national-id';

const today = new Date('2026-10-23T12:00:00Z');
describe('Egyptian national identity structure', () => {
  for (const id of ['28501011234567', '30002290112345', '32610230112345', '29912318812345']) {
    it(`accepts structurally valid ${id}`, () => expect(nationalIdError(id, today)).toBeNull());
  }
  for (const id of ['', 'abc', '2850101123456', '285010112345678', '18501011234567', '48501011234567',
    '28513011234567', '28501001234567', '28504311234567', '29902291234567', '30002301234567',
    '32610240112345', '28501010012345', '28501010512345', '28501012012345', '28501013012345', '28501019912345']) {
    it(`rejects malformed ${id}`, () => expect(nationalIdError(id, today)).not.toBeNull());
  }
  it('normalizes Arabic and Persian digits to the same identity', () => {
    expect(normalizeNationalId(' ٢٨٥٠١٠١١٢٣٤٥٦٧ ')).toBe('28501011234567');
    expect(normalizeNationalId('۲۸۵۰۱۰۱۱۲۳۴۵۶۷')).toBe('28501011234567');
    expect(nationalIdError('٢٨٥٠١٠١١٢٣٤٥٦٧', today)).toBeNull();
  });
  it('uses the Cairo calendar date even near UTC midnight', () => {
    expect(nationalIdError('32610230112345', new Date('2026-10-22T22:30:00Z'))).toBeNull();
    expect(nationalIdError('32610240112345', new Date('2026-10-22T22:30:00Z'))).not.toBeNull();
  });
});
