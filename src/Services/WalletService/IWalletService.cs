using DigitalWallet.Data;
using DigitalWallet.Results;

namespace DigitalWallet.Services.WalletService;

public interface IWalletService
{
    Task<Result<WalletDto>> Create(CreateWalletDto dto, CancellationToken token);

    Task<Result<WalletDto>> Fund(int walletId, decimal amount, CancellationToken token);

    Task<Result<WalletDto>> Transfer(int senderWalletId, decimal amount, int receiverWalletId, CancellationToken token);

    Task<Result<WalletDto>> Get(int walletId, CancellationToken token);
    // Task<Result<List<WalletTransactionDto>>> GetWalletTransactions();
}