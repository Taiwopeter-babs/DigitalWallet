using System.Text.Json.Serialization;
using DigitalWallet.Enums;

namespace DigitalWallet.Data;

public record TransactionEventDto
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }

    [JsonPropertyName("user_id")]
    public required string UserId { get; init; }

    [JsonPropertyName("transaction_id")]
    public required int TransactionId { get; init; }

    [JsonPropertyName("wallet_id")]
    public required int WalletId { get; init; }

    [JsonPropertyName("amount")]
    public required decimal Amount { get; init; }

    [JsonPropertyName("event_type")]
    public required EventType EventType { get; init; }

    [JsonPropertyName("event_data")]
    public required EventData EventData { get; init; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; init; }
}

public record CreateTransactionEventDto
{
    [JsonPropertyName("user_id")]
    public required string UserId { get; init; }

    [JsonPropertyName("transaction_id")]
    public required int TransactionId { get; init; }

    [JsonPropertyName("wallet_id")]
    public required int WalletId { get; init; }

    [JsonPropertyName("amount")]
    public required decimal Amount { get; init; }

    [JsonPropertyName("event_type")]
    public required EventType EventType { get; init; }

    [JsonPropertyName("event_data")]
    public required EventData EventData { get; init; }
}

public record WalletTransferEventDto
{
    [JsonPropertyName("user_id")]
    public required string UserId { get; init; }

    [JsonPropertyName("transaction_id")]
    public required int TransactionId { get; init; }

    [JsonPropertyName("wallet_id")]
    public required int WalletId { get; init; }

    [JsonPropertyName("amount")]
    public required decimal Amount { get; init; }

    [JsonPropertyName("event_type")]
    public required EventType EventType { get; init; }

    [JsonPropertyName("timestamp")]
    public required DateTime Timestamp { get; init; }
}

public record WalletFundEventDto
{
    [JsonPropertyName("user_id")]
    public required string UserId { get; init; }

    [JsonPropertyName("transaction_id")]
    public required int TransactionId { get; init; }

    [JsonPropertyName("wallet_id")]
    public required int WalletId { get; init; }

    [JsonPropertyName("amount")]
    public required decimal Amount { get; init; }

    [JsonPropertyName("event_type")]
    public required EventType EventType { get; init; }

    [JsonPropertyName("timestamp")]
    public required DateTime Timestamp { get; init; }
}