import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  reactCompiler: true,
  cacheComponents: false,
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
