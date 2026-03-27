using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sinag.Api.Models;

[Table("saved_estimates")]
public class SavedEstimate
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(100)]
    [Column("contact_email")]
    public string? ContactEmail { get; set; }

    [Column("kwh_consumed")]
    public int KwhConsumed { get; set; }

    [Column("system_size_kwp")]
    public decimal SystemSizeKwp { get; set; }

    [Column("estimate_json")]
    public string EstimateJson { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("expires_at")]
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(90);
}
