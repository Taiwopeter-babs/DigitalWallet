using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using System.Text.Json.Serialization;


namespace DigitalWallet.Models;

[Table("wallets")]
public class Wallet : BaseModel
{
    [Required]
    public string UserId { get; init; }

    public decimal Balance { get; set; }

    [JsonIgnore]
    public BigInteger Version { get; set; }
}