import { ArrowDownLeft, ArrowUpRight, Clock3, Plus } from "lucide-react";
import { activityDescription, formatMoney, type Activity, type Wallet } from "@/components/wallet-utils";

type ActivityListProps = {
  wallets: Wallet[];
  activity: Activity[];
};

export function ActivityList({ wallets, activity }: ActivityListProps) {
  return (
    <section className="panel activity-panel">
      <div className="section-heading">
        <div>
          <p className="eyebrow">THE LATEST</p>
          <h2>Recent activity</h2>
        </div>
        <span className="activity-live"><span className="green-indicator" /> Updates instantly</span>
      </div>

      {activity.length ? (
        <div className="activity-list">
          {activity.map((entry) => {
            const isIncoming = entry.type === "funded" || entry.type === "transfer-in";
            const isCreation = entry.type === "created";
            const icon =
              entry.type === "funded" || entry.type === "transfer-in"
                ? <ArrowDownLeft size={17} />
                : entry.type === "transfer-out"
                  ? <ArrowUpRight size={17} />
                  : <Plus size={17} />;

            return (
              <div className="activity-row" key={entry.id}>
                <div className={`activity-icon ${isCreation ? "activity-create" : isIncoming ? "activity-in" : "activity-out"}`}>
                  {icon}
                </div>
                <div className="activity-details">
                  <strong>{activityDescription(entry, wallets)}</strong>
                  <span>
                    Wallet #{entry.walletId} <span className="detail-dot">·</span>{" "}
                    {new Date(entry.createdAt).toLocaleString([], {
                      month: "short",
                      day: "numeric",
                      hour: "numeric",
                      minute: "2-digit",
                    })}
                  </span>
                </div>
                <span className={`activity-amount ${isCreation ? "amount-muted" : isIncoming ? "amount-positive" : ""}`}>
                  {isCreation ? "—" : `${isIncoming ? "+" : "−"}${formatMoney(entry.amountCents)}`}
                </span>
              </div>
            );
          })}
        </div>
      ) : (
        <div className="empty-state compact-empty">
          <div className="empty-icon"><Clock3 size={21} /></div>
          <strong>Nothing to see yet</strong>
          <span>Your wallet activity will show up here.</span>
        </div>
      )}
    </section>
  );
}
