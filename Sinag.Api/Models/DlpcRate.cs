using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sinag.Api.Models;

[Table("dlpc_rates")]
public class DlpcRate
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("component")]
    public string Component { get; set; } = string.Empty;

    [Column("rate_per_kwh")]
    public decimal RatePerKwh { get; set; }

    [Column("effective_date")]
    public DateOnly EffectiveDate { get; set; }

    [MaxLength(100)]
    [Column("source")]
    public string? Source { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
