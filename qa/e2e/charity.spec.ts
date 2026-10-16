import { test, expect, type Page } from '@playwright/test';

const realApi = process.env.CHARITY_E2E_API ?? 'http://localhost:5074';

async function connectRealBackend(page: Page) {
  await page.route('http://localhost:5072/api/**', async route => {
    const target = route.request().url().replace('http://localhost:5072', realApi);
    const response = await route.fetch({ url: target });
    await route.fulfill({ response });
  });
}

async function login(page: Page, username: string, password: string) {
  await page.goto('/login');
  await page.locator('input[name="username"]').fill(username);
  await page.locator('input[name="password"]').fill(password);
  await page.getByRole('button', { name: 'دخول للنظام' }).click();
  await expect(page).toHaveURL(/\/dashboard$/);
}

test.beforeEach(async ({ page }) => connectRealBackend(page));

test('real SQL identity rejects invalid credentials', async ({ page }) => {
  await page.goto('/login');
  await page.locator('input[name="username"]').fill('admin');
  await page.locator('input[name="password"]').fill('admin');
  await page.getByRole('button', { name: 'دخول للنظام' }).click();
  await expect(page.getByText('اسم المستخدم أو كلمة المرور غير صحيحة')).toBeVisible();
  await expect(page).toHaveURL(/\/login$/);
});

test('admin signs in, sees persisted representative data, and refresh keeps session', async ({ page }) => {
  await login(page, 'admin', 'Admin@123');
  await expect(page.getByText('جمعية أبواب الخير').first()).toBeVisible();
  await page.getByRole('link', { name: /إدارة المستفيدين/ }).click();
  await expect(page.getByText('هدى عبد الله علي')).toBeVisible();
  await page.reload();
  await expect(page.getByText('هدى عبد الله علي')).toBeVisible();
  const token = await page.evaluate(() => localStorage.getItem('token'));
  expect(token).toBeTruthy();
  const response = await page.request.get(`${realApi}/api/auth/me`, { headers: { Authorization: `Bearer ${token}` } });
  expect(response.ok()).toBeTruthy();
  expect((await response.json()).charityId).toBe('c1');
});

test('another charity sees shared families and only its own stock and donations', async ({ page }) => {
  await login(page, 'amal', 'Amal@123');
  const token = await page.evaluate(() => localStorage.getItem('token'));
  const headers = { Authorization: `Bearer ${token}` };
  const stock = await (await page.request.get(`${realApi}/api/inventory`, { headers })).json();
  const donations = await (await page.request.get(`${realApi}/api/donations`, { headers })).json();
  expect(stock.length).toBeGreaterThan(0);
  expect(stock.every((item: any) => item.charityId === 'c2')).toBe(true);
  expect(donations.every((donation: any) => donation.charityId === 'c2')).toBe(true);
  await page.getByRole('link', { name: /إدارة المستفيدين/ }).click();
  await expect(page.getByText('هدى عبد الله علي')).toBeVisible();
});

test('cash donation entered in the UI remains in SQL after page reload', async ({ page }) => {
  await login(page, 'admin', 'Admin@123');
  await page.getByRole('link', { name: /إدارة التبرعات/ }).click();
  const donor = `متبرع اختبار ${Date.now()}`;
  await page.locator('input[name="donorName"]').fill(donor);
  await page.locator('input[name="amount"]').fill('275');
  await page.locator('form button[type="submit"]').click();
  await expect(page.getByText(donor)).toBeVisible();
  await page.reload();
  await expect(page.getByText(donor)).toBeVisible();
  const token = await page.evaluate(() => localStorage.getItem('token'));
  const result = await page.request.get(`${realApi}/api/donations`, { headers: { Authorization: `Bearer ${token}` } });
  expect((await result.json()).some((d: any) => d.donorName === donor && d.amount === 275)).toBe(true);
});
