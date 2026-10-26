using DigitalWallet.Data;

namespace DigitalWallet.Repository;

public interface IHistoryRepository
{
    /// <summary>
    /// Get all events associated with a user.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<List<TransactionEvent>> GetUserActivity(Guid userId, CancellationToken token);


    Task<TransactionEvent?> GetTransactionEvent(int eventId, CancellationToken token);

    void SaveTransactionEvent(TransactionEvent transactionEvent, CancellationToken token);
}