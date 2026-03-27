using Plugin.Maui.OCR;

namespace Sinag.App.Features.Scan;

public class OcrService
{
    private readonly IOcrService _ocrService;

    public OcrService(IOcrService ocrService)
    {
        _ocrService = ocrService;
    }

    public async Task<OcrResult> RecognizeTextAsync(byte[] imageData)
    {
        await _ocrService.InitAsync();
        var result = await _ocrService.RecognizeTextAsync(imageData);
        return result;
    }
}
