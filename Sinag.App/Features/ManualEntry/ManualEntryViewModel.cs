using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sinag.App.Services;
using Sinag.Shared.Contracts;

namespace Sinag.App.Features.ManualEntry;

public partial class ManualEntryViewModel : ObservableObject
{
    private readonly ApiClient _apiClient;

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

    public List<string> Months { get; } = new()
    {
        "Enero", "Pebrero", "Marso", "Abril", "Mayo", "Hunyo",
        "Hulyo", "Agosto", "Setyembre", "Oktubre", "Nobyembre", "Disyembre"
    };

    public ManualEntryViewModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
        _selectedBillingMonthIndex = DateTime.Now.Month - 1;
    }

    [RelayCommand]
    private async Task Calculate()
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
}
