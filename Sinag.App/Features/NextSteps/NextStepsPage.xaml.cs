namespace Sinag.App.Features.NextSteps;

public partial class NextStepsPage : ContentPage
{
    public NextStepsPage(NextStepsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
