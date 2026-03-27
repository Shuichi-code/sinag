using System.Text.RegularExpressions;
using Sinag.App.Models;

namespace Sinag.App.Features.Scan;

public class BillParseException : Exception
{
    public BillParseException(string message) : base(message) { }
}

public class BillParser
{
    private static readonly Regex KwhRegex = new(@"(\d{1,4})\s*[kK][wW][hH]|[kK][wW][hH]\s*(\d{1,4})", RegexOptions.Compiled);
    private static readonly Regex AmountRegex = new(@"[\d,]+\.\d{2}", RegexOptions.Compiled);
    private static readonly Regex GenerationRateRegex = new(@"(\d+\.\d{2,4})", RegexOptions.Compiled);

    private static readonly string[] MonthNames =
    {
        "January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December",
        "Jan", "Feb", "Mar", "Apr", "May", "Jun",
        "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"
    };

    public BillData Parse(string recognizedText)
    {
        if (string.IsNullOrWhiteSpace(recognizedText))
            throw new BillParseException("No text recognized from the image.");

        var upperText = recognizedText.ToUpperInvariant();

        // Validate DLPC bill
        if (!upperText.Contains("DAVAO LIGHT") && !upperText.Contains("DLPC"))
            throw new BillParseException("Not a DLPC bill");

        var billData = new BillData
        {
            IsDlpcBill = true,
            BillingPeriodDays = 30, // default
            GenerationChargePerKwh = 6.52m // default DLPC generation rate
        };

        // Extract kWh
        ExtractKwh(recognizedText, billData);

        // Extract Total Amount Due
        ExtractTotalAmount(recognizedText, billData);

        // Extract Generation Charge
        ExtractGenerationCharge(recognizedText, billData);

        // Extract Billing Month
        ExtractBillingMonth(recognizedText, billData);

        return billData;
    }

    private void ExtractKwh(string text, BillData billData)
    {
        var match = KwhRegex.Match(text);
        if (match.Success)
        {
            var value = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
            if (int.TryParse(value, out var kwh) && kwh >= 1 && kwh <= 5000)
            {
                billData.KwhConsumed = kwh;
                billData.FieldConfidence["KwhConsumed"] = ConfidenceLevel.High;
                return;
            }
        }
        billData.FieldConfidence["KwhConsumed"] = ConfidenceLevel.Low;
    }

    private void ExtractTotalAmount(string text, BillData billData)
    {
        var lines = text.Split('\n');
        foreach (var line in lines)
        {
            var upper = line.ToUpperInvariant();
            if (upper.Contains("TOTAL AMOUNT DUE") || upper.Contains("TOTAL"))
            {
                var amountMatch = AmountRegex.Match(line);
                if (amountMatch.Success)
                {
                    var amountStr = amountMatch.Value.Replace(",", "");
                    if (decimal.TryParse(amountStr, out var amount) && amount >= 100 && amount <= 500_000)
                    {
                        billData.TotalAmountDue = amount;
                        billData.FieldConfidence["TotalAmountDue"] = ConfidenceLevel.High;
                        return;
                    }
                }
            }
        }

        // Fallback: find any large currency amount
        var allAmounts = AmountRegex.Matches(text);
        foreach (Match m in allAmounts)
        {
            var amountStr = m.Value.Replace(",", "");
            if (decimal.TryParse(amountStr, out var amount) && amount >= 100 && amount <= 500_000)
            {
                billData.TotalAmountDue = amount;
                billData.FieldConfidence["TotalAmountDue"] = ConfidenceLevel.Low;
                return;
            }
        }

        billData.FieldConfidence["TotalAmountDue"] = ConfidenceLevel.Low;
    }

    private void ExtractGenerationCharge(string text, BillData billData)
    {
        var lines = text.Split('\n');
        foreach (var line in lines)
        {
            var upper = line.ToUpperInvariant();
            if (upper.Contains("GENERATION"))
            {
                var rateMatch = GenerationRateRegex.Match(line);
                if (rateMatch.Success)
                {
                    if (decimal.TryParse(rateMatch.Value, out var rate) && rate >= 0.01m && rate <= 50m)
                    {
                        billData.GenerationChargePerKwh = rate;
                        billData.FieldConfidence["GenerationChargePerKwh"] = ConfidenceLevel.High;
                        return;
                    }
                }
            }
        }
        billData.FieldConfidence["GenerationChargePerKwh"] = ConfidenceLevel.Medium;
    }

    private void ExtractBillingMonth(string text, BillData billData)
    {
        foreach (var month in MonthNames)
        {
            if (text.Contains(month, StringComparison.OrdinalIgnoreCase))
            {
                var monthIndex = Array.FindIndex(MonthNames, m =>
                    m.Equals(month, StringComparison.OrdinalIgnoreCase));
                // Map to 1-12 (first 12 are full names, next 12 are abbreviations)
                billData.BillingMonth = (monthIndex % 12) + 1;
                billData.FieldConfidence["BillingMonth"] = ConfidenceLevel.Medium;
                return;
            }
        }
        billData.BillingMonth = DateTime.Now.Month;
        billData.FieldConfidence["BillingMonth"] = ConfidenceLevel.Low;
    }
}
