using System.Linq.Expressions;
using DigitalWallet.Data;
using DigitalWallet.Errors;
using Microsoft.EntityFrameworkCore;

namespace DigitalWallet.Repository;

public class WalletRepository : RepositoryBase<Wallet>, IWalletRepository
{
    public WalletRepository(DatabaseContext context) : base(context)
    {
    }

    public void CreateWallet(Wallet wallet, CancellationToken token)
    {
        Create(wallet);
    }


    public async Task<Wallet?>
        GetWallet(Expression<Func<Wallet, bool>> expression, CancellationToken token, bool trackChanges = false)
    {
        return await FindByCondition(expression, trackChanges).AsNoTracking().SingleOrDefaultAsync(token);
    }

    public async Task<List<Wallet>> GetUserWallets(Guid userId, CancellationToken token)
    {
        var userIdText = userId.ToString();
        return await _context.Wallets.Where(wallet => wallet.UserId == userIdText).AsNoTracking().ToListAsync(token);
    }

    public async Task<List<Wallet>> GetAllWallets(CancellationToken token)
    {
        return await _context.Wallets.AsNoTracking().ToListAsync(token);
    }

    public async Task<Wallet> FundWallet(int walletId, decimal amount, CancellationToken token)
    {
        if (amount <= 0) throw new InvalidAmountException(amount);

        var wallet = await FindByCondition(w => w.Id == walletId, true).SingleOrDefaultAsync(token);

        if (wallet is null) throw new NotFoundException("Wallet not found");

        wallet!.Balance += amount;

        return wallet;
    }

    public async Task Transfer(Wallet senderWallet, decimal amount, Wallet recipientWallet,
        CancellationToken token)
    {
        senderWallet.Balance -= amount;
        recipientWallet.Balance += amount;

        Update(senderWallet);
        Update(recipientWallet);
    }

    public void DeleteWallet(Wallet wallet, CancellationToken token)
    {
        Delete(wallet);
    }
}