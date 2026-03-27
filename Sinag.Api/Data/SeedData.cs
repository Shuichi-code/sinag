using Microsoft.EntityFrameworkCore;
using Sinag.Api.Models;

namespace Sinag.Api.Data;

public static class SeedData
{
    private static readonly DateOnly EffectiveDate = new(2026, 3, 1);

    public static void Seed(ModelBuilder modelBuilder)
    {
        SeedEquipmentPrices(modelBuilder);
        SeedComponentSpecs(modelBuilder);
        SeedDlpcRates(modelBuilder);
        SeedIrradianceData(modelBuilder);
    }

    private static void SeedEquipmentPrices(ModelBuilder modelBuilder)
    {
        // Prices per watt/kW/kWh from LakaSolar data with 10% Davao discount applied
        // Unit: per_watt for panels, per_kw for inverters, per_kwh for batteries, fixed for mounting/wiring/labor
        var id = 1;
        var prices = new List<EquipmentPrice>
        {
            // Panels (per watt)
            new() { Id = id++, Category = "panels", Tier = "budget", Unit = "per_watt", MinPricePhp = 22.00m, MaxPricePhp = 28.00m, DavaoDiscountPct = 10, Source = "LakaSolar 2026", EffectiveDate = EffectiveDate },
            new() { Id = id++, Category = "panels", Tier = "mid_range", Unit = "per_watt", MinPricePhp = 27.00m, MaxPricePhp = 35.00m, DavaoDiscountPct = 10, Source = "LakaSolar 2026", EffectiveDate = EffectiveDate },
            new() { Id = id++, Category = "panels", Tier = "premium", Unit = "per_watt", MinPricePhp = 34.00m, MaxPricePhp = 42.00m, DavaoDiscountPct = 10, Source = "LakaSolar 2026", EffectiveDate = EffectiveDate },

            // Inverters (per kW)
            new() { Id = id++, Category = "inverter", Tier = "budget", Unit = "per_kw", MinPricePhp = 4500.00m, MaxPricePhp = 6000.00m, DavaoDiscountPct = 10, Source = "LakaSolar 2026", EffectiveDate = EffectiveDate },
            new() { Id = id++, Category = "inverter", Tier = "mid_range", Unit = "per_kw", MinPricePhp = 6000.00m, MaxPricePhp = 8500.00m, DavaoDiscountPct = 10, Source = "LakaSolar 2026", EffectiveDate = EffectiveDate },
            new() { Id = id++, Category = "inverter", Tier = "premium", Unit = "per_kw", MinPricePhp = 8500.00m, MaxPricePhp = 12000.00m, DavaoDiscountPct = 10, Source = "LakaSolar 2026", EffectiveDate = EffectiveDate },

            // Batteries (per kWh)
            new() { Id = id++, Category = "battery", Tier = "budget", Unit = "per_kwh", MinPricePhp = 7000.00m, MaxPricePhp = 8800.00m, DavaoDiscountPct = 10, Source = "LakaSolar 2026", EffectiveDate = EffectiveDate },
            new() { Id = id++, Category = "battery", Tier = "mid_range", Unit = "per_kwh", MinPricePhp = 8800.00m, MaxPricePhp = 11000.00m, DavaoDiscountPct = 10, Source = "LakaSolar 2026", EffectiveDate = EffectiveDate },
            new() { Id = id++, Category = "battery", Tier = "premium", Unit = "per_kwh", MinPricePhp = 11000.00m, MaxPricePhp = 14000.00m, DavaoDiscountPct = 10, Source = "LakaSolar 2026", EffectiveDate = EffectiveDate },

            // Mounting (fixed per system)
            new() { Id = id++, Category = "mounting", Tier = "budget", Unit = "fixed", MinPricePhp = 7000.00m, MaxPricePhp = 10000.00m, DavaoDiscountPct = 10, Source = "LakaSolar 2026", EffectiveDate = EffectiveDate },
            new() { Id = id++, Category = "mounting", Tier = "mid_range", Unit = "fixed", MinPricePhp = 10000.00m, MaxPricePhp = 15000.00m, DavaoDiscountPct = 10, Source = "LakaSolar 2026", EffectiveDate = EffectiveDate },
            new() { Id = id++, Category = "mounting", Tier = "premium", Unit = "fixed", MinPricePhp = 15000.00m, MaxPricePhp = 22000.00m, DavaoDiscountPct = 10, Source = "LakaSolar 2026", EffectiveDate = EffectiveDate },

            // Wiring & Protection (fixed per system)
            new() { Id = id++, Category = "wiring", Tier = "budget", Unit = "fixed", MinPricePhp = 4400.00m, MaxPricePhp = 7000.00m, DavaoDiscountPct = 10, Source = "LakaSolar 2026", EffectiveDate = EffectiveDate },
            new() { Id = id++, Category = "wiring", Tier = "mid_range", Unit = "fixed", MinPricePhp = 7000.00m, MaxPricePhp = 10000.00m, DavaoDiscountPct = 10, Source = "LakaSolar 2026", EffectiveDate = EffectiveDate },
            new() { Id = id++, Category = "wiring", Tier = "premium", Unit = "fixed", MinPricePhp = 10000.00m, MaxPricePhp = 14000.00m, DavaoDiscountPct = 10, Source = "LakaSolar 2026", EffectiveDate = EffectiveDate },

            // Labor / Installation (fixed per system)
            new() { Id = id++, Category = "labor", Tier = "budget", Unit = "fixed", MinPricePhp = 13000.00m, MaxPricePhp = 22000.00m, DavaoDiscountPct = 10, Source = "LakaSolar 2026", EffectiveDate = EffectiveDate },
            new() { Id = id++, Category = "labor", Tier = "mid_range", Unit = "fixed", MinPricePhp = 18000.00m, MaxPricePhp = 28000.00m, DavaoDiscountPct = 10, Source = "LakaSolar 2026", EffectiveDate = EffectiveDate },
            new() { Id = id++, Category = "labor", Tier = "premium", Unit = "fixed", MinPricePhp = 25000.00m, MaxPricePhp = 38000.00m, DavaoDiscountPct = 10, Source = "LakaSolar 2026", EffectiveDate = EffectiveDate },
        };

        modelBuilder.Entity<EquipmentPrice>().HasData(prices);
    }

    private static void SeedComponentSpecs(ModelBuilder modelBuilder)
    {
        var id = 1;
        var specs = new List<ComponentSpec>
        {
            // Panels
            new() { Id = id++, Category = "panels", Tier = "budget", WattageOrCapacity = 450, Unit = "W", SpecLabelTemplate = "{wattage}W x {count}", AvailableSizesJson = "[450]", EffectiveDate = EffectiveDate },
            new() { Id = id++, Category = "panels", Tier = "mid_range", WattageOrCapacity = 550, Unit = "W", SpecLabelTemplate = "{wattage}W x {count}", AvailableSizesJson = "[550]", EffectiveDate = EffectiveDate },
            new() { Id = id++, Category = "panels", Tier = "premium", WattageOrCapacity = 600, Unit = "W", SpecLabelTemplate = "{wattage}W x {count}", AvailableSizesJson = "[600]", EffectiveDate = EffectiveDate },

            // Inverters
            new() { Id = id++, Category = "inverter", Tier = "budget", WattageOrCapacity = null, Unit = "kW", SpecLabelTemplate = "{capacity}kW Hybrid", AvailableSizesJson = "[3,5,8,10,15]", EffectiveDate = EffectiveDate },
            new() { Id = id++, Category = "inverter", Tier = "mid_range", WattageOrCapacity = null, Unit = "kW", SpecLabelTemplate = "{capacity}kW Hybrid", AvailableSizesJson = "[3,5,8,10,15]", EffectiveDate = EffectiveDate },
            new() { Id = id++, Category = "inverter", Tier = "premium", WattageOrCapacity = null, Unit = "kW", SpecLabelTemplate = "{capacity}kW Hybrid", AvailableSizesJson = "[3,5,8,10,15]", EffectiveDate = EffectiveDate },

            // Batteries
            new() { Id = id++, Category = "battery", Tier = "budget", WattageOrCapacity = null, Unit = "kWh", SpecLabelTemplate = "{capacity}kWh LiFePO4", AvailableSizesJson = "[5,10,15,20]", EffectiveDate = EffectiveDate },
            new() { Id = id++, Category = "battery", Tier = "mid_range", WattageOrCapacity = null, Unit = "kWh", SpecLabelTemplate = "{capacity}kWh LiFePO4", AvailableSizesJson = "[5,10,15,20]", EffectiveDate = EffectiveDate },
            new() { Id = id++, Category = "battery", Tier = "premium", WattageOrCapacity = null, Unit = "kWh", SpecLabelTemplate = "{capacity}kWh LiFePO4", AvailableSizesJson = "[5,10,15,20]", EffectiveDate = EffectiveDate },
        };

        modelBuilder.Entity<ComponentSpec>().HasData(specs);
    }

    private static void SeedDlpcRates(ModelBuilder modelBuilder)
    {
        // DLPC residential rate components (approximate 2026 rates)
        var id = 1;
        var rates = new List<DlpcRate>
        {
            new() { Id = id++, Component = "generation", RatePerKwh = 6.5200m, EffectiveDate = EffectiveDate, Source = "DLPC Rate Schedule 2026" },
            new() { Id = id++, Component = "transmission", RatePerKwh = 1.1500m, EffectiveDate = EffectiveDate, Source = "DLPC Rate Schedule 2026" },
            new() { Id = id++, Component = "distribution", RatePerKwh = 2.0800m, EffectiveDate = EffectiveDate, Source = "DLPC Rate Schedule 2026" },
            new() { Id = id++, Component = "system_loss", RatePerKwh = 0.6100m, EffectiveDate = EffectiveDate, Source = "DLPC Rate Schedule 2026" },
            new() { Id = id++, Component = "subsidies", RatePerKwh = 0.1600m, EffectiveDate = EffectiveDate, Source = "DLPC Rate Schedule 2026" },
            new() { Id = id++, Component = "taxes", RatePerKwh = 1.2700m, EffectiveDate = EffectiveDate, Source = "DLPC Rate Schedule 2026" },
            new() { Id = id++, Component = "universal_charges", RatePerKwh = 0.3200m, EffectiveDate = EffectiveDate, Source = "DLPC Rate Schedule 2026" },
            new() { Id = id++, Component = "fit_all", RatePerKwh = 0.2600m, EffectiveDate = EffectiveDate, Source = "DLPC Rate Schedule 2026" },
        };

        modelBuilder.Entity<DlpcRate>().HasData(rates);
    }

    private static void SeedIrradianceData(ModelBuilder modelBuilder)
    {
        // Davao City (7.0707°N, 125.6087°E) monthly average GHI and peak sun hours
        // Data from NASA POWER 20-year climatology
        var id = 1;
        var lat = 7.07070m;
        var lon = 125.60870m;

        var irradiance = new List<IrradianceData>
        {
            new() { Id = id++, Latitude = lat, Longitude = lon, Month = 1, GhiKwhM2Day = 4.52m, PeakSunHours = 4.52m, Source = "NASA POWER" },
            new() { Id = id++, Latitude = lat, Longitude = lon, Month = 2, GhiKwhM2Day = 4.89m, PeakSunHours = 4.89m, Source = "NASA POWER" },
            new() { Id = id++, Latitude = lat, Longitude = lon, Month = 3, GhiKwhM2Day = 5.21m, PeakSunHours = 5.21m, Source = "NASA POWER" },
            new() { Id = id++, Latitude = lat, Longitude = lon, Month = 4, GhiKwhM2Day = 5.35m, PeakSunHours = 5.35m, Source = "NASA POWER" },
            new() { Id = id++, Latitude = lat, Longitude = lon, Month = 5, GhiKwhM2Day = 4.98m, PeakSunHours = 4.98m, Source = "NASA POWER" },
            new() { Id = id++, Latitude = lat, Longitude = lon, Month = 6, GhiKwhM2Day = 4.62m, PeakSunHours = 4.62m, Source = "NASA POWER" },
            new() { Id = id++, Latitude = lat, Longitude = lon, Month = 7, GhiKwhM2Day = 4.48m, PeakSunHours = 4.48m, Source = "NASA POWER" },
            new() { Id = id++, Latitude = lat, Longitude = lon, Month = 8, GhiKwhM2Day = 4.55m, PeakSunHours = 4.55m, Source = "NASA POWER" },
            new() { Id = id++, Latitude = lat, Longitude = lon, Month = 9, GhiKwhM2Day = 4.71m, PeakSunHours = 4.71m, Source = "NASA POWER" },
            new() { Id = id++, Latitude = lat, Longitude = lon, Month = 10, GhiKwhM2Day = 4.65m, PeakSunHours = 4.65m, Source = "NASA POWER" },
            new() { Id = id++, Latitude = lat, Longitude = lon, Month = 11, GhiKwhM2Day = 4.42m, PeakSunHours = 4.42m, Source = "NASA POWER" },
            new() { Id = id++, Latitude = lat, Longitude = lon, Month = 12, GhiKwhM2Day = 4.28m, PeakSunHours = 4.28m, Source = "NASA POWER" },
        };

        modelBuilder.Entity<IrradianceData>().HasData(irradiance);
    }
}
