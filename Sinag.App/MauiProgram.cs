using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Plugin.Maui.OCR;
using Sinag.App.Features.Home;
using Sinag.App.Features.ManualEntry;
using Sinag.App.Features.NextSteps;
using Sinag.App.Features.Results;
using Sinag.App.Features.Review;
using Sinag.App.Features.Scan;
using Sinag.App.Services;

namespace Sinag.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseOcr()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("PlusJakartaSans-Variable.ttf", "PlusJakartaSans");
                fonts.AddFont("BeVietnamPro-Regular.ttf", "BeVietnamProRegular");
                fonts.AddFont("BeVietnamPro-Medium.ttf", "BeVietnamProMedium");
                fonts.AddFont("BeVietnamPro-Bold.ttf", "BeVietnamProBold");
            });

        // Services
        builder.Services.AddSingleton<OcrService>();
        builder.Services.AddSingleton<ApiClient>();
        builder.Services.AddSingleton<CacheService>();
        builder.Services.AddSingleton<ConnectivityService>();
        builder.Services.AddSingleton<ImagePreprocessor>();
        builder.Services.AddSingleton<BillParser>();

        // ViewModels
        builder.Services.AddTransient<HomeViewModel>();
        builder.Services.AddTransient<ScanViewModel>();
        builder.Services.AddTransient<ReviewViewModel>();
        builder.Services.AddTransient<ManualEntryViewModel>();
        builder.Services.AddTransient<ResultsViewModel>();
        builder.Services.AddTransient<NextStepsViewModel>();

        // Pages
        builder.Services.AddTransient<HomePage>();
        builder.Services.AddTransient<ScanPage>();
        builder.Services.AddTransient<ReviewPage>();
        builder.Services.AddTransient<ManualEntryPage>();
        builder.Services.AddTransient<ResultsPage>();
        builder.Services.AddTransient<NextStepsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
