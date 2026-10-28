using DigitalWallet.Data;
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


    public async Task<List<TransactionEvent>> GetUserActivity(Guid userId, CancellationToken token)
    {
        return await _context.TransactionEvents
            .Where(te => te.UserId == userId.ToString())
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
}