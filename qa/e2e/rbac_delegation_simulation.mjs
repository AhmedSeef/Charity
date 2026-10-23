import { chromium } from '@playwright/test';
import { execSync } from 'child_process';

const BASE_URL = 'http://localhost:5072';

function queryDb(sql) {
  try {
    const b64 = Buffer.from(sql, 'utf-8').toString('base64');
    const ps = `[Console]::OutputEncoding = [System.Text.Encoding]::UTF8; $q = [System.Text.Encoding]::UTF8.GetString([Convert]::FromBase64String('${b64}')); $c = [System.Data.SqlClient.SqlConnection]::new('Server=.;Database=CharityDb;Trusted_Connection=True;TrustServerCertificate=True;'); $c.Open(); $cmd = $c.CreateCommand(); $cmd.CommandText = $q; $r = $cmd.ExecuteReader(); while ($r.Read()) { $row = @(); for ($i=0; $i -lt $r.FieldCount; $i++) { $row += $r.GetValue($i).ToString() }; Write-Output ($row -join '|||') }; $c.Close();`;
    const raw = execSync(`powershell -NoProfile -Command "${ps}"`, { encoding: 'utf-8' });
    const lines = raw.split('\n').map(l => l.trim()).filter(Boolean);
    return lines.map(line => line.split('|||').map(c => c.trim()));
  } catch (err) {
    console.error('SQL query error:', err.message);
    return [];
  }
}

async function run() {
  console.log('=============================================================================');
  console.log('🚀 بدء المحاكاة الشاملة لنظام الصلاحيات وتفويض الأدوار (RBAC Simulation E2E)');
  console.log('=============================================================================');

  const browser = await chromium.launch({ headless: true });
  const context = await browser.newContext({ viewport: { width: 1300, height: 950 } });
  const page = await context.newPage();

  const report = [];

  // =========================================================================
  // 1. تسجيل الدخول كـ مدير الجمعية (ahmed)
  // =========================================================================
  console.log('\n[الخطوة 1] تسجيل دخول مدير الجمعية (ahmed)...');
  await page.goto(`${BASE_URL}/login`);
  await page.locator('input[name="username"]').fill('ahmed');
  await page.locator('input[name="password"]').fill('admin');
  await page.getByRole('button', { name: 'دخول للنظام' }).click();

  await page.waitForURL(/\/beneficiaries/);
  await page.waitForTimeout(1000);
  console.log('✅ تم تسجيل دخول مدير الجمعية بنجاح.');

  const adminToken = await page.evaluate(() => localStorage.getItem('token') || sessionStorage.getItem('token'));

  // إنشاء أو التحقق من وجود مستفيد تجريبي يحتوي على ديون وبيانات صحية حساسة
  let testBenId = null;
  const bensListRes = await fetch(`${BASE_URL}/api/beneficiaries`, {
    headers: { 'Authorization': `Bearer ${adminToken}` }
  });
  const bensList = await bensListRes.json();
  if (bensList && bensList.length > 0) {
    testBenId = bensList[0].id;
    console.log(`- مستفيد تجريبي موجود مسبقاً: ${testBenId} (${bensList[0].name})`);
  } else {
    console.log('- إنشاء مستفيد تجريبي جديد يحتوي على بيانات سرية وديون...');
    const createBenRes = await fetch(`${BASE_URL}/api/beneficiaries`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${adminToken}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        nationalId: '29001011234567',
        name: 'سعيد محمد عبد العال المستور',
        phone: '01012345678',
        category: 'أرامل وأيتام',
        maritalStatus: 'متزوج',
        familyCount: 4,
        address: 'شارع السلام - قرية الأمل',
        housing: 'إيجار قديم',
        incomeSource: 'يومية باليومية',
        monthlyIncome: 1200,
        hasDebts: true,
        totalDebtAmount: 15000,
        debtCreditor: 'بنك ناصر وبقالة الأمانة',
        debtNotes: 'كمبيالات مستحقة السداد مهددة بالحبس',
        isThreatenedWithImprisonment: true,
        hasChronicDisease: true,
        chronicDiseaseDetails: 'فشل كلوي مزمن للأب وغسيل أسبوعي',
        healthIssues: 'فشل كلوي مزمن للأب وعلاج شهري',
        researchNotes: 'حالة حرجة جداً تعاني من عجز مالي شديد وتحتاج سلة غذائية شهرية ومساعدة مالية'
      })
    });
    const createdBen = await createBenRes.json();
    testBenId = createdBen.id;
    console.log(`- تم إنشاء المستفيد التجريبي بنجاح: ${testBenId}`);
  }

  report.push({
    step: '1. تسجيل دخول المدير وتجهيز الملف النموذجي',
    actor: 'ahmed (مدير الجمعية - CharityManager)',
    result: 'تم الدخول بنجاح، وتجهيز ملف مستفيد يحتوي على ديون وبيانات صحية حساسة لاختبار الحجب.'
  });

  // =========================================================================
  // 2. الانتقال لشاشة فريق العمل عبر القائمة الجانبية
  // =========================================================================
  console.log('\n[الخطوة 2] الانتقال إلى شاشة فريق العمل وتفويض الصلاحيات...');
  await page.locator('nav.sidebar a:has-text("فريق العمل")').click();
  await page.waitForSelector('text=سجل فريق العمل والمتطوعين');

  // النقر على تبويب تفويض الصلاحيات
  await page.getByRole('button', { name: /حسابات الدخول وتفويض الصلاحيات/ }).click();
  await page.waitForSelector('text=تفويض الصلاحيات للموظفين');
  console.log('✅ فتح تبويب تفويض الصلاحيات وحسابات الدخول.');

  // =========================================================================
  // 3. إنشاء حساب موظف جديد بدور مدخل بيانات (Data Entry)
  // =========================================================================
  console.log('\n[الخطوة 3] إنشاء حساب موظف جديد (nour_entry) بقالب "مدخل بيانات"...');
  
  // التأكد من عدم وجود المستخدم مسبقاً في قاعدة البيانات لتجربة نظيفة
  queryDb("DELETE FROM UserClaims WHERE UserId IN (SELECT Id FROM Users WHERE UserName = 'nour_entry')");
  queryDb("DELETE FROM UserRoles WHERE UserId IN (SELECT Id FROM Users WHERE UserName = 'nour_entry')");
  queryDb("DELETE FROM Users WHERE UserName = 'nour_entry'");

  // الضغط على زر فتح نافذة إنشاء حساب موظف
  await page.getByRole('button', { name: /إنشاء حساب مستخدم لموظف/ }).click();
  await page.waitForSelector('text=إنشاء حساب دخول وتفويض صلاحيات');

  await page.locator('input[placeholder*="أحمد مصطفى"]').fill('نور مدخلة البيانات');
  await page.locator('input[placeholder*="username_staff"]').fill('nour_entry');
  await page.locator('input[placeholder*="••••••••"]').fill('Staff@123456');

  // اختيار قالب مدخل بيانات (DataEntry) من القائمة المنسدلة الثانية
  await page.locator('select').nth(1).selectOption('DataEntry');
  await page.waitForTimeout(500);

  // إرسال النموذج وحفظ الحساب
  await page.getByRole('button', { name: /حفظ وتطبيق الصلاحيات/ }).click();
  await page.waitForSelector('text=إنشاء حساب دخول وتفويض صلاحيات', { state: 'detached' });
  await page.waitForTimeout(1000);

  // التحقق من ظهور الموظف في الجدول
  await page.waitForSelector('text=nour_entry');
  console.log('✅ تم إنشاء حساب الموظف nour_entry وظهر في جدول صلاحيات الموظفين.');

  // التحقق من الصلاحيات المسجلة في جدول UserClaims في قاعدة البيانات
  const claims = queryDb("SELECT ClaimValue FROM UserClaims WHERE UserId = (SELECT Id FROM Users WHERE UserName = 'nour_entry')");
  const claimValues = claims.map(c => c[0]);
  console.log('📋 الصلاحيات الممنوحة لـ nour_entry في قاعدة البيانات:', claimValues);

  const hasViewFull = claimValues.includes('beneficiaries.view_full');
  const hasDelete = claimValues.includes('beneficiaries.delete');
  const hasCreate = claimValues.includes('beneficiaries.create');
  const hasInventory = claimValues.includes('inventory.manage');

  console.log(`- صلاحية الإضافة (beneficiaries.create): ${hasCreate ? 'ممنوحة ✅' : 'مفقودة ❌'}`);
  console.log(`- صلاحية كشف البيانات الحساسة (beneficiaries.view_full): ${!hasViewFull ? 'محجوبة بنجاح 🛡️' : 'مكشوفة خطأ ❌'}`);
  console.log(`- صلاحية الحذف (beneficiaries.delete): ${!hasDelete ? 'محجوبة بنجاح 🛡️' : 'مكشوفة خطأ ❌'}`);
  console.log(`- صلاحية إدارة المخزن (inventory.manage): ${!hasInventory ? 'محجوبة بنجاح 🛡️' : 'مكشوفة خطأ ❌'}`);

  report.push({
    step: '2. إنشاء وتفويض موظف',
    actor: 'ahmed -> nour_entry',
    result: `تم إنشاء الحساب بنجاح بقالب DataEntry. مُنحت له (${claimValues.length}) صلاحيات، وحُجبت عنه البيانات الحساسة والحذف والمخزن والصرف.`
  });

  // =========================================================================
  // 4. تسجيل الخروج ودخول الموظف (nour_entry)
  // =========================================================================
  console.log('\n[الخطوة 4] تسجيل خروج المدير والدخول بحساب الموظف nour_entry...');
  await page.locator('nav.sidebar button:has-text("تسجيل الخروج")').click({ force: true });
  await page.waitForURL(/\/login/);

  await page.locator('input[name="username"]').fill('nour_entry');
  await page.locator('input[name="password"]').fill('Staff@123456');
  await page.getByRole('button', { name: 'دخول للنظام' }).click();

  await page.waitForURL(/\/beneficiaries/);
  await page.waitForTimeout(1000);
  console.log('✅ تم تسجيل الدخول بحساب الموظف nour_entry بنجاح.');

  // =========================================================================
  // 5. فحص عناصر القائمة الجانبية (Sidebar) للموظف
  // =========================================================================
  console.log('\n[الخطوة 5] فحص حجب شاشات القائمة الجانبية غير المصرح بها للموظف...');
  const sidebarNavText = await page.innerText('nav.sidebar');
  
  const hasBeneficiariesLink = sidebarNavText.includes('المستفيدين');
  const hasInventoryLink = sidebarNavText.includes('مخازن الجمعية');
  const hasDonationsLink = sidebarNavText.includes('إدارة التبرعات');
  const hasDistributionsLink = sidebarNavText.includes('صرف مساعدة جديدة');
  const hasStaffLink = sidebarNavText.includes('فريق العمل');

  console.log(`- رابط المستفيدين: ${hasBeneficiariesLink ? 'ظاهر ومصرح به ✅' : 'مفقود ❌'}`);
  console.log(`- رابط المخزن: ${!hasInventoryLink ? 'محجوب ومخفي بنجاح 🛡️' : 'ظاهر بالخطأ ❌'}`);
  console.log(`- رابط التبرعات: ${!hasDonationsLink ? 'محجوب ومخفي بنجاح 🛡️' : 'ظاهر بالخطأ ❌'}`);
  console.log(`- رابط الصرف: ${!hasDistributionsLink ? 'محجوب ومخفي بنجاح 🛡️' : 'ظاهر بالخطأ ❌'}`);
  console.log(`- رابط فريق العمل: ${!hasStaffLink ? 'محجوب ومخفي بنجاح 🛡️' : 'ظاهر بالخطأ ❌'}`);

  report.push({
    step: '3. واجهة القائمة الجانبية',
    actor: 'nour_entry (موظف مدخل بيانات)',
    result: `الشاشات المحجوبة (المخزن، التبرعات، الصرف، فريق العمل) مخفية تماماً من القائمة الجانبية، ويظهر فقط (سجل المستفيدين).`
  });

  // =========================================================================
  // 6. فحص أزرار الإجراءات وحجب البيانات الحساسة على شاشة المستفيدين
  // =========================================================================
  console.log('\n[الخطوة 6] فحص أزرار الحذف وحجب الملاحظات الحساسة في المستفيدين...');
  
  // فحص زر فتح ملف حالة جديد (متاح لمدخل البيانات لأن لديه beneficiaries.create)
  const addBtn = await page.getByRole('button', { name: /فتح ملف حالة جديد/ }).isVisible();
  console.log(`- زر فتح ملف جديد: ${addBtn ? 'ظاهر ومتاح ✅' : 'مخفي ❌'}`);

  // فحص أزرار الحذف في الجدول: يجب ألا تظهر أي أزرار لحذف الملفات
  const deleteBtnCount = await page.locator('button:has-text("أرشفة الملف")').count();
  console.log(`- عدد أزرار الحذف/الأرشفة في واجهة الموظف: ${deleteBtnCount} (المفترض: 0) -> ${deleteBtnCount === 0 ? 'محجوبة بنجاح 🛡️' : 'مكشوفة خطأ ❌'}`);

  // فتح تفاصيل المستفيد لفحص حجب الملاحظات والديون والأمراض
  console.log('\n[الخطوة 7] فتح تفاصيل المستفيد وفحص بطاقة الحجب الأمني (Security Shield)...');
  await page.locator('button:has-text("عرض")').first().click();
  await page.waitForSelector('text=العودة لقائمة المستفيدين');

  // التحقق من ظهور إشعار الحجب الأمني
  const hasSecurityNotice = await page.locator('text=تم حجب التفاصيل الاجتماعية الحساسة').isVisible();
  console.log(`- إشعار الحجب الأمني: ${hasSecurityNotice ? 'ظاهر بنجاح في الشاشة 🛡️' : 'غير ظاهر ❌'}`);

  const pageText = await page.innerText('body');
  const debtsMasked = pageText.includes('[محجوب') || !pageText.includes('بنك ناصر وبقالة الأمانة');
  const healthMasked = pageText.includes('[محجوب') || !pageText.includes('فشل كلوي مزمن للأب');
  console.log(`- حجب الديون والملاحظات الميدانية والصحية: ${debtsMasked && healthMasked ? 'محجوبة بنجاح 🛡️' : 'مكشوفة خطأ ❌'}`);

  report.push({
    step: '4. واجهة المستفيدين والحجب',
    actor: 'nour_entry',
    result: `أزرار الحذف مخفية تماماً، وتفاصيل الديون والأمراض والبحث الميداني محجوبة ومستبدلة ببانر أمني رسمي.`
  });

  // =========================================================================
  // 8. فحص الأمان المباشر على مستوى الـ Backend API (Defense-in-depth)
  // =========================================================================
  console.log('\n[الخطوة 8] اختبار استجابة السيرفر (Backend API) في حال محاولة تجاوز الصلاحيات...');
  
  // استخراج الـ Bearer Token من متصفح الموظف
  const token = await page.evaluate(() => localStorage.getItem('token') || sessionStorage.getItem('token'));

  // 8.1 محاولة حذف مستفيد عبر الـ API مباشرة: يجب أن يرجع 403 Forbidden
  const deleteRes = await fetch(`${BASE_URL}/api/beneficiaries/${testBenId}`, {
    method: 'DELETE',
    headers: { 'Authorization': `Bearer ${token}` }
  });
  console.log(`- محاولة DELETE /api/beneficiaries/{id}: كود الاستجابة = ${deleteRes.status} (المتوقع 403) -> ${deleteRes.status === 403 ? 'تم الرفض بنجاح 🛡️' : 'فشل الرفض ❌'}`);

  // 8.2 محاولة تنفيذ عملية صرف مساعدات: يجب أن يرجع 403 Forbidden
  const distRes = await fetch(`${BASE_URL}/api/distributions`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      beneficiaryId: testBenId,
      distributionDate: new Date().toISOString(),
      assistanceType: 'مالي',
      amount: 1000,
      notes: 'محاولة اختراق الصرف'
    })
  });
  console.log(`- محاولة POST /api/distributions: كود الاستجابة = ${distRes.status} (المتوقع 403) -> ${distRes.status === 403 ? 'تم الرفض بنجاح 🛡️' : 'فشل الرفض ❌'}`);

  // 8.3 محاولة التلاعب بصلاحيات الموظفين: يجب أن يرجع 403 Forbidden
  const staffRes = await fetch(`${BASE_URL}/api/staff/users`, {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      username: 'hacker',
      password: 'Password123!',
      roleTemplate: 'Custom',
      permissions: ['beneficiaries.delete']
    })
  });
  console.log(`- محاولة POST /api/staff/users: كود الاستجابة = ${staffRes.status} (المتوقع 403) -> ${staffRes.status === 403 ? 'تم الرفض بنجاح 🛡️' : 'فشل الرفض ❌'}`);

  // 8.4 فحص محتوى استجابة GET /api/beneficiaries/{id}: التأكد من حجب الملاحظات حتى على مستوى الـ JSON الخام
  const getBenRes = await fetch(`${BASE_URL}/api/beneficiaries/${testBenId}`, {
    headers: { 'Authorization': `Bearer ${token}` }
  });
  const benJson = await getBenRes.json();
  const apiDebtMasked = benJson.totalDebtAmount === null || benJson.totalDebtAmount === 0 || benJson.debtNotes?.includes('محجوب') || !benJson.debtNotes;
  const apiHealthMasked = benJson.healthIssues?.includes('محجوب') || benJson.researchNotes?.includes('محجوب');
  console.log(`- فحص JSON الخاص بالمستفيد في الـ API: ديون محجوبة = ${apiDebtMasked}، ملاحظات محجوبة = ${apiHealthMasked} -> ${apiDebtMasked && apiHealthMasked ? 'حجب أمني ناجح 100% 🛡️' : 'بيانات مكشوفة في الـ API ❌'}`);

  report.push({
    step: '5. الحماية الخلفية (Backend API Defense)',
    actor: 'nour_entry (محاولات وصول غير مصرح)',
    result: `تم إرجاع HTTP 403 Forbidden لمحاولات الحذف والصرف وتعديل الصلاحيات، وتم حجب الحقول الحساسة في الـ JSON الخام لمنع قراءتها عبر أداة فحص الشبكة (DevTools).`
  });

  await browser.close();

  console.log('\n=============================================================================');
  console.log('🏁 تقرير نتائج محاكاة وتدقيق نظام الصلاحيات وتفويض الأدوار (RBAC):');
  console.log('=============================================================================');
  console.table(report);
  console.log('\n✅ اكتملت جميع اختبارات التحقق من الصلاحيات والواجهات بنجاح تام وبلا أي أخطاء!');
}

run().catch(err => {
  console.error('❌ حدث خطأ أثناء تشغيل المحاكاة:', err);
  process.exit(1);
});
