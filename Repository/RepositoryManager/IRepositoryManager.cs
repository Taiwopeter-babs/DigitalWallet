namespace DigitalWallet.Repository;

public interface IRepositoryManager
{
    IWalletRepository Wallet { get; }
    IHistoryRepository History { get; }
    IWalletTransactionRepository WalletTransaction { get; }
    IOutboxMessageRepository OutboxMessage { get; }

    Task SaveAsync(CancellationToken token);

    public Task BeginTransactionAsync(CancellationToken token);

    public Task CommitTransactionAsync(CancellationToken token);
    public Task RollbackTransactionAsync(CancellationToken token);
}