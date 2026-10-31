

using DigitalWallet.Data;
using Microsoft.EntityFrameworkCore;

namespace DigitalWallet.Services.KafkaService.WalletWorkerService;

public class WalletWorkerService : IWalletWorkerService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public WalletWorkerService(
      IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<bool> SaveTransactionAndInboxData(CreateTransactionEventDto transactionEventDto, CancellationToken token)
    {
        await using var scope = _serviceScopeFactory.CreateAsyncScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

        await using var transaction = await dbContext.Database.BeginTransactionAsync(token);

        try
        {
            Guid? correlationId = transactionEventDto.EventData.CorrelationId;
            var currentDatetime = DateTimeOffset.UtcNow;

            Console.WriteLine("Correlation Id: {0}", correlationId);

            if (correlationId is not Guid)
            {
                return false;
            }

            var inboxMessage = await dbContext.InboxMessages
                .Where(im => im.CorrelationId.Equals(correlationId))
                .Where(im => im.OutboxMessage.Payload.WalletId == transactionEventDto.WalletId)
                .Where(im => im.Received == null || im.Received != true)
                .FirstOrDefaultAsync(token);

            if (inboxMessage == null)
            {
                await transaction.RollbackAsync(token);
                return false;
            }

            inboxMessage.Received = true;
            inboxMessage.ReceivedAt = currentDatetime;


            var transactionEvent = transactionEventDto.ToTransactionEvent();

            dbContext.TransactionEvents.Add(transactionEvent);

            await dbContext.SaveChangesAsync(token);

            await transaction.CommitAsync(token);

            return true;
        }
        catch
        {
            await transaction.RollbackAsync(token);

            return false;
        }
    }
}
