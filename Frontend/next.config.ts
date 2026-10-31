import { NextConfig } from "next";
import { getBaseApiUrl } from "./utils/constants";

const nextConfig: NextConfig = {
  reactStrictMode: true,
  output: "standalone",
  async rewrites() {
    return [
      {
        source: "/digital-wallet-api/:path*",
        // 2. Next.js server forwards it to your secure Key Vault variable:
        destination: `${getBaseApiUrl()}/:path*`,
      },
    ];
  },
};

export default nextConfig;
