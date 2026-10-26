using DigitalWallet.Data;
using DigitalWallet.Repository;
using DigitalWallet.Results;


namespace DigitalWallet.Services.HistoryService;

public class HistoryService : IHistoryService
{
    private readonly IRepositoryManager _repo;


    public HistoryService(IRepositoryManager repo)
    {
        _repo = repo;
    }

    public async Task<Result<List<WalletTransactionDto>>> GetWalletHistory(int walletId,
        CancellationToken cancellationToken)
    {
        return await Result.TryAsync(_repo, async (repo, token) =>
        {
            var walletTransactions = await repo.WalletTransaction.GetWalletTransactions(walletId, token);

            var walletTransactionsDto = walletTransactions.Select(wt => wt.ToWalletTransactionDto()).ToList();

            return Result.Success(walletTransactionsDto);
        }, cancellationToken);
    }

    public async Task<Result<List<TransactionEventDto>>> GetUserActivity(Guid userId,
        CancellationToken cancellationToken)
    {
        return await Result.TryAsync(_repo, async (repo, token) =>
        {
            var userEvents = await repo.History.GetUserActivity(userId, token);

            var userEventsDto = userEvents.Select(ue => ue.ToTransactionEventDto()).ToList();

            return Result.Success(userEventsDto);
        }, cancellationToken);
    }

    public async Task<Result<TransactionEventDto>> SaveTransactionEvent(CreateTransactionEventDto transactionEventDto,
        CancellationToken cancellationToken)
    {
        return await Result.TryAsync(_repo, async (repo, token) =>
        {
            var transactionEvent = transactionEventDto.ToTransactionEvent();

            repo.History.SaveTransactionEvent(transactionEvent, token);

            await repo.SaveAsync(token);

            return Result.Success(transactionEvent.ToTransactionEventDto());
        }, cancellationToken);
    }
}