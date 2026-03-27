namespace Sinag.App.Features.ManualEntry;

public partial class ManualEntryPage : ContentPage
{
    public ManualEntryPage(ManualEntryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
