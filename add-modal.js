const fs = require('fs');
const file = 'd:/Charity/charity-app/src/app/beneficiaries/beneficiaries.html';
let html = fs.readFileSync(file, 'utf8');

html = html.replace('<th>ملخص بحث الحالة</th>', '<th>ملخص بحث الحالة</th><th>سجل الصرف</th>');
html = html.replace(
  /<td style="max-width: 250px; color: #555;">{{ b.researchNotes \|\| 'لم تدون ملاحظات' }}<\/td>/,
  '<td style="max-width: 250px; color: #555;">{{ b.researchNotes || \'لم تدون ملاحظات\' }}</td><td style="text-align: center;"><button (click)="openHistory(b, $event)" style="background: none; border: none; font-size: 24px; cursor: pointer;" title="سجل الصرف">👁️</button></td>'
);

const modal = `
<div *ngIf="historyModalOpen" style="position: fixed; top: 0; left: 0; width: 100%; height: 100%; background: rgba(0,0,0,0.5); display: flex; justify-content: center; align-items: center; z-index: 1000; padding: 15px; box-sizing: border-box;">
  <div style="background: white; padding: 20px; border-radius: 8px; width: 100%; max-width: 600px; max-height: 80vh; overflow-y: auto;">
    <div style="display: flex; justify-content: space-between; align-items: center; border-bottom: 2px solid #004d40; padding-bottom: 10px; margin-bottom: 15px;">
      <h3 style="margin: 0; color: #004d40;">سجل حركات: {{ selectedBenName }}</h3>
      <button (click)="closeHistory()" style="background: #d32f2f; color: white; border: none; padding: 5px 15px; border-radius: 5px; cursor: pointer; font-weight: bold;">إغلاق ✖</button>
    </div>
    <div *ngIf="selectedHistory.length === 0" style="text-align: center; padding: 20px; color: #666;">
      لم يتم تسجيل أي حركات صرف أو مساعدات لهذا المستفيد حتى الآن.
    </div>
    <div style="overflow-x: auto; width: 100%;" *ngIf="selectedHistory.length > 0">
      <table border="1" cellpadding="10" cellspacing="0" style="width: 100%; border-collapse: collapse; font-size: 14px;">
        <tr style="background-color: #e0f2f1; color: #004d40;">
          <th>التاريخ</th>
          <th>الصنف / المساعدة</th>
          <th>الكمية</th>
          <th>ملاحظات</th>
        </tr>
        <tr *ngFor="let h of selectedHistory">
          <td>{{ h.date }}</td>
          <td>{{ h.itemName }}</td>
          <td>{{ h.quantity }}</td>
          <td>{{ h.notes }}</td>
        </tr>
      </table>
    </div>
  </div>
</div>
`;

if (!html.includes('historyModalOpen')) {
  fs.writeFileSync(file, html + modal);
}
