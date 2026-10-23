// Structural validation; the check-digit algorithm is not officially published.
// Reference: https://vlenseg.com/en/egypt-national-id-api/
const governorates = new Set([
  '01', '02', '03', '04', '11', '12', '13', '14', '15', '16', '17', '18', '19',
  '21', '22', '23', '24', '25', '26', '27', '28', '29', '31', '32', '33', '34', '35', '88'
]);

export function normalizeNationalId(value: string | null | undefined): string {
  return (value ?? '').trim().replace(/[٠-٩۰-۹]/g, c =>
    String(c.charCodeAt(0) - (c <= '٩' ? '٠'.charCodeAt(0) : '۰'.charCodeAt(0))));
}

export function nationalIdError(value: string | null | undefined, today = new Date()): string | null {
  const id = normalizeNationalId(value);
  if (!/^[0-9]{14}$/.test(id)) return 'يجب أن يتكون الرقم القومي من 14 رقماً فقط.';
  if (!/^[23]/.test(id)) return 'كود القرن في الرقم القومي يجب أن يكون 2 أو 3.';
  const year = (id[0] === '2' ? 1900 : 2000) + Number(id.slice(1, 3));
  const month = Number(id.slice(3, 5));
  const day = Number(id.slice(5, 7));
  const birth = new Date(Date.UTC(year, month - 1, day));
  if (birth.getUTCFullYear() !== year || birth.getUTCMonth() !== month - 1 || birth.getUTCDate() !== day)
    return 'تاريخ الميلاد داخل الرقم القومي غير صحيح.';
  const parts = new Intl.DateTimeFormat('en-US', { timeZone: 'Africa/Cairo', year: 'numeric', month: '2-digit', day: '2-digit' }).formatToParts(today);
  const part = (type: string) => parts.find(p => p.type === type)!.value;
  if (`${year}${id.slice(3, 7)}` > `${part('year')}${part('month')}${part('day')}`)
    return 'تاريخ الميلاد داخل الرقم القومي لا يمكن أن يكون في المستقبل.';
  if (!governorates.has(id.slice(7, 9))) return 'كود محافظة الميلاد في الرقم القومي غير صحيح (88 لمواليد خارج مصر).';
  return null;
}
