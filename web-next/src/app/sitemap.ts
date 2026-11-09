import type { MetadataRoute } from 'next';

export default function sitemap(): MetadataRoute.Sitemap {
  const base = process.env.NEXT_PUBLIC_SITE_URL;
  if (!base) return [];
  return ['/login', '/register'].map(path => ({
    url: new URL(path, base).toString(),
    changeFrequency: 'monthly',
    priority: path === '/login' ? 0.7 : 0.5
  }));
}
