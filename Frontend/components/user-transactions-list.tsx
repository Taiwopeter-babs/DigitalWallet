import { ArrowDownLeft, ArrowLeftRight, ArrowUpRight, Clock3, RefreshCw } from "lucide-react";
import type { FormEvent } from "react";
import { formatMoney, type WalletTransaction } from "@/components/wallet-utils";

type UserTransactionsListProps = {
  transactions: WalletTransaction[];
  availableUserIds: string[];
  selectedUserId: string;
  onSelectedUserIdChange: (value: string) => void;
  onLoadUser: (event: FormEvent<HTMLFormElement>) => void;
  isLoading: boolean;
  isError: boolean;
  errorMessage?: string;
  isRefreshing: boolean;
  onRefresh: () => void;
};

function transactionLabel(type: WalletTransaction["type"]): string {
  switch (type) {
    case "Fund":
      return "Wallet funded";
    case "TransferIn":
      return "Transfer received";
    case "TransferOut":
      return "Transfer sent";
  }
}

export function UserTransactionsList({
  transactions,
  availableUserIds,
  selectedUserId,
  onSelectedUserIdChange,
  onLoadUser,
  isLoading,
  isError,
  errorMessage,
  isRefreshing,
  onRefresh,
}: UserTransactionsListProps) {
  return (
    <section className="panel activity-panel">
      <div className="section-heading">
        <div>
          <p className="eyebrow">WALLET HISTORY</p>
          <h2>All wallet transactions</h2>
        </div>
        <button className="activity-refresh" type="button" onClick={onRefresh} disabled={isRefreshing}>
          <RefreshCw size={14} className={isRefreshing ? "spinning" : ""} /> Refresh
        </button>
      </div>

      <form className="history-user-picker" onSubmit={onLoadUser}>
        <label htmlFor="transactions-user-id">Select a user ID</label>
        <div className="history-user-controls">
          <select
            id="transactions-user-id"
            onChange={(event) => onSelectedUserIdChange(event.target.value)}
            value={selectedUserId}
            disabled={!availableUserIds.length}
          >
            {availableUserIds.length ? (
              availableUserIds.map((userId) => (
                <option key={userId} value={userId}>{userId}</option>
              ))
            ) : (
              <option value="">No user IDs available</option>
            )}
          </select>
          <button className="secondary-button" type="submit" disabled={!selectedUserId}>
            Load user
          </button>
        </div>
      </form>

      {isLoading ? (
        <div className="empty-state compact-empty" aria-live="polite">Loading wallet transactions…</div>
      ) : isError ? (
        <div className="activity-error" role="alert">{errorMessage ?? "Could not load wallet transactions."}</div>
      ) : transactions.length ? (
        <div className="activity-list">
          {transactions.map((transaction) => {
            const isIncoming = transaction.status !== "Failed" && transaction.type !== "TransferOut";
            const icon = transaction.type === "Fund"
              ? <ArrowDownLeft size={17} />
              : <ArrowLeftRight size={17} />;

            return (
              <div className="activity-row" key={transaction.id}>
                <div className={`activity-icon ${transaction.status === "Failed" ? "activity-out" : isIncoming ? "activity-in" : "activity-out"}`}>
                  {transaction.status === "Failed" ? <ArrowUpRight size={17} /> : icon}
                </div>
                <div className="activity-details">
                  <strong>{transactionLabel(transaction.type)} · {transaction.status}</strong>
                  <span>
                    Wallet #{transaction.walletId} <span className="detail-dot">·</span>{" "}
                    {new Date(transaction.timestamp).toLocaleString([], {
                      month: "short",
                      day: "numeric",
                      hour: "numeric",
                      minute: "2-digit",
                    })}
                  </span>
                </div>
                <span className={`activity-amount ${transaction.status === "Failed" ? "amount-failed" : isIncoming ? "amount-positive" : ""}`}>
                  {transaction.status === "Failed" ? "" : isIncoming ? "+" : "−"}{formatMoney(transaction.amountCents)}
                </span>
              </div>
            );
          })}
        </div>
      ) : (
        <div className="empty-state compact-empty">
          <div className="empty-icon"><Clock3 size={21} /></div>
          <strong>No wallet transactions yet</strong>
          <span>Transactions across this user&apos;s wallets will appear here.</span>
        </div>
      )}
    </section>
  );
}
