namespace Sinag.Shared.Contracts;

public class EstimateResponse
{
    public decimal SystemSizeKwp { get; set; }
    public decimal DailyConsumptionKwh { get; set; }
    public decimal PeakSunHours { get; set; }
    public BomResult Bom { get; set; } = new();
    public FinancialResult Financial { get; set; } = new();
    public EstimateMetadata Metadata { get; set; } = new();
}

public class BomResult
{
    public BomTier Budget { get; set; } = new();
    public BomTier MidRange { get; set; } = new();
    public BomTier Premium { get; set; } = new();
}

public class FinancialResult
{
    public decimal CurrentMonthlyBill { get; set; }
    public decimal GenerationChargeOffsetKwh { get; set; }
    public decimal MonthlyGenerationSavings { get; set; }
    public decimal RemainingFixedCharges { get; set; }
    public decimal EstimatedMonthlyBillAfterSolar { get; set; }
    public decimal MonthlySavings { get; set; }
    public int PaybackPeriodMonths { get; set; }
    public decimal TwentyFiveYearSavings { get; set; }
}

public class EstimateMetadata
{
    public string PricingAsOf { get; set; } = string.Empty;
    public int IrradianceMonth { get; set; }
    public string PeakSunHoursSource { get; set; } = string.Empty;
    public bool DavaoDiscountApplied { get; set; }
    public string Disclaimer { get; set; } = "Prices are estimates based on current Davao market rates. Actual costs may vary by installer.";
}
