namespace Sinag.Shared.Contracts;

public class BomTier
{
    public BomItem Panels { get; set; } = new();
    public BomItem Inverter { get; set; } = new();
    public BomItem? Battery { get; set; }
    public BomItem Mounting { get; set; } = new();
    public BomItem Wiring { get; set; } = new();
    public BomItem Labor { get; set; } = new();
    public CostRange TotalEstimate { get; set; } = new();
}

public class BomItem
{
    public string Spec { get; set; } = string.Empty;
    public CostRange EstimatedCost { get; set; } = new();
}

public class CostRange
{
    public decimal Min { get; set; }
    public decimal Max { get; set; }
}
