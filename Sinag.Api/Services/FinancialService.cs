using Sinag.Shared.Contracts;

namespace Sinag.Api.Services;

public class FinancialService
{
    private const decimal AnnualDegradationRate = 0.005m;  // 0.5%/year
    private const decimal AnnualRateIncrease = 0.03m;       // 3%/year

    public FinancialResult Calculate(
        decimal systemSizeKwp,
        decimal peakSunHours,
        int kwhConsumed,
        decimal generationChargePerKwh,
        decimal totalAmountDue,
        CostRange systemCostRange)
    {
        var pr = 0.78m;
        var monthlySolarProduction = systemSizeKwp * peakSunHours * 30 * pr;
        var monthlyOffsetKwh = Math.Min(monthlySolarProduction, kwhConsumed);
        var monthlyGenerationSavings = Math.Round(monthlyOffsetKwh * generationChargePerKwh, 2);
        var remainingFixedCharges = Math.Round(totalAmountDue - monthlyGenerationSavings, 2);

        // Use midpoint of cost range for payback calculation
        var midSystemCost = (systemCostRange.Min + systemCostRange.Max) / 2;
        var paybackMonths = monthlyGenerationSavings > 0
            ? (int)Math.Ceiling(midSystemCost / monthlyGenerationSavings)
            : 0;

        var twentyFiveYearSavings = Calculate25YearSavings(
            monthlyGenerationSavings, midSystemCost);

        return new FinancialResult
        {
            CurrentMonthlyBill = totalAmountDue,
            GenerationChargeOffsetKwh = Math.Round(monthlyOffsetKwh, 2),
            MonthlyGenerationSavings = monthlyGenerationSavings,
            RemainingFixedCharges = remainingFixedCharges,
            EstimatedMonthlyBillAfterSolar = Math.Max(0, remainingFixedCharges),
            MonthlySavings = monthlyGenerationSavings,
            PaybackPeriodMonths = paybackMonths,
            TwentyFiveYearSavings = twentyFiveYearSavings,
        };
    }

    public decimal Calculate25YearSavings(decimal baseMonthlySavings, decimal systemCost)
    {
        var totalSavings = 0m;
        var currentAnnualSavings = baseMonthlySavings * 12;

        for (var year = 1; year <= 25; year++)
        {
            // Apply degradation (production decreases)
            var degradationFactor = 1 - (AnnualDegradationRate * year);
            // Apply rate increase (electricity gets more expensive)
            var rateFactor = (decimal)Math.Pow((double)(1 + AnnualRateIncrease), year);

            var yearSavings = currentAnnualSavings * degradationFactor * rateFactor;
            totalSavings += yearSavings;
        }

        return Math.Round(totalSavings - systemCost, 2);
    }
}
