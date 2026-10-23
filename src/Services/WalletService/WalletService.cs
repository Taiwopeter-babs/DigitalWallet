using DigitalWallet.Data;
using DigitalWallet.Enums;
using DigitalWallet.Errors;
using DigitalWallet.Repository;
using DigitalWallet.Results;
using DigitalWallet.Services.KafkaService;
using DigitalWallet.Storage;
using Microsoft.Extensions.Options;


namespace DigitalWallet.Services.WalletService;

public class WalletService : IWalletService
{
    private readonly IRepositoryManager _repo;
    private readonly IKafkaProducerService _kafkaProducerService;
    private readonly WalletProducerOptions _options;


    public WalletService(IRepositoryManager repo, IKafkaProducerService kafkaProducerService,
        IOptions<WalletProducerOptions> options)
    {
        _repo = repo;
        _kafkaProducerService = kafkaProducerService;
        _options = options.Value;
    }

    public async Task<Result<WalletDto>> Create(CreateWalletDto walletDto, CancellationToken cancellationToken)
    {
        return await Result.TryAsync<WalletDto>(_repo, async (repo, token) =>
        {
            var wallet = await repo.Wallet.GetWallet(wa => wa.UserId.Equals(walletDto.UserId), token);

            if (wallet != null) return Result.Success(wallet.ToWalletDto());

            var newWallet = walletDto.ToWallet();

            repo.Wallet.CreateWallet(newWallet, token);

            await repo.SaveAsync(token);

            return Result.Success(newWallet.ToWalletDto());
        }, cancellationToken);
    }

    public async Task<Result<WalletDto>> Fund(int walletId, decimal amount, CancellationToken token)
    {
        return await Result.TryAsync<WalletDto>(_repo, async (repo, cancellationToken) =>
        {
            // Validation here
            if (!IsValidAmount(amount)) return Result.BadRequest<WalletDto>("Invalid amount");

            await repo.BeginTransactionAsync(cancellationToken);

            var wallet = await repo.Wallet.GetWallet(wa => wa.Id.Equals(walletId), cancellationToken);

            if (wallet is null) throw new NotFoundException("Wallet Not Found");

            // Fund wallet
            await repo.Wallet.FundWallet(walletId, amount, cancellationToken);

            var walletTransaction = new CreateWalletTransactionDto
            {
                WalletId = walletId,
                Amount = amount,
                Status = TransactionStatus.Completed,
                Type = TransactionType.Fund
            }.ToWalletTransaction();

            repo.WalletTransaction.CreateWalletTransaction(walletTransaction,
                cancellationToken);

            await repo.CommitTransactionAsync(cancellationToken);

            // Publish event here
            var walletFundKafkaEventDto = new KafkaWalletFundEventDto
            {
                UserId = wallet.UserId,
                WalletId = wallet.Id,
                TransactionId = walletTransaction.Id,
                Amount = amount,
                EventType = EventType.WalletFunded,
                TransactionStatus = TransactionStatus.Completed,
                TransactionType = TransactionType.Fund,
                Timestamp = DateTime.Now
            };

            await _kafkaProducerService.PublishKafkaEvent(_options.WalletFundingTopic,
                _options.WalletProducerKafkaKey,
                walletFundKafkaEventDto, cancellationToken);

            return Result.Success(wallet.ToWalletDto());
            ;
        }, token);
    }

    public async Task<Result<WalletDto>> Transfer(int senderWalletId, decimal amount, int receiverWalletId,
        CancellationToken token)
    {
        return await Result.TryAsync<WalletDto>(_repo, async (repo, cancellationToken) =>
        {
            if (!IsValidAmount(amount)) return Result.BadRequest<WalletDto>("Invalid amount");

            await repo.BeginTransactionAsync(cancellationToken);

            var walletResults =
                await Task.WhenAll(repo.Wallet.GetWallet(wa => wa.Id.Equals(senderWalletId), cancellationToken),
                    repo.Wallet.GetWallet(wa => wa.Id.Equals(receiverWalletId), cancellationToken));

            var senderWallet = walletResults[0];
            var receiverWallet = walletResults[1];

            if (senderWallet is null) throw new NotFoundException("Sender Wallet Not Found");

            if (receiverWallet is null) throw new NotFoundException("Receiver Wallet Not Found");

            if (!IsSufficientBalance(senderWallet.Balance, amount))
            {
                var failedWalletTransaction = new CreateWalletTransactionDto
                {
                    WalletId = senderWalletId,
                    Amount = amount,
                    Status = TransactionStatus.Failed,
                    Type = TransactionType.TransferOut
                }.ToWalletTransaction();

                repo.WalletTransaction.CreateWalletTransaction(failedWalletTransaction,
                    cancellationToken);

                await repo.CommitTransactionAsync(cancellationToken);

                var failedKafkaEventDto = new KafkaWalletTransferEventDto
                {
                    SenderId = senderWallet.UserId,
                    ReceiverId = receiverWallet.UserId,
                    SenderWalletId = senderWallet.Id,
                    ReceiverWalletId = receiverWallet.Id,
                    TransactionId = failedWalletTransaction.Id,
                    Amount = failedWalletTransaction.Amount,
                    EventType = EventType.TransferFailed,
                    TransactionStatus = TransactionStatus.Failed,
                    TransactionType = TransactionType.TransferOut,
                    Timestamp = DateTime.Now
                };

                await _kafkaProducerService.PublishKafkaEvent(_options.WalletTransferTopic,
                    _options.WalletProducerKafkaKey,
                    failedKafkaEventDto, cancellationToken);


                return Result.BadRequest<WalletDto>("Insufficient balance");
            }


            await repo.Wallet.Transfer(senderWallet, amount, receiverWallet, cancellationToken);

            var baseWalletTransactionDto = new CreateWalletTransactionDto
            {
                WalletId = senderWalletId,
                Amount = amount,
                Status = TransactionStatus.Completed,
                Type = TransactionType.TransferOut
            };


            var senderWalletTransaction =
                (baseWalletTransactionDto with { WalletId = senderWalletId }).ToWalletTransaction();

            var receiverWalletTransaction =
                (baseWalletTransactionDto with { WalletId = receiverWalletId }).ToWalletTransaction();


            repo.WalletTransaction.CreateWalletTransaction(senderWalletTransaction,
                cancellationToken);

            repo.WalletTransaction.CreateWalletTransaction(receiverWalletTransaction,
                cancellationToken);

            await repo.CommitTransactionAsync(cancellationToken);

            var baseTransferSuccessKafkaEventDto = new KafkaWalletTransferEventDto
            {
                SenderId = senderWallet.UserId,
                ReceiverId = receiverWallet.UserId,
                SenderWalletId = senderWallet.Id,
                ReceiverWalletId = receiverWallet.Id,
                TransactionId = 0,
                Amount = amount,
                EventType = EventType.TransferCompleted,
                TransactionStatus = TransactionStatus.Completed,
                TransactionType = TransactionType.TransferIn,
                Timestamp = DateTime.Now
            };

            var senderTransferKafkaEventDto = baseTransferSuccessKafkaEventDto with
            {
                TransactionId = senderWalletTransaction.Id,
                TransactionType = TransactionType.TransferOut
            };


            var receiverTransferKafkaEventDto = baseTransferSuccessKafkaEventDto with
            {
                TransactionId = receiverWalletTransaction.Id,
                TransactionType = TransactionType.TransferIn
            };


            var senderEvent = _kafkaProducerService.PublishKafkaEvent(_options.WalletTransferTopic,
                _options.WalletProducerKafkaKey,
                senderTransferKafkaEventDto, cancellationToken);

            var receiverEvent = _kafkaProducerService.PublishKafkaEvent(_options.WalletTransferTopic,
                _options.WalletProducerKafkaKey,
                receiverTransferKafkaEventDto, cancellationToken);


            await Task.WhenAll(senderEvent, receiverEvent);


            return Result.Success(senderWallet.ToWalletDto());
        }, token);
    }

    public async Task<Result<WalletDto>> Get(int walletId, CancellationToken token)
    {
        return await Result.TryAsync<WalletDto>(_repo, async (repo, cancellationToken) =>
        {
            var wallet = await repo.Wallet.GetWallet(wa => wa.Id.Equals(walletId), cancellationToken);

            return wallet is null
                ? Result.NotFound<WalletDto>("Wallet Not Found")
                : Result.Success(wallet.ToWalletDto());
        }, token);
    }


    private static bool IsSufficientBalance(decimal balance, decimal amount)
    {
        return balance > 0 && balance - amount >= 0;
    }

    private static bool IsValidAmount(decimal amount)
    {
        return amount >= 0;
    }


    /// <summary>
    /// Persists the failed message to a relational database outbox table when Kafka is completely offline.
    /// </summary>
//     private async Task SaveToOutboxDatabaseAsync(
//         string topic,
//         string key,
//         string messageValue,
//         string errorMessage,
//         CancellationToken cancellationToken)
//     {
//         try
//         {
//             // Example: Saving to your database via an outbox service/repository
//             await _outboxService.SaveMessageAsync(new OutboxMessage
//             {
//                 Topic = topic,
//                 Key = key,
//                 Payload = messageValue,
//                 ErrorReason = errorMessage,
//                 CreatedAt = DateTime.UtcNow,
//                 Processed = false
//             }, cancellationToken);
//
//             _logger.LogCritical(
//                 "CRITICAL: Kafka and DLQ are unreachable. Message for topic '{Topic}' with key '{Key}' successfully saved to Database Outbox table for background retry.",
//                 topic, key);
//         }
//         catch (Exception dbEx)
//         {
//             // Absolute worst-case scenario: Kafka is down, DLQ is down, and the Database is down.
//             _logger.LogCritical(
//                 dbEx,
//                 "FATAL: Outbox database save failed for topic '{Topic}' with key '{Key}'. Message loss imminent!",
//                 topic, key);
//
//             // Options here: Write to local disk file system as a last resort, or rethrow.
//             throw;
//         }
//     }
}