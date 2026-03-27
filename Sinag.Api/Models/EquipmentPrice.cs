using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sinag.Api.Models;

[Table("equipment_prices")]
public class EquipmentPrice
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("category")]
    public string Category { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    [Column("tier")]
    public string Tier { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    [Column("unit")]
    public string Unit { get; set; } = string.Empty;

    [Column("min_price_php")]
    public decimal MinPricePhp { get; set; }

    [Column("max_price_php")]
    public decimal MaxPricePhp { get; set; }

    [Column("davao_discount_pct")]
    public decimal DavaoDiscountPct { get; set; } = 10.00m;

    [MaxLength(100)]
    [Column("source")]
    public string? Source { get; set; }

    [Column("effective_date")]
    public DateOnly EffectiveDate { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
