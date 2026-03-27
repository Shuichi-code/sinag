using System.Net.Http.Json;
using Sinag.Shared.Contracts;

namespace Sinag.App.Services;

public class ApiClient
{
    private readonly HttpClient _httpClient;

    public ApiClient()
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5000")
        };
    }

    public ApiClient(string baseUrl)
    {
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };
    }

    public async Task<EstimateResponse?> PostEstimateAsync(EstimateRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/v1/estimate", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<EstimateResponse>();
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<PricingResponse?> GetPricingAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<PricingResponse>("/api/v1/pricing");
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<RatesResponse?> GetRatesAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<RatesResponse>("/api/v1/rates");
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<bool> GetHealthAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/health");
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
