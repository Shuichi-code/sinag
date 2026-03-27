using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Sinag.Api.Data;
using Sinag.Api.Models;

namespace Sinag.Api.Services;

public class IrradianceService
{
    private readonly AppDbContext _db;
    private readonly HttpClient _httpClient;
    private readonly ILogger<IrradianceService> _logger;

    private const decimal DavaoLatitude = 7.0707m;
    private const decimal DavaoLongitude = 125.6087m;

    public IrradianceService(AppDbContext db, HttpClient httpClient, ILogger<IrradianceService> logger)
    {
        _db = db;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<decimal> GetPeakSunHours(int month)
    {
        // Try cache first
        var cached = await _db.IrradianceCache
            .FirstOrDefaultAsync(i => i.Latitude == DavaoLatitude
                                      && i.Longitude == DavaoLongitude
                                      && i.Month == month);

        if (cached != null)
            return cached.PeakSunHours;

        // Try to fetch from NASA POWER API
        try
        {
            await FetchAndCacheFromNasa();
            cached = await _db.IrradianceCache
                .FirstOrDefaultAsync(i => i.Latitude == DavaoLatitude
                                          && i.Longitude == DavaoLongitude
                                          && i.Month == month);
            if (cached != null)
                return cached.PeakSunHours;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to fetch irradiance data from NASA POWER API");
        }

        // Fallback: use a reasonable average for Davao
        return 4.72m;
    }

    private async Task FetchAndCacheFromNasa()
    {
        var url = $"https://power.larc.nasa.gov/api/temporal/monthly/point?" +
                  $"parameters=ALLSKY_SFC_SW_DWN&community=RE&longitude={DavaoLongitude}&latitude={DavaoLatitude}" +
                  $"&start=2004&end=2023&format=JSON";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        var parameters = doc.RootElement
            .GetProperty("properties")
            .GetProperty("parameter")
            .GetProperty("ALLSKY_SFC_SW_DWN");

        // Calculate monthly averages across all years
        var monthlyTotals = new decimal[12];
        var monthlyCounts = new int[12];

        foreach (var property in parameters.EnumerateObject())
        {
            var key = property.Name;
            if (key.Length == 6 && int.TryParse(key[4..], out var monthNum) && monthNum >= 1 && monthNum <= 12)
            {
                if (property.Value.TryGetDecimal(out var value) && value > 0)
                {
                    monthlyTotals[monthNum - 1] += value;
                    monthlyCounts[monthNum - 1]++;
                }
            }
        }

        for (var month = 1; month <= 12; month++)
        {
            var count = monthlyCounts[month - 1];
            if (count == 0) continue;

            var avgGhi = Math.Round(monthlyTotals[month - 1] / count, 2);

            var existing = await _db.IrradianceCache
                .FirstOrDefaultAsync(i => i.Latitude == DavaoLatitude
                                          && i.Longitude == DavaoLongitude
                                          && i.Month == month);
            if (existing != null)
            {
                existing.GhiKwhM2Day = avgGhi;
                existing.PeakSunHours = avgGhi;
                existing.FetchedAt = DateTime.UtcNow;
            }
            else
            {
                _db.IrradianceCache.Add(new IrradianceData
                {
                    Latitude = DavaoLatitude,
                    Longitude = DavaoLongitude,
                    Month = month,
                    GhiKwhM2Day = avgGhi,
                    PeakSunHours = avgGhi,
                    Source = "NASA POWER",
                    FetchedAt = DateTime.UtcNow,
                });
            }
        }

        await _db.SaveChangesAsync();
    }
}
