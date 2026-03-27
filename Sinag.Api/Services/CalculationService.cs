namespace Sinag.Api.Services;

public class CalculationService
{
    private const decimal PerformanceRatio = 0.78m;

    public decimal CalculateDailyConsumption(int kwhConsumed, int billingPeriodDays)
    {
        return (decimal)kwhConsumed / billingPeriodDays;
    }

    public decimal CalculateSystemSizeKwp(decimal dailyConsumptionKwh, decimal peakSunHours)
    {
        var requiredDailyOutput = dailyConsumptionKwh / PerformanceRatio;
        var systemSizeKwp = requiredDailyOutput / peakSunHours;
        return Math.Round(systemSizeKwp, 1);
    }

    public decimal CalculateMonthlyProduction(decimal systemSizeKwp, decimal peakSunHours)
    {
        return systemSizeKwp * peakSunHours * 30 * PerformanceRatio;
    }

    public int CalculatePanelCount(decimal systemSizeKwp, int panelWattage)
    {
        return (int)Math.Ceiling(systemSizeKwp * 1000 / panelWattage);
    }

    public int SelectInverterSizeKw(decimal systemSizeKwp)
    {
        int[] standardSizes = [3, 5, 8, 10, 15];
        foreach (var size in standardSizes)
        {
            if (size >= systemSizeKwp)
                return size;
        }
        return standardSizes[^1];
    }

    public int SelectBatterySizeKwh(decimal dailyConsumptionKwh)
    {
        var requiredCapacity = dailyConsumptionKwh * 0.33m;
        int[] standardSizes = [5, 10, 15, 20];
        foreach (var size in standardSizes)
        {
            if (size >= requiredCapacity)
                return size;
        }
        return standardSizes[^1];
    }
}
