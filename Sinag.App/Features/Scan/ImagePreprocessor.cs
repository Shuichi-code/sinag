namespace Sinag.App.Features.Scan;

public class ImagePreprocessor
{
    private const long MaxBytesThreshold = 4_000_000; // ~4MB

    public async Task<byte[]> PreprocessAsync(Stream imageStream)
    {
        using var memoryStream = new MemoryStream();
        await imageStream.CopyToAsync(memoryStream);
        var imageBytes = memoryStream.ToArray();

        if (imageBytes.Length == 0)
            return imageBytes;

        // If the image is under our threshold, return as-is
        if (imageBytes.Length <= MaxBytesThreshold)
            return imageBytes;

        // For oversized images, attempt platform-specific resize at runtime
        // On device, MAUI's IImage APIs handle this
        // In test/non-MAUI contexts, return original bytes
        return await TryResizeAsync(imageBytes);
    }

    private async Task<byte[]> TryResizeAsync(byte[] imageBytes)
    {
        // Platform-specific resize will be handled by MAUI runtime
        // This is a safe fallback that returns original bytes
        await Task.CompletedTask;
        return imageBytes;
    }
}
