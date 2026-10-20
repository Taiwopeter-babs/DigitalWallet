using System.Numerics;
using System.Text.Json.Serialization;
using DigitalWallet.Shared;

namespace DigitalWallet.DTO;

public record WalletDTO
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("user_id")]
    public string UserId { get; init; }

    [JsonPropertyName("balance")]
    public BigInteger Balance { get; init; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }

    [JsonPropertyName("updated_at")]
    public DateTime UpdatedAt { get; init; }
}

public record WalletTransactionDTO
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("id")]
    public int WalletId { get; init; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; init; }

    [JsonPropertyName("type")]
    public TransactionType Balance { get; init; }

    [JsonPropertyName("status")]
    public TransactionStatus UpdatedAt { get; init; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; init; }
}