using Sinag.App.Features.ManualEntry;
using Sinag.App.Features.NextSteps;
using Sinag.App.Features.Results;
using Sinag.App.Features.Review;
using Sinag.App.Features.Scan;

namespace Sinag.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("ScanPage", typeof(ScanPage));
        Routing.RegisterRoute("ReviewPage", typeof(ReviewPage));
        Routing.RegisterRoute("ManualEntryPage", typeof(ManualEntryPage));
        Routing.RegisterRoute("ResultsPage", typeof(ResultsPage));
        Routing.RegisterRoute("NextStepsPage", typeof(NextStepsPage));
    }
}
