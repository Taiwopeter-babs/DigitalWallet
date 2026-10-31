using DigitalWallet.Data;

namespace DigitalWallet.Repository;

public interface IOutboxMessageRepository
{
    void CreateOutboxMessage(OutboxMessage message);
    Task UpdateOutboxMessage(OutboxMessage message);
}