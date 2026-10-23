import assert from 'node:assert/strict';
import fs from 'node:fs';

const fixture = JSON.parse(fs.readFileSync(new URL('./AUDIT_LOAD_RESULT_2026-10-23.json', import.meta.url)));
const password = process.env.CHARITY_QA_PASSWORD || 'QaDemo@2026';
const review = { database: fixture.database, charities: [], issues: [] };

async function api(path, token, method = 'GET', body) {
  const response = await fetch(fixture.base + path, {
    method,
    headers: { ...(token ? { Authorization: `Bearer ${token}` } : {}), ...(body ? { 'Content-Type': 'application/json' } : {}) },
    body: body ? JSON.stringify(body) : undefined,
  });
  assert.equal(response.status, 200, `${path}: ${response.status}`);
  return response.json();
}

for (const charity of fixture.charities) {
  const token = (await api('/api/auth/login', null, 'POST', { username: charity.manager, password })).token;
  const first = await api('/api/audit-logs?page=1&pageSize=100', token);
  const logs = [...first.items];
  for (let page = 2; logs.length < first.totalCount; page++) {
    logs.push(...(await api(`/api/audit-logs?page=${page}&pageSize=100`, token)).items);
  }
  const missing = logs.filter(x => !x.userId || !x.username || !x.action || !x.entityType || !x.entityId || !x.occurredAt);
  const wrongActor = logs.filter(x => ![charity.manager, ...Object.values(charity.staff)].includes(x.username));
  const actors = Object.fromEntries(Object.entries(Object.groupBy(logs, x => x.username)).map(([actor, events]) => [actor, events.length]));
  const entities = Object.fromEntries(Object.entries(Object.groupBy(logs, x => x.entityType)).map(([entity, events]) => [entity, events.length]));
  const actions = Object.fromEntries(Object.entries(Object.groupBy(logs, x => x.action)).map(([action, events]) => [action, events.length]));
  review.charities.push({ charityId: charity.charityId, manager: charity.manager, logCount: logs.length, actors, entities, actions,
    missingRequiredFields: missing.length, unexpectedActors: wrongActor.length });
  if (missing.length) review.issues.push(`${charity.manager}: ${missing.length} incomplete audit events`);
  if (wrongActor.length) review.issues.push(`${charity.manager}: ${wrongActor.length} unexpected actors`);
}
fs.writeFileSync(new URL('./AUDIT_LOG_REVIEW_2026-10-23.json', import.meta.url), JSON.stringify(review, null, 2));
console.log(JSON.stringify(review, null, 2));
if (review.issues.length) process.exitCode = 1;
