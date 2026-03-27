using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sinag.App.Models;

namespace Sinag.App.Features.Scan;

public partial class ScanViewModel : ObservableObject
{
    private readonly OcrService _ocrService;
    private readonly ImagePreprocessor _imagePreprocessor;
    private readonly BillParser _billParser;

    [ObservableProperty]
    private bool _isProcessing;

    [ObservableProperty]
    private string _statusText = "Ilagay ang iyong bill sa patag na surface at kunan ng litrato mula sa itaas";

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

    public ScanViewModel(OcrService ocrService, ImagePreprocessor imagePreprocessor, BillParser billParser)
    {
        _ocrService = ocrService;
        _imagePreprocessor = imagePreprocessor;
        _billParser = billParser;
    }

    [RelayCommand]
    private async Task CapturePhoto()
    {
        await ProcessImage(() => MediaPicker.Default.CapturePhotoAsync());
    }

    [RelayCommand]
    private async Task PickFromGallery()
    {
        await ProcessImage(() => MediaPicker.Default.PickPhotoAsync());
    }

    [RelayCommand]
    private async Task GoToManualEntry()
    {
        await Shell.Current.GoToAsync("ManualEntryPage");
    }

    private async Task ProcessImage(Func<Task<FileResult?>> imageSource)
    {
        try
        {
            HasError = false;
            ErrorMessage = string.Empty;

            var photo = await imageSource();
            if (photo == null) return;

            IsProcessing = true;
            StatusText = "Pinoproseso ang iyong bill...";

            using var stream = await photo.OpenReadAsync();
            var preprocessed = await _imagePreprocessor.PreprocessAsync(stream);

            StatusText = "Binabasa ang teksto...";
            var ocrResult = await _ocrService.RecognizeTextAsync(preprocessed);

            if (!ocrResult.Success || string.IsNullOrWhiteSpace(ocrResult.AllText))
            {
                HasError = true;
                ErrorMessage = "Hindi mabasa ang bill. Subukan muli o mag-type ng manual.";
                StatusText = "Hindi matagumpay ang pag-scan";
                return;
            }

            StatusText = "Kinukuha ang data...";
            var billData = _billParser.Parse(ocrResult.AllText);

            var navParams = new Dictionary<string, object>
            {
                { "BillData", billData }
            };
            await Shell.Current.GoToAsync("ReviewPage", navParams);
        }
        catch (BillParseException ex)
        {
            HasError = true;
            ErrorMessage = ex.Message;
            StatusText = "Hindi matagumpay ang pag-scan";
        }
        catch (Exception)
        {
            HasError = true;
            ErrorMessage = "May error na nangyari. Subukan muli o mag-type ng manual.";
            StatusText = "Hindi matagumpay ang pag-scan";
        }
        finally
        {
            IsProcessing = false;
        }
    }
}
