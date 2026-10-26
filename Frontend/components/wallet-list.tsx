import { ChevronDown, ShieldCheck, Wallet as WalletIcon } from "lucide-react";
import { formatMoney, type Wallet } from "@/components/wallet-utils";

type WalletListProps = {
  wallets: Wallet[];
};

export function WalletList({ wallets }: WalletListProps) {
  return (
    <section className="panel wallets-panel">
      <div className="section-heading">
        <div>
          <p className="eyebrow">YOUR MONEY</p>
          <h2>Your wallets <span className="count-pill">{wallets.length}</span></h2>
        </div>
        <span className="subtle-label">Balances in USD</span>
      </div>

      {wallets.length ? (
        <div className="wallet-list">
          {wallets.map((wallet, index) => (
            <article className="wallet-row" key={wallet.id}>
              <div className={`wallet-avatar avatar-${index % 4}`}>
                {wallet.owner.split(/\s+/).map((part) => part[0]).slice(0, 2).join("").toUpperCase()}
              </div>
              <div className="wallet-details">
                <strong>{wallet.owner}</strong>
                <span>Wallet #{wallet.id} <span className="detail-dot">·</span> {wallet.userId.slice(0, 8)}…</span>
              </div>
              <div className="wallet-amount">
                <strong>{formatMoney(wallet.balanceCents)}</strong>
                <span>Available balance</span>
              </div>
              <div className="wallet-row-icon" aria-hidden="true"><ChevronDown size={16} /></div>
            </article>
          ))}
        </div>
      ) : (
        <div className="empty-state">
          <div className="empty-icon"><WalletIcon size={22} /></div>
          <strong>No wallets just yet</strong>
          <span>Create a wallet for this user to get started.</span>
        </div>
      )}

      <div className="wallets-footnote">
        <ShieldCheck size={15} />
        <span>Wallet balances are loaded from the wallet service.</span>
      </div>
    </section>
  );
}
