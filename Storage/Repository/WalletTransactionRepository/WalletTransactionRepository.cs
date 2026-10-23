using System.Linq.Expressions;
using DigitalWallet.Data;
using DigitalWallet.Errors;
using Microsoft.EntityFrameworkCore;

namespace DigitalWallet.Repository;

public class WalletTransactionRepository : RepositoryBase<WalletTransaction>, IWalletTransactionRepository
{
    public WalletTransactionRepository(DatabaseContext context) : base(context)
    {
    }

    public void CreateWalletTransaction(WalletTransaction walletTransaction, CancellationToken token)
    {
        _context.WalletTransactions.Add(walletTransaction);
    }

    public async Task<List<WalletTransaction>> GetWalletTransactions(int walletId, CancellationToken token)
    {
        return await _context.WalletTransactions
            .Where(w => w.WalletId.Equals(walletId))
            .AsNoTracking()
            .ToListAsync(token);
    }
}