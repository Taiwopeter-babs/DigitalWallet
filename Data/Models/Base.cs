using System.ComponentModel.DataAnnotations;


namespace DigitalWallet.Data;

public abstract class BaseModel
{
    [Key]
    public int Id { get; set; }

    [ConfigurationKeyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [ConfigurationKeyName("updated_at")]
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}