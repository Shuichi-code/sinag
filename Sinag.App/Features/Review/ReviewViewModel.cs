using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sinag.App.Models;
using Sinag.App.Services;
using Sinag.Shared.Contracts;

namespace Sinag.App.Features.Review;

[QueryProperty(nameof(BillData), "BillData")]
public partial class ReviewViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;
    private readonly CacheService _cacheService;
    private readonly ConnectivityService _connectivityService;

    [ObservableProperty]
    private BillData? _billData;

    [ObservableProperty]
    private string _kwhConsumedText = string.Empty;

    [ObservableProperty]
    private string _billingPeriodDaysText = "30";

    [ObservableProperty]
    private int _selectedBillingMonthIndex;

    [ObservableProperty]
    private string _generationChargeText = "6.52";

    [ObservableProperty]
    private string _totalAmountDueText = string.Empty;

    [ObservableProperty]
    private bool _includeBattery;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private ConfidenceLevel _kwhConfidence = ConfidenceLevel.High;

    [ObservableProperty]
    private ConfidenceLevel _totalAmountConfidence = ConfidenceLevel.High;

    [ObservableProperty]
    private ConfidenceLevel _generationChargeConfidence = ConfidenceLevel.High;

    [ObservableProperty]
    private ConfidenceLevel _billingMonthConfidence = ConfidenceLevel.High;

    public List<string> Months { get; } = new()
    {
        "Enero", "Pebrero", "Marso", "Abril", "Mayo", "Hunyo",
        "Hulyo", "Agosto", "Setyembre", "Oktubre", "Nobyembre", "Disyembre"
    };

    public ReviewViewModel(ApiClient apiClient, CacheService cacheService, ConnectivityService connectivityService)
    {
        _apiClient = apiClient;
        _cacheService = cacheService;
        _connectivityService = connectivityService;
    }

    partial void OnBillDataChanged(BillData? value)
    {
        if (value == null) return;

        KwhConsumedText = value.KwhConsumed > 0 ? value.KwhConsumed.ToString() : string.Empty;
        BillingPeriodDaysText = value.BillingPeriodDays > 0 ? value.BillingPeriodDays.ToString() : "30";
        SelectedBillingMonthIndex = value.BillingMonth > 0 ? value.BillingMonth - 1 : DateTime.Now.Month - 1;
        GenerationChargeText = value.GenerationChargePerKwh > 0 ? value.GenerationChargePerKwh.ToString("F2") : "6.52";
        TotalAmountDueText = value.TotalAmountDue > 0 ? value.TotalAmountDue.ToString("F2") : string.Empty;

        // Set confidence levels
        KwhConfidence = value.FieldConfidence.GetValueOrDefault("KwhConsumed", ConfidenceLevel.High);
        TotalAmountConfidence = value.FieldConfidence.GetValueOrDefault("TotalAmountDue", ConfidenceLevel.High);
        GenerationChargeConfidence = value.FieldConfidence.GetValueOrDefault("GenerationChargePerKwh", ConfidenceLevel.High);
        BillingMonthConfidence = value.FieldConfidence.GetValueOrDefault("BillingMonth", ConfidenceLevel.High);
    }

    [RelayCommand]
    private async Task Confirm()
    {
        if (!int.TryParse(KwhConsumedText, out var kwh) || kwh < 1)
        {
            await Shell.Current.DisplayAlert("Error", "Maglagay ng valid na kWh consumed.", "OK");
            return;
        }

        IsLoading = true;

        try
        {
            int.TryParse(BillingPeriodDaysText, out var billingDays);
            if (billingDays < 25 || billingDays > 35) billingDays = 30;

            decimal.TryParse(GenerationChargeText, out var genCharge);
            if (genCharge <= 0) genCharge = 6.52m;

            decimal.TryParse(TotalAmountDueText, out var totalAmount);

            var request = new EstimateRequest
            {
                KwhConsumed = kwh,
                BillingPeriodDays = billingDays,
                BillingMonth = SelectedBillingMonthIndex + 1,
                GenerationChargePerKwh = genCharge,
                TotalAmountDue = totalAmount,
                IncludeBattery = IncludeBattery
            };

            var response = await _apiClient.PostEstimateAsync(request);
            if (response == null)
            {
                await Shell.Current.DisplayAlert("Error", "Hindi makakonekta sa server. Subukan muli.", "OK");
                return;
            }

            var navParams = new Dictionary<string, object>
            {
                { "EstimateResponse", response },
                { "EstimateRequest", request }
            };
            await Shell.Current.GoToAsync("ResultsPage", navParams);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task Rescan()
    {
        await Shell.Current.GoToAsync("ScanPage");
    }
}
