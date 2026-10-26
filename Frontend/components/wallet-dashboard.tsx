"use client";

import { useEffect, useMemo, useState, useSyncExternalStore, type FormEvent } from "react";
import { CircleAlert, RefreshCw, ShieldCheck, Wallet as WalletIcon } from "lucide-react";
import { ActivityList } from "@/components/activity-list";
import { OverviewPanel } from "@/components/overview-panel";
import { Toast } from "@/components/toast";
import { TransactionForms } from "@/components/transaction-forms";
import { WalletList } from "@/components/wallet-list";
import { useWalletApi } from "@/hooks/use-wallet-api";
import { type Activity, type Wallet, formatMoney } from "@/components/wallet-utils";

type Notice = {
  kind: "success" | "error";
  message: string;
};

const EMPTY_WALLETS: Wallet[] = [];
const USER_ID_STORAGE_KEY = "digital-wallet-user-id-v1";
let generatedUserId = "";
let userIdPersistenceWarning = false;
const userIdListeners = new Set<() => void>();

function isGuid(value: string) {
  return /^[0-9a-f]{8}-[0-9a-f]{4}-[1-8][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(value);
}

function saveUserId(userId: string) {
  try {
    window.localStorage.setItem(USER_ID_STORAGE_KEY, userId);
  } catch {
    userIdPersistenceWarning = true;
  }
}

function subscribeToUserId(listener: () => void) {
  userIdListeners.add(listener);
  return () => userIdListeners.delete(listener);
}

function getUserIdSnapshot() {
  if (!generatedUserId) {
    let savedUserId: string | null = null;
    try {
      savedUserId = window.localStorage.getItem(USER_ID_STORAGE_KEY);
    } catch {
      userIdPersistenceWarning = true;
    }
    generatedUserId = savedUserId && isGuid(savedUserId) ? savedUserId : window.crypto.randomUUID();
    if (generatedUserId !== savedUserId) saveUserId(generatedUserId);
  }
  return generatedUserId;
}

function getUserIdPersistenceWarning() {
  return userIdPersistenceWarning;
}

function generateNewUserId() {
  generatedUserId = window.crypto.randomUUID();
  saveUserId(generatedUserId);
  userIdListeners.forEach((listener) => listener());
}

function parseAmount(value: string): number | null {
  if (!/^\d+(?:\.\d{1,2})?$/.test(value.trim())) return null;
  const amount = Number(value);
  if (!Number.isFinite(amount) || amount <= 0) return null;
  const cents = Math.round(amount * 100);
  return Number.isSafeInteger(cents) ? cents : null;
}

function errorMessage(error: unknown): string {
  return error instanceof Error ? error.message : "The wallet request failed.";
}

function createActivity(walletId: number, type: Activity["type"], amountCents = 0, counterpartyId?: number): Activity {
  return {
    id: crypto.randomUUID(),
    walletId,
    type,
    amountCents,
    counterpartyId,
    createdAt: new Date().toISOString(),
  };
}

export function WalletDashboard() {
  const userId = useSyncExternalStore(subscribeToUserId, getUserIdSnapshot, () => "");
  const cannotPersistUserId = useSyncExternalStore(subscribeToUserId, getUserIdPersistenceWarning, () => false);
  const [activity, setActivity] = useState<Activity[]>([]);
  const [notice, setNotice] = useState<Notice | null>(null);
  const [fundWalletId, setFundWalletId] = useState("");
  const [fundAmount, setFundAmount] = useState("");
  const [senderWalletId, setSenderWalletId] = useState("");
  const [receiverWalletId, setReceiverWalletId] = useState("");
  const [transferAmount, setTransferAmount] = useState("");
  const { allWalletsQuery, createWallet, fundWallet, transfer, isValidUserId } = useWalletApi();

  const wallets = allWalletsQuery.data ?? EMPTY_WALLETS;

  const walletsById = useMemo(() => [...wallets].sort((first, second) => first.id - second.id), [wallets]);
  
  const totalBalance = useMemo(() => wallets.reduce((total, wallet) => total + wallet.balanceCents, 0), [wallets]);
  
  const selectedFundWalletId = walletsById.some((wallet) => String(wallet.id) === fundWalletId)
    ? fundWalletId
    : String(walletsById[0]?.id ?? "");
  
    const selectedSenderWalletId = walletsById.some((wallet) => String(wallet.id) === senderWalletId)
    ? senderWalletId
    : String(walletsById[0]?.id ?? "");
  
    const selectedReceiverWalletId = walletsById.some(
    (wallet) => String(wallet.id) === receiverWalletId && receiverWalletId !== selectedSenderWalletId,
  )
    ? receiverWalletId
    : String(walletsById.find((wallet) => String(wallet.id) !== selectedSenderWalletId)?.id ?? "");

  useEffect(() => {
    if (!notice) return;
    const timeout = window.setTimeout(() => setNotice(null), 4500);
    return () => window.clearTimeout(timeout);
  }, [notice]);

  function showNotice(kind: Notice["kind"], message: string) {
    setNotice({ kind, message });
  }

  async function handleCreateWallet(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const normalizedUserId = userId.trim();
    if (!isValidUserId(normalizedUserId)) {
      showNotice("error", "Enter a valid user ID (UUID) to create a wallet.");
      return;
    }

    try {
      const wallet = await createWallet.mutateAsync({ userId: normalizedUserId });
      setActivity((current) => [createActivity(wallet.id, "created"), ...current]);
      showNotice("success", `Wallet #${wallet.id} is ready.`);
    } catch (error) {
      showNotice("error", errorMessage(error));
    }
  }

  async function handleFundWallet(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const walletId = Number(selectedFundWalletId);
    const amountCents = parseAmount(fundAmount);
    const wallet = wallets.find((item) => item.id === walletId);
    if (!wallet) {
      showNotice("error", "Choose a wallet to fund.");
      return;
    }
    if (amountCents === null) {
      showNotice("error", "Enter an amount greater than $0 with up to two decimal places.");
      return;
    }

    try {
      await fundWallet.mutateAsync({ walletId, amount: amountCents / 100 });
      setActivity((current) => [createActivity(walletId, "funded", amountCents), ...current]);
      setFundAmount("");
      showNotice("success", `${formatMoney(amountCents)} added to wallet #${walletId}.`);
    } catch (error) {
      showNotice("error", errorMessage(error));
    }
  }

  async function handleTransfer(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    const senderId = Number(selectedSenderWalletId);
    const receiverId = Number(selectedReceiverWalletId);
    const sender = wallets.find((wallet) => wallet.id === senderId);
    const receiver = wallets.find((wallet) => wallet.id === receiverId);
    const amountCents = parseAmount(transferAmount);

    if (!sender || !receiver) {
      showNotice("error", "Choose both a sending wallet and a receiving wallet.");
      return;
    }
    if (senderId === receiverId) {
      showNotice("error", "Choose two different wallets for a transfer.");
      return;
    }
    if (amountCents === null) {
      showNotice("error", "Enter an amount greater than $0 with up to two decimal places.");
      return;
    }
    if (sender.balanceCents < amountCents) {
      showNotice("error", `${sender.owner} does not have enough funds for this transfer.`);
      return;
    }

    try {
      await transfer.mutateAsync({
        senderWalletId: senderId,
        receiverWalletId: receiverId,
        amount: amountCents / 100,
      });
      setActivity((current) => [
        createActivity(senderId, "transfer-out", amountCents, receiverId),
        createActivity(receiverId, "transfer-in", amountCents, senderId),
        ...current,
      ]);
      setTransferAmount("");
      showNotice("success", `${formatMoney(amountCents)} sent to wallet #${receiverId}.`);
    } catch (error) {
      showNotice("error", errorMessage(error));
    }
  }

  function updateSender(value: string) {
    setSenderWalletId(value);
    if (value === receiverWalletId) {
      const alternative = walletsById.find((wallet) => String(wallet.id) !== value);
      setReceiverWalletId(alternative ? String(alternative.id) : "");
    }
  }

  return (
    <div className="app-shell">
      <header className="topbar">
        <a className="brand" href="#top" aria-label="Digital Wallet home">
          <span className="brand-mark">
            <WalletIcon size={19} strokeWidth={2.4} />
          </span>
          <span>
            pocket<span className="brand-accent">wise</span>
          </span>
        </a>
        <div className="topbar-right">
          <span className="demo-badge">
            <span className="demo-dot" /> Connected wallet service
          </span>
          <span className="topbar-divider" />
          <span className="profile-avatar" aria-hidden="true">
            PW
          </span>
          <span className="profile-label">Your workspace</span>
        </div>
      </header>

      <main id="top" className="page-content">
        <div className="welcome-row">
          <div>
            <p className="eyebrow">WALLET OVERVIEW</p>
            <h1>Money, moving simply.</h1>
            <p className="welcome-copy">Balances and wallets are loaded from your wallet service.</p>
          </div>
          <div className="welcome-actions">
            <span className="secure-note">
              <ShieldCheck size={15} /> Connected to your wallet API
            </span>
            <button
              className="reset-button"
              onClick={() => void allWalletsQuery.refetch()}
              type="button"
              disabled={allWalletsQuery.isFetching}
            >
              <RefreshCw size={15} className={allWalletsQuery.isFetching ? "spinning" : ""} />
              Refresh wallets
            </button>
          </div>
        </div>

        {allWalletsQuery.isError && (
          <div className="storage-warning" role="alert">
            <CircleAlert size={17} />
            <span>{errorMessage(allWalletsQuery.error)}</span>
          </div>
        )}

        {userId && !isValidUserId(userId) && (
          <div className="storage-warning" role="alert">
            <CircleAlert size={17} />
            <span>Enter a valid UUID to create a wallet. The all-wallets list is not filtered by user ID.</span>
          </div>
        )}
        {cannotPersistUserId && (
          <div className="storage-warning" role="status">
            <CircleAlert size={17} />
            <span>The generated user ID could not be saved in this browser, so it may change after a reload.</span>
          </div>
        )}

        <OverviewPanel
          totalBalance={totalBalance}
          walletCount={wallets.length}
          movementCount={activity.filter((entry) => entry.type !== "created").length}
        />

        <section className="workspace-grid" aria-label="Manage wallets">
          <div className="main-column">
            {allWalletsQuery.isPending ? (
              <section className="panel wallets-panel" aria-live="polite">
                Loading wallets…
              </section>
            ) : (
              <WalletList wallets={walletsById} />
            )}
            <ActivityList wallets={wallets} activity={activity.slice(0, 6)} />
          </div>

          <div className="actions-column">
            <TransactionForms
              userId={userId}
              fundWalletId={selectedFundWalletId}
              fundAmount={fundAmount}
              senderWalletId={selectedSenderWalletId}
              receiverWalletId={selectedReceiverWalletId}
              transferAmount={transferAmount}
              wallets={walletsById}
              isValidUserId={isValidUserId(userId)}
              isCreating={createWallet.isPending}
              isFunding={fundWallet.isPending}
              isTransferring={transfer.isPending}
              onGenerateUserId={generateNewUserId}
              setFundWalletId={setFundWalletId}
              setFundAmount={setFundAmount}
              setSenderWalletId={updateSender}
              setReceiverWalletId={setReceiverWalletId}
              setTransferAmount={setTransferAmount}
              onCreateWallet={handleCreateWallet}
              onFundWallet={handleFundWallet}
              onTransfer={handleTransfer}
            />
          </div>
        </section>

        <footer className="page-footer">
          <span>
            pocketwise <span className="footer-dot">·</span> Wallet service dashboard
          </span>
          <span>
            <ShieldCheck size={14} /> Transactions are sent to the configured API
          </span>
        </footer>
      </main>

      {notice && <Toast notice={notice} onDismiss={() => setNotice(null)} />}
    </div>
  );
}
