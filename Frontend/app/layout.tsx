import type { Metadata } from "next";
import type { ReactNode } from "react";
import { QueryProvider } from "@/components/query-provider";
import "./globals.css";

export const metadata: Metadata = {
  title: "Digital Wallet — Simple money movement",
  description: "A modern wallet dashboard for creating wallets, funding them, and transferring balances.",
};

export default function RootLayout({ children }: { children: ReactNode }) {
  return (
    <html lang="en">
      <body>
        <QueryProvider>{children}</QueryProvider>
      </body>
    </html>
  );
}
