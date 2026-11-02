import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  reactCompiler: true,
  // Cache Components keeps uncached user-specific data out of shared caches.
  cacheComponents: true,
  partialPrefetching: true,
  typedRoutes: false,

  allowedDevOrigins: [
    "repulsive-werewolf-7vjrgqqp9vrv3wr6w-3000.app.github.dev",
  ],

  experimental: {
    serverActions: {
      allowedOrigins: [
        "repulsive-werewolf-7vjrgqqp9vrv3wr6w-3000.app.github.dev",
        "localhost:3000",
      ],
    },
  },
};

export default nextConfig;
