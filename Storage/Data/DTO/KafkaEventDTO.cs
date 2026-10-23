using System.Text.Json.Serialization;
using DigitalWallet.Enums;


namespace DigitalWallet.Data;

public record KafkaWalletFundEventDto
{
    [JsonPropertyName("user_id")]
    public required string UserId { get; init; }


    [JsonPropertyName("wallet_id")]
    public required int WalletId { get; init; }


    [JsonPropertyName("transaction_id")]
    public required int TransactionId { get; init; }


    [JsonPropertyName("amount")]
    public required decimal Amount { get; init; }

    [JsonPropertyName("event_type")]
    public required EventType EventType { get; init; }


    [JsonPropertyName("transaction_status")]
    public required TransactionStatus TransactionStatus { get; init; }

    [JsonPropertyName("transaction_type")]
    public required TransactionType TransactionType { get; init; }

    [JsonPropertyName("timestamp")]
    public required DateTime Timestamp { get; init; }
}

public record KafkaWalletTransferEventDto
{
    [JsonPropertyName("sender_id")]
    public required string SenderId { get; init; }

    [JsonPropertyName("receiver_id")]
    public required string ReceiverId { get; init; }

    [JsonPropertyName("sender_wallet_id")]
    public required int SenderWalletId { get; init; }

    [JsonPropertyName("receiver_wallet_id")]
    public required int ReceiverWalletId { get; init; }

    [JsonPropertyName("transaction_id")]
    public required int TransactionId { get; init; }


    [JsonPropertyName("amount")]
    public required decimal Amount { get; init; }

    [JsonPropertyName("event_type")]
    public required EventType EventType { get; init; }


    [JsonPropertyName("transaction_status")]
    public required TransactionStatus TransactionStatus { get; init; }

    [JsonPropertyName("transaction_type")]
    public required TransactionType TransactionType { get; init; }

    [JsonPropertyName("timestamp")]
    public required DateTime Timestamp { get; init; }
}