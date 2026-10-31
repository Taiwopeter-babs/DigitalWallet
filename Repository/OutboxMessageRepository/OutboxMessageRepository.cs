using DigitalWallet.Data;

namespace DigitalWallet.Repository;

public class OutboxMessageRepository : RepositoryBase<OutboxMessage>, IOutboxMessageRepository
{
    public OutboxMessageRepository(DatabaseContext context) : base(context)
    {
    }

    public void CreateOutboxMessage(OutboxMessage message)
    {
        Create(message);
    }

    public async Task UpdateOutboxMessage(OutboxMessage message)
    {
        _context.OutboxMessages.Update(message);
    }
}