import { ArrowLeftRight, Wallet as WalletIcon } from "lucide-react";
import { formatMoney } from "@/components/wallet-utils";

type OverviewPanelProps = {
  totalBalance: number;
  walletCount: number;
  movementCount: number;
};

export function OverviewPanel({ totalBalance, walletCount, movementCount }: OverviewPanelProps) {
  return (
    <section className="overview-grid" aria-label="Wallet summary">
      <article className="balance-card">
        <div className="balance-card-top">
          <span className="balance-label">TOTAL BALANCE</span>
          <span className="balance-icon"><WalletIcon size={19} /></span>
        </div>
        <div className="total-balance">{formatMoney(totalBalance)}</div>
        <div className="balance-foot">
          <span><span className="green-indicator" /> Across all your wallets</span>
          <span>{walletCount} {walletCount === 1 ? "wallet" : "wallets"}</span>
        </div>
        <div className="balance-decoration decoration-one" />
        <div className="balance-decoration decoration-two" />
      </article>

      <div className="overview-stat">
        <div className="stat-icon mint"><WalletIcon size={18} /></div>
        <span className="stat-caption">YOUR WALLETS</span>
        <strong>{String(walletCount).padStart(2, "0")}</strong>
        <span className="stat-foot">Ready when you are</span>
      </div>

      <div className="overview-stat">
        <div className="stat-icon lavender"><ArrowLeftRight size={18} /></div>
        <span className="stat-caption">THIS SESSION</span>
        <strong>{movementCount}</strong>
        <span className="stat-foot">Money movements</span>
      </div>
    </section>
  );
}
