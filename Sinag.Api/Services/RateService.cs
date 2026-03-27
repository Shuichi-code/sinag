using Microsoft.EntityFrameworkCore;
using Sinag.Api.Data;
using Sinag.Shared.Contracts;

namespace Sinag.Api.Services;

public class RateService
{
    private readonly AppDbContext _db;

    public RateService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<RatesResponse> GetCurrentRates()
    {
        var rates = await _db.DlpcRates.ToListAsync();

        return new RatesResponse
        {
            Components = rates.Select(r => new RateComponent
            {
                Component = r.Component,
                RatePerKwh = r.RatePerKwh,
            }).ToList(),
            EffectiveDate = rates.FirstOrDefault()?.EffectiveDate.ToString("yyyy-MM-dd") ?? "",
            TotalBlendedRate = rates.Sum(r => r.RatePerKwh),
        };
    }

    public async Task<decimal> GetGenerationRate()
    {
        var genRate = await _db.DlpcRates
            .Where(r => r.Component == "generation")
            .OrderByDescending(r => r.EffectiveDate)
            .FirstOrDefaultAsync();

        return genRate?.RatePerKwh ?? 6.52m;
    }
}
