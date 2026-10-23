using DigitalWallet.Data;
using DigitalWallet.Results;

namespace DigitalWallet.Services;

public interface IHistoryService
{
    Task<Result<List<WalletTransactionDto>>> GetWalletHistory(int walletId, CancellationToken cancellationToken);
    Task<Result<List<TransactionEventDto>>> GetUserActivity(string userId, CancellationToken cancellationToken);

    Task<Result<TransactionEventDto>> SaveTransactionEvent(CreateTransactionEventDto transactionEventDto,
        CancellationToken cancellationToken);
}