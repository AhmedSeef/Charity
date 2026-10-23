import assert from 'node:assert/strict';
import fs from 'node:fs';

const base = process.env.CHARITY_QA_URL || 'http://localhost:5073';
const run = new Date().toISOString().replace(/\D/g, '').slice(0, 14);
const password = process.env.CHARITY_QA_PASSWORD || 'QaDemo@2026';
const result = { run, database: 'CharityQA_AuditDemo_20261023', base, charities: [], checks: {} };

async function request(path, token, method = 'GET', body, expected = [200]) {
  const response = await fetch(base + path, {
    method,
    headers: { ...(token ? { Authorization: `Bearer ${token}` } : {}), ...(body === undefined ? {} : { 'Content-Type': 'application/json' }) },
    body: body === undefined ? undefined : JSON.stringify(body),
  });
  const raw = await response.text();
  let data;
  try { data = raw ? JSON.parse(raw) : null; } catch { data = raw; }
  assert.ok(expected.includes(response.status), `${method} ${path}: expected ${expected}, got ${response.status}: ${raw.slice(0, 500)}`);
  return { status: response.status, data };
}

async function login(username, userPassword = password) {
  return (await request('/api/auth/login', null, 'POST', { username, password: userPassword })).data.token;
}

async function allLogs(token) {
  const first = (await request('/api/audit-logs?page=1&pageSize=100', token)).data;
  const items = [...first.items];
  for (let page = 2; items.length < first.totalCount; page++) {
    items.push(...(await request(`/api/audit-logs?page=${page}&pageSize=100`, token)).data.items);
  }
  return items;
}

try {
  const admin = await login('admin', process.env.CHARITY_QA_ADMIN_PASSWORD || 'admin');
  const definitions = [
    { label: 'الرسالة', districtId: 'dist_motobas', governorateId: 'gov_kfs' },
    { label: 'الأمل', districtId: 'dist_desouk', governorateId: 'gov_kfs' },
  ];
  const permissions = {
    researcher: ['beneficiaries.view', 'beneficiaries.view_full', 'beneficiaries.create', 'beneficiaries.edit'],
    warehouse: ['inventory.view', 'inventory.manage'],
    donations: ['donations.view', 'donations.create'],
    disburser: ['beneficiaries.view', 'disbursement.view', 'disbursement.execute'],
    observer: [],
  };
  const contexts = [];

  for (const [tenantIndex, def] of definitions.entries()) {
    const managerName = `qa_${run}_${tenantIndex + 1}_manager`;
    const charity = (await request('/api/charities', admin, 'POST', {
      name: `جمعية ${def.label} لاختبار التدقيق ${run}`,
      roleDesc: 'بيانات QA مستقلة لاختبار الصلاحيات وسجل العمليات',
      phone: `0100000${String(tenantIndex + 1).padStart(4, '0')}`,
      address: `كفر الشيخ - ${def.label}`,
      license: `QA-${run}-${tenantIndex + 1}`,
      governorateId: def.governorateId,
      districtId: def.districtId,
      managerUsername: managerName,
      managerPassword: password,
      managerFullName: `مدير جمعية ${def.label} QA`,
    }, [201])).data;
    const manager = await login(managerName);
    const staff = {};
    for (const [role, rolePermissions] of Object.entries(permissions)) {
      const username = `qa_${run}_${tenantIndex + 1}_${role}`;
      const created = (await request('/api/staff/users', manager, 'POST', {
        username, password, fullName: `${role} ${def.label} QA`, permissions: rolePermissions,
      }, [201])).data;
      staff[role] = { username, id: created.id, token: await login(username), permissions: rolePermissions };
    }
    const category = (await request('/api/expense-categories', manager, 'POST', {
      name: `مساعدات نقدية ${run} ${def.label}`,
      description: 'بند تجربة سجلات الصرف',
    }, [201])).data;
    const edited = (await request('/api/expense-categories', manager, 'POST', { name: `بند تعديل وحذف ${run} ${def.label}` }, [201])).data;
    await request(`/api/expense-categories/${edited.id}`, manager, 'PUT', { name: `بند بعد التعديل ${run}`, isActive: true });
    await request(`/api/expense-categories/${edited.id}`, manager, 'DELETE', undefined, [204]);

    const families = [];
    for (let i = 0; i < 12; i++) {
      const nationalId = `278010101${String((Number(run.slice(-5)) + tenantIndex * 100 + i) % 100000).padStart(5, '0')}`;
      families.push((await request('/api/beneficiaries', staff.researcher.token, 'POST', {
        name: `أسرة ${def.label} رقم ${i + 1} - تجربة ${run}`,
        nationalId, phone: `0101${String(i + tenantIndex * 100).padStart(7, '0')}`,
        address: `${def.label} - شارع التعاون - منزل ${i + 1}`,
        researchNotes: 'ملف تجريبي للتدقيق والصرف',
      }, [201])).data);
    }
    const items = [];
    for (let i = 0; i < 6; i++) {
      items.push((await request('/api/inventory', staff.warehouse.token, 'POST', {
        code: `QA-${run}-${tenantIndex + 1}-${i + 1}`,
        itemName: ['كرتونة غذاء', 'بطانية', 'دواء', 'ملابس', 'شنطة مدرسة', 'مواد نظافة'][i] + ` ${def.label}`,
        kind: 'صنف ملموس', unit: 'قطعة', quantity: 40, minAlert: 5,
      }, [201])).data);
    }
    for (let i = 0; i < 12; i++) {
      const donation = i % 2 === 0
        ? { donorName: `متبرع نقدي ${i + 1} ${def.label}`, donationKind: 'نقدي', amount: 1500 + i * 100 }
        : { donorName: `متبرع عيني ${i + 1} ${def.label}`, donationKind: 'عيني', inventoryItemId: items[i % items.length].id, quantity: 4 };
      await request('/api/donations', staff.donations.token, 'POST', donation);
    }
    for (let i = 0; i < 12; i++) {
      const aid = i % 2 === 0
        ? { beneficiaryId: families[i].id, kind: 'إعانة نقدية', expenseCategoryId: category.id, amount: 125 + i }
        : { beneficiaryId: families[i].id, kind: 'صنف ملموس', inventoryItemId: items[i % items.length].id, quantity: 2 };
      await request('/api/distributions', staff.disburser.token, 'POST', aid);
    }

    const denied = [
      await request('/api/donations', staff.observer.token, 'GET', undefined, [403]),
      await request('/api/audit-logs', staff.observer.token, 'GET', undefined, [403]),
      await request('/api/inventory', staff.observer.token, 'GET', undefined, [403]),
      await request('/api/beneficiaries', staff.observer.token, 'GET', undefined, [403]),
      await request('/api/beneficiaries', staff.donations.token, 'POST', { name: 'محاولة غير مفوضة' }, [403]),
    ];
    const logs = await allLogs(manager);
    for (const role of ['researcher', 'warehouse', 'donations', 'disburser']) {
      assert.ok(logs.some(x => x.username === staff[role].username && x.action === 'Create'), `Missing Create log for ${role}`);
    }
    const categoryEvents = logs.filter(x => x.entityId === edited.id).map(x => x.action).sort();
    assert.deepEqual(categoryEvents, ['Create', 'Delete', 'Update']);
    const filtered = (await request(`/api/audit-logs?name=${staff.researcher.username}&action=Create&pageSize=100`, manager)).data;
    assert.ok(filtered.items.length >= 12);
    assert.ok(filtered.items.every(x => x.username === staff.researcher.username && x.action === 'Create'));

    const row = { charityId: charity.id, charityName: charity.name, manager: managerName,
      staff: Object.fromEntries(Object.entries(staff).map(([role, x]) => [role, x.username])),
      created: { staffUsers: 5, families: families.length, inventoryItems: items.length, donations: 12, distributions: 12 },
      auditLogCount: logs.length, auditActions: Object.groupBy(logs, x => x.action),
      deniedChecks: denied.length, categoryAudit: categoryEvents };
    row.auditActions = Object.fromEntries(Object.entries(row.auditActions).map(([action, values]) => [action, values.length]));
    result.charities.push(row);
    contexts.push({ manager, staff, families, items, category, logs, row });
    console.log(`Completed ${charity.name}: ${logs.length} audit events`);
  }

  for (let i = 0; i < 2; i++) {
    const own = contexts[i], other = contexts[1 - i];
    const visibleItems = (await request('/api/inventory', own.manager)).data;
    const visibleDonations = (await request('/api/donations', own.manager)).data;
    assert.ok(visibleItems.some(x => x.id === own.items[0].id));
    assert.ok(!visibleItems.some(x => x.id === other.items[0].id));
    assert.ok(visibleDonations.length >= 12);
    assert.ok(!visibleDonations.some(x => x.charityId === other.row.charityId));
    assert.ok(!own.logs.some(x => other.logs.some(y => y.id === x.id)));
    await request(`/api/inventory/${other.items[0].id}`, own.manager, 'PUT', {
      itemName: 'محاولة تعديل عبر جمعية أخرى', kind: 'صنف ملموس', unit: 'قطعة', quantity: 99,
    }, [403]);
    await request(`/api/beneficiaries/${other.families[0].id}`, own.manager, 'PUT', { name: 'محاولة تعديل عبر جمعية أخرى' }, [403]);
    const sharedFamilies = (await request('/api/beneficiaries', own.manager)).data;
    assert.ok(sharedFamilies.some(x => x.id === other.families[0].id));
  }
  result.checks.crossCharityIsolation = 'passed';

  for (const context of contexts) {
    const donationUser = context.staff.donations;
    await request(`/api/staff/users/${donationUser.id}/permissions`, context.manager, 'PUT', { roleTemplate: 'Custom', permissions: [] });
    await request('/api/donations', donationUser.token, 'GET', undefined, [401]);
    const renewed = await login(donationUser.username);
    await request('/api/donations', renewed, 'GET', undefined, [403]);
  }
  result.checks.permissionRevocation = 'passed';
  result.checks.staffRestrictions = 'passed';
  result.checks.auditActorAndActionFilters = 'passed';
  result.status = 'passed';
} catch (error) {
  result.status = 'failed';
  result.error = String(error.stack || error);
  process.exitCode = 1;
} finally {
  fs.writeFileSync(new URL('./AUDIT_LOAD_RESULT_2026-10-23.json', import.meta.url), JSON.stringify(result, null, 2));
  console.log(JSON.stringify({ status: result.status, error: result.error, charities: result.charities.map(x => ({ name: x.charityName, auditLogCount: x.auditLogCount })) }, null, 2));
}
