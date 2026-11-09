import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  async headers() {
    return [{
      source: '/bff/:path*',
      headers: [
        { key: 'Cache-Control', value: 'private, no-store' },
        { key: 'X-Robots-Tag', value: 'noindex, nofollow, noarchive' },
        { key: 'X-Content-Type-Options', value: 'nosniff' }
      ]
    }];
  },
  reactCompiler: true,
  // Cache Components keeps uncached user-specific data out of shared caches.
  cacheComponents: true,
  partialPrefetching: true,
  typedRoutes: false,

  allowedDevOrigins: [
    "repulsive-werewolf-7vjrgqqp9vrv3wr6w-3000.app.github.dev",
  ],

  experimental: {
    proxyClientMaxBodySize: '10mb',
    serverActions: {
      bodySizeLimit: '1mb',
      allowedOrigins: [
        "repulsive-werewolf-7vjrgqqp9vrv3wr6w-3000.app.github.dev",
        "localhost:3000",
      ],
    },
  },
};

export default nextConfig;
