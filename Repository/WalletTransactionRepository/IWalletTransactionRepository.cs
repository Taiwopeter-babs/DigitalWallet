using System.Linq.Expressions;
using DigitalWallet.Data;

namespace DigitalWallet.Repository;

public interface IWalletTransactionRepository
{
    void CreateWalletTransaction(WalletTransaction walletTransaction, CancellationToken token);

    /// <summary>
    /// Get all transactions in a wallet
    /// </summary>
    /// <param name="walletId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<List<WalletTransaction>> GetWalletTransactions(
        int walletId,
        CancellationToken token);

    Task<List<WalletTransaction>> GetUserWalletTransactions(Guid userId, CancellationToken token);
}