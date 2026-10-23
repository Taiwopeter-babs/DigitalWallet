using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using System.Text.Json.Serialization;

namespace DigitalWallet.Data;

[Table("wallets")]
public class Wallet : BaseModel
{
    [Required]
    public string UserId { get; init; }

    [Required]
    public decimal Balance { get; set; }

    [JsonIgnore]
    public uint Version { get; set; }

    [JsonIgnore]
    public ICollection<WalletTransaction> Transactions { get; set; } =
        new List<WalletTransaction>();
}