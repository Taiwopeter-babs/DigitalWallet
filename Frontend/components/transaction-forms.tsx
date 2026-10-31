import type { FormEvent } from "react";
import { ArrowDownLeft, ArrowLeftRight, ArrowUpRight, Plus } from "lucide-react";
import type { Wallet } from "@/components/wallet-utils";

type TransactionFormsProps = {
  userId: string;
  fundWalletId: string;
  fundAmount: string;
  senderWalletId: string;
  receiverWalletId: string;
  transferAmount: string;
  wallets: Wallet[];
  isValidUserId: boolean;
  isCreating: boolean;
  isFunding: boolean;
  isTransferring: boolean;
  setFundWalletId: (value: string) => void;
  setFundAmount: (value: string) => void;
  setSenderWalletId: (value: string) => void;
  setReceiverWalletId: (value: string) => void;
  setTransferAmount: (value: string) => void;
  onGenerateUserId: () => void;
  onCreateWallet: (event: FormEvent<HTMLFormElement>) => void;
  onFundWallet: (event: FormEvent<HTMLFormElement>) => void;
  onTransfer: (event: FormEvent<HTMLFormElement>) => void;
};

export function TransactionForms({
  userId,
  fundWalletId,
  fundAmount,
  senderWalletId,
  receiverWalletId,
  transferAmount,
  wallets,
  isValidUserId,
  isCreating,
  isFunding,
  isTransferring,
  setFundWalletId,
  setFundAmount,
  setSenderWalletId,
  setReceiverWalletId,
  setTransferAmount,
  onGenerateUserId,
  onCreateWallet,
  onFundWallet,
  onTransfer,
}: TransactionFormsProps) {
  return (
    <>
      <section className="panel action-panel">
        <div className="action-heading">
          <div className="action-mark">
            <Plus size={19} />
          </div>
          <div>
            <p className="eyebrow">GET STARTED</p>
            <h2>Create a wallet</h2>
          </div>
        </div>
        <p className="action-description">
          Wallets are associated with a user ID. Use the same ID to view that user&apos;s wallets.
        </p>
        <form onSubmit={onCreateWallet}>
          <label htmlFor="user-id">User ID</label>
          <input
            id="user-id"
            autoComplete="off"
            readOnly
            placeholder="Generating a unique ID..."
            value={userId}
          />
          <button className="secondary-button" type="button" onClick={onGenerateUserId}>
            Generate another ID
          </button>
          <button className="primary-button" type="submit" disabled={!isValidUserId || isCreating}>
            {isCreating ? "Creating..." : "Create wallet"} <ArrowUpRight size={16} />
          </button>
        </form>
      </section>

      <section className="panel action-panel">
        <div className="action-heading">
          <div className="action-mark action-mark-blue">
            <ArrowDownLeft size={19} />
          </div>
          <div>
            <p className="eyebrow">ADD MONEY</p>
            <h2>Fund a wallet</h2>
          </div>
        </div>
        <p className="action-description">Add funds to a wallet through the wallet service.</p>
        <form onSubmit={onFundWallet}>
          <label htmlFor="fund-wallet">Choose wallet</label>
          <select
            id="fund-wallet"
            onChange={(event) => setFundWalletId(event.target.value)}
            value={fundWalletId}
            disabled={!wallets.length}
          >
            {wallets.length ? (
              wallets.map((wallet) => (
                <option key={wallet.id} value={wallet.id}>
                  #{wallet.id} · {wallet.owner}
                </option>
              ))
            ) : (
              <option value="">No wallets available</option>
            )}
          </select>
          <label htmlFor="fund-amount">
            Amount <span className="field-hint">USD</span>
          </label>
          <div className="amount-input-wrap">
            <span>$</span>
            <input
              id="fund-amount"
              inputMode="decimal"
              onChange={(event) => setFundAmount(event.target.value)}
              placeholder="0.00"
              value={fundAmount}
            />
          </div>
          <button className="primary-button" type="submit" disabled={!wallets.length || isFunding}>
            {isFunding ? "Adding funds..." : "Add funds"} <ArrowUpRight size={16} />
          </button>
        </form>
      </section>

      <section className="panel action-panel transfer-panel">
        <div className="action-heading">
          <div className="action-mark action-mark-purple">
            <ArrowLeftRight size={19} />
          </div>
          <div>
            <p className="eyebrow">MOVE MONEY</p>
            <h2>Make a transfer</h2>
          </div>
        </div>
        <p className="action-description">Move funds between two different wallets.</p>
        <form onSubmit={onTransfer}>
          <label htmlFor="sender-wallet">From</label>
          <select
            id="sender-wallet"
            onChange={(event) => setSenderWalletId(event.target.value)}
            value={senderWalletId}
            disabled={wallets.length < 2}
          >
            {wallets.map((wallet) => (
              <option key={wallet.id} value={wallet.id}>
                #{wallet.id} · {wallet.owner}
              </option>
            ))}
            {wallets.length < 2 && <option value="">Create another wallet</option>}
          </select>

          <label htmlFor="receiver-wallet">To</label>
          <select
            id="receiver-wallet"
            onChange={(event) => setReceiverWalletId(event.target.value)}
            value={receiverWalletId}
            disabled={wallets.length < 2}
          >
            {wallets
              .filter((wallet) => String(wallet.id) !== senderWalletId)
              .map((wallet) => (
                <option key={wallet.id} value={wallet.id}>
                  #{wallet.id} · {wallet.owner}
                </option>
              ))}
            {wallets.length < 2 && <option value="">Create another wallet</option>}
          </select>

          <label htmlFor="transfer-amount">
            Amount <span className="field-hint">USD</span>
          </label>
          <div className="amount-input-wrap">
            <span>$</span>
            <input
              id="transfer-amount"
              inputMode="decimal"
              onChange={(event) => setTransferAmount(event.target.value)}
              placeholder="0.00"
              value={transferAmount}
            />
          </div>
          <button className="primary-button" type="submit" disabled={wallets.length < 2 || isTransferring}>
            {isTransferring ? "Transferring..." : "Transfer funds"} <ArrowUpRight size={16} />
          </button>
          {wallets.length < 2 && <p className="inline-hint">You need at least two wallets to make a transfer.</p>}
        </form>
      </section>
    </>
  );
}
