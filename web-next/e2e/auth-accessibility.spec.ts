import { test, expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';

test.describe('Authentication and privacy', () => {
  test('login page renders accessible labelled fields', async ({ page }) => {
    await page.goto('/login');

    await expect(
      page.getByRole('heading', { name: 'Sign in to RondiTrack' })
    ).toBeVisible();

    await expect(page.getByLabel('Email')).toBeVisible();
    await expect(page.getByLabel('Password')).toBeVisible();
    await expect(
      page.getByRole('button', { name: 'Sign in' })
    ).toBeVisible();
  });

  test('registration page renders accessible labelled fields', async ({ page }) => {
    await page.goto('/register');

    await expect(
      page.getByRole('heading', { name: 'Create your RondiTrack account' })
    ).toBeVisible();

    await expect(page.getByLabel('Full name')).toBeVisible();
    await expect(page.getByLabel('Email')).toBeVisible();
    await expect(page.getByLabel('Password')).toBeVisible();
  });

  test('signed-out visitor is redirected from activity', async ({ page }) => {
    await page.goto('/activity');

    await expect(page).toHaveURL(/\/login\?next=/);
    await expect(
      page.getByRole('heading', { name: 'Sign in to RondiTrack' })
    ).toBeVisible();
  });

  for (const protectedPath of [
    '/dashboard/member',
    '/dashboard/treasurer',
    '/stokvels',
    '/stokvels/11111111-1111-4111-8111-111111111111'
  ]) {
    test(`signed-out visitor cannot open ${protectedPath}`, async ({ page }) => {
      await page.goto(protectedPath);
      await expect(page).toHaveURL(/\/login\?next=/);
      const destination = new URL(page.url());
      expect(destination.searchParams.get('next')).toBe(protectedPath);
    });
  }

  test('robots and sitemap do not advertise private application routes', async ({ request }) => {
    const [robots, sitemap] = await Promise.all([
      request.get('/robots.txt'),
      request.get('/sitemap.xml')
    ]);
    expect(robots.ok()).toBeTruthy();
    expect(await robots.text()).toContain('Allow: /');
    expect(sitemap.ok()).toBeTruthy();
    const xml = await sitemap.text();
    expect(xml).not.toContain('/dashboard');
    expect(xml).not.toContain('/stokvels');
    expect(xml).not.toContain('/activity');
  });

  test('BFF activity returns JSON 401 without a session', async ({ request }) => {
    const response = await request.get('/bff/activity');

    expect(response.status()).toBe(401);
    expect(response.headers()['content-type']).toContain('application/json');
    expect(response.headers()['cache-control']).toContain('no-store');
    expect(response.headers()['x-robots-tag']).toContain('noindex');

    const body = await response.json();
    expect(body.title).toBe('Your session has ended.');
  });

  test('login page has no automatically detected axe violations', async ({ page }) => {
    await page.goto('/login');

    const results = await new AxeBuilder({ page }).analyze();

    expect(
      results.violations.map(violation => ({
        id: violation.id,
        impact: violation.impact,
        description: violation.description,
        nodes: violation.nodes.length
      }))
    ).toEqual([]);
  });

  test('registration page has no automatically detected axe violations', async ({ page }) => {
    await page.goto('/register');

    const results = await new AxeBuilder({ page }).analyze();

    expect(
      results.violations.map(violation => ({
        id: violation.id,
        impact: violation.impact,
        description: violation.description,
        nodes: violation.nodes.length
      }))
    ).toEqual([]);
  });
});
