using Microsoft.EntityFrameworkCore;
using Sinag.Api.Data;
using Sinag.Shared.Contracts;

namespace Sinag.Api.Services;

public class PricingService
{
    private readonly AppDbContext _db;

    public PricingService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PricingResponse> GetCurrentPricing()
    {
        var prices = await _db.EquipmentPrices.ToListAsync();

        return new PricingResponse
        {
            Categories = prices.Select(p => new PricingCategory
            {
                Category = p.Category,
                Tier = p.Tier,
                Unit = p.Unit,
                MinPricePhp = p.MinPricePhp,
                MaxPricePhp = p.MaxPricePhp,
                DavaoDiscountPct = p.DavaoDiscountPct,
            }).ToList(),
            EffectiveDate = prices.FirstOrDefault()?.EffectiveDate.ToString("yyyy-MM-dd") ?? "",
        };
    }
}
