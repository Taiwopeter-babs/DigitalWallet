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

export type UserActivity = {
  id: number;
  userId: string;
  transactionId: number;
  walletId: number;
  amountCents: number;
  eventType: "wallet-created" | "wallet-funded" | "transfer-completed" | "transfer-failed";
  timestamp: string;
};

export type UserActivityApiDto = {
  id: number;
  user_id: string;
  transaction_id: number;
  wallet_id: number;
  amount: number;
  event_type: number | string;
  timestamp: string;
};

export type WalletTransaction = {
  id: number;
  walletId: number;
  amountCents: number;
  status: "Completed" | "Failed";
  type: "Fund" | "TransferOut" | "TransferIn";
  timestamp: string;
};

export type WalletTransactionApiDto = {
  id: number;
  wallet_id: number;
  amount: number;
  status: string;
  type: string;
  timestamp: string;
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
