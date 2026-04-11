import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  output: 'standalone',
  images: {
    remotePatterns: [
      { protocol: 'https', hostname: '**' },
      { protocol: 'http', hostname: 'localhost', port: '5049', pathname: '/**' },
      { protocol: 'http', hostname: '127.0.0.1', port: '5049', pathname: '/**' },
      { protocol: 'https', hostname: 'shopfinity-api.onrender.com', pathname: '/**' },
    ],
  },
};
export default nextConfig;