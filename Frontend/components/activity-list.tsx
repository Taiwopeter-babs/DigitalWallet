import { Activity as ActivityIcon, ArrowDownLeft, ArrowUpRight, Clock3, Plus, RefreshCw } from "lucide-react";
import type { FormEvent } from "react";
import { formatMoney, type UserActivity } from "@/components/wallet-utils";

type ActivityListProps = {
  activity: UserActivity[];
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

function activityDescription(eventType: UserActivity["eventType"]): string {
  switch (eventType) {
    case "wallet-created":
      return "Wallet created";
    case "wallet-funded":
      return "Funds added";
    case "transfer-completed":
      return "Transfer completed";
    case "transfer-failed":
      return "Transfer failed";
  }
}

export function ActivityList({
  activity,
  availableUserIds,
  selectedUserId,
  onSelectedUserIdChange,
  onLoadUser,
  isLoading,
  isError,
  errorMessage,
  isRefreshing,
  onRefresh,
}: ActivityListProps) {
  return (
    <section className="panel activity-panel">
      <div className="section-heading">
        <div>
          <p className="eyebrow">USER HISTORY</p>
          <h2>All user activity</h2>
        </div>
        <button className="activity-refresh" type="button" onClick={onRefresh} disabled={isRefreshing}>
          <RefreshCw size={14} className={isRefreshing ? "spinning" : ""} /> Refresh
        </button>
      </div>

      <form className="history-user-picker" onSubmit={onLoadUser}>
        <label htmlFor="history-user-id">Select a user ID</label>
        <div className="history-user-controls">
          <select
            id="history-user-id"
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
            Load user events
          </button>
        </div>
      </form>

      {isLoading ? (
        <div className="empty-state compact-empty" aria-live="polite">Loading user activity…</div>
      ) : isError ? (
        <div className="activity-error" role="alert">
          <ActivityIcon size={18} />
          <span>{errorMessage ?? "Could not load activity."}</span>
        </div>
      ) : activity.length ? (
        <div className="activity-list">
          {activity.map((entry) => {
            const isIncoming = entry.eventType === "wallet-funded";
            const isCreation = entry.eventType === "wallet-created";
            const isFailed = entry.eventType === "transfer-failed";
            const icon =
              entry.eventType === "wallet-funded"
                ? <ArrowDownLeft size={17} />
                : entry.eventType === "transfer-completed" || isFailed
                  ? <ArrowUpRight size={17} />
                  : <Plus size={17} />;

            return (
              <div className="activity-row" key={entry.id}>
                <div className={`activity-icon ${isCreation ? "activity-create" : isIncoming ? "activity-in" : "activity-out"}`}>
                  {icon}
                </div>
                <div className="activity-details">
                  <strong>{activityDescription(entry.eventType)}</strong>
                  <span>
                    Wallet #{entry.walletId} <span className="detail-dot">·</span>{" "}
                    {new Date(entry.timestamp).toLocaleString([], {
                      month: "short",
                      day: "numeric",
                      hour: "numeric",
                      minute: "2-digit",
                    })}
                    <span className="detail-dot"> · </span>
                    Transaction #{entry.transactionId}
                  </span>
                </div>
                <span className={`activity-amount ${isCreation ? "amount-muted" : isIncoming ? "amount-positive" : isFailed ? "amount-failed" : ""}`}>
                  {isCreation ? "—" : `${isIncoming ? "+" : isFailed ? "" : "−"}${formatMoney(entry.amountCents)}`}
                </span>
              </div>
            );
          })}
        </div>
      ) : (
        <div className="empty-state compact-empty">
          <div className="empty-icon"><Clock3 size={21} /></div>
          <strong>No user activity yet</strong>
          <span>Wallet events from the API will appear here.</span>
        </div>
      )}
    </section>
  );
}
