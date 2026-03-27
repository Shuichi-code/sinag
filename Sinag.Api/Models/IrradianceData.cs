using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sinag.Api.Models;

[Table("irradiance_cache")]
public class IrradianceData
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("latitude")]
    public decimal Latitude { get; set; }

    [Column("longitude")]
    public decimal Longitude { get; set; }

    [Column("month")]
    public int Month { get; set; }

    [Column("ghi_kwh_m2_day")]
    public decimal GhiKwhM2Day { get; set; }

    [Column("peak_sun_hours")]
    public decimal PeakSunHours { get; set; }

    [MaxLength(50)]
    [Column("source")]
    public string? Source { get; set; }

    [Column("fetched_at")]
    public DateTime FetchedAt { get; set; } = DateTime.UtcNow;
}
