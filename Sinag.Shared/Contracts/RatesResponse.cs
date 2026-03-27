namespace Sinag.Shared.Contracts;

public class RatesResponse
{
    public List<RateComponent> Components { get; set; } = new();
    public string EffectiveDate { get; set; } = string.Empty;
    public decimal TotalBlendedRate { get; set; }
}

public class RateComponent
{
    public string Component { get; set; } = string.Empty;
    public decimal RatePerKwh { get; set; }
}
