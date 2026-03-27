using System.Text.Json;
using Sinag.Shared.Contracts;

namespace Sinag.App.Services;

public class CacheService
{
    private const string PricingKey = "cached_pricing";
    private const string RatesKey = "cached_rates";
    private const string CacheDateKey = "cache_date";
    private static readonly TimeSpan CacheExpiry = TimeSpan.FromDays(7);

    public void SavePricing(PricingResponse pricing)
    {
        var json = JsonSerializer.Serialize(pricing);
        Preferences.Set(PricingKey, json);
        Preferences.Set(CacheDateKey, DateTime.UtcNow.ToString("o"));
    }

    public PricingResponse? GetCachedPricing()
    {
        var json = Preferences.Get(PricingKey, string.Empty);
        if (string.IsNullOrEmpty(json)) return null;
        if (IsCacheExpired()) return null;
        return JsonSerializer.Deserialize<PricingResponse>(json);
    }

    public void SaveRates(RatesResponse rates)
    {
        var json = JsonSerializer.Serialize(rates);
        Preferences.Set(RatesKey, json);
        Preferences.Set(CacheDateKey, DateTime.UtcNow.ToString("o"));
    }

    public RatesResponse? GetCachedRates()
    {
        var json = Preferences.Get(RatesKey, string.Empty);
        if (string.IsNullOrEmpty(json)) return null;
        if (IsCacheExpired()) return null;
        return JsonSerializer.Deserialize<RatesResponse>(json);
    }

    public DateTime? GetCacheDate()
    {
        var dateStr = Preferences.Get(CacheDateKey, string.Empty);
        if (string.IsNullOrEmpty(dateStr)) return null;
        if (DateTime.TryParse(dateStr, out var date)) return date;
        return null;
    }

    private bool IsCacheExpired()
    {
        var cacheDate = GetCacheDate();
        if (cacheDate == null) return true;
        return DateTime.UtcNow - cacheDate.Value > CacheExpiry;
    }
}
