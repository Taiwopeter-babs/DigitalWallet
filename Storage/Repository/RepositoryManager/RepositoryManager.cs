using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using DigitalWallet.Data;

namespace DigitalWallet.Repository;

public sealed class RepositoryManager : IRepositoryManager
{
    private readonly DatabaseContext _context;
    private IDbContextTransaction? _currentTransaction;

    private readonly Lazy<IWalletRepository> _walletRepository;
    private readonly Lazy<IHistoryRepository> _historyRepository;
    private readonly Lazy<IWalletTransactionRepository> _walletTransactionRepository;


    public RepositoryManager(DatabaseContext context)
    {
        _context = context;
        _walletRepository = new Lazy<IWalletRepository>(() => new WalletRepository(context));
        _historyRepository = new Lazy<IHistoryRepository>(() => new HistoryRepository(context));
        _walletTransactionRepository =
            new Lazy<IWalletTransactionRepository>(() => new WalletTransactionRepository(context));
    }


    public IWalletRepository Wallet => _walletRepository.Value;
    public IHistoryRepository History => _historyRepository.Value;
    public IWalletTransactionRepository WalletTransaction => _walletTransactionRepository.Value;


    public async Task SaveAsync(CancellationToken token)
    {
        await _context.SaveChangesAsync(token);
    }


    public async Task BeginTransactionAsync(CancellationToken token)
    {
        if (_currentTransaction != null) return;

        _currentTransaction = await _context.Database.BeginTransactionAsync(token);
    }

    public async Task CommitTransactionAsync(CancellationToken token)
    {
        try
        {
            await _context.SaveChangesAsync(token);
            if (_currentTransaction != null) await _currentTransaction.CommitAsync(token);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (_currentTransaction != null) await _currentTransaction.RollbackAsync(token);
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken token)
    {
        try
        {
            if (_currentTransaction != null) await _currentTransaction.RollbackAsync(token);
        }
        finally
        {
            if (_currentTransaction != null)
            {
                _currentTransaction.Dispose();
                _currentTransaction = null;
            }
        }
    }

    public void Dispose()
    {
        _currentTransaction?.Dispose();
        _context.Dispose();
    }
}