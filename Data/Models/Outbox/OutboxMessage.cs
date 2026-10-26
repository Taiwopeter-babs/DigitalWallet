using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace DigitalWallet.Data;

[Table("outbox")]
public class OutboxMessage : BaseModel
{
    [Required]
    public required string Topic { get; set; }

    [Required]
    public required string Key { get; set; }

    [Required]
    public required OutboxMessagePayload Payload { get; set; }

    [Required]
    public bool Processed { get; set; }

    public DateTimeOffset? ProcessedAt { get; set; }
    public string? ErrorMessage { get; set; }
}

[ComplexType]
public class OutboxMessagePayload : TransactionEventData
{


}