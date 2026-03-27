namespace Sinag.Shared.Contracts;

public class PricingResponse
{
    public List<PricingCategory> Categories { get; set; } = new();
    public string EffectiveDate { get; set; } = string.Empty;
}

public class PricingCategory
{
    public string Category { get; set; } = string.Empty;
    public string Tier { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public decimal MinPricePhp { get; set; }
    public decimal MaxPricePhp { get; set; }
    public decimal DavaoDiscountPct { get; set; }
}
