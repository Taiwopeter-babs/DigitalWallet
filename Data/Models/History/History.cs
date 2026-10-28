using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using DigitalWallet.Enums;
using DigitalWallet.Utils;

namespace DigitalWallet.Data;

[Table("transaction_events")]
public class TransactionEvent : BaseModel
{
    [Required]
    public int WalletId { get; set; }

    [Required]
    public required string UserId { get; set; }

    public decimal? Amount { get; set; }

    [Required]
    public EventType EventType { get; set; }

    public int? TransactionId { get; set; }

    [Required]
    public required TransactionEventData EventData { get; set; }
}

[ComplexType]
public class TransactionEventData
{
    public int? TransactionId { get; set; }
    public int WalletId { get; set; }

    [MaxLength(50)]
    public required string UserId { get; set; }

    public decimal? Amount { get; set; }

    public Guid? CorrelationId { get; set; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required EventType EventType { get; set; }

    [JsonConverter(typeof(IsoDateTimeConverter))]
    public required DateTimeOffset Timestamp { get; set; }
}