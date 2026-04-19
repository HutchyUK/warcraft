import type { NextConfig } from "next";

const apiUrl = process.env.INTERNAL_API_URL ?? process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5000";

const nextConfig: NextConfig = {
  // Required for the Docker runtime stage — produces a minimal standalone server
  output: "standalone",

  // Proxy /api/* through Vercel so cookies are same-site (avoids third-party cookie blocking)
  async rewrites() {
    return [
      {
        source: "/api/:path*",
        destination: `${apiUrl}/api/:path*`,
      },
    ];
  },
};

export default nextConfig;
