using Sinag.Api.Services;

namespace Sinag.Api.Tests;

public class CalculationServiceTests
{
    private readonly CalculationService _sut = new();

    [Theory]
    [InlineData(450, 30, 15.0)]
    [InlineData(300, 30, 10.0)]
    [InlineData(900, 30, 30.0)]
    [InlineData(450, 31, 14.52)] // rounded
    public void CalculateDailyConsumption_ReturnsCorrectValue(int kwh, int days, decimal expected)
    {
        var result = _sut.CalculateDailyConsumption(kwh, days);
        Assert.Equal(expected, Math.Round(result, 2));
    }

    [Fact]
    public void CalculateSystemSizeKwp_StandardCase()
    {
        // 15 kWh/day, 4.8 peak sun hours
        // Required output = 15 / 0.78 = 19.23 kWh
        // System size = 19.23 / 4.8 = 4.0 kWp
        var result = _sut.CalculateSystemSizeKwp(15.0m, 4.8m);
        Assert.Equal(4.0m, result);
    }

    [Fact]
    public void CalculateSystemSizeKwp_HighConsumption()
    {
        // 2000 kWh / 30 days = 66.67 kWh/day
        var daily = _sut.CalculateDailyConsumption(2000, 30);
        var result = _sut.CalculateSystemSizeKwp(daily, 5.21m);
        Assert.True(result > 10);
    }

    [Fact]
    public void CalculateSystemSizeKwp_LowConsumption()
    {
        // 50 kWh / 30 days = 1.67 kWh/day
        var daily = _sut.CalculateDailyConsumption(50, 30);
        var result = _sut.CalculateSystemSizeKwp(daily, 4.8m);
        Assert.True(result > 0);
        Assert.True(result < 2);
    }

    [Theory]
    [InlineData(450, 4000, 9)]   // 4.0 kWp: ceil(4000/450) = 9
    [InlineData(550, 4000, 8)]   // ceil(4000/550) = 8
    [InlineData(600, 4000, 7)]   // ceil(4000/600) = 7
    [InlineData(450, 5000, 12)]  // 5.0 kWp: ceil(5000/450) = 12
    public void CalculatePanelCount_ReturnsCorrectCount(int wattage, int systemWatts, int expected)
    {
        var systemKwp = (decimal)systemWatts / 1000;
        var result = _sut.CalculatePanelCount(systemKwp, wattage);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(2.5, 3)]
    [InlineData(3.0, 3)]
    [InlineData(4.8, 5)]
    [InlineData(5.0, 5)]
    [InlineData(7.5, 8)]
    [InlineData(9.0, 10)]
    [InlineData(12.0, 15)]
    [InlineData(16.0, 15)] // caps at 15
    public void SelectInverterSizeKw_RoundsUpToStandardSize(decimal systemKwp, int expected)
    {
        var result = _sut.SelectInverterSizeKw(systemKwp);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(15.0, 5)]   // 15 * 0.33 = 4.95 → 5 kWh
    [InlineData(20.0, 10)]  // 20 * 0.33 = 6.6 → 10 kWh
    [InlineData(30.0, 10)]  // 30 * 0.33 = 9.9 → 10 kWh
    [InlineData(45.0, 15)]  // 45 * 0.33 = 14.85 → 15 kWh
    [InlineData(60.0, 20)]  // 60 * 0.33 = 19.8 → 20 kWh
    [InlineData(70.0, 20)]  // 70 * 0.33 = 23.1 → caps at 20 kWh
    public void SelectBatterySizeKwh_RoundsUpToStandardSize(decimal dailyKwh, int expected)
    {
        var result = _sut.SelectBatterySizeKwh(dailyKwh);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void CalculateMonthlyProduction_StandardCase()
    {
        // 5 kWp * 4.8 hours * 30 days * 0.78 = 561.6 kWh
        var result = _sut.CalculateMonthlyProduction(5.0m, 4.8m);
        Assert.Equal(561.6m, result);
    }
}
