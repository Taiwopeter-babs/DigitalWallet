using System.Linq.Expressions;
using DigitalWallet.Data;

namespace DigitalWallet.Repository;

public interface IWalletRepository
{
    void CreateWallet(Wallet wallet, CancellationToken token);

    Task<Wallet> FundWallet(int walletId, decimal amount, CancellationToken token);
    Task Transfer(Wallet senderWallet, decimal amount, Wallet receiverWallet, CancellationToken token);

    Task<Wallet?> GetWallet(Expression<Func<Wallet, bool>> expression, CancellationToken token,
        bool trackChanges = false);

    Task<List<Wallet>> GetWallets(CancellationToken token);

    void DeleteWallet(Wallet wallet, CancellationToken token);
}