using Sinag.Api.Data;
using Sinag.Shared.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Sinag.Api.Services;

public class BomService
{
    private readonly AppDbContext _db;
    private readonly CalculationService _calc;

    public BomService(AppDbContext db, CalculationService calc)
    {
        _db = db;
        _calc = calc;
    }

    public async Task<BomResult> GenerateBom(decimal systemSizeKwp, decimal dailyConsumptionKwh, bool includeBattery)
    {
        var prices = await _db.EquipmentPrices.ToListAsync();
        var specs = await _db.ComponentSpecs.ToListAsync();

        return new BomResult
        {
            Budget = BuildTier("budget", systemSizeKwp, dailyConsumptionKwh, includeBattery, prices, specs),
            MidRange = BuildTier("mid_range", systemSizeKwp, dailyConsumptionKwh, includeBattery, prices, specs),
            Premium = BuildTier("premium", systemSizeKwp, dailyConsumptionKwh, includeBattery, prices, specs),
        };
    }

    private BomTier BuildTier(
        string tier,
        decimal systemSizeKwp,
        decimal dailyConsumptionKwh,
        bool includeBattery,
        List<Models.EquipmentPrice> prices,
        List<Models.ComponentSpec> specs)
    {
        var panelSpec = specs.First(s => s.Category == "panels" && s.Tier == tier);
        var panelWattage = (int)panelSpec.WattageOrCapacity!;
        var panelCount = _calc.CalculatePanelCount(systemSizeKwp, panelWattage);
        var totalPanelWatts = panelCount * panelWattage;

        var inverterSizeKw = _calc.SelectInverterSizeKw(systemSizeKwp);

        var panelPrice = prices.First(p => p.Category == "panels" && p.Tier == tier);
        var inverterPrice = prices.First(p => p.Category == "inverter" && p.Tier == tier);
        var mountingPrice = prices.First(p => p.Category == "mounting" && p.Tier == tier);
        var wiringPrice = prices.First(p => p.Category == "wiring" && p.Tier == tier);
        var laborPrice = prices.First(p => p.Category == "labor" && p.Tier == tier);

        var panels = new BomItem
        {
            Spec = $"{panelWattage}W x {panelCount}",
            EstimatedCost = ApplyDiscount(new CostRange
            {
                Min = panelPrice.MinPricePhp * totalPanelWatts,
                Max = panelPrice.MaxPricePhp * totalPanelWatts,
            }, panelPrice.DavaoDiscountPct),
        };

        var inverter = new BomItem
        {
            Spec = $"{inverterSizeKw}kW Hybrid",
            EstimatedCost = ApplyDiscount(new CostRange
            {
                Min = inverterPrice.MinPricePhp * inverterSizeKw,
                Max = inverterPrice.MaxPricePhp * inverterSizeKw,
            }, inverterPrice.DavaoDiscountPct),
        };

        var mounting = new BomItem
        {
            Spec = "Roof-mount kit",
            EstimatedCost = ApplyDiscount(new CostRange
            {
                Min = mountingPrice.MinPricePhp,
                Max = mountingPrice.MaxPricePhp,
            }, mountingPrice.DavaoDiscountPct),
        };

        var wiring = new BomItem
        {
            Spec = "MC4, breakers, cables",
            EstimatedCost = ApplyDiscount(new CostRange
            {
                Min = wiringPrice.MinPricePhp,
                Max = wiringPrice.MaxPricePhp,
            }, wiringPrice.DavaoDiscountPct),
        };

        var labor = new BomItem
        {
            Spec = "Installation",
            EstimatedCost = ApplyDiscount(new CostRange
            {
                Min = laborPrice.MinPricePhp,
                Max = laborPrice.MaxPricePhp,
            }, laborPrice.DavaoDiscountPct),
        };

        BomItem? battery = null;
        if (includeBattery)
        {
            var batterySizeKwh = _calc.SelectBatterySizeKwh(dailyConsumptionKwh);
            var batteryPrice = prices.First(p => p.Category == "battery" && p.Tier == tier);
            battery = new BomItem
            {
                Spec = $"{batterySizeKwh}kWh LiFePO4",
                EstimatedCost = ApplyDiscount(new CostRange
                {
                    Min = batteryPrice.MinPricePhp * batterySizeKwh,
                    Max = batteryPrice.MaxPricePhp * batterySizeKwh,
                }, batteryPrice.DavaoDiscountPct),
            };
        }

        var totalMin = panels.EstimatedCost.Min + inverter.EstimatedCost.Min +
                        mounting.EstimatedCost.Min + wiring.EstimatedCost.Min + labor.EstimatedCost.Min;
        var totalMax = panels.EstimatedCost.Max + inverter.EstimatedCost.Max +
                        mounting.EstimatedCost.Max + wiring.EstimatedCost.Max + labor.EstimatedCost.Max;

        if (battery != null)
        {
            totalMin += battery.EstimatedCost.Min;
            totalMax += battery.EstimatedCost.Max;
        }

        return new BomTier
        {
            Panels = panels,
            Inverter = inverter,
            Battery = battery,
            Mounting = mounting,
            Wiring = wiring,
            Labor = labor,
            TotalEstimate = new CostRange { Min = Math.Round(totalMin, 2), Max = Math.Round(totalMax, 2) },
        };
    }

    private static CostRange ApplyDiscount(CostRange original, decimal discountPct)
    {
        var factor = 1 - (discountPct / 100);
        return new CostRange
        {
            Min = Math.Round(original.Min * factor, 2),
            Max = Math.Round(original.Max * factor, 2),
        };
    }
}
