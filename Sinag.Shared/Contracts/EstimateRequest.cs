namespace Sinag.Shared.Contracts;

public class EstimateRequest
{
    public int KwhConsumed { get; set; }
    public int BillingPeriodDays { get; set; }
    public int BillingMonth { get; set; }
    public decimal GenerationChargePerKwh { get; set; }
    public decimal TotalAmountDue { get; set; }
    public bool IncludeBattery { get; set; }
}
