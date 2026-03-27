namespace Sinag.App.Models;

public enum ConfidenceLevel
{
    High,
    Medium,
    Low
}

public class BillData
{
    public int KwhConsumed { get; set; }
    public int BillingPeriodDays { get; set; }
    public int BillingMonth { get; set; }
    public decimal GenerationChargePerKwh { get; set; }
    public decimal TotalAmountDue { get; set; }
    public bool IsDlpcBill { get; set; }
    public Dictionary<string, ConfidenceLevel> FieldConfidence { get; set; } = new();
}
