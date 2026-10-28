

using DigitalWallet.Data;

namespace DigitalWallet.Services.KafkaService.WalletWorkerService;

public interface IWalletWorkerService
{
    Task<bool> SaveTransactionAndInboxData(CreateTransactionEventDto transactionEventDto, CancellationToken token);
}
