using Sinag.Api.Services;

namespace Sinag.Api.Endpoints;

public static class RatesEndpoints
{
    public static void MapRatesEndpoints(this WebApplication app)
    {
        app.MapGet("/api/v1/rates", async (RateService rates) =>
        {
            var result = await rates.GetCurrentRates();
            return Results.Ok(result);
        });
    }
}
