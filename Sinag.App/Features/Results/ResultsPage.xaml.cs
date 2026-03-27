namespace Sinag.App.Features.Results;

public partial class ResultsPage : ContentPage
{
    public ResultsPage(ResultsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
