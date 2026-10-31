using DigitalWallet.Data;

namespace DigitalWallet.Repository;

public class InboxMessageRepository : RepositoryBase<InboxMessage>, IInboxMessageRepository
{
    public InboxMessageRepository(DatabaseContext context) : base(context)
    {
    }

    public void CreateInboxMessage(InboxMessage message)
    {
        Create(message);
    }

    public async Task UpdateInboxMessage(InboxMessage message)
    {
        _context.InboxMessages.Update(message);
    }
}