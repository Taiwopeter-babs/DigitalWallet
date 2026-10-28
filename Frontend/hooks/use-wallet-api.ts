"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import type {
  ApiResponse,
  UserActivity,
  UserActivityApiDto,
  Wallet,
  WalletApiDto,
  WalletTransaction,
  WalletTransactionApiDto,
} from "@/components/wallet-utils";

const API_BASE_URL = (process.env.NEXT_PUBLIC_WALLET_API_URL ?? "http://localhost:5001").replace(/\/$/, "");

type CreateWalletInput = {
  userId: string;
};

type FundWalletInput = {
  walletId: number;
  amount: number;
};

type TransferInput = {
  senderWalletId: number;
  receiverWalletId: number;
  amount: number;
};

export function isGuid(value: string): boolean {
  return /^[0-9a-f]{8}-[0-9a-f]{4}-[1-8][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(value);
}

function walletFromDto(dto: WalletApiDto): Wallet {
  if (
    !Number.isInteger(dto.id) ||
    !Number.isFinite(dto.balance) ||
    !Number.isFinite(dto.balance * 100) ||
    typeof dto.user_id !== "string"
  ) {
    throw new Error("The wallet service returned an invalid wallet.");
  }

  return {
    id: dto.id,
    userId: dto.user_id,
    owner: `User ${dto.user_id.slice(0, 8)}`,
    balanceCents: Math.round(dto.balance * 100),
    createdAt: dto.created_at,
    updatedAt: dto.updated_at,
  };
}

function activityFromDto(dto: UserActivityApiDto): UserActivity {
  const eventTypes = {
    0: "wallet-created",
    1: "wallet-funded",
    2: "transfer-completed",
    3: "transfer-failed",
    WalletCreated: "wallet-created",
    WalletFunded: "wallet-funded",
    TransferCompleted: "transfer-completed",
    TransferFailed: "transfer-failed",
  } as const;
  const eventType = eventTypes[dto.event_type as keyof typeof eventTypes];

  if (
    !Number.isInteger(dto.id) ||
    !Number.isInteger(dto.wallet_id) ||
    typeof dto.user_id !== "string" ||
    typeof eventType !== "string" ||
    typeof dto.timestamp !== "string" ||
    Number.isNaN(Date.parse(dto.timestamp))
  ) {
    throw new Error("The wallet service returned an invalid activity event.");
  }

  return {
    id: dto.id,
    userId: dto.user_id,
    transactionId: dto.transaction_id,
    walletId: dto.wallet_id,
    amountCents: Math.round(dto.amount * 100),
    eventType,
    timestamp: dto.timestamp,
  };
}

function walletTransactionFromDto(dto: WalletTransactionApiDto): WalletTransaction {
  if (
    !Number.isInteger(dto.id) ||
    !Number.isInteger(dto.wallet_id) ||
    !Number.isFinite(dto.amount) ||
    (dto.status !== "Completed" && dto.status !== "Failed") ||
    (dto.type !== "Fund" && dto.type !== "TransferOut" && dto.type !== "TransferIn") ||
    typeof dto.timestamp !== "string" ||
    Number.isNaN(Date.parse(dto.timestamp))
  ) {
    throw new Error("The wallet service returned an invalid wallet transaction.");
  }

  return {
    id: dto.id,
    walletId: dto.wallet_id,
    amountCents: Math.round(dto.amount * 100),
    status: dto.status,
    type: dto.type,
    timestamp: dto.timestamp,
  };
}

function responseError<T>(body: ApiResponse<T> | null, status: number): string {
  if (body?.message) return body.message;
  const validationErrors = body?.error ? Object.values(body.error).flat().join(" ") : "";
  if (validationErrors) return validationErrors;
  return `Wallet service request failed (${status}).`;
}

async function request<T>(path: string, init?: RequestInit): Promise<T> {
  let response: Response;
  try {
    response = await fetch(`${API_BASE_URL}${path}`, {
      ...init,
      headers: {
        Accept: "application/json",
        ...(init?.body ? { "Content-Type": "application/json" } : {}),
        ...init?.headers,
      },
    });
  } catch (error) {
    if (error instanceof Error && error.name === "AbortError") throw error;
    throw new Error(`Could not reach the wallet service at ${API_BASE_URL}.`);
  }

  let body: ApiResponse<T> | null = null;
  try {
    body = (await response.json()) as ApiResponse<T>;
  } catch {
    if (response.ok) throw new Error("The wallet service returned an unreadable response.");
  }

  if (!response.ok || !body?.success || body.data === null) {
    throw new Error(responseError(body, response.status));
  }

  return body.data;
}

async function post<T>(path: string, body: unknown): Promise<T> {
  return request<T>(path, {
    method: "POST",
    body: JSON.stringify(body),
  });
}

export function useWalletApi(userId: string, historyUserId = userId, transactionsUserId = historyUserId) {
  const queryClient = useQueryClient();
  const queryKey = ["wallets"];

  const allWalletsQuery = useQuery({
    queryKey,
    queryFn: async ({ signal }) => {
      const data = await request<WalletApiDto[]>("/api/wallets/", { signal });
      return data.map(walletFromDto);
    },
  });
  console.log(userId)

  const userActivityQuery = useQuery({
    queryKey: ["user-activity", historyUserId],
    enabled: isGuid(historyUserId),
    queryFn: async ({ signal }) => {
      const data = await request<UserActivityApiDto[]>(`/api/users/${encodeURIComponent(historyUserId)}/activity`, {
        signal,
      });
      return data
        .map(activityFromDto)
        .sort((first, second) => Date.parse(second.timestamp) - Date.parse(first.timestamp));
    },
  });

  const userTransactionsQuery = useQuery({
    queryKey: ["user-transactions", transactionsUserId],
    enabled: isGuid(transactionsUserId),
    queryFn: async ({ signal }) => {
      const data = await request<WalletTransactionApiDto[]>(
        `/api/users/${encodeURIComponent(transactionsUserId)}/transactions`,
        { signal },
      );
      return data
        .map(walletTransactionFromDto)
        .sort((first, second) => Date.parse(second.timestamp) - Date.parse(first.timestamp));
    },
  });

  function refreshWalletData() {
    return Promise.all([
      queryClient.invalidateQueries({ queryKey }),
      queryClient.invalidateQueries({ queryKey: ["user-activity"] }),
      queryClient.invalidateQueries({ queryKey: ["user-transactions"] }),
    ]);
  }

  const createWallet = useMutation({
    mutationFn: ({ userId }: CreateWalletInput) => post<WalletApiDto>("/api/wallets/", { user_id: userId }),
    onSuccess: refreshWalletData,
  });

  const fundWallet = useMutation({
    mutationFn: ({ walletId, amount }: FundWalletInput) =>
      post<WalletApiDto>(`/api/wallets/${walletId}/fund`, {
        amount,
        walletId,
      }),
    onSuccess: refreshWalletData,
  });

  const transfer = useMutation({
    mutationFn: ({ senderWalletId, receiverWalletId, amount }: TransferInput) =>
      post<WalletApiDto>(`/api/wallets/${senderWalletId}/transfer`, {
        sender_wallet_id: senderWalletId,
        receiver_wallet_id: receiverWalletId,
        amount,
      }),
    onSuccess: refreshWalletData,
  });

  return {
    allWalletsQuery,
    userActivityQuery,
    userTransactionsQuery,
    createWallet,
    fundWallet,
    transfer,
    isValidUserId: isGuid,
  };
}
