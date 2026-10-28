using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace DigitalWallet.Data;

[Table("inbox_messages")]
public class InboxMessage : BaseModel
{
    [Required]
    public required string Topic { get; set; }

    [Required]
    public required string Key { get; set; }

    public bool? Received { get; set; }

    public DateTimeOffset? ReceivedAt { get; set; }

    public Guid? CorrelationId { get; set; }

    [Required]
    public int OutboxId { get; set; }

    public OutboxMessage OutboxMessage { get; set; } = null!;

}
