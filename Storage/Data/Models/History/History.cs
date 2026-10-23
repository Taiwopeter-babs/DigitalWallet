using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DigitalWallet.Enums;

namespace DigitalWallet.Data;

[Table("transaction_events")]
public class TransactionEvent : BaseModel
{
    [Required]
    public int WalletId { get; set; }

    [Required]
    public string UserId { get; set; }

    [Required]
    public decimal Amount { get; set; }

    [Required]
    public EventType EventType { get; set; }

    [Required]
    public int TransactionId { get; set; }

    [Required]
    public required EventData EventData { get; set; }
}

public record EventData
{
    public int TransactionId { get; init; }
    public int WalletId { get; init; }
    public required string UserId { get; init; }
    public required decimal Amount { get; init; }
    public required EventType Type { get; init; }
    public required DateTime TimeStamp { get; init; }
}