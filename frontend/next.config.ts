import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // Required for the Docker runtime stage — produces a minimal standalone server
  output: "standalone",
};

export default nextConfig;
