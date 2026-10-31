using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DigitalWallet.Data;

[Table("wallets")]
public class Wallet : BaseModel
{
    [Required]
    public required string UserId { get; init; }

    [Required]
    public decimal Balance { get; set; }

    [JsonIgnore]
    public uint Version { get; set; }

    [JsonIgnore]
    public ICollection<WalletTransaction> Transactions { get; set; } =
        [];
}