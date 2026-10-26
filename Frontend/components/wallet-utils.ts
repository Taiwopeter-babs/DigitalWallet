export type Wallet = {
  id: number;
  userId: string;
  owner: string;
  balanceCents: number;
  createdAt: string;
  updatedAt: string;
};

export type ActivityType = "created" | "funded" | "transfer-out" | "transfer-in";

export type Activity = {
  id: string;
  walletId: number;
  counterpartyId?: number;
  type: ActivityType;
  amountCents: number;
  createdAt: string;
};

export type WalletApiDto = {
  id: number;
  user_id: string;
  balance: number;
  created_at: string;
  updated_at: string;
};

export type ApiResponse<T> = {
  success: boolean;
  data: T | null;
  message: string | null;
  error: Record<string, string[]> | null;
};

const moneyFormat = new Intl.NumberFormat("en-US", {
  style: "currency",
  currency: "NGN",
});

export function formatMoney(cents: number): string {
  return moneyFormat.format(cents / 100);
}

export function activityDescription(entry: Activity, wallets: Wallet[]): string {
  const counterparty = wallets.find((wallet) => wallet.id === entry.counterpartyId);

  switch (entry.type) {
    case "created":
      return "Wallet created";
    case "funded":
      return "Funds added";
    case "transfer-out":
      return `Transfer to ${counterparty?.owner ?? `Wallet #${entry.counterpartyId}`}`;
    case "transfer-in":
      return `Transfer from ${counterparty?.owner ?? `Wallet #${entry.counterpartyId}`}`;
    default:
      return "Wallet activity";
  }
}
