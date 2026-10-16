import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: '.',
  testMatch: '*.spec.ts',
  fullyParallel: false,
  workers: 1,
  timeout: 30_000,
  use: { ...devices['Desktop Chrome'], baseURL: 'http://localhost:4200', trace: 'retain-on-failure', screenshot: 'only-on-failure' },
  reporter: [['list'], ['html', { open: 'never' }]]
});
