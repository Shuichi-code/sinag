using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sinag.App.Services;

namespace Sinag.App.Features.Home;

public partial class HomeViewModel : ObservableObject
{
    [ObservableProperty]
    private string _selectedLanguage = LocalizationService.Instance.CurrentLanguage;

    [RelayCommand]
    private async Task ScanBill()
    {
        await Shell.Current.GoToAsync("ScanPage");
    }

    [RelayCommand]
    private async Task ManualEntry()
    {
        await Shell.Current.GoToAsync("ManualEntryPage");
    }

    [RelayCommand]
    private void SetLanguage(string code)
    {
        if (LocalizationService.Instance.CurrentLanguage == code) return;
        SelectedLanguage = code;
        LocalizationService.Instance.SetLanguage(code);
    }
}
