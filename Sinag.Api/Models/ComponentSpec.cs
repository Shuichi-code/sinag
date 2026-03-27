using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sinag.Api.Models;

[Table("component_specs")]
public class ComponentSpec
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

    [Column("wattage_or_capacity")]
    public decimal? WattageOrCapacity { get; set; }

    [MaxLength(10)]
    [Column("unit")]
    public string? Unit { get; set; }

    [MaxLength(100)]
    [Column("spec_label_template")]
    public string? SpecLabelTemplate { get; set; }

    [Column("available_sizes")]
    public string? AvailableSizesJson { get; set; }

    [Column("effective_date")]
    public DateOnly EffectiveDate { get; set; }
}
