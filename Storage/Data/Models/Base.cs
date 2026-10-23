using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;

namespace DigitalWallet.Data;

public abstract class BaseModel
{
    [Key]
    public int Id { get; set; }

    [ConfigurationKeyName("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [ConfigurationKeyName("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}