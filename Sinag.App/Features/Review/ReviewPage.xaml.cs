namespace Sinag.App.Features.Review;

public partial class ReviewPage : ContentPage
{
    public ReviewPage(ReviewViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
