using Sinag.Api.Services;

namespace Sinag.Api.Endpoints;

public static class PricingEndpoints
{
    public static void MapPricingEndpoints(this WebApplication app)
    {
        app.MapGet("/api/v1/pricing", async (PricingService pricing) =>
        {
            var result = await pricing.GetCurrentPricing();
            return Results.Ok(result);
        });
    }
}
