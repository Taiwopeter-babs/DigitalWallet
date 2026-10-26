"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import type { ApiResponse, Wallet, WalletApiDto } from "@/components/wallet-utils";

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

export function useWalletApi() {
  const queryClient = useQueryClient();
  const queryKey = ["wallets"];

  const allWalletsQuery = useQuery({
    queryKey,
    queryFn: async ({ signal }) => {
      const data = await request<WalletApiDto[]>("/api/wallets/", { signal });
      return data.map(walletFromDto);
    },
  });

  const createWallet = useMutation({
    mutationFn: ({ userId }: CreateWalletInput) =>
      post<WalletApiDto>("/api/wallets/", { user_id: userId }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey }),
  });

  const fundWallet = useMutation({
    mutationFn: ({ walletId, amount }: FundWalletInput) =>
      post<WalletApiDto>(`/api/wallets/${walletId}/fund`, {
        amount,
        walletId,
      }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey }),
  });

  const transfer = useMutation({
    mutationFn: ({ senderWalletId, receiverWalletId, amount }: TransferInput) =>
      post<WalletApiDto>(`/api/wallets/${senderWalletId}/transfer`, {
        sender_wallet_id: senderWalletId,
        receiver_wallet_id: receiverWalletId,
        amount,
      }),
    onSuccess: () => queryClient.invalidateQueries({ queryKey }),
  });

  return {
    allWalletsQuery,
    createWallet,
    fundWallet,
    transfer,
    isValidUserId: isGuid,
  };
}
