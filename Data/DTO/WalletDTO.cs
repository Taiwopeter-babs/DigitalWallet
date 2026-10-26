using System.Text.Json.Serialization;
using DigitalWallet.Enums;

namespace DigitalWallet.Data;

public record WalletDto
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("user_id")]
    public string UserId { get; init; }

    [JsonPropertyName("balance")]
    public decimal Balance { get; init; }

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; init; }

    [JsonPropertyName("updated_at")]
    public DateTimeOffset UpdatedAt { get; init; }
}

public record CreateWalletDto
{
    [JsonPropertyName("user_id")]
    public string UserId { get; init; }
}

public record FundWalletDto
{
    [JsonPropertyName("amount")]
    public decimal Amount { get; init; }

    [JsonPropertyName("walletId")]
    public decimal WalletId { get; init; }
}

public record WalletTransferDto
{
    [JsonPropertyName("amount")]
    public decimal Amount { get; init; }

    [JsonPropertyName("sender_wallet_id")]
    public int SenderWalletId { get; init; }

    [JsonPropertyName("receiver_wallet_id")]
    public int ReceiverWalletId { get; init; }
}

public record WalletTransactionDto
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }

    [JsonPropertyName("wallet_id")]
    public required int WalletId { get; init; }

    [JsonPropertyName("amount")]
    public required decimal Amount { get; init; }

    [JsonPropertyName("status")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required TransactionStatus Status { get; init; }

    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required TransactionType Type { get; init; }

    [JsonPropertyName("timestamp")]
    public required DateTimeOffset Timestamp { get; init; }
}

public record CreateWalletTransactionDto
{
    public int Id { get; init; }
    public required int WalletId { get; init; }

    public required decimal Amount { get; init; }

    public required TransactionStatus Status { get; init; }

    public required TransactionType Type { get; init; }
}