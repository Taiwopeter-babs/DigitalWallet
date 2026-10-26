using DigitalWallet.Data;
using DigitalWallet.Enums;
using DigitalWallet.Errors;
using DigitalWallet.Repository;
using DigitalWallet.Results;
using DigitalWallet.Storage;
using FluentValidation;
using Microsoft.Extensions.Options;


namespace DigitalWallet.Services.WalletService;

public class WalletService : IWalletService
{
    private readonly IRepositoryManager _repo;
    private readonly WalletProducerOptions _options;
    private readonly IValidator<CreateWalletDto> _walletCreateValidator;


    public WalletService(IRepositoryManager repo,
        IOptions<WalletProducerOptions> options,
        IValidator<CreateWalletDto> walletCreateValidator)
    {
        _repo = repo;
        _options = options.Value;
        _walletCreateValidator = walletCreateValidator;

    }

    public async Task<Result<WalletDto>> Create(CreateWalletDto walletDto, CancellationToken cancellationToken)
    {
        return await Result.TryAsync(_repo, async (repo, token) =>
        {

            var wallet = await repo.Wallet.GetWallet(wa => wa.UserId.Equals(walletDto.UserId), token);

            if (wallet != null) return Result.Success(wallet.ToWalletDto());

            await repo.BeginTransactionAsync(token);

            var newWallet = walletDto.ToWallet();

            repo.Wallet.CreateWallet(newWallet, token);

            await repo.SaveAsync(token);

            var outboxMessage = new OutboxMessage
            {
                Key = _options.WalletProducerKafkaKey,
                Topic = _options.WalletCreationTopic,
                Payload = new OutboxMessagePayload
                {
                    UserId = newWallet.UserId,
                    WalletId = newWallet.Id,
                    EventType = EventType.WalletCreated,
                    Timestamp = DateTimeOffset.UtcNow
                }
            };

            repo.OutboxMessage.CreateOutboxMessage(outboxMessage);

            await repo.CommitTransactionAsync(token);


            return Result.Success(newWallet.ToWalletDto());
        }, cancellationToken);
    }

    public async Task<Result<WalletDto>> Fund(int walletId, decimal amount, CancellationToken token)
    {
        return await Result.TryAsync(_repo, async (repo, cancellationToken) =>
        {
            // Validation here
            if (!IsValidAmount(amount)) return Result.BadRequest<WalletDto>("Invalid amount");

            await repo.BeginTransactionAsync(cancellationToken);

            var wallet = await repo.Wallet.GetWallet(wa => wa.Id.Equals(walletId), cancellationToken) ?? throw new NotFoundException("Wallet Not Found");

            // Fund wallet
            await repo.Wallet.FundWallet(walletId, amount, cancellationToken);

            var walletTransaction = new WalletTransaction
            {
                WalletId = walletId,
                Amount = amount,
                Status = TransactionStatus.Completed,
                Type = TransactionType.Fund
            };

            repo.WalletTransaction.CreateWalletTransaction(walletTransaction,
                cancellationToken);

            await repo.SaveAsync(token);


            var outboxMessage = new OutboxMessage
            {
                Key = _options.WalletProducerKafkaKey,
                Topic = _options.WalletFundingTopic,
                Payload = new OutboxMessagePayload
                {
                    UserId = wallet.UserId,
                    TransactionId = walletTransaction.Id,
                    Amount = amount,
                    WalletId = wallet.Id,
                    EventType = EventType.WalletFunded,
                    Timestamp = DateTimeOffset.UtcNow
                }
            };

            repo.OutboxMessage.CreateOutboxMessage(outboxMessage);

            await repo.CommitTransactionAsync(cancellationToken);

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

            var senderWallet = await repo.Wallet.GetWallet(wa => wa.Id.Equals(senderWalletId), cancellationToken);
            var receiverWallet = await repo.Wallet.GetWallet(wa => wa.Id.Equals(receiverWalletId), cancellationToken);


            if (senderWallet is null) throw new NotFoundException("Sender Wallet Not Found");

            if (receiverWallet is null) throw new NotFoundException("Receiver Wallet Not Found");

            if (!IsSufficientBalance(senderWallet.Balance, amount))
            {
                var failedWalletTransaction = new WalletTransaction
                {
                    WalletId = senderWalletId,
                    Amount = amount,
                    Status = TransactionStatus.Failed,
                    Type = TransactionType.TransferOut
                };

                repo.WalletTransaction.CreateWalletTransaction(failedWalletTransaction,
                    cancellationToken);

                await repo.SaveAsync(token);

                var failedOutboxMessage = new OutboxMessage
                {
                    Key = _options.WalletProducerKafkaKey,
                    Topic = _options.WalletTransferTopic,
                    Payload = new OutboxMessagePayload
                    {
                        UserId = senderWallet.UserId,
                        TransactionId = failedWalletTransaction.Id,
                        WalletId = senderWallet.Id,
                        Amount = amount,
                        EventType = EventType.TransferFailed,
                        Timestamp = DateTimeOffset.UtcNow

                    }
                };

                repo.OutboxMessage.CreateOutboxMessage(failedOutboxMessage);

                await repo.CommitTransactionAsync(cancellationToken);

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
                (baseWalletTransactionDto with { WalletId = receiverWalletId, Type = TransactionType.TransferIn })
                .ToWalletTransaction();


            repo.WalletTransaction.CreateWalletTransaction(senderWalletTransaction,
                cancellationToken);

            repo.WalletTransaction.CreateWalletTransaction(receiverWalletTransaction,
                cancellationToken);

            await repo.SaveAsync(token);

            var senderOutboxMessage = new OutboxMessage
            {
                Topic = _options.WalletTransferTopic,
                Key = _options.WalletProducerKafkaKey,
                Payload = new OutboxMessagePayload
                {
                    UserId = senderWallet.UserId,
                    TransactionId = senderWalletTransaction.Id,
                    WalletId = senderWallet.Id,
                    Amount = amount,
                    EventType = EventType.TransferCompleted,
                    Timestamp = DateTimeOffset.UtcNow

                }
            };

            var receiverOutboxMessage = new OutboxMessage
            {
                Topic = _options.WalletTransferTopic,
                Key = _options.WalletProducerKafkaKey,
                Payload = new OutboxMessagePayload
                {
                    UserId = receiverWallet.UserId,
                    TransactionId = receiverWalletTransaction.Id,
                    WalletId = receiverWallet.Id,
                    Amount = amount,
                    EventType = EventType.TransferCompleted,
                    Timestamp = DateTimeOffset.UtcNow
                }
            };

            repo.OutboxMessage.CreateOutboxMessage(senderOutboxMessage);
            repo.OutboxMessage.CreateOutboxMessage(receiverOutboxMessage);

            await repo.CommitTransactionAsync(cancellationToken);


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

    public async Task<Result<List<WalletDto>>> GetUserWallets(Guid userId, CancellationToken token)
    {
        return await Result.TryAsync(_repo, async (repo, cancellationToken) =>
        {
            var userWallets = await repo.Wallet.GetUserWallets(userId, cancellationToken);

            var userWalletDtos = userWallets.Select(wa => wa.ToWalletDto()).ToList();

            return Result.Success(userWalletDtos);
        }, token);
    }

    public async Task<Result<List<WalletDto>>> GetAllWallets(CancellationToken token)
    {
        return await Result.TryAsync(_repo, async (repo, cancellationToken) =>
        {
            var wallets = await repo.Wallet.GetAllWallets(cancellationToken);
            return Result.Success(wallets.Select(wallet => wallet.ToWalletDto()).ToList());
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
}