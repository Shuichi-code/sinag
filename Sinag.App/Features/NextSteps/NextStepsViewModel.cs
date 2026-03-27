using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sinag.App.Models;

namespace Sinag.App.Features.NextSteps;

public partial class NextStepsViewModel : ObservableObject
{
    public List<InstallerInfo> Installers { get; } = new()
    {
        new InstallerInfo
        {
            Name = "Sunstruck Solar Solutions",
            Description = "DOE Accredited",
            Website = "https://sunstruck.ph",
            IsDoeAccredited = true
        },
        new InstallerInfo
        {
            Name = "Electro-Jake Solar",
            Phone = "0910-555-5655",
            Website = "https://electrojake.com"
        },
        new InstallerInfo
        {
            Name = "MPPT Solar Energy Corp",
            Phone = "082-298-4106",
            Website = "https://mpptsolarenergy.com"
        },
        new InstallerInfo
        {
            Name = "Solar Powerhaus",
            Phone = "0917-659-7300",
            Website = "https://solarpowerhaus.com"
        },
        new InstallerInfo
        {
            Name = "Flaretech Solar Technology",
            Phone = "0917-708-6347",
            Website = "https://flaretechsolartechnology.com"
        },
        new InstallerInfo
        {
            Name = "Prime Solar PH",
            Phone = "0915-136-0841",
            Website = "https://primesolarph.com"
        },
        new InstallerInfo
        {
            Name = "WCTI Solar",
            Phone = "082-221-2589",
            Description = "Est. 1989"
        }
    };

    [RelayCommand]
    private async Task CallInstaller(InstallerInfo installer)
    {
        if (string.IsNullOrEmpty(installer.Phone)) return;

        try
        {
            PhoneDialer.Default.Open(installer.Phone.Replace("-", ""));
        }
        catch (Exception)
        {
            await Shell.Current.DisplayAlert("Error", "Hindi ma-open ang phone dialer.", "OK");
        }
    }

    [RelayCommand]
    private async Task OpenWebsite(InstallerInfo installer)
    {
        if (string.IsNullOrEmpty(installer.Website)) return;

        try
        {
            await Browser.Default.OpenAsync(new Uri(installer.Website), BrowserLaunchMode.SystemPreferred);
        }
        catch (Exception)
        {
            await Shell.Current.DisplayAlert("Error", "Hindi ma-open ang website.", "OK");
        }
    }

    [RelayCommand]
    private async Task GoHome()
    {
        await Shell.Current.GoToAsync("//HomePage");
    }
}
