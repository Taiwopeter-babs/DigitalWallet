namespace DigitalWallet.Data;

public static class HistoryExtension
{
    extension(TransactionEvent transactionEvent)
    {
        public TransactionEventDto ToTransactionEventDto()
        {
            return new TransactionEventDto
            {
                Id = transactionEvent.Id,
                UserId = transactionEvent.UserId,
                TransactionId = transactionEvent.TransactionId,
                WalletId = transactionEvent.WalletId,
                Amount = transactionEvent.Amount,
                EventType = transactionEvent.EventType,
                EventData = transactionEvent.EventData,
                Timestamp = transactionEvent.CreatedAt
            };
        }
    }

    extension(TransactionEventDto eventDto)
    {
        public TransactionEvent ToTransactionEvent()
        {
            return new TransactionEvent
            {
                Id = eventDto.Id,
                UserId = eventDto.UserId,
                TransactionId = eventDto.TransactionId,
                WalletId = eventDto.WalletId,
                Amount = eventDto.Amount,
                EventType = eventDto.EventType,
                EventData = eventDto.EventData
            };
        }
    }

    extension(CreateTransactionEventDto eventDto)
    {
        public TransactionEvent ToTransactionEvent()
        {
            return new TransactionEvent
            {
                UserId = eventDto.UserId,
                TransactionId = eventDto.TransactionId,
                WalletId = eventDto.WalletId,
                Amount = eventDto.Amount,
                EventType = eventDto.EventType,
                EventData = eventDto.EventData
            };
        }
    }

    extension(WalletEventDto eventDto)
    {
        public CreateTransactionEventDto ToCreateTransactionEventDto()
        {
            return new CreateTransactionEventDto
            {
                UserId = eventDto.UserId,
                TransactionId = eventDto.TransactionId,
                WalletId = eventDto.WalletId,
                Amount = eventDto.Amount,
                EventType = eventDto.EventType,
                EventData = new TransactionEventData
                {
                    Amount = eventDto.Amount,
                    EventType = eventDto.EventType,
                    UserId = eventDto.UserId,
                    TransactionId = eventDto.TransactionId,
                    Timestamp = eventDto.Timestamp,
                    WalletId = eventDto.WalletId, CorrelationId = eventDto.CorrelationId

                }
            };
        }
    }
}