import { Component } from '@angular/core';

@Component({
  selector: 'app-home',
  standalone: true,
  template: `<section dir="rtl" style="padding:32px"><h1>الرئيسية</h1><p>أهلًا بك. اختر من القائمة إحدى الصفحات المتاحة لحسابك.</p><p>إذا كنت تحتاج صلاحية إضافية، تواصل مع مدير الجمعية.</p></section>`
})
export class HomeComponent {}
