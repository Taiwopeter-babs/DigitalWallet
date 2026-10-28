using DigitalWallet.Data;

namespace DigitalWallet.Repository;

public interface IInboxMessageRepository
{
    void CreateInboxMessage(InboxMessage message);
    Task UpdateInboxMessage(InboxMessage message);
}