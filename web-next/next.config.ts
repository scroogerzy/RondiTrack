import type { NextConfig } from "next";
import { withSentryConfig } from "@sentry/nextjs";

const trustedServerActionOrigin = new URL(process.env.NEXT_PUBLIC_SITE_URL || "http://localhost:3000").host;

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
  output: "standalone",

  allowedDevOrigins: [
    "repulsive-werewolf-7vjrgqqp9vrv3wr6w-3000.app.github.dev",
  ],

  experimental: {
    proxyClientMaxBodySize: '10mb',
    serverActions: {
      bodySizeLimit: '1mb',
      allowedOrigins: [trustedServerActionOrigin],
    },
  },
};

const sentryBuildOptions = {
  org: process.env.SENTRY_ORG,
  project: process.env.SENTRY_PROJECT,
  authToken: process.env.SENTRY_AUTH_TOKEN,
  silent: !process.env.CI,
  sourcemaps: { deleteSourcemapsAfterUpload: true },
  telemetry: false,
};

export default process.env.SENTRY_AUTH_TOKEN
  ? withSentryConfig(nextConfig, sentryBuildOptions)
  : nextConfig;
