using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using DigitalWallet.Enums;

namespace DigitalWallet.Data;

[Table("wallet_transactions")]
public class WalletTransaction : BaseModel
{
    [Required]
    public decimal Amount { get; set; }

    [Required]
    public TransactionType Type { get; set; }


    [Required]
    public TransactionStatus Status { get; set; }

    [Required]
    public int WalletId { get; set; }

    public Guid? CorrelationId { get; set; }

    [JsonIgnore]
    public Wallet Wallet { get; set; } = null!;
}