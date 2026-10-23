using System.Linq.Expressions;
using DigitalWallet.Data;
using DigitalWallet.Errors;
using Microsoft.EntityFrameworkCore;

namespace DigitalWallet.Repository;

public class HistoryRepository : RepositoryBase<TransactionEvent>, IHistoryRepository
{
    public HistoryRepository(DatabaseContext context) : base(context)
    {
    }

    public async Task<List<WalletTransaction>> GetWalletTransactions(int walletId, CancellationToken token)
    {
        return await _context.WalletTransactions
            .Where(w => w.WalletId.Equals(walletId))
            .AsNoTracking()
            .ToListAsync(token);
    }


    public async Task<List<TransactionEvent>> GetUserActivity(int userId, CancellationToken token)
    {
        return await _context.TransactionEvents
            .Where(w => w.UserId.Equals(userId))
            .AsNoTracking()
            .ToListAsync(token);
    }

    public async Task<TransactionEvent?> GetTransactionEvent(int eventId, CancellationToken token)
    {
        return await _context.TransactionEvents
            .Where(te => te.Id.Equals(eventId))
            .AsNoTracking()
            .SingleOrDefaultAsync(token);
    }

    public void SaveTransactionEvent(TransactionEvent transactionEvent, CancellationToken token)
    {
        Create(transactionEvent);
    }

    public async Task<List<TransactionEvent>> GetUserActivity(string userId, CancellationToken token)
    {
        return await _context.TransactionEvents
            .Where(te => te.UserId.Equals(userId))
            .AsNoTracking()
            .ToListAsync(token);
    }
}