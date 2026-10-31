using System.Text.Json.Serialization;
using DigitalWallet.Enums;
using DigitalWallet.Utils;

namespace DigitalWallet.Data;

public record TransactionEventDto
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }

    [JsonPropertyName("user_id")]
    public required string UserId { get; init; }

    [JsonPropertyName("transaction_id")]
    public int? TransactionId { get; init; }

    [JsonPropertyName("wallet_id")]
    public required int WalletId { get; init; }

    [JsonPropertyName("amount")]
    public decimal? Amount { get; init; }

    [JsonPropertyName("event_type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required EventType EventType { get; init; }

    [JsonPropertyName("event_data")]
    public required TransactionEventData EventData { get; init; }

    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; init; }
}

public record CreateTransactionEventDto
{
    [JsonPropertyName("user_id")]
    public required string UserId { get; init; }

    [JsonPropertyName("transaction_id")]
    public int? TransactionId { get; init; }

    [JsonPropertyName("wallet_id")]
    public required int WalletId { get; init; }

    [JsonPropertyName("amount")]
    public decimal? Amount { get; init; }

    [JsonPropertyName("event_type")]
    public required EventType EventType { get; init; }

    [JsonPropertyName("event_data")]
    public required TransactionEventData EventData { get; init; }
}

public record WalletEventDto
{

    public required string UserId { get; init; }

    public int? TransactionId { get; init; }


    public required int WalletId { get; init; }

    public decimal? Amount { get; init; }

    public Guid? CorrelationId { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required EventType EventType { get; init; }

    [JsonConverter(typeof(IsoDateTimeConverter))]
    public required DateTimeOffset Timestamp { get; init; }
}