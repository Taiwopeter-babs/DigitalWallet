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
    public DateTime CreatedAt { get; init; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; init; }
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

public record WalletTransactionDto
{
    [JsonPropertyName("id")]
    public required int Id { get; init; }

    [JsonPropertyName("wallet_id")]
    public required int WalletId { get; init; }

    [JsonPropertyName("amount")]
    public required decimal Amount { get; init; }

    [JsonPropertyName("status")]
    public required TransactionStatus Status { get; init; }

    [JsonPropertyName("type")]
    public required TransactionType Type { get; init; }

    [JsonPropertyName("timestamp")]
    public required DateTime Timestamp { get; init; }
}

public record CreateWalletTransactionDto
{
    [JsonPropertyName("wallet_id")]
    public required int WalletId { get; init; }

    [JsonPropertyName("amount")]
    public required decimal Amount { get; init; }

    [JsonPropertyName("status")]
    public required TransactionStatus Status { get; init; }

    [JsonPropertyName("type")]
    public required TransactionType Type { get; init; }
}