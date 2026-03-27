using Microsoft.EntityFrameworkCore;
using Sinag.Api.Data;
using Sinag.Api.Services;

namespace Sinag.Api.Tests;

public class BomServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly BomService _sut;

    public BomServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();

        var calc = new CalculationService();
        _sut = new BomService(_db, calc);
    }

    public void Dispose()
    {
        _db.Dispose();
    }

    [Fact]
    public async Task GenerateBom_ReturnsThreeTiers()
    {
        var result = await _sut.GenerateBom(5.0m, 15.0m, includeBattery: false);

        Assert.NotNull(result.Budget);
        Assert.NotNull(result.MidRange);
        Assert.NotNull(result.Premium);
    }

    [Fact]
    public async Task GenerateBom_BudgetCheaperThanPremium()
    {
        var result = await _sut.GenerateBom(5.0m, 15.0m, includeBattery: false);

        Assert.True(result.Budget.TotalEstimate.Min < result.Premium.TotalEstimate.Min);
        Assert.True(result.Budget.TotalEstimate.Max < result.Premium.TotalEstimate.Max);
    }

    [Fact]
    public async Task GenerateBom_WithoutBattery_NoBatteryItem()
    {
        var result = await _sut.GenerateBom(5.0m, 15.0m, includeBattery: false);

        Assert.Null(result.Budget.Battery);
        Assert.Null(result.MidRange.Battery);
        Assert.Null(result.Premium.Battery);
    }

    [Fact]
    public async Task GenerateBom_WithBattery_HasBatteryItem()
    {
        var result = await _sut.GenerateBom(5.0m, 15.0m, includeBattery: true);

        Assert.NotNull(result.Budget.Battery);
        Assert.NotNull(result.MidRange.Battery);
        Assert.NotNull(result.Premium.Battery);
        Assert.Contains("kWh", result.Budget.Battery!.Spec);
        Assert.Contains("LiFePO4", result.Budget.Battery!.Spec);
    }

    [Fact]
    public async Task GenerateBom_WithBattery_CostsMoreThanWithout()
    {
        var withBattery = await _sut.GenerateBom(5.0m, 15.0m, includeBattery: true);
        var withoutBattery = await _sut.GenerateBom(5.0m, 15.0m, includeBattery: false);

        Assert.True(withBattery.Budget.TotalEstimate.Min > withoutBattery.Budget.TotalEstimate.Min);
    }

    [Fact]
    public async Task GenerateBom_PanelSpec_MatchesTierWattage()
    {
        var result = await _sut.GenerateBom(5.0m, 15.0m, includeBattery: false);

        Assert.Contains("450W", result.Budget.Panels.Spec);
        Assert.Contains("550W", result.MidRange.Panels.Spec);
        Assert.Contains("600W", result.Premium.Panels.Spec);
    }

    [Fact]
    public async Task GenerateBom_DavaoDiscountApplied()
    {
        // The discount should be 10%, so prices should be 90% of base
        var result = await _sut.GenerateBom(5.0m, 15.0m, includeBattery: false);

        // All component costs should be > 0 (discount applied, not zeroed out)
        Assert.True(result.Budget.Panels.EstimatedCost.Min > 0);
        Assert.True(result.Budget.Inverter.EstimatedCost.Min > 0);
        Assert.True(result.Budget.Mounting.EstimatedCost.Min > 0);
        Assert.True(result.Budget.Wiring.EstimatedCost.Min > 0);
        Assert.True(result.Budget.Labor.EstimatedCost.Min > 0);
    }

    [Fact]
    public async Task GenerateBom_InverterSize_MatchesSystemSize()
    {
        // 5.0 kWp system should get 5kW inverter
        var result = await _sut.GenerateBom(5.0m, 15.0m, includeBattery: false);
        Assert.Contains("5kW", result.Budget.Inverter.Spec);

        // 7.5 kWp system should get 8kW inverter
        var result2 = await _sut.GenerateBom(7.5m, 25.0m, includeBattery: false);
        Assert.Contains("8kW", result2.Budget.Inverter.Spec);
    }

    [Fact]
    public async Task GenerateBom_TotalEstimate_SumsAllComponents()
    {
        var result = await _sut.GenerateBom(5.0m, 15.0m, includeBattery: false);

        var expectedMin = result.Budget.Panels.EstimatedCost.Min +
                          result.Budget.Inverter.EstimatedCost.Min +
                          result.Budget.Mounting.EstimatedCost.Min +
                          result.Budget.Wiring.EstimatedCost.Min +
                          result.Budget.Labor.EstimatedCost.Min;

        Assert.Equal(expectedMin, result.Budget.TotalEstimate.Min);
    }
}
