namespace Sinag.App.Tests;

/// <summary>
/// ImagePreprocessor tests are limited in a non-MAUI test context since
/// actual image resize requires platform-specific MAUI graphics APIs.
/// Full integration tests should run on device/emulator.
/// </summary>
public class ImagePreprocessorTests
{
    [Fact]
    public void Placeholder_ImagePreprocessorRequiresDeviceTesting()
    {
        // ImagePreprocessor depends on MAUI platform APIs for image resizing.
        // Meaningful tests require running on an Android emulator or device.
        // This placeholder ensures the test class exists for future device-level tests.
        Assert.True(true);
    }
}
