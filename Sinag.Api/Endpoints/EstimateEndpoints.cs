using Sinag.Api.Services;
using Sinag.Shared.Contracts;

namespace Sinag.Api.Endpoints;

public static class EstimateEndpoints
{
    public static void MapEstimateEndpoints(this WebApplication app)
    {
        app.MapPost("/api/v1/estimate", async (
            EstimateRequest request,
            CalculationService calc,
            BomService bom,
            FinancialService financial,
            IrradianceService irradiance) =>
        {
            // Input validation
            var errors = ValidateRequest(request);
            if (errors.Count > 0)
                return Results.BadRequest(new { errors });

            // Get peak sun hours for the billing month
            var peakSunHours = await irradiance.GetPeakSunHours(request.BillingMonth);

            // System sizing
            var dailyConsumption = calc.CalculateDailyConsumption(request.KwhConsumed, request.BillingPeriodDays);
            var systemSizeKwp = calc.CalculateSystemSizeKwp(dailyConsumption, peakSunHours);

            // Generate BOM
            var bomResult = await bom.GenerateBom(systemSizeKwp, dailyConsumption, request.IncludeBattery);

            // Use budget tier for financial calculations (most conservative payback estimate)
            var financialResult = financial.Calculate(
                systemSizeKwp,
                peakSunHours,
                request.KwhConsumed,
                request.GenerationChargePerKwh,
                request.TotalAmountDue,
                bomResult.Budget.TotalEstimate);

            var response = new EstimateResponse
            {
                SystemSizeKwp = systemSizeKwp,
                DailyConsumptionKwh = Math.Round(dailyConsumption, 2),
                PeakSunHours = peakSunHours,
                Bom = bomResult,
                Financial = financialResult,
                Metadata = new EstimateMetadata
                {
                    PricingAsOf = "2026-03-01",
                    IrradianceMonth = request.BillingMonth,
                    PeakSunHoursSource = "NASA POWER (Davao City, 20-year average)",
                    DavaoDiscountApplied = true,
                },
            };

            return Results.Ok(response);
        })
        .RequireRateLimiting("estimate");
    }

    private static List<string> ValidateRequest(EstimateRequest request)
    {
        var errors = new List<string>();

        if (request.KwhConsumed < 1 || request.KwhConsumed > 5000)
            errors.Add("kwhConsumed must be between 1 and 5000");

        if (request.BillingPeriodDays < 25 || request.BillingPeriodDays > 35)
            errors.Add("billingPeriodDays must be between 25 and 35");

        if (request.BillingMonth < 1 || request.BillingMonth > 12)
            errors.Add("billingMonth must be between 1 and 12");

        if (request.GenerationChargePerKwh < 0.01m || request.GenerationChargePerKwh > 50.00m)
            errors.Add("generationChargePerKwh must be between 0.01 and 50.00");

        if (request.TotalAmountDue < 100 || request.TotalAmountDue > 500000)
            errors.Add("totalAmountDue must be between 100 and 500,000");

        return errors;
    }
}
