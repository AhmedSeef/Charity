const fs = require('fs');
const file = 'd:/Charity/charity-app/src/app/beneficiaries/beneficiaries.html';
let html = fs.readFileSync(file, 'utf8');

html = html.replace(
  /<td style="max-width: 250px; color: #555;">\{\{ b\.researchNotes.*?<\/td>/,
  '<td style="max-width: 250px; color: #555;">{{ b.researchNotes || \'لم تدون ملاحظات\' }}</td><td style="text-align: center;"><button (click)="openHistory(b, $event)" style="background: none; border: none; font-size: 24px; cursor: pointer;" title="سجل الصرف">👁️</button></td>'
);

fs.writeFileSync(file, html);
