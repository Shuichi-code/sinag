using Sinag.Api.Services;
using Sinag.Shared.Contracts;

namespace Sinag.Api.Tests;

public class FinancialServiceTests
{
    private readonly FinancialService _sut = new();

    [Fact]
    public void Calculate_UsesGenerationChargeNotBlendedRate()
    {
        var result = _sut.Calculate(
            systemSizeKwp: 5.0m,
            peakSunHours: 4.8m,
            kwhConsumed: 450,
            generationChargePerKwh: 6.52m,
            totalAmountDue: 6215.50m,
            systemCostRange: new CostRange { Min = 130000, Max = 180000 });

        // Monthly savings should be based on generation charge (6.52), not blended rate (~13.81)
        // Monthly production = 5.0 * 4.8 * 30 * 0.78 = 561.6 kWh
        // Offset = min(561.6, 450) = 450 kWh
        // Savings = 450 * 6.52 = 2934.00
        Assert.Equal(2934.00m, result.MonthlyGenerationSavings);
        Assert.Equal(450m, result.GenerationChargeOffsetKwh);
    }

    [Fact]
    public void Calculate_RemainingFixedCharges()
    {
        var result = _sut.Calculate(
            systemSizeKwp: 5.0m,
            peakSunHours: 4.8m,
            kwhConsumed: 450,
            generationChargePerKwh: 6.52m,
            totalAmountDue: 6215.50m,
            systemCostRange: new CostRange { Min = 130000, Max = 180000 });

        // Remaining = 6215.50 - 2934.00 = 3281.50
        Assert.Equal(3281.50m, result.RemainingFixedCharges);
        Assert.Equal(3281.50m, result.EstimatedMonthlyBillAfterSolar);
    }

    [Fact]
    public void Calculate_PaybackPeriod_UsesMidpointCost()
    {
        var result = _sut.Calculate(
            systemSizeKwp: 5.0m,
            peakSunHours: 4.8m,
            kwhConsumed: 450,
            generationChargePerKwh: 6.52m,
            totalAmountDue: 6215.50m,
            systemCostRange: new CostRange { Min = 130000, Max = 180000 });

        // Midpoint = (130000 + 180000) / 2 = 155000
        // Payback = ceil(155000 / 2934) = 53 months
        Assert.Equal(53, result.PaybackPeriodMonths);
    }

    [Fact]
    public void Calculate_LowConsumption_StillWorks()
    {
        var result = _sut.Calculate(
            systemSizeKwp: 1.0m,
            peakSunHours: 4.8m,
            kwhConsumed: 50,
            generationChargePerKwh: 6.52m,
            totalAmountDue: 800m,
            systemCostRange: new CostRange { Min = 50000, Max = 70000 });

        Assert.True(result.MonthlySavings > 0);
        Assert.True(result.PaybackPeriodMonths > 0);
    }

    [Fact]
    public void Calculate_HighConsumption()
    {
        var result = _sut.Calculate(
            systemSizeKwp: 16.0m,
            peakSunHours: 5.21m,
            kwhConsumed: 2000,
            generationChargePerKwh: 6.52m,
            totalAmountDue: 25000m,
            systemCostRange: new CostRange { Min = 400000, Max = 550000 });

        Assert.True(result.MonthlySavings > 0);
        Assert.True(result.PaybackPeriodMonths > 0);
    }

    [Fact]
    public void Calculate_SolarProductionExceedsConsumption_CapsOffset()
    {
        // Small consumption but big system — offset should be capped at consumption
        var result = _sut.Calculate(
            systemSizeKwp: 10.0m,
            peakSunHours: 5.0m,
            kwhConsumed: 100,
            generationChargePerKwh: 6.52m,
            totalAmountDue: 1500m,
            systemCostRange: new CostRange { Min = 200000, Max = 300000 });

        // Monthly production = 10 * 5 * 30 * 0.78 = 1170 kWh, but consumption is only 100
        Assert.Equal(100m, result.GenerationChargeOffsetKwh);
        Assert.Equal(652.00m, result.MonthlyGenerationSavings); // 100 * 6.52
    }

    [Fact]
    public void Calculate25YearSavings_IsPositiveForTypicalCase()
    {
        var result = _sut.Calculate25YearSavings(
            baseMonthlySavings: 2934.00m,
            systemCost: 155000m);

        // Over 25 years with rate increases, savings should far exceed system cost
        Assert.True(result > 0);
        Assert.True(result > 500000); // Should be substantial
    }

    [Fact]
    public void Calculate25YearSavings_AccountsForDegradationAndRateIncrease()
    {
        var savings = _sut.Calculate25YearSavings(
            baseMonthlySavings: 1000m,
            systemCost: 0m);

        // Without degradation or rate increase: 1000 * 12 * 25 = 300,000
        // With +3% rate increase, savings should be higher than flat
        // With -0.5% degradation, savings should be slightly lower
        // Net effect: rate increase dominates, so should be > 300,000
        Assert.True(savings > 300000);
    }
}
